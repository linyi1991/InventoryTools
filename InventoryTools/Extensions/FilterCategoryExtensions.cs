using System;
using InventoryTools.Logic.Filters;

namespace InventoryTools.Extensions;

public static class FilterCategoryExtensions
{
    public static string FormattedName(this FilterCategory filterCategory)
    {
        return filterCategory switch
        {
            FilterCategory.Basic => "基本",
            FilterCategory.Columns => "欄位",
            FilterCategory.CraftColumns => "製作欄位",
            FilterCategory.Acquisition => "取得方式",
            FilterCategory.Gathering => "採集",
            FilterCategory.Searching => "搜尋",
            FilterCategory.Display => "顯示",
            FilterCategory.Inventories => "庫存",
            FilterCategory.Advanced => "進階",
            FilterCategory.IngredientSourcing => "素材取得",
            FilterCategory.ZonePreference => "區域偏好",
            FilterCategory.WorldPricePreference => "世界價格偏好",
            FilterCategory.Sources => "來源",
            FilterCategory.Uses => "用途",
            FilterCategory.SourceCategories => "來源（分類）",
            FilterCategory.UseCategories => "用途（分類）",
            FilterCategory.Crafting => "製作",
            FilterCategory.Market => "市場",
            FilterCategory.Settings => "設定",
            FilterCategory.Stats => "統計",
            FilterCategory.CompletionTracking => "完成追蹤",
            _ => filterCategory.ToString().ToSentence()
        };
    }
}
