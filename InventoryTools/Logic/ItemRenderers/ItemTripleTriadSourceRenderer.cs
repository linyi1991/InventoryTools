using System;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemTripleTriadSourceRenderer : ItemInfoRenderer<ItemTripleTriadSource>
{
    public ItemTripleTriadSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.TripleTriad;
    public override string SingularName => "九宮幻卡";
    public override string HelpText => "此物品是否可透過九宮幻卡對戰取得？";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => (source) =>
    {
        var asSource = this.AsSource(source);

        ImGui.TextUnformatted("對戰費用：" + asSource.TripleTriadRow.Base.Fee + SeIconChar.Gil.ToIconString());
        ImGui.TextUnformatted("使用地區規則：" + (asSource.TripleTriadRow.Base.UsesRegionalRules ? "Yes" : "No"));

        DrawSection("Rules: ", asSource.TripleTriadRow.Base.TripleTriadRule.Where(c => c.RowId != 0).DistinctBy(c => c.RowId).Select(c => c.Value.Name.ToImGuiString()).ToList());

        foreach (var npc in asSource.TripleTriadRow.ENpcBaseRows)
        {
            DrawLocations(npc.Resident.Value.Singular.ToImGuiString(), npc.Locations.ToList());
        }
    };

    public override Func<ItemSource, string> GetName => (source) =>
    {
        return source.Item.NameString;
    };

    public override Func<ItemSource, int> GetIcon => (source) =>
    {
        return Icons.TripleTriadIcon;
    };

    public override Func<ItemSource, string> GetDescription => (source) =>
    {
        return source.Item.NameString;
    };
}