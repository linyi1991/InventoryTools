using System.Numerics;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public class HelpWindow : GenericWindow
    {
        private readonly InventoryToolsConfiguration _configuration;

        public HelpWindow(ILogger<HelpWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "Help Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _configuration = configuration;
        }
        public override void Initialize()
        {
            WindowName = "說明";
            Key = "help";
        }

        public override bool SaveState => false;
        public override Vector2? DefaultSize { get; } = new Vector2(700, 700);
        public override  Vector2? MaxSize { get; } = new Vector2(2000, 2000);
        public override  Vector2? MinSize { get; } = new Vector2(200, 200);
        public override string GenericKey { get; } = "help";
        public override string GenericName { get; } = "說明";
        public override bool DestroyOnClose => true;


        public override void Draw()
        {
            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    if (ImGui.Selectable("1. 一般說明###1. General", _configuration.SelectedHelpPage == 0))
                    {
                        _configuration.SelectedHelpPage = 0;
                    }

                    if (ImGui.Selectable("2. 篩選基礎###2. Filter Basics", _configuration.SelectedHelpPage == 1))
                    {
                        _configuration.SelectedHelpPage = 1;
                    }

                    if (ImGui.Selectable("3. 篩選語法###3. Filtering", _configuration.SelectedHelpPage == 2))
                    {
                        _configuration.SelectedHelpPage = 2;
                    }

                    if (ImGui.Selectable("4. 關於###4. About", _configuration.SelectedHelpPage == 3))
                    {
                        _configuration.SelectedHelpPage = 3;
                    }
                }
            }

            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("###ivHelpView", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (_configuration.SelectedHelpPage == 0)
                    {
                        ImGui.TextWrapped(
                            "Allagan Tools 提供三項主要功能：追蹤與顯示庫存、規劃製作，以及查詢物品資訊。其他功能請參閱功能說明。");
                        ImGui.TextWrapped(
                            "本插件參考了 Teamcraft 與 Garland Tools 的部分設計。");
                        ImGui.NewLine();
                        ImGui.TextUnformatted("庫存追蹤：");
                        ImGui.Separator();
                        ImGui.TextWrapped("插件會盡可能追蹤庫存，但部分庫存必須先在遊戲中開啟一次才能記錄。若看不到雇員、部隊倉庫或投影台等資料，請先開啟對應介面。");
                        ImGui.TextWrapped("庫存記錄完成後，可建立清單縮小搜尋範圍、整理物品，並使用其他功能。");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("製作規劃：");
                        ImGui.Separator();
                        ImGui.TextWrapped("製作視窗可建立待製作物品清單，將物品拆解成各項素材，列出缺少的數量、現有素材的位置，以及缺少素材的採集或購買地點。");
                        ImGui.TextWrapped("如果使用過 Teamcraft，應該很快就能上手。");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("物品資訊：");
                        ImGui.Separator();
                        ImGui.TextWrapped("插件提供完整的物品資料，內容與 Garland Tools 類似。在插件中點擊物品圖示即可開啟詳細資訊視窗。");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("醒目標示：");
                        ImGui.Separator();
                        ImGui.TextWrapped("物品清單與製作清單皆可啟用醒目標示，在遊戲庫存中標出物品位置。視窗開啟時勾選「標示」即可啟用；若要使用巨集切換背景標示，請參閱指令說明。");
                        ImGui.NewLine();

                        ImGui.TextUnformatted("此處提供基本操作說明，更多資訊請參閱 Wiki。");
                        if (ImGui.Button("開啟 Wiki###Open Wiki"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }
                    }
                    else if (_configuration.SelectedHelpPage == 1)
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("清單是查看、搜尋與整理物品的主要方式。");
                        ImGui.Text("目前可建立三種類型的清單。");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("搜尋清單");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();

                        ImGui.TextUnformatted("此類清單可搜尋所有庫存中的特定物品。若只需要找物品而不需要整理，請使用此類型。");
                        ImGui.TextUnformatted("使用範例：");
                        ImGui.BulletText("尋找製作所需的素材。");
                        ImGui.BulletText("尋找存放在某處的家具。");
                        ImGui.BulletText("查看剛取得物品的價值。");
                        ImGui.BulletText("確認投影台或收藏櫃是否已存有特定物品。");
                        ImGui.BulletText("不必前往傳喚鈴即可查看雇員裝備。");
                        ImGui.BulletText("檢查持有的物品是否可存入收藏櫃。");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("整理篩選");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("此類清單在搜尋清單的基礎上，可指定物品要收納的位置，並提供合適的整理方案。");
                        ImGui.TextUnformatted("使用範例：");
                        ImGui.BulletText("製作後收納素材，避免分散重複堆疊。");
                        ImGui.BulletText("將指定物品等級以上的物品存放於陸行鳥鞍囊備用。");
                        ImGui.BulletText("找出部隊倉庫特有的物品，並集中收納至該處。");
                        ImGui.PopTextWrapPos();

                        ImGui.NewLine();
                        ImGui.Text("遊戲物品篩選");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("此篩選可搜尋遊戲物品圖鑑中的所有物品。");
                        ImGui.TextUnformatted("使用範例：");
                        ImGui.BulletText("搜尋投影裝備");
                        ImGui.BulletText("查看尚未取得的坐騎／寵物");
                        ImGui.BulletText("追蹤遊戲內物品的價格");
                        ImGui.PopTextWrapPos();
                    }
                    else if (_configuration.SelectedHelpPage == 2)
                    {
                        ImGui.TextUnformatted("進階搜尋／篩選語法：");
                        ImGui.Separator();
                        ImGui.TextWrapped(
                            "建立清單或搜尋清單結果時，可使用運算子精確篩選。可用語法依欄位類型而異，目前支援 !、<、>、>=、<=、=。");
                        ImGui.TextWrapped(
                            "!－排除包含輸入內容的結果，適用於文字與數字。");
                        ImGui.TextWrapped(
                            "<－顯示小於輸入值的結果，適用於數字。");
                        ImGui.TextWrapped(
                            ">－顯示大於輸入值的結果，適用於數字。");
                        ImGui.TextWrapped(
                            ">=－顯示大於或等於輸入值的結果，適用於數字。");
                        ImGui.TextWrapped(
                            "<=－顯示小於或等於輸入值的結果，適用於數字。");
                        ImGui.TextWrapped(
                            "=－顯示完全等於輸入內容的結果，適用於文字與數字。");
                        ImGui.TextWrapped(
                            "&& 與 || 分別代表「且」與「或」，可串接多個篩選條件。");
                    }
                    else if (_configuration.SelectedHelpPage == 3)
                    {
                        ImGui.TextUnformatted("關於：");
                        ImGui.TextUnformatted(
                            "原作者利用業餘時間開發此插件，並希望持續提供更新。");
                        ImGui.TextUnformatted(
                            "如遇問題，請透過插件安裝器的意見回饋按鈕回報。");
                        ImGui.TextUnformatted("插件 Wiki：");
                        ImGui.SameLine();
                        if (ImGui.Button("開啟###Open##WikiBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }

                        ImGui.TextUnformatted("發現錯誤？");
                        ImGui.SameLine();
                        if (ImGui.Button("開啟###Open##BugBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/issues".OpenBrowser();
                        }
                    }
                }
            }
        }

        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {

        }
    }
}
