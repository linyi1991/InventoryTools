using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftListModeFilter : ChoiceFilter<CraftListMode>
{
    public CraftListModeFilter(ILogger<CraftListModeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override CraftListMode CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CraftListMode;
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        this.UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftListMode newValue)
    {
        configuration.CraftList.CraftListMode = newValue;
    }

    public override string Key { get; set; } = "CraftListMode";
    public override string Name { get; set; } = "製作清單模式";

    public override string HelpText { get; set; } =
        "一般模式會在完成製作後扣除所需數量；備貨模式則依角色庫存數量計算目標存量。";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override CraftListMode DefaultValue { get; set; } = CraftListMode.Normal;
    public override List<CraftListMode> GetChoices(FilterConfiguration configuration)
    {
        return new List<CraftListMode>()
        {
            CraftListMode.Normal,
            CraftListMode.Stock,
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftListMode choice)
    {
        return choice.ToString();
    }
}