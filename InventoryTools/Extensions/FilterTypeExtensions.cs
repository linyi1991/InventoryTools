using InventoryTools.Logic;

namespace InventoryTools.Extensions;

public static class FilterTypeExtensions
{
    public static string FormattedName(this FilterType filterType)
    {
        return filterType switch
        {
            FilterType.None => "無",
            FilterType.SearchFilter => "搜尋清單",
            FilterType.SortingFilter => "整理清單",
            FilterType.GameItemFilter => "遊戲物品清單",
            FilterType.CraftFilter => "製作清單",
            FilterType.HistoryFilter => "歷史清單",
            FilterType.CuratedList => "自訂清單",
            _ => "未知"
        };
    }
}
