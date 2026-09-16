using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Mediator;
using InventoryTools.Ui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemDungeonDropSourceRenderer : ItemInfoRenderer<ItemDungeonDropSource>
{
    public ItemDungeonDropSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.DungeonDrop;
    public override string SingularName => "迷宮掉落";
    public override string PluralName => "迷宮掉落";
    public override string HelpText => "此物品是否由迷宮中的怪物掉落？";
    public override bool ShouldGroup => true;
    public override IReadOnlyList<ItemInfoRenderCategory> Categories => [ItemInfoRenderCategory.Duty];

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var dungeonDropSource = AsSource(source);
        ImGui.Text("迷宮：" + dungeonDropSource.ContentFinderCondition.FormattedName);
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var dungeonDropSource = AsSource(source);

        return "迷宮：" + dungeonDropSource.ContentFinderCondition.FormattedName;
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.DutyIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var dungeonDropSource = AsSource(source);

        return new List<MessageBase>()
            { new OpenUintWindowMessage(typeof(DutyWindow), dungeonDropSource.ContentFinderCondition.RowId) };
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return $"{asSource.ContentFinderCondition.FormattedName}";
    };
}