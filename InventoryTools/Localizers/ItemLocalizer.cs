using System.Collections.Generic;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Localizers;

public class ItemLocalizer
{
    private readonly ExcelSheet<Addon> _addonSheet;
    private Dictionary<uint, string> _cabinetNames;

    public ItemLocalizer(ExcelSheet<Addon> addonSheet)
    {
        _addonSheet = addonSheet;
        _cabinetNames = new();
    }

    public string CabinetName(InventoryItem inventoryItem)
    {
        if (inventoryItem.SortedContainer != InventoryType.Armoire)
        {
            return "";
        }

        var cabinetCategory = inventoryItem.Item.CabinetCategory;
        if (cabinetCategory == null)
        {
            return "Unknown Cabinet";
        }

        if (_cabinetNames.TryGetValue(cabinetCategory.Base.Category.RowId, out string? cabinetName))
        {
            return cabinetName;
        }

        cabinetName = _addonSheet.GetRowOrDefault(cabinetCategory.Base.Category.RowId)?.Text.ExtractText() ??
                      "Addon Text Not Found";

        _cabinetNames[cabinetCategory.Base.Category.RowId] = cabinetName;

        return cabinetName;
    }

    public string ItemDescription(InventoryItem inventoryItem)
    {
        if (inventoryItem.IsEmpty)
        {
            return "Empty";
        }

        var _item = inventoryItem.Item.NameString.ToString();
        if (inventoryItem.IsHQ)
        {
            _item += " (HQ)";
        }
        else if (inventoryItem.IsCollectible)
        {
            _item += " (Collectible)";
        }
        else
        {
            _item += " (NQ)";
        }

        if (inventoryItem.SortedCategory == InventoryCategory.Currency)
        {
            _item += " - " + SortedContainerName(inventoryItem);
        }
        else
        {
            _item += " - " + SortedContainerName(inventoryItem) + " - " + (inventoryItem.SortedSlotIndex + 1);
        }


        return _item;
    }

    public string FormattedBagLocation(InventoryItem inventoryItem)
    {
        if (inventoryItem.SortedContainer is InventoryType.GlamourChest or InventoryType.Currency or InventoryType.RetainerGil or InventoryType.FreeCompanyGil or InventoryType.Crystal or InventoryType.RetainerCrystal)
        {
            return SortedContainerName(inventoryItem);
        }
        return SortedContainerName(inventoryItem) + " - " + (inventoryItem.SortedSlotIndex + 1);
    }

    public string SortedContainerName(InventoryItem inventoryItem)
    {
        if(inventoryItem.SortedContainer is InventoryType.Bag0 or InventoryType.RetainerBag0)
        {
            return "背包 1";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag1 or InventoryType.RetainerBag1)
        {
            return "背包 2";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag2 or InventoryType.RetainerBag2)
        {
            return "背包 3";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag3 or InventoryType.RetainerBag3)
        {
            return "背包 4";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerBag4)
        {
            return "背包 5";
        }
        if(inventoryItem.SortedContainer is InventoryType.SaddleBag0)
        {
            return "陸行鳥鞍囊（左）";
        }
        if(inventoryItem.SortedContainer is InventoryType.SaddleBag1)
        {
            return "陸行鳥鞍囊（右）";
        }
        if(inventoryItem.SortedContainer is InventoryType.PremiumSaddleBag0)
        {
            return "付費陸行鳥鞍囊（左）";
        }
        if(inventoryItem.SortedContainer is InventoryType.PremiumSaddleBag1)
        {
            return "付費陸行鳥鞍囊（右）";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryBody)
        {
            return "兵裝庫－身體";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryEar)
        {
            return "兵裝庫－耳飾";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryFeet)
        {
            return "兵裝庫－腳部";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryHand)
        {
            return "兵裝庫－手部";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryHead)
        {
            return "兵裝庫－頭部";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryLegs)
        {
            return "兵裝庫－腿部";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryMain)
        {
            return "兵裝庫－主手";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryNeck)
        {
            return "兵裝庫－頸飾";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryOff)
        {
            return "兵裝庫－副手";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryRing)
        {
            return "兵裝庫－戒指";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryWaist)
        {
            return "兵裝庫－腰部";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryWrist)
        {
            return "兵裝庫－腕飾";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmorySoulCrystal)
        {
            return "兵裝庫－靈魂水晶";
        }
        if(inventoryItem.SortedContainer is InventoryType.GearSet0)
        {
            return "已裝備";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerEquippedGear)
        {
            return "已裝備";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag0)
        {
            return "部隊倉庫－1";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag1)
        {
            return "部隊倉庫－2";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag2)
        {
            return "部隊倉庫－3";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag3)
        {
            return "部隊倉庫－4";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag4)
        {
            return "部隊倉庫－5";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag5)
        {
            return "部隊倉庫－6";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag6)
        {
            return "部隊倉庫－7";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag7)
        {
            return "部隊倉庫－8";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag8)
        {
            return "部隊倉庫－9";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag9)
        {
            return "部隊倉庫－10";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag10)
        {
            return "部隊倉庫－11";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerMarket)
        {
            return "市場出售欄";
        }
        if(inventoryItem.SortedContainer is InventoryType.GlamourChest)
        {
            return "投影台";
        }
        if(inventoryItem.SortedContainer is InventoryType.Armoire)
        {
            return "收藏櫃－" + CabinetName(inventoryItem);
        }
        if(inventoryItem.SortedContainer is InventoryType.Currency)
        {
            return "貨幣";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyGil)
        {
            return "部隊－金幣";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerGil)
        {
            return "貨幣";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyCrystal)
        {
            return "部隊－水晶";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyCurrency)
        {
            return "部隊－貨幣";
        }
        if(inventoryItem.SortedContainer is InventoryType.Crystal or InventoryType.RetainerCrystal)
        {
            return "水晶";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorAppearance)
        {
            return "房屋外觀";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorAppearance)
        {
            return "房屋內裝";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorStoreroom)
        {
            return "房屋外部倉庫";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorStoreroom1 or InventoryType.HousingInteriorStoreroom2 or InventoryType.HousingInteriorStoreroom2 or InventoryType.HousingInteriorStoreroom3 or InventoryType.HousingInteriorStoreroom4 or InventoryType.HousingInteriorStoreroom5 or InventoryType.HousingInteriorStoreroom6 or InventoryType.HousingInteriorStoreroom7 or InventoryType.HousingInteriorStoreroom8)
        {
            return "房屋內部倉庫";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorPlacedItems1 or InventoryType.HousingInteriorPlacedItems2 or InventoryType.HousingInteriorPlacedItems2 or InventoryType.HousingInteriorPlacedItems3 or InventoryType.HousingInteriorPlacedItems4 or InventoryType.HousingInteriorPlacedItems5 or InventoryType.HousingInteriorPlacedItems6 or InventoryType.HousingInteriorPlacedItems7 or InventoryType.HousingInteriorPlacedItems8)
        {
            return "房屋內部擺設";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorPlacedItems)
        {
            return "房屋外部擺設";
        }

        return inventoryItem.SortedContainer.ToString();
    }
}
