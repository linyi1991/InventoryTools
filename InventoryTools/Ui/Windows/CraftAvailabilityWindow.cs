using System;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.CraftAvailability;
using InventoryTools.Logic;
using InventoryTools.Services;
using AllaganLib.GameSheets.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public sealed class CraftAvailabilityWindow : GenericWindow, IMenuWindow
{
    private static readonly (CraftAvailabilityCategory Category, string Label)[] Categories =
    {
        (CraftAvailabilityCategory.All, "全部"),
        (CraftAvailabilityCategory.CraftingGear, "製作裝備"),
        (CraftAvailabilityCategory.GatheringGear, "採集裝備"),
        (CraftAvailabilityCategory.CombatGear, "戰鬥裝備"),
        (CraftAvailabilityCategory.Food, "料理"),
        (CraftAvailabilityCategory.Medicine, "藥品"),
        (CraftAvailabilityCategory.IntermediateMaterial, "中間素材"),
    };

    private readonly CraftAvailabilityService _service;
    private readonly ArtisanCraftService _artisanCraftService;
    private readonly ItemSheet _itemSheet;
    private readonly ImGuiTooltipService _tooltipService;
    private readonly System.Collections.Generic.Dictionary<uint, int> _craftAmounts = new();
    private readonly System.Collections.Generic.Dictionary<uint, string> _hqPredictions = new();
    private CraftAvailabilityCategory _category = CraftAvailabilityCategory.All;
    private bool _craftableOnly = true;
    private bool _includeRetainers = true;
    private bool _includeSubrecipes;
    private bool _ignoreCrystals;
    private string _search = string.Empty;

    public CraftAvailabilityWindow(ILogger<CraftAvailabilityWindow> logger, MediatorService mediator,
        ImGuiService imGuiService, InventoryToolsConfiguration configuration, CraftAvailabilityService service,
        ArtisanCraftService artisanCraftService,
        ItemSheet itemSheet, ImGuiTooltipService tooltipService,
        string name = "Craft Availability Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _service = service;
        _artisanCraftService = artisanCraftService;
        _itemSheet = itemSheet;
        _tooltipService = tooltipService;
    }

    public override void Initialize()
    {
        WindowName = "我的庫存能做什麼";
        Key = "craft-availability";
    }

    public override void Draw()
    {
        var selectedLabel = Array.Find(Categories, c => c.Category == _category).Label;
        ImGui.SetNextItemWidth(150 * ImGui.GetIO().FontGlobalScale);
        if (ImGui.BeginCombo("分類", selectedLabel))
        {
            foreach (var entry in Categories)
            {
                if (ImGui.Selectable(entry.Label, entry.Category == _category))
                    _category = entry.Category;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(220 * ImGui.GetIO().FontGlobalScale);
        ImGui.InputTextWithHint("##craftAvailabilitySearch", "搜尋配方名稱", ref _search, 100);

        ImGui.Checkbox("只顯示目前能做", ref _craftableOnly);
        ImGui.SameLine();
        ImGui.Checkbox("計算所有僱員庫存", ref _includeRetainers);
        ImGui.SameLine();
        ImGui.Checkbox("包含子配方", ref _includeSubrecipes);
        ImGui.SameLine();
        ImGui.Checkbox("忽略水晶", ref _ignoreCrystals);

        ImGui.TextDisabled($"庫存或選項改變時才重新計算；已建立 {_service.IndexedIngredientCount:N0} 個素材反向索引。");
        ImGui.TextColored(new Vector4(0.35f, 0.9f, 0.45f, 1f),
            "品質安全鎖已啟用：HQ 成品須達 100%；收藏品須達 Artisan 所選檔位；固定品質成品可正常製作。 ");
        ImGui.Separator();

        var results = _service.GetResults(_includeRetainers, _includeSubrecipes, _ignoreCrystals, _craftableOnly, _category);
        if (ImGui.BeginTable("craftAvailabilityTable", 6,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("配方名稱", ImGuiTableColumnFlags.WidthStretch, 2.5f);
            ImGui.TableSetupColumn("職業", ImGuiTableColumnFlags.WidthStretch, 1f);
            ImGui.TableSetupColumn("等級", ImGuiTableColumnFlags.WidthFixed, 70);
            ImGui.TableSetupColumn("HQ／模擬", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("最多可製作", ImGuiTableColumnFlags.WidthFixed, 110);
            ImGui.TableSetupColumn("操作", ImGuiTableColumnFlags.WidthFixed, 180);
            ImGui.TableHeadersRow();

            foreach (var result in results)
            {
                if (!string.IsNullOrWhiteSpace(_search) && !result.Name.Contains(_search, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(result.Name);
                var item = _itemSheet.GetRowOrDefault(result.ItemId);
                if (item != null)
                    _tooltipService.DrawItemTooltip(new SearchResult(item), true);
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(result.CraftType);
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(result.RecipeLevel.ToString());
                ImGui.TableNextColumn();
                var canBeHq = item?.Base.CanBeHq == true;
                var isCollectable = item?.Base.IsCollectable == true;
                if (canBeHq || isCollectable)
                {
                    var hasPrediction = _hqPredictions.TryGetValue(result.RecipeId, out var prediction);
                    var safe = hasPrediction && prediction!.StartsWith("SAFE|", StringComparison.Ordinal);
                    var label = !hasPrediction ? (isCollectable ? "收藏品待預測" : "待預測") :
                        safe ? (isCollectable ? "收藏價值達標" : "保證 HQ") :
                        (isCollectable ? "收藏價值未達" : "無法保證");
                    ImGui.TextColored(safe ? new Vector4(0.35f, 0.9f, 0.45f, 1f) :
                        hasPrediction ? new Vector4(1f, 0.45f, 0.35f, 1f) : new Vector4(0.8f, 0.8f, 0.8f, 1f), label);
                    if (hasPrediction && ImGui.IsItemHovered())
                        ImGui.SetTooltip(prediction!.Contains('|') ? prediction[(prediction.IndexOf('|') + 1)..] : prediction);
                    if (ImGui.SmallButton($"模擬製作##predict-hq-{result.RecipeId}"))
                        _hqPredictions[result.RecipeId] = _artisanCraftService.PredictHq(result.RecipeId);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("只執行 Artisan 求解器模擬，不會取料、切換職業、使用食藥或開始製作。修改裝備／食藥／求解器設定後可再次模擬。");
                }
                else
                {
                    ImGui.TextColored(new Vector4(0.55f, 0.75f, 1f, 1f), "固定品質");
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("此成品本來就不存在 HQ 版本，因此不套用 HQ 安全鎖，可直接正常製作。");
                }
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(result.MaxCrafts == 0 ? "—" : $"{result.MaxCrafts:N0} 次 / {result.MaxCrafts * result.Yield:N0} 個");
                ImGui.TableNextColumn();
                if (!_craftAmounts.TryGetValue(result.RecipeId, out var amount))
                    amount = 1;
                ImGui.SetNextItemWidth(70 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.InputInt($"##amount-{result.RecipeId}", ref amount, 1, 10))
                    amount = Math.Clamp(amount, 1, (int)Math.Max(1u, result.MaxCrafts));
                _craftAmounts[result.RecipeId] = amount;
                ImGui.SameLine();
                if (ImGui.SmallButton($"製作##craft-{result.RecipeId}"))
                    _artisanCraftService.PrepareAndCraft(result.RecipeId, amount, _includeSubrecipes);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(isCollectable
                        ? "真正取料、切換職業並製作；會重新模擬並確認達到 Artisan 設定的收藏品目標檔位。"
                        : !canBeHq
                        ? "真正取料、切換職業並製作；此成品為固定品質，不需要 HQ 判斷。"
                        : "真正取料、切換職業並製作；按下後會重新模擬，只有預測品質 100% 且技能成功率皆為 100% 才會開始。");
            }
            ImGui.EndTable();
        }
    }

    public override bool SaveState => true;
    public override Vector2? DefaultSize { get; } = new(850, 620);
    public override Vector2? MaxSize { get; } = new(5000, 5000);
    public override Vector2? MinSize { get; } = new(620, 360);
    public override string GenericKey => "craft-availability";
    public override string GenericName => "我的庫存能做什麼";
    public override bool DestroyOnClose => false;
    public override FilterConfiguration? SelectedConfiguration => null;
    public override void Invalidate() => _service.Invalidate();
}
