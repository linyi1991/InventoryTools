using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public abstract class ItemFieldOpCofferSourceRenderer<T> : ItemInfoRenderer<T> where T : ItemFieldOpCofferSource
{
    private readonly ItemInfoType _itemInfoType;

    public ItemFieldOpCofferSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface, ItemInfoType itemInfoType) : base(textureProvider, pluginInterface, itemSheet, mapSheet)
    {
        _itemInfoType = itemInfoType;
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => _itemInfoType;
    public override bool ShouldGroup => true;
    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        ImGui.Text("掉落來源：" + asSource.CofferType + " coffer");
        if (asSource.Min != null && asSource.Max != null)
        {
            ImGui.SameLine();
            if (asSource.Min == asSource.Max)
            {
                ImGui.Text("（掉落 1 個）");
            }
            else
            {
                ImGui.Text("（掉落數量：" + asSource.Min.Value + " - " + asSource.Max.Value + ")");
            }
        }

        if (asSource.Probability != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"{asSource.Probability.Value}%");
        }
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.GoldChest2;

    public override Func<ItemSource, string> GetName => _ => "";

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return asSource.CofferType + " coffer";
    };
}

public class ItemPagosTreasureSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemPagosTreasureCofferSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pagos];

    public ItemPagosTreasureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.PagosTreasure)
    {
    }

    public override string SingularName => "優雷卡恆冰之地（寶箱）";
    public override string HelpText => "此物品是否來自優雷卡恆冰之地的寶箱？";
}

public class ItemPyrosTreasureSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemPyrosTreasureCofferSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Pyros];

    public ItemPyrosTreasureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.PyrosTreasure)
    {
    }

    public override string SingularName => "優雷卡湧火之地（寶箱）";
    public override string HelpText => "此物品是否來自優雷卡湧火之地的寶箱？";
}

public class ItemHydatosTreasureSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemHydatosTreasureCofferSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.Hydatos];

    public ItemHydatosTreasureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.HydatosTreasure)
    {
    }

    public override string SingularName => "優雷卡豐水之地（寶箱）";
    public override string HelpText => "此物品是否來自優雷卡豐水之地的寶箱？";
}

public class ItemOccultTreasureSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemOccultTreasureCofferSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.OccultCrescent];

    public ItemOccultTreasureSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.OccultTreasure)
    {
    }

    public override string SingularName => "新月島（寶箱）";
    public override string HelpText => "此物品是否來自新月島的寶箱？";
}

public class ItemOccultPotSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemOccultPotSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.OccultCrescent];

    public ItemOccultPotSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.OccultPot)
    {
    }

    public override string SingularName => "新月島（陶罐）";
    public override string HelpText => "此物品是否來自新月島的陶罐？";
}

public class ItemOccultGoldenCofferSourceRenderer : ItemFieldOpCofferSourceRenderer<ItemOccultGoldenCofferSource>
{
    public override IReadOnlyList<ItemInfoRenderCategory>? Categories { get; } =
        [ItemInfoRenderCategory.FieldOperation, ItemInfoRenderCategory.OccultCrescent];

    public ItemOccultGoldenCofferSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider, IDalamudPluginInterface pluginInterface) : base(itemSheet, mapSheet, textureProvider, pluginInterface, ItemInfoType.OccultGoldenCoffer)
    {
    }

    public override string SingularName => "新月島（金色寶箱）";
    public override string HelpText => "此物品是否來自新月島的金色寶箱？";
}