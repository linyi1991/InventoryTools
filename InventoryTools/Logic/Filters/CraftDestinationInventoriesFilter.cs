using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftDestinationInventoriesFilter : InventoryScopeFilter
{
    public CraftDestinationInventoriesFilter(InventoryScopePicker scopePicker, ILogger<CraftDestinationInventoriesFilter> logger, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftDestinationInventories";
    public override string Name { get; set; } = "取物目的地庫存";

    public override string HelpText { get; set; } =
        "製作清單應將來源庫存中找到的物品整理至哪些庫存？";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override List<InventorySearchScope>? GenerateDefaultScope()
    {
        return new List<InventorySearchScope>()
        {
            new InventorySearchScope() { ActiveCharacter = true, Categories = [InventoryCategory.CharacterBags] }
        };
    }

    public override int Order { get; set; } = -2;
}