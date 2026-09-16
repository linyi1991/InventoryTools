using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GenericHasSourceCategoryFilter : BooleanFilter, IGenericFilter
{
    private readonly ItemInfoRenderCategory _renderCategory;
    private readonly ItemInfoRenderService _infoRenderService;
    private ItemInfoType[]? _sourceTypes;
    private readonly string _key;

    public override int LabelSize { get; set; } = 250;

    public delegate GenericHasSourceCategoryFilter Factory(ItemInfoRenderCategory renderCategory);

    public GenericHasSourceCategoryFilter(ItemInfoRenderCategory renderCategory, ItemInfoRenderService infoRenderService, ILogger<GenericHasSourceCategoryFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _renderCategory = renderCategory;
        _infoRenderService = infoRenderService;
        _key = "HasSourceCat" + (uint)_renderCategory;
    }

    public override string Key {
        get => _key;
        set
        {

        }
    }
    public override string Name {
        get => _infoRenderService.GetCategoryName(_renderCategory);
        set
        {

        }
    }
    public override string HelpText {
        get => "物品是否可透過此方式取得：" +  _infoRenderService.GetCategoryName(_renderCategory).ToLower() + "？\n\n包含下列來源：" + string.Join(",", _infoRenderService.GetSourcesByCategory(_renderCategory).Select(c => c.SingularName));
        set
        {

        }
    }

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.SourceCategories;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = this.CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        _sourceTypes ??= _infoRenderService.GetSourcesByCategory(_renderCategory).Select(c => c.Type).ToArray();

        return currentValue == true ? item.HasSourcesByType(_sourceTypes) : !item.HasSourcesByType(_sourceTypes);
    }
}