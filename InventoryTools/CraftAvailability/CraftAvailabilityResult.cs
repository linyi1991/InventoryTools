using System.Collections.Generic;

namespace InventoryTools.CraftAvailability;

public sealed record CraftAvailabilityResult(
    uint RecipeId,
    uint ItemId,
    string Name,
    string CraftType,
    uint RecipeLevel,
    uint MaxCrafts,
    uint Yield,
    CraftAvailabilityCategory Category,
    IReadOnlyDictionary<uint, uint> Ingredients,
    IReadOnlyDictionary<uint, uint> MissingItems);
