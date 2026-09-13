using System;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Extensions
{
    public static class SettingSubCategoryExtensions
    {
        public static string FormattedName(this SettingSubCategory settingSubCategory)
        {
            switch (settingSubCategory)
            {
                case SettingSubCategory.Experimental:
                    return "實驗性功能";
                case SettingSubCategory.Fun:
                    return "娛樂功能";
                case SettingSubCategory.Highlighting:
                    return "標示";
                case SettingSubCategory.DestinationHighlighting:
                    return "目的地標示";
                case SettingSubCategory.RetainerHighlighting:
                    return "雇員標示";
                case SettingSubCategory.Market:
                    return "市場價格";
                case SettingSubCategory.General:
                    return "一般";
                case SettingSubCategory.Subsetting:
                    return "詳細設定";
                case SettingSubCategory.Visuals:
                    return "外觀";
                case SettingSubCategory.WindowLayout:
                    return "視窗版面";
                case SettingSubCategory.AutoSave:
                    return "自動儲存";
                case SettingSubCategory.FilterSettings:
                    return "清單設定";
                case SettingSubCategory.ActiveLists:
                    return "目前清單";
                case SettingSubCategory.ContextMenus:
                    return "右鍵選單";
                case SettingSubCategory.Hotkeys:
                    return "快捷鍵";
                case SettingSubCategory.IgnoreEscape:
                    return "忽略 Esc 鍵";
                case SettingSubCategory.SourceGrouping:
                    return "取得來源分組";
                case SettingSubCategory.UseGrouping:
                    return "用途分組";
                case SettingSubCategory.Colours:
                    return "顏色";
                case SettingSubCategory.AddItemLocations:
                    return "物品持有位置";
                case SettingSubCategory.MarketPricing:
                    return "市場價格";
                case SettingSubCategory.AmountToRetrieve:
                    return "需取出數量";
                case SettingSubCategory.ItemUnlockStatus:
                    return "物品解鎖狀態";
                case SettingSubCategory.SourceInformation:
                    return "取得來源資訊";
                case SettingSubCategory.UseInformation:
                    return "用途資訊";
                case SettingSubCategory.AcquisitionTracker:
                    return "取得追蹤器";
                case SettingSubCategory.IngredientPatch:
                    return "素材版本";
            }
            return settingSubCategory.ToString();
        }
    }
}
