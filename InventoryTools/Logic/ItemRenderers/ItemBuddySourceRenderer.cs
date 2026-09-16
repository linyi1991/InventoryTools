using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemBuddySourceRenderer : ItemInfoRenderer<ItemBuddySource>
{
    public ItemBuddySourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Use;
    public override ItemInfoType Type => ItemInfoType.BuddyItem;
    public override string SingularName => "陸行鳥夥伴使用";
    public override bool ShouldGroup => false;
    public override string HelpText => "此物品是否可對陸行鳥夥伴使用？";

    public override Action<ItemSource> DrawTooltip => source =>
    {
        var asSource = AsSource(source);
        var usedField = asSource.BuddyItem.Value.UseField;
        var usedTraining = asSource.BuddyItem.Value.UseTraining;
        var usedDyeing = asSource.BuddyItem.Value.Unknown0;

        if (usedField)
        {
            ImGui.Text("戰鬥：增加陸行鳥夥伴獲得的經驗值。");
        }

        if (usedTraining)
        {
            ImGui.Text("鳥房：訓練寄養陸行鳥夥伴的飼料。");
        }

        if (usedDyeing)
        {
            ImGui.Text("染色：用於改變陸行鳥羽毛顏色。");
        }
    };
    public override Func<ItemSource, string> GetName => source =>
    {
        var asSource = AsSource(source);
        var usedField = asSource.BuddyItem.Value.UseField;
        var usedTraining = asSource.BuddyItem.Value.UseTraining;
        var usedDyeing = asSource.BuddyItem.Value.Unknown0;
        var name = new List<string>();


        if (usedField)
        {
            name.Add("戰鬥");
        }

        if (usedTraining)
        {
            name.Add("訓練");
        }

        if (usedDyeing)
        {
            name.Add("染色");
        }

        return "陸行鳥：" + string.Join(", ", name);
    };

    public override Func<ItemSource, int> GetIcon => _ => Icons.ChocoboIcon;

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        var usedField = asSource.BuddyItem.Value.UseField;
        var usedTraining = asSource.BuddyItem.Value.UseTraining;
        var usedDyeing = asSource.BuddyItem.Value.Unknown0;
        var name = new List<string>();

        if (usedField)
        {
            name.Add("戰鬥");
        }

        if (usedTraining)
        {
            name.Add("訓練");
        }

        if (usedDyeing)
        {
            name.Add("染色");
        }

        return "用途：" + string.Join(", ", name);
    };
}