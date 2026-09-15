using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.CraftAvailability;

/// <summary>
/// Builds a recipe reverse index once and only recalculates availability after inventory/options change.
/// It deliberately consumes the monitor's public snapshot and does not alter inventory scanning or tooltips.
/// </summary>
public sealed class CraftAvailabilityService : IDisposable
{
    private const uint MaxDisplayedCrafts = 9999;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly RecipeSheet _recipeSheet;
    private readonly ItemSheet _itemSheet;
    private readonly ILogger<CraftAvailabilityService> _logger;
    private readonly Dictionary<uint, List<RecipeRow>> _recipesByIngredient = new();
    private readonly Dictionary<uint, List<RecipeRow>> _recipesByResult = new();
    private List<RecipeRow> _recipes = new();
    private IReadOnlyList<CraftAvailabilityResult> _cachedResults = Array.Empty<CraftAvailabilityResult>();
    private CacheKey? _cacheKey;
    private long _inventoryRevision;

    public CraftAvailabilityService(IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor,
        RecipeSheet recipeSheet, ItemSheet itemSheet, ILogger<CraftAvailabilityService> logger)
    {
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _recipeSheet = recipeSheet;
        _itemSheet = itemSheet;
        _logger = logger;
        BuildRecipeIndexes();
        _inventoryMonitor.OnInventoryChanged += OnInventoryChanged;
    }

    public int IndexedIngredientCount => _recipesByIngredient.Count;

    public IReadOnlyList<CraftAvailabilityResult> GetResults(bool includeRetainers, bool includeSubrecipes,
        bool ignoreCrystals, bool craftableOnly, CraftAvailabilityCategory category)
    {
        var key = new CacheKey(_inventoryRevision, includeRetainers, includeSubrecipes, ignoreCrystals, craftableOnly, category);
        if (_cacheKey == key)
            return _cachedResults;

        var inventory = BuildInventorySnapshot(includeRetainers);
        var results = new List<CraftAvailabilityResult>();
        foreach (var recipe in _recipes)
        {
            var item = _itemSheet.GetRowOrDefault(recipe.Base.ItemResult.RowId);
            if (item == null || string.IsNullOrWhiteSpace(item.NameString))
                continue;

            var recipeCategory = Classify(item);
            if (category != CraftAvailabilityCategory.All && recipeCategory != category)
                continue;

            var maxCrafts = CalculateMaxCrafts(recipe, inventory, includeSubrecipes, ignoreCrystals);
            if (craftableOnly && maxCrafts == 0)
                continue;

            results.Add(new CraftAvailabilityResult(
                recipe.RowId,
                item.RowId,
                item.NameString,
                recipe.Base.CraftType.ValueNullable?.Name.ExtractText() ?? "未知",
                recipe.RecipeLevelTable?.Base.ClassJobLevel ?? recipe.Base.RecipeLevelTable.RowId,
                maxCrafts,
                Math.Max(1u, recipe.Base.AmountResult),
                recipeCategory,
                recipe.IngredientCounts
                    .Where(c => c.Key != 0 && c.Value != 0)
                    .ToDictionary(c => (uint)c.Key, c => (uint)c.Value),
                new Dictionary<uint, uint>()));
        }

        _cachedResults = results
            .OrderByDescending(c => c.MaxCrafts > 0)
            .ThenByDescending(c => c.RecipeLevel)
            .ThenBy(c => c.Name, StringComparer.CurrentCulture)
            .ToArray();
        _cacheKey = key;
        return _cachedResults;
    }

    public void Invalidate()
    {
        _inventoryRevision++;
        _cacheKey = null;
    }

    private void BuildRecipeIndexes()
    {
        _recipes = _recipeSheet.Where(c => c.RowId != 0 && c.Base.ItemResult.RowId != 0 && c.IngredientCounts.Any(i => i.Key != 0 && i.Value != 0)).ToList();
        foreach (var recipe in _recipes)
        {
            if (!_recipesByResult.TryGetValue(recipe.Base.ItemResult.RowId, out var resultRecipes))
                _recipesByResult[recipe.Base.ItemResult.RowId] = resultRecipes = new List<RecipeRow>();
            resultRecipes.Add(recipe);

            foreach (var ingredientId in recipe.IngredientCounts.Where(c => c.Key != 0 && c.Value != 0).Select(c => (uint)c.Key).Distinct())
            {
                if (!_recipesByIngredient.TryGetValue(ingredientId, out var ingredientRecipes))
                    _recipesByIngredient[ingredientId] = ingredientRecipes = new List<RecipeRow>();
                ingredientRecipes.Add(recipe);
            }
        }
        _logger.LogInformation("Indexed {RecipeCount} recipes and {IngredientCount} recipe ingredients for craft availability.", _recipes.Count, _recipesByIngredient.Count);
    }

