using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Web;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using InventoryTools.Logic;

using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Humanizer;
using InventoryTools.Localizers;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Widgets;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;
using OtterGui.Log;
using OtterGui.Widgets;
using ImGuiTable = OtterGui.ImGuiTable;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public class WorldPicker : FilterComboBase<World>
    {
        public HashSet<uint> SelectedWorldIds { get; set; } = new();
        public WorldPicker(IReadOnlyList<World> items, bool keepStorage, Logger log) : base(items, keepStorage, log)
        {

        }

        protected override string ToString(World obj)
        {
            return obj.Name.ExtractText();
        }
    }
    public class ItemWindow : UintWindow
    {
        private readonly IMarketBoardService _marketBoardService;
        private readonly IFramework _framework;
        private readonly ICommandManager _commandManager;
        private readonly IListService _listService;
        private readonly ItemSheet _itemSheet;
        private readonly ExcelSheet<World> _worldSheet;
        private readonly IGameInterface _gameInterface;
        private readonly IMarketCache _marketCache;
        private readonly IChatUtilities _chatUtilities;
        private readonly Logger _otterLogger;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IClipboardService _clipboardService;
        private readonly ItemInfoRenderService _itemInfoRenderService;
        private readonly BNpcNameSheet _bNpcNameSheet;
        private readonly MapSheet _mapSheet;
        private readonly IUnlockTrackerService _unlockTrackerService;
        private readonly ImGuiTooltipService _tooltipService;
        private readonly ImGuiTooltipModeSetting _tooltipModeSetting;
        private readonly ItemLocalizer _itemLocalizer;
        private readonly TeleporterService _teleporterService;
        private readonly CraftList.Factory _craftListFactory;
        private HashSet<uint> _marketRefreshing = new();
        private readonly Dictionary<uint, DateTime> _marketRequestStarted = new();
        private List<uint> _availableMarketWorlds = new();
        private bool _showAllTwWorlds = true;
        private DateTime _nextMarketRefresh = DateTime.MinValue;
        private HoverButton _refreshPricesButton = new();

        public ItemWindow(ILogger<ItemWindow> logger, MediatorService mediator, ImGuiService imGuiService,
            InventoryToolsConfiguration configuration, IMarketBoardService marketBoardService, IFramework framework,
            ICommandManager commandManager, IListService listService, ItemSheet itemSheet, ExcelSheet<World> worldSheet,
            IGameInterface gameInterface, IMarketCache marketCache, IChatUtilities chatUtilities, Logger otterLogger,
            IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, IClipboardService clipboardService,
            ItemInfoRenderService itemInfoRenderService, BNpcNameSheet bNpcNameSheet, MapSheet mapSheet, IUnlockTrackerService unlockTrackerService,
            ImGuiTooltipService tooltipService, ImGuiTooltipModeSetting tooltipModeSetting, ItemLocalizer itemLocalizer, TeleporterService teleporterService,
            CraftList.Factory craftListFactory,
            string name = "Item Window") : base(
            logger, mediator, imGuiService, configuration, name)
        {
            _marketBoardService = marketBoardService;
            _framework = framework;
            _commandManager = commandManager;
            _listService = listService;
            _itemSheet = itemSheet;
            _worldSheet = worldSheet;
            _gameInterface = gameInterface;
            _marketCache = marketCache;
            _chatUtilities = chatUtilities;
            _otterLogger = otterLogger;
            _inventoryMonitor = inventoryMonitor;
            _characterMonitor = characterMonitor;
            _clipboardService = clipboardService;
            _itemInfoRenderService = itemInfoRenderService;
            _bNpcNameSheet = bNpcNameSheet;
            _mapSheet = mapSheet;
            _unlockTrackerService = unlockTrackerService;
            _tooltipService = tooltipService;
            _tooltipModeSetting = tooltipModeSetting;
            _itemLocalizer = itemLocalizer;
            _teleporterService = teleporterService;
            _craftListFactory = craftListFactory;
        }

        private void MarketCacheUpdated(MarketCacheUpdatedMessage obj)
        {
            if (obj.itemId == WindowId)
            {
                GetMarketPrices();
                _marketRefreshing.Remove(obj.worldId);
                _marketRequestStarted.Remove(obj.worldId);
            }
        }

        public override void Initialize(uint itemId)
        {
            base.Initialize(itemId);
             Flags = ImGuiWindowFlags.NoSavedSettings;
            _itemId = itemId;
            var worlds = _worldSheet.Where(c => TwMarketWorlds.IsAvailableForMarket(c.RowId, c.IsPublic)).ToList();
            _picker = new WorldPicker(worlds, true, _otterLogger);
            _availableMarketWorlds = worlds.Select(world => world.RowId).ToList();
            MediatorService.Subscribe<MarketCacheUpdatedMessage>(this, MarketCacheUpdated);
            if (Item != null)
            {
                WindowName = "Allagan Tools - " + Item.NameString;
                Key = "item_" + itemId;
                RetainerTasks = Item.GetSourcesByCategory<ItemVentureSource>(ItemInfoCategory.AllVentures).Select(c => c.RetainerTaskRow).ToArray();
                RecipesResult = Item.GetSourcesByType<ItemCraftResultSource>(ItemInfoType.CraftRecipe).Select(c => c.Recipe).ToArray();
                RecipesAsRequirement = Item.RecipesAsRequirement.ToArray();
                Uses = Item.Uses;
                Vendors = new List<(IShop shop, ENpcResidentRow? npc, ILocation? location)>();

                foreach (var shopSource in Item.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop))
                {
                    var vendor = shopSource.Shop;
                    if (vendor.Name == "")
                    {
                        continue;
                    }
                    if (!vendor.ENpcs.Any())
                    {
                        Vendors.Add(new (vendor, null, null));
                    }
                    else
                    {
                        foreach (var npc in vendor.ENpcs)
                        {
                            if (npc.IsHouseVendorChild) continue;
                            if (!npc.Locations.Any())
                            {
                                Vendors.Add(new (vendor, npc.ENpcResidentRow, null));
                            }
                            else
                            {
                                foreach (var location in npc.Locations)
                                {
                                    Vendors.Add(new (vendor, npc.ENpcResidentRow, location));
                                }
                            }
                        }
                    }
                }
                Vendors = Vendors.OrderByDescending(c => c.npc != null && c.location != null).ToList();
                GatheringSources = Item.GetSourcesByCategory<ItemGatheringSource>(ItemInfoCategory.Gathering).ToList();
                SharedModels = Item.GetSharedModels();
                MobDrops = Item.GetSourcesByType<ItemMonsterDropSource>(ItemInfoType.Monster).Select(c => c.MobDrop).ToArray();
                OwnedItems = _inventoryMonitor.AllItems.Where(c => c.ItemId == itemId).ToList();
                if (Configuration.AutomaticallyDownloadMarketPrices)
                {
                    RequestMarketPrices(false);
                }
                GetMarketPrices();
            }
            else
            {
                RetainerTasks = [];
                RecipesResult = [];
                RecipesAsRequirement = [];
                GatheringSources = new();
                Vendors = new();
                SharedModels = new();
                MobDrops = [];
                Sources = [];
                Uses = [];
                OwnedItems = new List<CriticalCommonLib.Models.InventoryItem>();
                WindowName = "無效的物品";
                Key = "item_unknown";
            }
        }

        public override bool SaveState => false;
        private uint _itemId;
        private ItemRow? Item => _itemSheet.GetRow(_itemId);
        private CraftItem? _craftItem;
        private List<MarketPricing> _marketPrices = new List<MarketPricing>();
        private WorldPicker _picker;
        private Dictionary<uint, string>? _craftTypes;
        private uint? _craftTypeId;

        private List<uint> GetMarketWorldIds() => TwMarketWorlds.ForItem(
            _marketBoardService.GetDefaultWorlds(), _picker.SelectedWorldIds,
            _availableMarketWorlds, _showAllTwWorlds);

        private void GetMarketPrices()
        {
            _marketPrices = _marketCache.GetPricing(_itemId, GetMarketWorldIds(), false);
        }

        private void RequestMarketPrices(bool forceCheck = true)
        {
            if (Item == null || (forceCheck && DateTime.UtcNow < _nextMarketRefresh))
                return;
            if (forceCheck)
                _nextMarketRefresh = DateTime.UtcNow.AddSeconds(5);

            GetMarketPrices();
            foreach (var worldId in GetMarketWorldIds())
            {
                // Cached worlds need no pending spinner when simply opening/changing scope.
                if (!forceCheck && _marketPrices.Any(price => price.WorldId == worldId && price.listings != null))
                    continue;
                if (_marketCache.RequestCheck(Item.RowId, worldId, forceCheck))
                {
                    _marketRefreshing.Add(worldId);
                    _marketRequestStarted[worldId] = DateTime.UtcNow;
                }
            }
        }
        public List<ItemRow> SharedModels { get;set; }

        private List<ItemGatheringSource> GatheringSources { get;set; }

        private List<(IShop shop, ENpcResidentRow? npc, ILocation? location)> Vendors { get;set; }

        private RecipeRow[] RecipesAsRequirement { get;set;  }

        private RecipeRow[] RecipesResult { get;set; }

        private RetainerTaskRow[] RetainerTasks { get; set; }

        private MobDrop[] MobDrops { get;set; }

        private List<ItemSource> Sources { get; set; }
        private List<ItemSource> Uses { get; set; }

        private List<CriticalCommonLib.Models.InventoryItem> OwnedItems { get; set; }

        public override string GenericName { get; } = "物品";
        public override bool DestroyOnClose => true;
        public override void Draw()
        {
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                CurrentPosition = ImGui.GetWindowPos();
            }

            if (Item == null)
            {
                ImGui.TextUnformatted("找不到物品 ID：" + _itemId + "。");
            }
            else
            {
                ImGui.TextUnformatted("物品品級：" + Item.Base.LevelItem.RowId);
                ImGui.TextUnformatted("加入版本：" + Item.Patch);
                if (Item.CanBeDesynthed && Item.Base.ClassJobRepair.RowId != 0)
                {
                    ImGui.TextUnformatted("可分解職業：" + (Item.Base.ClassJobRepair.ValueNullable?.Name.ToString().ToTitleCase() ?? "未知"));
                }

                var description = Item.Base.Description.ExtractText();
                if (description != "")
                {
                    ImGui.PushTextWrapPos();
                    ImGui.TextUnformatted(description);
                    ImGui.PopTextWrapPos();
                }

                if (Item.CanBeAcquired)
                {
                    var hasAcquired = _unlockTrackerService.IsUnlocked(Item);
                    ImGui.TextUnformatted("取得狀態：" + (hasAcquired == null ? "檢查中" : hasAcquired == true ? "已取得" : "尚未取得"));
                }

                if (Item.SellToVendorPrice != 0)
                {
                    ImGui.TextUnformatted("出售給商店：" + Item.SellToVendorPrice + SeIconChar.Gil.ToIconString());
                }

                if (Item.BuyFromVendorPrice != 0 && Item.HasSourcesByType(ItemInfoType.GilShop))
                {
                    ImGui.TextUnformatted("商店購買價：" + Item.BuyFromVendorPrice + SeIconChar.Gil.ToIconString());
                }

                if (Item.BuyFromVendorPrice != 0 && Item.HasSourcesByType(ItemInfoType.CalamitySalvagerShop))
                {
                    ImGui.TextUnformatted("失物管理人購買價：" + Item.BuyFromVendorPrice + SeIconChar.Gil.ToIconString());
                }
                ImGui.Image(ImGuiService.GetIconTexture(Item.Icon).Handle, new Vector2(100, 100) * ImGui.GetIO().FontGlobalScale);
                if (_tooltipModeSetting.CurrentValue(Configuration) != ImGuiTooltipMode.Never)
                {
                    _tooltipService.DrawItemTooltip(new SearchResult(Item));
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow))
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && (ImGui.IsMouseReleased(ImGuiMouseButton.Right) || ImGui.IsMouseReleased(ImGuiMouseButton.Left)))
                {
                    ImGui.OpenPopup("RightClick" + _itemId);
                }


                using (var popup = ImRaii.Popup("RightClick" + _itemId))
                {
                    if (popup)
                    {
                        this.MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(Item));
                    }
                }

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("garlandtools").Handle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://www.garlandtools.org/db/#item/{Item.GarlandToolsId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("在 Garland Tools 開啟");
                ImGui.SameLine();

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("teamcraft").Handle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://ffxivteamcraft.com/db/en/item/{_itemId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("在 Teamcraft 開啟");
                ImGui.SameLine();

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("universalis").Handle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    $"https://universalis.app/market/{_itemId}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("在 Universalis 開啟");
                ImGui.SameLine();

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("gamerescape").Handle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    var name = Item.NameString.Replace(' ', '_');
                    name = name.Replace('–', '-');

                    if (name.StartsWith("_")) // "level sync" icon
                        name = name.Substring(2);
                    $"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(name)}?useskin=Vector".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("在 Gamer Escape 開啟");
                ImGui.SameLine();

                if (ImGui.ImageButton(ImGuiService.GetImageTexture("consolegameswiki").Handle,
                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                {
                    var name = Item.NameString.Replace("#"," ").Replace("  ", " ").Replace(' ', '_');
                    name = name.Replace('–', '-');

                    if (name.StartsWith("_")) // "level sync" icon
                        name = name.Substring(2);
                    $"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(name)}".OpenBrowser();
                }
                ImGuiUtil.HoverTooltip("在 Console Games Wiki 開啟");

                if (Item.CanOpenCraftingLog)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66456).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        var result = _gameInterface.OpenCraftingLog(_itemId);
                        if (!result)
                        {
                            _chatUtilities.PrintError("目前正在製作，無法開啟製作筆記。");
                        }
                    }

                    ImGuiUtil.HoverTooltip("可製作－開啟製作筆記");
                }
                if (Item.CanBeCrafted)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(60858).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        ImGui.OpenPopup("AddCraftList" + _itemId);
                    }

                    using (var popup = ImRaii.Popup("AddCraftList" + _itemId))
                    {
                        if (popup)
                        {
                            var craftFilters =
                                _listService.Lists.Where(c =>
                                    c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault);
                            foreach (var filter in craftFilters)
                            {
                                using (ImRaii.PushId(filter.Key))
                                {
                                    if (ImGui.Selectable("加入製作清單－" + filter.Name))
                                    {
                                        _framework.RunOnFrameworkThread(() =>
                                        {
                                            filter.CraftList.AddCraftItem(_itemId, 1, InventoryItem.ItemFlags.None);
                                            filter.NeedsRefresh = true;
                                            MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                            MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), filter));
                                        });
                                    }
                                }
                            }
                        }
                    }

                    ImGuiUtil.HoverTooltip("可製作－加入製作清單");
                }
                if (Item.CanBeGathered)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66457).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _gameInterface.OpenGatheringLog(_itemId);
                    }

                    ImGuiUtil.HoverTooltip("可採集－開啟採集筆記");

                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(63900).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _commandManager.ProcessCommand("/gather " + Item.NameString);
                    }

                    ImGuiUtil.HoverTooltip("可採集－使用 GatherBuddy 採集");
                }

                if (Item.ObtainedFishing)
                {
                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(66457).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _gameInterface.OpenFishingLog(_itemId, Item.ObtainedSpearFishing);
                    }

                    ImGuiUtil.HoverTooltip("可釣魚－開啟釣魚手冊");

                    ImGui.SameLine();
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(63900).Handle,
                            new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale))
                    {
                        _commandManager.ProcessCommand("/gatherfish " + Item.NameString);
                    }

                    ImGuiUtil.HoverTooltip("可釣魚－使用 GatherBuddy 釣魚");
                }

                ImGui.Separator();

                DrawSources();

                DrawUses();

                DrawOwned();

                DrawMobDrops();

                DrawVendors();

                DrawIshgardRestoration();

                DrawMarketPricing();

                DrawRetainerTasks();

                DrawGatheringSources();

                DrawRecipes();

                DrawSharedModels();

                DrawCraftRecipe();


