using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class FilterORFilter : BooleanFilter
    {
        public override string Key { get; set; } = "ORFilter";
        public override string Name { get; set; } = "使用「或」串接篩選條件";

        public override string HelpText { get; set; } =
            "預設以「且」套用所有篩選條件；啟用後改為符合任一條件即可。";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Advanced;



        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.UseORFiltering;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.UseORFiltering = newValue;
        }

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public FilterORFilter(ILogger<FilterORFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}