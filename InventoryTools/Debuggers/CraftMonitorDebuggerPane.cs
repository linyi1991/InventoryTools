using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib.Crafting;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Debuggers;

public class CraftMonitorDebuggerPane : IDebugPane
{
    private readonly ICraftMonitor _craftMonitor;
    private readonly ItemSheet _itemSheet;

    public CraftMonitorDebuggerPane(ICraftMonitor craftMonitor, ItemSheet itemSheet)
    {
        _craftMonitor = craftMonitor;
        _itemSheet = itemSheet;
    }
    public string Name =>  "Craft Monitor";
    public unsafe void Draw()
    {
        var craftMonitorAgent = _craftMonitor.Agent;
        var simpleCraftMonitorAgent = _craftMonitor.SimpleAgent;
        if (craftMonitorAgent != null)
        {
            ImGui.Text($"Craft Monitor Pointer: {(ulong)craftMonitorAgent.Agent:X}");
            ImGui.TextUnformatted("是否為練習製作：" + craftMonitorAgent.IsTrialSynthesis);
            ImGui.TextUnformatted("進展：" + craftMonitorAgent.Progress);
            ImGui.TextUnformatted("所需總進展：" +
                _craftMonitor.RecipeLevelTable?.ProgressRequired(_craftMonitor
                    .CurrentRecipe) ?? "Unknown");
            ImGui.TextUnformatted("品質：" + craftMonitorAgent.Quality);
            ImGui.TextUnformatted("狀態：" + craftMonitorAgent.Status);
            ImGui.TextUnformatted("步驟：" + craftMonitorAgent.Step);
            ImGui.TextUnformatted("耐久：" + craftMonitorAgent.Durability);
            ImGui.TextUnformatted("HQ 機率：" + craftMonitorAgent.HqChance);
            ImGui.TextUnformatted("物品：" +
                                  (_itemSheet.GetRow(craftMonitorAgent.ResultItemId)
                                      ?.NameString ?? "Unknown"));
            ImGui.TextUnformatted(
                "目前配方：" + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
            ImGui.TextUnformatted(
                "配方難度：" + _craftMonitor.RecipeLevelTable?.Base.Difficulty ??
                "Unknown");
            ImGui.TextUnformatted(
                "配方難度係數：" +
                _craftMonitor.CurrentRecipe?.Base.DifficultyFactor ??
                "Unknown");
            ImGui.TextUnformatted(
                "配方耐久：" + _craftMonitor.RecipeLevelTable?.Base.Durability ??
                "Unknown");
            ImGui.TextUnformatted("建議作業精度：" +
                _craftMonitor.RecipeLevelTable?.Base.SuggestedCraftsmanship ?? "Unknown");
            ImGui.TextUnformatted(
                "目前製作類型：" + _craftMonitor.CraftType ?? "Unknown");
        }
        else if (simpleCraftMonitorAgent != null)
        {
            ImGui.Text($"Simple Craft Monitor Pointer: {(ulong)simpleCraftMonitorAgent.Agent:X}");
            ImGui.TextUnformatted("NQ 完成：" + simpleCraftMonitorAgent.NqCompleted);
            ImGui.TextUnformatted("HQ 完成：" + simpleCraftMonitorAgent.HqCompleted);
            ImGui.TextUnformatted("失敗：" + simpleCraftMonitorAgent.TotalFailed);
            ImGui.TextUnformatted("完成總數：" + simpleCraftMonitorAgent.TotalCompleted);
            ImGui.TextUnformatted("總計：" + simpleCraftMonitorAgent.Total);
            ImGui.TextUnformatted("物品：" + _itemSheet
                .GetRowOrDefault(simpleCraftMonitorAgent.ResultItemId)?.NameString.ToString() ?? "Unknown");
            ImGui.TextUnformatted(
                "目前配方：" + _craftMonitor.CurrentRecipe?.RowId ?? "Unknown");
            ImGui.TextUnformatted(
                "目前製作類型：" + _craftMonitor.CraftType ?? "Unknown");
        }
        else
        {
            ImGui.TextUnformatted("目前未在製作。");
        }
    }
}