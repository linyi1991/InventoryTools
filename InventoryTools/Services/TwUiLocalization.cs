using System;
using System.Collections.Generic;

namespace InventoryTools.Services;

/// <summary>
/// Traditional Chinese display-only translations for the API13 main UI.
/// Stored list names, configuration keys, ImGui IDs, commands and IPC remain unchanged.
/// </summary>
public static class TwUiLocalization
{
    private static readonly Dictionary<string, string> KnownListNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["All"] = "全部庫存",
        ["Character"] = "角色背包",
        ["Favourites"] = "最愛物品",
        ["Favorites"] = "最愛物品",
        ["Free Company"] = "部隊倉庫",
        ["Full Item Catalog"] = "完整物品圖鑑",
        ["History"] = "庫存歷史",
        ["Housing"] = "房屋庫存",
        ["Retainers"] = "所有雇員",
        ["All Lists"] = "所有清單",
        ["Untitled"] = "未命名",
    };

    private static readonly Dictionary<string, string> ColumnNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Favourite?"] = "最愛？",
        ["Name"] = "名稱",
        ["Type"] = "類型",
        ["Quantity"] = "數量",
        ["Owner"] = "持有者",
        ["Location"] = "位置",
        ["Icon"] = "圖示",
        ["Category"] = "分類",
        ["Acquisition"] = "取得來源",
        ["Uses"] = "用途",
        ["Source World"] = "來源世界",
        ["Last Seen"] = "最後出現",
        ["Date/Time"] = "日期／時間",
        ["Event"] = "事件",
        ["Amount"] = "數量",
        ["Shortcuts"] = "快捷操作",
        ["Settings"] = "設定",
        ["Zone"] = "區域",
        ["Required"] = "需要",
        ["Inventory"] = "庫存",
        ["Next Step"] = "下一步",
        ["Patch"] = "版本",
        ["Dye"] = "染色",
        ["Gearsets"] = "套裝",
        ["Retainer Unit Price"] = "雇員單價",
        ["MB Avg. Price NQ"] = "市場平均價 NQ",
        ["MB Avg. Price HQ"] = "市場平均價 HQ",
        ["MB Min. Price NQ"] = "市場最低價 NQ",
        ["MB Min. Price HQ"] = "市場最低價 HQ",
        ["MB Avg. Price NQ/HQ"] = "市場平均價 NQ／HQ",
        ["MB Min. Price NQ/HQ"] = "市場最低價 NQ／HQ",
        ["MB Avg. Total NQ/HQ"] = "市場平均總價 NQ／HQ",
        ["MB Min. Total NQ/HQ"] = "市場最低總價 NQ／HQ",
    };

    private static readonly Dictionary<string, string> ColumnHelpTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["最愛？"] = "顯示物品是否已加入最愛；點擊圖示可加入或移除最愛。",
        ["名稱"] = "顯示物品名稱；可在欄位下方輸入文字篩選物品。",
        ["名稱與圖示"] = "顯示物品名稱與圖示；將滑鼠移到物品上可查看詳細資訊。",
        ["類型"] = "顯示物品的品質或類型，例如 NQ、HQ 或收藏品。",
        ["數量"] = "顯示此庫存項目的持有數量。",
        ["數量／可用總數"] = "顯示此格的數量，以及目前搜尋範圍內可使用的總數。",
        ["持有者"] = "顯示持有此物品的角色、雇員或部隊倉庫。",
        ["所屬角色"] = "顯示此物品所屬的角色或雇員。",
        ["位置"] = "顯示物品位於哪個背包、兵裝庫、雇員或其他庫存容器。",
        ["庫存位置"] = "顯示物品所在的庫存容器與格位。",
        ["來源"] = "顯示持有此物品的角色、雇員或庫存來源。",
        ["來源世界"] = "顯示市場價格資料所使用的伺服器世界。",
        ["目的地"] = "顯示整理時應移往的位置；歷史清單則顯示物品曾移往的位置。",
        ["分類"] = "顯示物品分類，可用來篩選相同類別的物品。",
        ["Category (Basic)"] = "顯示物品的基本分類。",
        ["Category (Marketboard)"] = "顯示物品在市場布告板中的分類。",
        ["取得來源"] = "顯示可取得此物品的方式，例如商店、製作、採集或任務。",
        ["用途"] = "顯示此物品可用於哪些配方、交換或其他用途。",
        ["圖示"] = "顯示物品圖示；點擊可開啟物品詳細資訊。",
        ["稀有度"] = "顯示物品的稀有度。",
        ["物品 ID"] = "顯示遊戲內部使用的物品編號。",
        ["版本"] = "顯示此物品加入遊戲時的版本。",
        ["染色"] = "顯示物品目前使用的染色。",
        ["套裝"] = "顯示哪些套裝配置正在使用此物品。",
        ["快捷操作"] = "顯示可對此物品執行的快捷按鈕。",
        ["設定"] = "調整此製作項目的來源、數量與市場價格偏好。",
        ["需要"] = "顯示完成製作清單所需的數量。",
        ["庫存"] = "顯示角色與外部庫存中已持有的數量。",
        ["可用總數"] = "顯示目前搜尋範圍內可使用的總數。",
        ["可製作數量"] = "依現有素材計算目前可製作的數量。",
        ["需取出數量"] = "顯示需要從角色、雇員或其他庫存取出的數量。",
        ["缺少數量"] = "顯示扣除現有庫存後仍缺少的數量。",
        ["下一步"] = "顯示完成製作清單時建議進行的下一個步驟。",
        ["區域"] = "顯示取得或製作此物品所涉及的區域。",
        ["最後出現"] = "顯示掃描庫存時最後一次看見此物品的時間。",
        ["日期／時間"] = "顯示庫存歷史事件發生的日期與時間。",
        ["事件"] = "顯示庫存歷史中發生的變更原因。",
        ["市場價格"] = "顯示 Universalis 提供的目前市場價格。",
        ["雇員單價"] = "顯示雇員目前設定的單件販售價格。",
        ["市場平均價 NQ"] = "顯示普通品質物品的市場平均單價。",
        ["市場平均價 HQ"] = "顯示高品質物品的市場平均單價。",
        ["市場最低價 NQ"] = "顯示普通品質物品目前的市場最低單價。",
        ["市場最低價 HQ"] = "顯示高品質物品目前的市場最低單價。",
        ["市場平均價 NQ／HQ"] = "顯示普通品質與高品質物品的市場平均單價。",
        ["市場最低價 NQ／HQ"] = "顯示普通品質與高品質物品的市場最低單價。",
        ["市場平均總價 NQ／HQ"] = "以數量乘以平均單價，顯示普通品質與高品質的預估總價。",
        ["市場最低總價 NQ／HQ"] = "以數量乘以最低單價，顯示普通品質與高品質的最低總價。",
    };

    private static readonly Dictionary<string, string> ItemInfoNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Achievement"] = "成就",
        ["Airship Exploration"] = "飛空艇探索",
        ["Anima Shop"] = "元靈武器商店",
        ["Anima Weapon"] = "元靈武器",
        ["Aquarium"] = "水族箱",
        ["Battle Leve"] = "戰鬥理符",
        ["Bicolor Gemstone Shop"] = "雙色寶石商店",
        ["Bought on SQ Store(real money)"] = "商城購買（現金商品）",
        ["Bozja"] = "博茲雅",
        ["Calamity Salvager"] = "失物管理人",
        ["Card Pack"] = "幻卡包",
        ["Coffer"] = "寶箱",
        ["Collectables Exchange Shop"] = "收藏品交易所",
        ["Company Craft"] = "部隊製作",
        ["Company Craft Ingredient"] = "部隊製作素材",
        ["Company Craft Prototype"] = "部隊製作原型",
        ["Company Leve"] = "部隊理符",
        ["Craft Ingredient"] = "製作素材",
        ["Craft Leve"] = "製作理符",
        ["Craft Recipe"] = "製作配方",
        ["Custom Delivery"] = "老主顧交易",
        ["Desynthesis"] = "分解",
        ["Dungeon Boss Chest"] = "迷宮首領寶箱",
        ["Dungeon Boss Drop"] = "迷宮首領掉落",
        ["Dungeon Chest"] = "迷宮寶箱",
        ["Dungeon Drop"] = "迷宮掉落",
        ["Dye"] = "染色",
        ["Eureka Anemos"] = "優雷卡常風之地",
        ["Eureka Pagos"] = "優雷卡恆冰之地",
        ["Eureka Pyros"] = "優雷卡湧火之地",
        ["Eureka Hydatos"] = "優雷卡豐水之地",
        ["Eureka Orthos"] = "正統優雷卡",
        ["Exterior Furniture"] = "戶外庭具",
        ["Fate"] = "危命任務",
        ["Field Exploration Venture (Combat)"] = "平地探索（戰鬥雇員）",
        ["Fishing"] = "釣魚",
        ["Free Company Shop"] = "部隊商店",
        ["Gardening"] = "園藝",
        ["Gardening Crossbreed"] = "園藝雜交",
        ["Gardening Crossbreed Seed"] = "園藝雜交種子",
        ["Gathering Leve"] = "採集理符",
        ["Gil Shop"] = "金幣商店",
        ["Glamour Ready Set"] = "套裝投影",
        ["Glamour Ready Set Item"] = "套裝投影物品",
        ["Grand Company Expert Delivery"] = "軍隊籌備專家交納",
        ["Grand Company Shop"] = "軍隊商店",
        ["Grand Company Supply & Provisioning"] = "軍隊籌備／補給品交納",
        ["Harvesting"] = "割草",
        ["Heaven on High"] = "天之御柱",
        ["Highland Exploration Venture (Mining)"] = "山岳探索（採礦雇員）",
        ["Interior Furniture"] = "室內家具",
        ["Logging"] = "伐木",
        ["Logogram"] = "文理碎晶",
        ["Loot"] = "戰利品",
        ["Mining"] = "採礦",
        ["Monster Drop"] = "怪物掉落",
        ["Palace of the Dead"] = "死者宮殿",
        ["Quarrying"] = "碎石",
        ["Quick Venture"] = "快速探索",
        ["Reduction"] = "以太還原",
        ["Sky Builder Hand In"] = "重建伊修加德交納",
        ["Sky Builder Inspection"] = "重建伊修加德檢查",
        ["Spearfishing"] = "刺魚",
        ["Special Shop"] = "特殊商店",
        ["Stored in Armoire"] = "可存放於收藏櫃",
        ["Submarine Exploration"] = "潛水艇探索",
        ["Triple Triad Card"] = "九宮幻卡",
        ["Used on Chocobo Companion"] = "陸行鳥夥伴使用",
        ["Venture (Botany)"] = "雇員探險（園藝）",
        ["Venture (Combat)"] = "雇員探險（戰鬥）",
        ["Venture (Fishing)"] = "雇員探險（捕魚）",
        ["Venture (Mining)"] = "雇員探險（採礦）",
        ["Waterside Exploration Venture (Fishing)"] = "水岸探索（捕魚雇員）",
        ["Woodland Exploration Venture (Botany)"] = "森林探索（園藝雇員）",
        ["Zodiac Weapon"] = "古武",
    };

    public static string ListName(string value)
        => KnownListNames.TryGetValue(value, out var translated) ? translated : value;

    public static string ColumnName(string value)
        => ColumnNames.TryGetValue(value, out var translated) ? translated : value;

    public static string ItemInfoName(string value)
    {
        if (ItemInfoNames.TryGetValue(value, out var translated))
            return translated;

        return value
            .Replace(" (Hidden)", "（隱藏）", StringComparison.OrdinalIgnoreCase)
            .Replace(" (Timed)", "（限時）", StringComparison.OrdinalIgnoreCase)
            .Replace(" (Ephemeral)", "（傳說）", StringComparison.OrdinalIgnoreCase)
            .Replace(" (Treasure Coffer)", "（寶箱）", StringComparison.OrdinalIgnoreCase)
            .Replace(" (Golden Coffer)", "（金色寶箱）", StringComparison.OrdinalIgnoreCase)
            .Replace(" (Pot)", "（陶罐）", StringComparison.OrdinalIgnoreCase);
    }

    public static string TooltipLocation(string value)
        => value
            .Replace("Bags", "背包", StringComparison.OrdinalIgnoreCase)
            .Replace("Currency", "貨幣", StringComparison.OrdinalIgnoreCase)
            .Replace("Saddlebag", "陸行鳥鞍囊", StringComparison.OrdinalIgnoreCase)
            .Replace("Armoury", "兵裝庫", StringComparison.OrdinalIgnoreCase)
            .Replace("Inventory", "庫存", StringComparison.OrdinalIgnoreCase);

    public static string ColumnHelp(string columnName, string? currentHelp)
    {
        var translatedName = ColumnName(columnName);
        if (ColumnHelpTexts.TryGetValue(translatedName, out var translated))
            return translated;

        if (!string.IsNullOrWhiteSpace(currentHelp) && ContainsCjk(currentHelp))
            return currentHelp;

        return $"說明「{translatedName}」欄位所顯示的資料；可使用欄位下方的控制項進行篩選。";
    }

    private static bool ContainsCjk(string value)
    {
        foreach (var character in value)
        {
            if (character is >= '\u3400' and <= '\u9fff')
                return true;
        }

        return false;
    }
}
