using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemArmoireSourceRenderer : ItemInfoRenderer<ItemArmoireSource>
{
    public ItemArmoireSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.Armoire;
    public override string SingularName => "可存放於收藏櫃";
    public override bool ShouldGroup => true;
    public override string HelpText => "此物品是否可存入收藏櫃？";

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text("分類：" +
                   (asSource.Cabinet.CabinetCategory?.Base.Category.Value.Text.ExtractText() ?? "Unknown"));
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);

        return "分類：" + asSource.Cabinet.CabinetCategory?.Base.Category.Value.Text.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.ArmoireIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return $"可放置於：{asSource.Cabinet.CabinetCategory?.Base.Category.Value.Text}";
    };
}