#if DEBUG
                if (ImGui.CollapsingHeader("偵錯###Debug"))
                {
                    ImGui.TextUnformatted("物品 ID：" + _itemId);
                    if (ImGui.Button("複製###Copy"))
                    {
                        _clipboardService.CopyToClipboard(_itemId.ToString());
                    }

                    Utils.PrintOutObject(Item, 0, new List<string>());
                }
#endif

            }
        }

        private void DrawSources()
        {
            if (Item == null)
            {
                return;
            }
            if (ImGui.CollapsingHeader("取得來源（" + Item.Sources.Count + "）", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var messages = _itemInfoRenderService.DrawItemSourceIcons("Sources", new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, Item.Sources.ToList());
                MediatorService.Publish(messages);
            }
        }

        private bool DrawCraftRecipe()
        {
            bool hasInformation = false;

            if (Item is { CanBeCrafted: true })
            {
                var recipes = Item.Recipes;
                if (_craftTypes == null)
                {
                    var craftTypes = new Dictionary<uint, string>();
                    if (Item.IsCompanyCraft)
                    {
                        craftTypes.Add(0, "全部");
                        var companyCraftIndex = 1u;
                        if (Item.CompanyCraftSequence != null)
                        {
                            var craftParts = Item.CompanyCraftSequence.CompanyCraftParts;
                            foreach (var craftPart in craftParts)
                            {
                                if (craftPart.Base.CompanyCraftType.ValueNullable == null) continue;
                                craftTypes.Add(companyCraftIndex,
                                    craftPart.Base.CompanyCraftType.Value.Name.ExtractText());
                                companyCraftIndex++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var recipe in recipes)
                        {
                            craftTypes[recipe.RowId] = recipe.CraftType?.FormattedName ?? "未知製作職業";
                        }
                    }

                    _craftTypes = craftTypes;
                }

                if (_craftTypeId == null && _craftTypes.Count != 0)
                {
                    _craftTypeId = _craftTypes.First().Key;
                }
                else if(_craftTypeId == null)
                {
                    _craftTypeId = 0;
                }

                string headerName = "製作此物品所需素材";
                if (ImGui.CollapsingHeader(headerName))
                {
                    if (_craftTypes.Count > 1)
                    {
                        using (var combo = ImRaii.Combo("製作職業",
                                   _craftTypes.GetValueOrDefault(_craftTypeId.Value, "")))
                        {
                            if (combo)
                            {
                                foreach (var craftType in _craftTypes)
                                {
                                    if (ImGui.Selectable(craftType.Value))
                                    {
                                        _craftTypeId = craftType.Key;
                                        _craftItem = null;
                                    }
                                }
                            }
                        }
                    }

                    if (Item.IsCompanyCraft)
                    {
                        if (_craftItem == null)
                        {
                            var craftList = _craftListFactory.Invoke();
                            craftList.AddCraftItem(Item.RowId, 1, InventoryItem.ItemFlags.None,
                                _craftTypeId == 0 ? null : _craftTypeId - 1);
                            craftList.GenerateCraftChildren();
                            _craftItem = craftList.CraftItems.First();
                        }
                    }
                    else
                    {
                        if (_craftItem == null)
                        {
                            var craftList = _craftListFactory.Invoke();
                            craftList.AddCraftItem(Item.RowId);
                            if (_craftTypeId != null)
                            {
                                craftList.SetCraftRecipe(Item.RowId, _craftTypeId.Value);
                            }

                            craftList.GenerateCraftChildren();
                            _craftItem = craftList.CraftItems.First();
                        }
                    }

                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var index = 0;
                    foreach (var craftItem in _craftItem.ChildCrafts)
                    {
                        var item = _itemSheet.GetRowOrDefault(craftItem.ItemId);
                        if (item != null)
                        {
                            using (ImRaii.PushId(index))
                            {
                                if (ImGui.ImageButton(ImGuiService.GetIconTexture(item.Icon).Handle, new(32, 32)))
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    ImGui.OpenPopup("RightClick" + item.RowId);
                                }

                                using (var popup = ImRaii.Popup("RightClick" + item.RowId))
                                {
                                    if (popup)
                                    {
                                        MediatorService.Publish(ImGuiService.ImGuiMenuService
                                            .DrawRightClickPopup(item));
                                    }
                                }

                                float lastButtonX2 = ImGui.GetItemRectMax().X;
                                float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                ImGuiUtil.HoverTooltip(item.NameString + " - " + craftItem.QuantityRequired);
                                if (index + 1 < _craftItem.ChildCrafts.Count && nextButtonX2 < windowVisibleX2)
                                {
                                    ImGui.SameLine();
                                }
                            }

                            index++;
                        }
                    }
                }


            }

            return hasInformation;
        }


        private bool DrawSharedModels()
        {
            bool hasInformation = false;
            if (SharedModels.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("共用外觀模型（" + SharedModels.Count + "）"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < SharedModels.Count; index++)
                    {
                        using (ImRaii.PushId(index))
                        {
                            var sharedModel = SharedModels[index];
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(sharedModel.Icon).Handle,
                                    new(32, 32)))
                            {
                                MediatorService.Publish(
                                    new OpenUintWindowMessage(typeof(ItemWindow), sharedModel.RowId));
                            }

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClick" + sharedModel.RowId);
                            }

                            using (var popup = ImRaii.Popup("RightClick" + sharedModel.RowId))
                            {
                                if (popup)
                                {
                                    MediatorService.Publish(
                                        ImGuiService.ImGuiMenuService.DrawRightClickPopup(sharedModel));
                                }
                            }

                            float lastButtonX2 = ImGui.GetItemRectMax().X;
                            float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                            ImGuiUtil.HoverTooltip(sharedModel.NameString);
                            if (index + 1 < SharedModels.Count && nextButtonX2 < windowVisibleX2)
                            {
                                ImGui.SameLine();
                            }
                        }
                    }
                }
            }

            return hasInformation;
        }

        private bool DrawRecipes()
        {
            bool hasInformation = false;
            if (RecipesAsRequirement.Length != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("使用此物品的配方（" + RecipesAsRequirement.Length + "）"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    for (var index = 0; index < RecipesAsRequirement.Length; index++)
                    {
                        using (ImRaii.PushId(index))
                        {
                            var recipe = RecipesAsRequirement[index];
                            if (recipe.ItemResult != null)
                            {
                                var icon = ImGuiService.GetIconTexture(recipe.ItemResult.Icon);
                                if (ImGui.ImageButton(icon.Handle,
                                        new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, new(0, 0), new(1, 1), 0))
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow),
                                        recipe.ItemResult.RowId));
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    ImGui.OpenPopup("RightClick" + recipe.RowId);
                                }

                                using (var popup = ImRaii.Popup("RightClick" + recipe.RowId))
                                {
                                    if (popup)
                                    {
                                        if (recipe.ItemResult != null)
                                        {
                                            MediatorService.Publish(
                                                ImGuiService.ImGuiMenuService.DrawRightClickPopup(recipe.ItemResult));
                                        }
                                    }
                                }

                                float lastButtonX2 = ImGui.GetItemRectMax().X;
                                float nextButtonX2 = lastButtonX2 + style.ItemSpacing.X + 32;
                                ImGuiUtil.HoverTooltip(recipe.ItemResult!.NameString + " - " +
                                                       (recipe.CraftType?.FormattedName ?? "未知"));
                                if (index + 1 < RecipesAsRequirement.Length && nextButtonX2 < windowVisibleX2)
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }
                    }
                }
            }

            return hasInformation;
        }

        private bool DrawGatheringSources()
        {
            var hasInformation = false;
            if (GatheringSources.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("採集地點（" + GatheringSources.Count + "）"))
                {
                    ImGuiTable.DrawTable("Gathering", GatheringSources, DrawGatheringRow,
                        ImGuiTableFlags.None, new[] { "", "等級", "位置", "" });
                }
            }

            return hasInformation;
        }

        private bool DrawRetainerTasks()
        {
            bool hasInformation = false;
            if (RetainerTasks.Length != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("雇員探險（" + RetainerTasks.Count() + "）"))
                {
                    ImGuiTable.DrawTable("Ventures", RetainerTasks, DrawRetainerRow, ImGuiTableFlags.SizingStretchProp,
                        new[] { "名稱", "時間", "數量" });
                }
            }

            return hasInformation;
        }

        private bool DrawVendors()
        {
            bool hasInformation = false;
            if (Vendors.Count != 0)
            {
                hasInformation = true;
                if (ImGui.CollapsingHeader("商店（" + Vendors.Count + "）"))
                {
                    ImGui.TextUnformatted("可購買商店：");
                    ImGuiTable.DrawTable("VendorsText", Vendors, DrawSupplierRow, ImGuiTableFlags.None,
                        new[] { "商店名稱","NPC", "位置", "" });
                }
            }

            return hasInformation;
        }

        private void DrawOwned()
        {
            if (ImGui.CollapsingHeader("持有位置（" + OwnedItems.Count + "）"))
            {
                ImGuiTable.DrawTable("OwnedItems", OwnedItems, DrawOwnedItem, ImGuiTableFlags.None,
                    new[] { "角色／雇員","位置", "數量", "HQ？" });
            }
        }

        private void DrawOwnedItem(CriticalCommonLib.Models.InventoryItem obj)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(_characterMonitor.GetCharacterNameById(obj.RetainerId));
            ImGui.TableNextColumn();
            ImGui.TextWrapped(_itemLocalizer.FormattedBagLocation(obj));
            if (obj.SortedCategory == InventoryCategory.GlamourChest && obj.GlamourId != 0)
            {
                ImGui.SameLine();
                ImGui.Image(this.ImGuiService.GetIconTexture(Icons.MannequinIcon).Handle, new Vector2(16,16));
                if (ImGui.IsItemHovered())
                {
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip)
                        {
                            ImGui.TextUnformatted("此物品已合併為一個可直接投影的套裝物品。");
                        }
                    }
                }
            }
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantity.ToString());
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.IsHQ ? "是" : "否");
        }


        void DrawSupplierRow((IShop shop, ENpcResidentRow? npc, ILocation? location) tuple)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped(tuple.shop.Name);
            if (tuple.npc != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.npc.Base.Singular.ExtractText());
            }
            if (tuple.npc != null && tuple.location != null)
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped(tuple.location + " ( " + Math.Round(tuple.location.MapX, 2) + "/" +
                                  Math.Round(tuple.location.MapY, 2) + ")");
                ImGui.TableNextColumn();
                if (ImGui.Button("傳送##t" + tuple.shop.RowId + "_" + tuple.npc.RowId + "_" +
                                 tuple.location.Map.RowId))
                {
                    var nearestAetheryte = _teleporterService.GetNearestAetheryte(tuple.location);
                    if (nearestAetheryte != null)
                    {
                        MediatorService.Publish(new RequestTeleportMessage(nearestAetheryte.Value.RowId));
                    }
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
                if (ImGui.Button("地圖連結##ml" + tuple.shop.RowId + "_" + tuple.npc.RowId + "_" +
                                 tuple.location.Map.RowId))
                {
                    _chatUtilities.PrintFullMapLink(tuple.location, Item?.NameString ?? "");
                }
            }
            else if (tuple.npc is { ENpcBase.IsHouseVendor: true })
            {
                ImGui.TableNextColumn();
                ImGui.TextWrapped("房屋商人");
                ImGuiService.HelpMarker("可放置在個人房屋或公寓內的商人 NPC。");
                ImGui.TableNextColumn();
            }
            else
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }

        }

        private void DrawMarketPricing()
        {
            if (Item is { CanBePlacedOnMarket: true })
            {
                // A failed/limited HTTP request does not send a cache-update event.
                foreach (var worldId in _marketRequestStarted.Where(pair =>
                             DateTime.UtcNow - pair.Value > TimeSpan.FromMinutes(2)).Select(pair => pair.Key).ToArray())
                {
                    _marketRequestStarted.Remove(worldId);
                    _marketRefreshing.Remove(worldId);
                }
                var prePosition = ImGui.GetCursorPos();
                if (ImGui.CollapsingHeader("市場價格",
                        ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if(_marketRefreshing.Count != 0)
                    {
                        var postPosition = ImGui.GetCursorPos();
                        prePosition.X = ImGui.GetWindowWidth() - 20;
                        prePosition.Y = prePosition.Y + 6;
                        ImGui.SetCursorPos(prePosition);
                        float nextDot = 3.0f;
                        if (_marketRefreshing.Count != 0)
                        {
                            ImGuiService.SpinnerDots("hai", ref nextDot, 7, 1);
                        }

                        ImGui.SetCursorPos(postPosition);
                    }


                    if (ImGui.Checkbox("顯示全部繁中伺服器##twMarketWorlds", ref _showAllTwWorlds))
                    {
                        GetMarketPrices();
                        if (Configuration.AutomaticallyDownloadMarketPrices)
                            RequestMarketPrices(false);
                    }
                    ImGuiUtil.HoverTooltip("只比較此物品在各繁中服的價格，不改動全域估價、庫存預載或製作成本設定。無資料時可按右側重新整理。");
                    if (_showAllTwWorlds && !_availableMarketWorlds.Any(TwMarketWorlds.IsTraditionalChinese))
                        ImGui.TextWrapped("遊戲資料中沒有可用的繁中伺服器；仍顯示原查價範圍。");

                    var selected = 0;
                    if (_picker.Draw("世界", "", "", ref selected, 100, 20, ImGuiComboFlags.None))
                    {
                        var world = _picker.Items[selected];
                        _picker.SelectedWorldIds.Add(world.RowId);
                        GetMarketPrices();
                        RequestMarketPrices(false);
                    }

                    if (_picker.SelectedWorldIds.Count != 0)
                    {
                        ImGui.SameLine();
                    }

                    ImGuiStylePtr style = ImGui.GetStyle();
                    float windowVisibleX = ImGui.GetWindowContentRegionMax().X - style.ScrollbarSize;
                    float X = ImGui.GetCursorPosX();

                    var count = 0;

                    foreach (var selectedWorldId in _picker.SelectedWorldIds)
                    {
                        var selectedWorld = _worldSheet.GetRowOrDefault((uint)selectedWorldId);
                        if (selectedWorld != null)
                        {
                            var selectedWorldFormattedName = selectedWorld.Value.Name.ExtractText() + " X";
                            var itemWidth = ImGui.CalcTextSize(selectedWorldFormattedName).X  + (2 * ImGui.GetStyle().FramePadding.X) + 5;
                            if (windowVisibleX > X + itemWidth)
                            {
                                if (count != 0)
                                {
                                    ImGui.SameLine();
                                }

                                X += itemWidth;
                            }
                            else
                            {
                                X = itemWidth;
                                ImGui.NewLine();
                            }

                            count++;

                            if (ImGui.Button(selectedWorldFormattedName))
                            {
                                _picker.SelectedWorldIds.Remove(selectedWorldId);
                                GetMarketPrices();
                                break;
                            }
                        }
                    }

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 22 - ImGui.GetStyle().FramePadding.X);
                    if (_refreshPricesButton.Draw(ImGuiService.GetImageTexture("refresh-web").Handle, "refreshPrices"))
                    {
                        RequestMarketPrices();
                    }
                    ImGuiUtil.HoverTooltip("重新整理表格中所有伺服器的價格（每 5 秒可送出一次）。價格由 Universalis 玩家回報，並非即時完整市場。");
                    ImGuiTable.DrawTable("MarketPrices", GetMarketWorldIds(), DrawMarketRow, ImGuiTableFlags.None,
                        new[] { "伺服器","快取更新", "在售筆數", "最低價（NQ／HQ）" });
                }
            }

            void DrawMarketRow(uint worldId)
            {
                var obj = _marketPrices.FirstOrDefault(price => price.WorldId == worldId);
                ImGui.TableNextColumn();
                ImGui.TextWrapped(_worldSheet.GetRowOrDefault(worldId)?.Name.ExtractText() ?? $"世界 {worldId}");
                ImGui.TableNextColumn();
                if (obj == null)
                {
                    ImGui.TextWrapped(_marketRefreshing.Contains(worldId) ? "查詢中…" : "尚無資料／請重新整理");
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted("—");
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted("—／—");
                    return;
                }
                var elapsed = DateTime.Now - obj.LastUpdate;
                ImGui.TextWrapped(elapsed.TotalMinutes < 60
                    ? $"{Math.Max(0, (int)Math.Round(elapsed.TotalMinutes))} 分鐘前"
                    : $"{Math.Max(0, (int)Math.Round(elapsed.TotalHours))} 小時前");
                ImGui.TableNextColumn();
                ImGui.TextWrapped(obj.Available.ToString());
                ImGui.TableNextColumn();
                var nq = obj.MinPriceNq > 0 ? obj.MinPriceNq.ToString("N0", CultureInfo.InvariantCulture) + SeIconChar.Gil.ToIconString() : "—";
                var hq = obj.MinPriceHq > 0 ? obj.MinPriceHq.ToString("N0", CultureInfo.InvariantCulture) + SeIconChar.Gil.ToIconString() : "—";
                ImGui.TextWrapped(nq + "／" + hq);
            }
        }

        private void DrawIshgardRestoration()
        {
            if (Item?.HasUsesByType(ItemInfoType.SkybuilderHandIn) ?? false)
            {
                var skybuilderHandIn = Item.GetUsesByType<ItemSkybuilderHandInSource>(ItemInfoType.SkybuilderHandIn).First();
                if (ImGui.CollapsingHeader("伊修加德重建", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var supplyItem = skybuilderHandIn.HWDCrafterSupplyParams;
                    using (var table = ImRaii.Table("SupplyItems", 4 ,ImGuiTableFlags.None))
                    {
                        if (table.Success)
                        {
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("階段");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("收藏品價值");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("XP");
                            ImGui.TableNextColumn();
                            ImGui.TableHeader("工票");

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("基礎");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.BaseCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.BaseCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.BaseCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("中等");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.MidCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.MidCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.MidCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("高等");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped(supplyItem.HighCollectableRating.ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.HighCollectableReward.ValueNullable?.ExpReward ?? 0).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped((supplyItem.HighCollectableReward.ValueNullable?.ScriptRewardAmount ?? 0)
                                .ToString());
                        }
                    }
                }
            }
        }

        private void DrawMobDrops()
        {
            if (MobDrops.Length != 0)
            {
                if (ImGui.CollapsingHeader("怪物掉落（" + MobDrops.Length + "）", ImGuiTreeNodeFlags.CollapsingHeader))
                {
                     var mobDrops = MobDrops;
                     for (var index = 0; index < mobDrops.Length; index++)
                     {
                         var mobDrop = mobDrops[index];
                         var bnpcName = _bNpcNameSheet.GetRowOrDefault(mobDrop.BNpcNameId);
                         if (bnpcName != null)
                         {
                             var mobSpawns = bnpcName.MobSpawnPositions.GroupBy(c => c.TerritoryType.RowId).ToList();
                             if (mobSpawns.Count != 0)
                             {
                                 using (ImRaii.PushId("MobDrop" + index))
                                 {
                                     if (ImGui.CollapsingHeader("  " +
                                                                bnpcName.Base.Singular.ExtractText() + "(" +
                                                                mobSpawns.Count + ")",
                                             ImGuiTreeNodeFlags.CollapsingHeader))
                                     {
                                         ImGuiTable.DrawTable("MobSpawns" + index, mobSpawns, DrawMobSpawn,
                                             ImGuiTableFlags.None,
                                             new[] { "地圖", "出現位置" });
                                     }
                                 }
                             }
                             else
                             {
                                 ImGui.TextUnformatted("尚無已知出現位置。");
                             }
                         }
                     }
                }
            }
        }

        private void DrawUses()
        {
            if (Item == null)
            {
                return;
            }

            if (ImGui.CollapsingHeader("用途（" + Item.Uses.Count + "）", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var messages = _itemInfoRenderService.DrawItemUseIcons("Uses", new Vector2(32, 32) * ImGui.GetIO().FontGlobalScale, Item.Uses.ToList());
                MediatorService.Publish(messages);
            }
        }

        private void DrawMobSpawn(IGrouping<uint, MobSpawnPosition> mobSpawnPositions)
        {
            ImGui.TableNextColumn();
            var territoryType = mobSpawnPositions.First().TerritoryType.Value;
            ImGui.TextUnformatted(territoryType.PlaceName.Value.Name.ExtractText());
            ImGui.TableNextColumn();

            using (var locationScrollChild = ImRaii.Child(territoryType.RowId + "LocationScroll",
                       new Vector2(ImGui.GetColumnWidth() * ImGui.GetIO().FontGlobalScale,
                           32 + ImGui.GetStyle().CellPadding.Y) * ImGui.GetIO().FontGlobalScale, false))
            {
                if (locationScrollChild.Success)
                {
                    var columnWidth = ImGui.GetColumnWidth() - ImGui.GetStyle().ItemSpacing.X;
                    var itemWidth = (32 + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                    var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                    maxItems = maxItems == 0 ? 1 : maxItems;
                    maxItems--;
                    var count = 0;
                    for (var index = 0; index < mobSpawnPositions.ToList().Count; index++)
                    {
                        var position = mobSpawnPositions.ToList()[index];
                        var territory = position.TerritoryType;
                        if (territory.ValueNullable?.PlaceName.ValueNullable != null)
                        {
                            using (ImRaii.PushId(index))
                            {
                                if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).Handle,
                                        new Vector2(32 * ImGui.GetIO().FontGlobalScale,
                                            32 * ImGui.GetIO().FontGlobalScale),
                                        new Vector2(0, 0), new Vector2(1, 1), 0))
                                {
                                    _chatUtilities.PrintFullMapLink(position,
                                        position.TerritoryType.Value.PlaceName.Value.Name.ExtractText());
                                }

                                if (ImGui.IsItemHovered())
                                {
                                    using var tt = ImRaii.Tooltip();
                                    ImGui.TextUnformatted(
                                        position.TerritoryType.Value.PlaceName.Value.Name.ExtractText());
                                }

                                if ((count + 1) % maxItems != 0)
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }

                        count++;
                    }
                }
            }
        }

        private void DrawGatheringRow(ItemGatheringSource obj)
        {
            ImGui.TableNextColumn();
            using (ImRaii.PushId(obj.GetHashCode()))
            {
                var source = obj.Item;
                if (ImGui.ImageButton(ImGuiService.GetIconTexture(source.Icon).Handle, new(32, 32)))
                {
                    _gameInterface.OpenGatheringLog(_itemId);
                }

                ImGuiUtil.HoverTooltip(source.NameString + "－開啟採集筆記");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(obj.GatheringItem.Base.GatheringItemLevel.RowId.ToString());
                ImGui.TableNextColumn();
                if (obj.MapIds != null)
                {
                    foreach (var location in obj.MapIds)
                    {
                        var map = _mapSheet.GetRowOrDefault(location);
                        if (map != null)
                        {
                            ImGui.TextWrapped(map.FormattedName);
                        }
                    }
                }
            }
        }

        private void DrawRetainerRow(RetainerTaskRow obj)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped( obj.FormattedName);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(obj.DurationString);
            ImGui.TableNextColumn();
            ImGui.TextWrapped(obj.Quantities);
        }

        public override void Invalidate()
        {

        }

        public override FilterConfiguration? SelectedConfiguration => null;
        public override Vector2? DefaultSize { get; } = new Vector2(500, 800);
        public override Vector2? MaxSize => new (800, 1500);
        public override Vector2? MinSize => new (100, 100);

        public override bool SavePosition => true;

        public override string GenericKey => "item";

    }
}
