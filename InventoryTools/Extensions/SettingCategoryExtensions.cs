using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Extensions
{
    public static class SettingCategoryExtensions
    {
        public static string FormattedName(this SettingCategory settingCategory)
        {
            switch (settingCategory)
            {
                case SettingCategory.General:
                    return "一般";
                case SettingCategory.Visuals:
                    return "外觀";
                case SettingCategory.MarketBoard:
                    return "市場布告板";
                case SettingCategory.CraftOverlay:
                    return "製作浮動視窗";
                case SettingCategory.CraftTracker:
                    return "製作追蹤（舊版）";
                case SettingCategory.ToolTips:
                    return "物品提示";
                case SettingCategory.Hotkeys:
                    return "快捷鍵";
                case SettingCategory.History:
                    return "歷史紀錄";
                case SettingCategory.Windows:
                    return "視窗";
                case SettingCategory.Lists:
                    return "清單";
                case SettingCategory.ContextMenu:
                    return "右鍵選單";
                case SettingCategory.MobSpawnTracker:
                    return "怪物位置追蹤";
                case SettingCategory.TitleMenuButtons:
                    return "標題畫面按鈕";
                case SettingCategory.AutoSave:
                    return "自動儲存";
                case SettingCategory.Items:
                    return "物品資訊";
                case SettingCategory.Highlighting:
                    return "標示";
                case SettingCategory.EquipmentRecommendation:
                    return "裝備推薦";
            }
            return settingCategory.ToString();
        }
    }
}
