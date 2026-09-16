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

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAquariumUseRenderer : ItemInfoRenderer<ItemAquariumSource>
{
    public ItemAquariumUseRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.Aquarium;
    public override string SingularName => "水族箱";
    public override string PluralName => "水族箱";
    public override string HelpText => "此物品是否可放入水族箱？";
    public override bool ShouldGroup => false;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var aquariumSource = AsSource(source);
        ImGui.Text("尺寸：" + aquariumSource.AquariumFish.Size);
        ImGui.Text("水域類型：" + aquariumSource.AquariumFish.Base.AquariumWater.Value.Name.ExtractText());
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var aquariumSource = AsSource(source);

        return "水族箱：" + aquariumSource.AquariumFish.Base.AquariumWater.Value.Name.ExtractText() + " (" +
               aquariumSource.AquariumFish.Size + " )";
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.AquariumIcon;

    public override Func<ItemSource, List<MessageBase>?>? OnClick => source =>
    {
        var aquariumSource = AsSource(source);
        //Open an aquarium window or something
        return null;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return $"可放入 {asSource.AquariumFish.Size} 型水族箱，水域：{asSource.AquariumFish.Base.AquariumWater.Value.Name}";
    };
}