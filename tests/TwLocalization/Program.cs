using InventoryTools.Services;

var checks = 0;
void Equal(string expected, string actual)
{
    if (expected != actual) throw new Exception($"Expected '{expected}', got '{actual}'");
    checks++;
}
Equal("採礦（隱藏）", TwUiLocalization.ItemInfoName("Mining (Hidden)"));
Equal("伐木（定時）", TwUiLocalization.ItemInfoName("Logging (Timed)"));
Equal("割草（限時）", TwUiLocalization.ItemInfoName("Harvesting (Ephemeral)"));
Equal("新月島（金色寶箱）", TwUiLocalization.ItemInfoName("Occult Crescent (Golden Coffer)"));
Equal("採礦（隱藏）（定時）", TwUiLocalization.ItemInfoName("Mining (Hidden) (Timed)"));
Equal("可存入收藏櫃？", TwUiLocalization.ColumnName("Is Armoire?"));
Equal("未分類", TwUiLocalization.ColumnName("Unsorted"));
Equal("市場最低單價 HQ", TwUiLocalization.ColumnName("Market Board Minimum Price HQ"));
Equal("My custom list 泰坦", TwUiLocalization.ListName("My custom list 泰坦"));
Equal("UnknownFutureRenderer", TwUiLocalization.ItemInfoName("UnknownFutureRenderer"));
Equal("Custom Column", TwUiLocalization.ColumnName("Custom Column"));
Equal("是", TwSettingsLocalization.Translate("Yes"));
Equal("否", TwSettingsLocalization.Translate("No"));
Equal("不適用", TwSettingsLocalization.Translate("N/A"));
Equal("", TwSettingsLocalization.Translate(null));
Equal("true", TwSettingsLocalization.Translate("true"));
Equal("false", TwSettingsLocalization.Translate("false"));
Equal("CraftWorldPriceUseHomeWorld", TwSettingsLocalization.Translate("CraftWorldPriceUseHomeWorld"));
Equal("單一表格", TwSettingsLocalization.Translate("Single Table"));
var publicWorlds = Enumerable.Range(4028, 8).Select(x => (uint)x).Concat(new uint[] { 72, 4027, 4036 }).ToArray();
Equal("4028,4029,4030,4031,4032,4033,4034,4035",
    string.Join(",", TwMarketWorlds.ForItem(new uint[] { 4028 }, Array.Empty<uint>(), publicWorlds, true)));
Equal("4028", string.Join(",", TwMarketWorlds.ForItem(new uint[] { 4028 }, Array.Empty<uint>(), publicWorlds, false)));
Equal("72,4028,4030", string.Join(",", TwMarketWorlds.ForItem(new uint[] { 0, 4028, 4028 },
    new uint[] { 72, 4030 }, Array.Empty<uint>(), true)));
Equal("False", TwMarketWorlds.IsTraditionalChinese(72).ToString());
Equal("False", TwMarketWorlds.IsTraditionalChinese(4036).ToString());
Equal("4028,4035", string.Join(",", TwMarketWorlds.ForItem(Array.Empty<uint>(), Array.Empty<uint>(), new uint[] { 4028, 4035 }, true)));
// Regression: all eight TW rows actually have IsPublic=false in the TC World sheet.
var twPrivateRows = Enumerable.Range(4028, 8).Select(id => (Id: (uint)id, IsPublic: false));
var pickerWorlds = twPrivateRows.Where(row => TwMarketWorlds.IsAvailableForMarket(row.Id, row.IsPublic))
    .Select(row => row.Id).ToArray();
Equal("8", pickerWorlds.Length.ToString());
Equal("4028,4029,4030,4031,4032,4033,4034,4035",
    string.Join(",", TwMarketWorlds.ForItem(new uint[] { 4035 }, Array.Empty<uint>(), pickerWorlds, true)));
Equal("4035", string.Join(",", TwMarketWorlds.ForItem(new uint[] { 4035 }, Array.Empty<uint>(), pickerWorlds, false)));
Equal("False", TwMarketWorlds.IsAvailableForMarket(3000, false).ToString());
Equal("True", TwMarketWorlds.IsAvailableForMarket(72, true).ToString());
Console.WriteLine($"PASS: {checks} runtime localization checks; dictionary initialization and unknown-value fallbacks verified.");

// The real setting interface depends on Dalamud; this pure display test needs only its two text fields.
namespace InventoryTools.Logic.Settings.Abstract
{
    public interface ISetting
    {
        string Name { get; set; }
        string HelpText { get; set; }
    }
}
