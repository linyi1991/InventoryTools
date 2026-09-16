using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftSourceInventoriesFilter : InventoryScopeFilter
{
    public CraftSourceInventoriesFilter(InventoryScopePicker scopePicker, ILogger<CraftSourceInventoriesFilter> logger, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftSourceInventories";
    public override string Name { get; set; } = "取物來源庫存";

    public override string HelpText { get; set; } =
        "製作清單應從哪些庫存查找可取出的素材？找到的物品會列於「雇員／背包中的物品」，並依清單設定在採集前或採集後取出。";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override List<InventorySearchScope>? GenerateDefaultScope()
    {
        return new List<InventorySearchScope>()
        {
            new InventorySearchScope() { ActiveCharacter = true, Categories = [InventoryCategory.RetainerBags, InventoryCategory.FreeCompanyBags, InventoryCategory.CharacterSaddleBags, InventoryCategory.CharacterPremiumSaddleBags] }
        };
    }

    public override int Order { get; set; } = -3;
}