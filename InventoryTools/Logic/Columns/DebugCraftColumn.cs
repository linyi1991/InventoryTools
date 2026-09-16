using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class DebugCraftColumn : TextColumn
    {
        public DebugCraftColumn(ILogger<DebugCraftColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Debug;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return "";
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            if (searchResult.CraftItem == null) return null;

            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            ImGui.Text("需要：" +  searchResult.CraftItem.QuantityRequired);
            ImGui.Text("尚需：" +  searchResult.CraftItem.QuantityNeeded);
            ImGui.Text("更新前尚需：" +  searchResult.CraftItem.QuantityNeededPreUpdate);
            ImGui.Text("可用：" +  searchResult.CraftItem.QuantityAvailable);
            ImGui.Text("已備妥：" +  searchResult.CraftItem.QuantityReady);
            ImGui.Text("可製作：" +  searchResult.CraftItem.QuantityCanCraft);
            ImGui.Text("將取出：" + searchResult.CraftItem.QuantityWillRetrieve);
            return null;
        }

        public override string Name { get; set; } = "Debug - Craft";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows craft debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}