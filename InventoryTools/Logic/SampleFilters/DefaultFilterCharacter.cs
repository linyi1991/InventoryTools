using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class DefaultFilterCharacter : ISampleFilter
{
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly IListService _listService;

    public DefaultFilterCharacter(FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter, IListService listService)
    {
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
        _listService = listService;
    }
    public bool ShouldAdd { get; set; }
    public FilterConfiguration AddFilter()
    {
        var allItemsFilter = _filterConfigFactory.Invoke();
        allItemsFilter.Name = SampleDefaultName;
        allItemsFilter.FilterType = FilterType.SearchFilter;
        allItemsFilter.DisplayInTabs = true;
        _sourceInventoriesFilter.UpdateFilterConfiguration(allItemsFilter, new List<InventorySearchScope>()
        {
            new()
            {
                ActiveCharacter = true,
                CharacterTypes = [CharacterType.Character]
            }
        });
        _listService.AddDefaultColumns(allItemsFilter);
        _listService.AddList(allItemsFilter);
        return allItemsFilter;
    }

    public string Name => "角色背包";
    public string SampleDefaultName => "角色背包";

    public string SampleDescription =>
        "建立顯示目前角色背包物品的清單。";

    public SampleFilterType SampleFilterType => SampleFilterType.Default;
}
