using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class ItemFilter : UintMultipleChoiceFilter
{
    private readonly ItemSheet _itemSheet;

    public ItemFilter(ILogger<ItemFilter> logger, ImGuiService imGuiService, ItemSheet itemSheet) : base(logger, imGuiService)
    {
        _itemSheet = itemSheet;
    }

    public override string Key { get; set; } = "ItemFilter";
    public override string Name { get; set; } = "名稱（選取清單）";

    public override string HelpText { get; set; } =
        "選擇物品後，只顯示選取的物品。建議改用自訂清單，但此篩選仍可使用。";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override List<uint> DefaultValue { get; set; } = new();

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var searchItems = CurrentValue(configuration).ToList();
        if (searchItems.Count == 0)
        {
            return null;
        }

        if (searchItems.Contains(item.RowId))
        {
            return true;
        }

        return false;
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        return _itemSheet.ItemsSearchStringsById;
    }

    public override bool HideAlreadyPicked { get; set; }
}