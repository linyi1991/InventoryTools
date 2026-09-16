using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReverseListDisplayFilter : BooleanFilter
{
    public CraftReverseListDisplayFilter(ILogger<CraftReverseListDisplayFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftReverseListDisplay";
    public override string Name { get; set; } = "反向排列製作清單？";

    public override string HelpText { get; set; } =
        "是否反向顯示製作清單，讓成品位於底部？僅適用於單一表格模式。";

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }
}