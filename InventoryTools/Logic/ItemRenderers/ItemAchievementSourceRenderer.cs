using System;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Logic.ItemRenderers;

public class ItemAchievementSourceRenderer : ItemInfoRenderer<ItemAchievementSource>
{
    public ItemAchievementSourceRenderer(ItemSheet itemSheet, MapSheet mapSheet, ITextureProvider textureProvider,
        IDalamudPluginInterface dalamudPluginInterface) : base(textureProvider, dalamudPluginInterface, itemSheet, mapSheet)
    {
    }

    public override RendererType RendererType => RendererType.Source;
    public override ItemInfoType Type => ItemInfoType.Achievement;
    public override string SingularName => "成就";

    public override string? PluralName => "成就";
    public override string HelpText => "此物品是否可透過成就取得？";
    public override bool ShouldGroup => true;

    public override Action<ItemSource> DrawTooltip => source =>
    {
        ImGui.Text(this.GetDescription(source));
    };

    public override Func<ItemSource, string> GetName => source =>
    {
        var achievementSource = AsSource(source);
        return achievementSource.Achievement.Value.Name.ExtractText();
    };

    public override Func<ItemSource, int> GetIcon => source =>
    {
        return Icons.AchievementCertIcon;
    };

    public override Func<ItemSource, string> GetDescription => source =>
    {
        var asSource = AsSource(source);
        return
            $"{asSource.Achievement.Value.Name.ExtractText()} ({asSource.Achievement.Value.AchievementCategory.Value.Name.ExtractText()})";
    };
}