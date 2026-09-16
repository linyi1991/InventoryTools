using System.Collections.Generic;
using System.Linq;

namespace InventoryTools.Services;

/// <summary>Item-window comparison scope only; never changes global pricing or crafting preferences.</summary>
public static class TwMarketWorlds
{
    // TW World IDs, not the identically named international worlds.
    // Names are resolved from the game sheet. TW rows have IsPublic=false on the TC client.
    public static bool IsTraditionalChinese(uint worldId) => worldId is >= 4028 and <= 4035;

    public static bool IsAvailableForMarket(uint worldId, bool isPublic)
        => isPublic || IsTraditionalChinese(worldId);

    public static List<uint> ForItem(IEnumerable<uint> defaults, IEnumerable<uint> selected,
        IEnumerable<uint> availableWorlds, bool includeTw)
    {
        var worlds = defaults.Concat(selected).Where(id => id != 0).ToHashSet();
        if (includeTw)
            worlds.UnionWith(availableWorlds.Where(IsTraditionalChinese));
        return worlds.OrderBy(id => id).ToList();
    }
}
