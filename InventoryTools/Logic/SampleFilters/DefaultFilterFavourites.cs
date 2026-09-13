using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class DefaultFilterFavourites : ISampleFilter
{
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly FavouritesFilter _favouritesFilter;
    private readonly IListService _listService;

    public DefaultFilterFavourites(FilterConfiguration.Factory filterConfigFactory, FavouritesFilter favouritesFilter, IListService listService)
    {
        _filterConfigFactory = filterConfigFactory;
        _favouritesFilter = favouritesFilter;
        _listService = listService;
    }
    public bool ShouldAdd { get; set; }
    public FilterConfiguration AddFilter()
    {
        var allItemsFilter = _filterConfigFactory.Invoke();
        allItemsFilter.Name = SampleDefaultName;
        allItemsFilter.FilterType = FilterType.GameItemFilter;
        allItemsFilter.DisplayInTabs = true;
        _favouritesFilter.UpdateFilterConfiguration(allItemsFilter, true);
        _listService.AddDefaultColumns(allItemsFilter);
        _listService.AddList(allItemsFilter);
        return allItemsFilter;
    }

    public string Name => "最愛物品";
    public string SampleDefaultName => "最愛物品";

    public string SampleDescription =>
        "建立顯示所有已加入最愛物品的清單。";

    public SampleFilterType SampleFilterType => SampleFilterType.Default;
}
