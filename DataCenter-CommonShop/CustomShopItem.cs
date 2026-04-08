using Il2Cpp;
using UnityEngine;

namespace DataCenter_CommonShop;

public class CustomShopItem
{
    public string Name;
    public int Price;
    public Sprite Icon;
    public Action OnBuy;
    public PlayerManager.ObjectInHand TemplateType;
    public int TemplateID;
    public Color? BackgroundColor;
    public GameObject CustomPrefab = null;
    public Action<GameObject> OnUIReady = null;
    public int? ResultItemID = null;
    public Action<int> OnCheckout = null;
    public string Category = null;
    public string SubCategory = null;

    /// <summary>Assigns a vanilla base-game category to this item.</summary>
    public void SetCategory(VanillaCategory category) => Category = category.ToShopString();
    
    internal static ShopItem FindTemplate(ComputerShop shop, PlayerManager.ObjectInHand type, int id)
    {
        if (shop.shopItems == null) return null;
        
        foreach (var si in shop.shopItems)
        {
            if (si?.shopItemSO != null && si.shopItemSO.itemType == type && si.shopItemSO.itemID == id)
                return si;
        }
        
        return shop.shopItems.Length > 0 ? shop.shopItems[0] : null; 
    }
}