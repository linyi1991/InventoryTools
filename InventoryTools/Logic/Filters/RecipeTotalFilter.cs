using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class RecipeTotalFilter : StringFilter
{

    public RecipeTotalFilter(ILogger<RecipeTotalFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        ShowOperatorTooltip = true;
    }

    public override string Key { get; set; } = "RecipeTotalFilter";
    public override string Name { get; set; } = "配方總數";
    public override string HelpText { get; set; } = "使用此物品作為素材的配方數量。";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        if (!item.RecipesAsRequirement.Count.PassesFilter(currentValue))
        {
            return false;
        }

        return true;
    }
}