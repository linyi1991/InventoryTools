using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationInventoriesFilter : InventoryScopeFilter
    {
        public DestinationInventoriesFilter(ILogger<DestinationInventoriesFilter> logger, InventoryScopePicker scopePicker, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
        {
        }
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "DestinationInventories";
        public override string Name { get; set; } = "目的地庫存";
        public override string HelpText { get; set; } =
            "設定收納目的地。插件會規劃將來源庫存中的物品整理至目的地庫存；下方會列出符合範圍的庫存。";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override List<InventorySearchScope>? DefaultValue { get; set; } = null;
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter;

        public override List<InventorySearchScope>? GenerateDefaultScope()
        {
            return null;
        }
    }
}