    private Dictionary<uint, long> BuildInventorySnapshot(bool includeRetainers)
    {
        var allowedOwners = new HashSet<ulong>();
        var activeCharacterId = _characterMonitor.ActiveCharacterId;
        if (activeCharacterId != 0)
            allowedOwners.Add(activeCharacterId);
        if (includeRetainers)
            foreach (var retainer in _characterMonitor.GetRetainerCharacters(activeCharacterId))
                allowedOwners.Add(retainer.Key);

        return _inventoryMonitor.AllItems
            .Where(c => c.ItemId != 0 && c.Quantity != 0 && allowedOwners.Contains(c.RetainerId))
            // MAX must only count locations Artisan can consume directly or
            // retrieve through the summoning-bell workflow. Retainer market,
            // equipped gear, saddlebags, housing and other characters are
            // visible to AllaganTools but cannot satisfy an Artisan list.
            .Where(c => c.RetainerId == activeCharacterId
                ? c.SortedContainer is CriticalCommonLib.Enums.InventoryType.Bag0
                    or CriticalCommonLib.Enums.InventoryType.Bag1
                    or CriticalCommonLib.Enums.InventoryType.Bag2
                    or CriticalCommonLib.Enums.InventoryType.Bag3
                    or CriticalCommonLib.Enums.InventoryType.Crystal
                : c.SortedContainer is CriticalCommonLib.Enums.InventoryType.RetainerBag0
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag1
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag2
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag3
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag4
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag5
                    or CriticalCommonLib.Enums.InventoryType.RetainerBag6
                    or CriticalCommonLib.Enums.InventoryType.RetainerCrystal)
            .GroupBy(c => c.ItemId)
            .ToDictionary(c => c.Key, c => c.Sum(i => (long)i.Quantity));
    }

    private uint CalculateMaxCrafts(RecipeRow recipe, Dictionary<uint, long> inventory, bool includeSubrecipes, bool ignoreCrystals)
    {
        if (!CanCraft(recipe, 1, inventory, includeSubrecipes, ignoreCrystals))
            return 0;

        uint low = 1;
        uint high = 2;
        while (high < MaxDisplayedCrafts && CanCraft(recipe, high, inventory, includeSubrecipes, ignoreCrystals))
        {
            low = high;
            high = Math.Min(MaxDisplayedCrafts, high * 2);
        }
        if (high == MaxDisplayedCrafts && CanCraft(recipe, high, inventory, includeSubrecipes, ignoreCrystals))
            return high;

        while (low + 1 < high)
        {
            var middle = low + ((high - low) / 2);
            if (CanCraft(recipe, middle, inventory, includeSubrecipes, ignoreCrystals)) low = middle;
            else high = middle;
        }
        return low;
    }

    private bool CanCraft(RecipeRow recipe, uint craftCount, Dictionary<uint, long> inventory, bool includeSubrecipes, bool ignoreCrystals)
    {
        var remaining = new Dictionary<uint, long>(inventory);
        var stack = new HashSet<uint> { recipe.Base.ItemResult.RowId };
        foreach (var ingredient in recipe.IngredientCounts)
        {
            if (ingredient.Key == 0 || ingredient.Value == 0 || ignoreCrystals && IsCrystal((uint)ingredient.Key))
                continue;
            if (!Consume((uint)ingredient.Key, checked((long)ingredient.Value * craftCount), remaining, includeSubrecipes, ignoreCrystals, stack))
                return false;
        }
        return true;
    }

    private bool Consume(uint itemId, long required, Dictionary<uint, long> inventory, bool includeSubrecipes,
        bool ignoreCrystals, HashSet<uint> stack)
    {
        inventory.TryGetValue(itemId, out var available);
        var used = Math.Min(available, required);
        inventory[itemId] = available - used;
        required -= used;
        if (required == 0)
            return true;
        if (!includeSubrecipes || stack.Contains(itemId) || !_recipesByResult.TryGetValue(itemId, out var recipes))
            return false;

        var recipe = recipes[0];
        var yield = Math.Max(1u, recipe.Base.AmountResult);
        var crafts = (required + yield - 1) / yield;
        var branchInventory = new Dictionary<uint, long>(inventory);
        var branchStack = new HashSet<uint>(stack) { itemId };
        foreach (var ingredient in recipe.IngredientCounts)
        {
            if (ingredient.Key == 0 || ingredient.Value == 0 || ignoreCrystals && IsCrystal((uint)ingredient.Key))
                continue;
            if (!Consume((uint)ingredient.Key, checked((long)ingredient.Value * crafts), branchInventory, true, ignoreCrystals, branchStack))
                return false;
        }
        branchInventory[itemId] = (crafts * yield) - required;
        inventory.Clear();
        foreach (var pair in branchInventory) inventory[pair.Key] = pair.Value;
        return true;
    }

    private static bool IsCrystal(uint itemId) => itemId is >= 2 and <= 19;

    private static CraftAvailabilityCategory Classify(ItemRow item)
    {
        var uiCategory = item.Base.ItemUICategory.RowId;
        if (uiCategory == 43) return CraftAvailabilityCategory.Food;
        if (uiCategory == 44) return CraftAvailabilityCategory.Medicine;
        if (item.Base.EquipSlotCategory.RowId != 0)
        {
            IEnumerable<uint> jobs = item.ClassJobCategory?.ClassJobIds ?? new List<uint>();
            if (jobs.Any(c => c is >= 8 and <= 15)) return CraftAvailabilityCategory.CraftingGear;
            if (jobs.Any(c => c is >= 16 and <= 18)) return CraftAvailabilityCategory.GatheringGear;
            return CraftAvailabilityCategory.CombatGear;
        }
        return CraftAvailabilityCategory.IntermediateMaterial;
    }

    private void OnInventoryChanged(List<InventoryChange> _, InventoryMonitor.ItemChanges? __) => Invalidate();

    public void Dispose() => _inventoryMonitor.OnInventoryChanged -= OnInventoryChanged;

    private sealed record CacheKey(long Revision, bool Retainers, bool Subrecipes, bool IgnoreCrystals,
        bool CraftableOnly, CraftAvailabilityCategory Category);
}
