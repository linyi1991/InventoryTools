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
    private DateTime _nextPredictionPoll = DateTime.MinValue;
    private CraftAvailabilityCategory _category = CraftAvailabilityCategory.All;
    private bool _craftableOnly = true;
    private bool _includeRetainers = true;
    private bool _includeSubrecipes = true;
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
        PollPendingPredictions();
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
        ImGui.Checkbox("遞迴製作半成品", ref _includeSubrecipes);
        ImGui.SameLine();
        ImGui.Checkbox("忽略水晶", ref _ignoreCrystals);

        ImGui.TextDisabled($"庫存或選項改變時才重新計算；已建立 {_service.IndexedIngredientCount:N0} 個素材反向索引。");
        ImGui.TextWrapped("開啟「遞迴製作半成品」後，會把庫存原料可先做出的半成品繼續投入下一層配方，計算每種成品各自最多可製作的次數。每列都是獨立估算，同一批材料不能同時完成所有列；MAX 只會填入該列上限，仍要再按「製作」才會交給 Artisan，Artisan 會先處理需要的子配方。");
        ImGui.TextColored(new Vector4(0.35f, 0.9f, 0.45f, 1f),
            "品質安全鎖：Craftimizer 2.11 為主求解器；HQ 須從 0 初始品質達 100%，收藏品須達 Artisan 所選檔位。計算失敗時 Artisan 可安全備援，但未達標不會開始製作。");
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
            ImGui.TableSetupColumn("操作", ImGuiTableColumnFlags.WidthFixed, 225);
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
                    var pending = hasPrediction && prediction!.StartsWith("PENDING|", StringComparison.Ordinal);
                    var safe = hasPrediction && prediction!.StartsWith("SAFE|", StringComparison.Ordinal);
                    var label = pending ? "Craftimizer 計算中" : !hasPrediction ? (isCollectable ? "收藏品待預測" : "待預測") :
                        safe ? (isCollectable ? "收藏價值達標" : "保證 HQ") :
                        (isCollectable ? "收藏價值未達" : "無法保證");
                    ImGui.TextColored(pending ? new Vector4(0.55f, 0.75f, 1f, 1f) : safe ? new Vector4(0.35f, 0.9f, 0.45f, 1f) :
                        hasPrediction ? new Vector4(1f, 0.45f, 0.35f, 1f) : new Vector4(0.8f, 0.8f, 0.8f, 1f), label);
                    if (hasPrediction && ImGui.IsItemHovered())
                        ImGui.SetTooltip(prediction!.Contains('|') ? prediction[(prediction.IndexOf('|') + 1)..] : prediction);
                    if (ImGui.SmallButton($"模擬製作##predict-hq-{result.RecipeId}"))
                        _hqPredictions[result.RecipeId] = _artisanCraftService.StartHqPrediction(result.RecipeId);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("在背景執行 Craftimizer 2.11 Next Action 全流程模擬，不會取料、切換職業、實際使用食藥或開始製作。會採用目前裝備及 Artisan 已設定的食藥數值；修改後請重新模擬。");
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
                amount = Math.Clamp(amount, 1, (int)Math.Max(1u, result.MaxCrafts));
                ImGui.SetNextItemWidth(70 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.InputInt($"##amount-{result.RecipeId}", ref amount, 1, 10))
                    amount = Math.Clamp(amount, 1, (int)Math.Max(1u, result.MaxCrafts));
                _craftAmounts[result.RecipeId] = amount;
                ImGui.SameLine();
                if (result.MaxCrafts == 0) ImGui.BeginDisabled();
                if (ImGui.SmallButton($"MAX##max-craft-{result.RecipeId}"))
                {
                    amount = checked((int)result.MaxCrafts);
                    _craftAmounts[result.RecipeId] = amount;
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("將數量填成此配方依目前庫存獨立估算的最大製作次數；不會立即開始製作。");
                ImGui.SameLine();
                if (ImGui.SmallButton($"製作##craft-{result.RecipeId}"))
                    _artisanCraftService.PrepareAndCraft(result.RecipeId, amount, _includeSubrecipes);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(isCollectable
                        ? "交給 Artisan 背景執行 Craftimizer 2.11 預測；達到 Artisan 設定的收藏品檔位後，才取料、切換職業並以 Craftimizer 實際製作。"
                        : !canBeHq
                        ? "交給 Artisan 取料、切換職業並以 Craftimizer 2.11 實際製作；此成品為固定品質，不需要 HQ 判斷。"
                        : "交給 Artisan 背景執行 Craftimizer 2.11 預測；只有從 0 初始品質達到 100%，且模擬技能成功率皆為 100%，才會取料並開始製作。若 Craftimizer 中途失敗，Artisan 只會使用安全備援。");
                if (result.MaxCrafts == 0) ImGui.EndDisabled();
            }
            ImGui.EndTable();
        }
    }

    private void PollPendingPredictions()
    {
        if (DateTime.UtcNow < _nextPredictionPoll)
            return;
        _nextPredictionPoll = DateTime.UtcNow.AddMilliseconds(250);
        foreach (var recipeId in System.Linq.Enumerable.ToArray(
                     System.Linq.Enumerable.Where(_hqPredictions,
                         pair => pair.Value.StartsWith("PENDING|", StringComparison.Ordinal))))
            _hqPredictions[recipeId.Key] = _artisanCraftService.PollHqPrediction(recipeId.Key);
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
