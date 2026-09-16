using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftStagingAreaFilter : InventoryScopeFilter
{
    public CraftStagingAreaFilter(InventoryScopePicker scopePicker, ILogger<CraftStagingAreaFilter> logger, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftStagingArea";
    public override string Name { get; set; } = "素材準備區";

    public override string HelpText { get; set; } =
        "設定製作時視為可用庫存的準備區。預設為目前角色的背包、水晶與貨幣，也可自行加入陸行鳥鞍囊等庫存。";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override List<InventorySearchScope>? GenerateDefaultScope()
    {
        return new List<InventorySearchScope>()
        {
            new InventorySearchScope() { ActiveCharacter = true, CharacterTypes = [CharacterType.Character], Categories = [InventoryCategory.CharacterBags, InventoryCategory.Currency, InventoryCategory.Crystals] }
        };
    }

    public override int Order { get; set; } = -1;
}