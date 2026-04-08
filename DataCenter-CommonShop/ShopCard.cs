using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DataCenter_CommonShop;

internal static class ShopCard
{
    internal static void Create(ComputerShop shop, Transform parent, ShopItem template, CustomShopItem data)
    {
        GameObject card;
        if (data.CustomPrefab != null)
        {
            card = Object.Instantiate(data.CustomPrefab, parent);
        }
        else
        {
            card = Object.Instantiate(template.gameObject, parent);
        }
        
        card.name = $"ModCard_{data.Name.Replace(" ", "_")}";
        
        var si = card.GetComponent<ShopItem>();
        if (si != null) Object.DestroyImmediate(si);

        foreach (var txt in card.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            string n = txt.name.ToLower();
            if (n == "textprice") txt.text = $"{data.Price} $";
            else if (n == "text") txt.text = data.Name;
        }

        foreach (var img in card.GetComponentsInChildren<Image>(true))
        {
            string n = img.name.ToLower();
            if (n == "bcg" && data.BackgroundColor.HasValue)
            {
                img.color = data.BackgroundColor.Value;
            }
            else if (n == "image" && template.shopItemSO != null)
            {
                img.sprite = data.Icon ?? template.shopItemSO.sprite;
                img.color = Color.white; 
            }
        }

        var btnExt = card.GetComponentInChildren<ButtonExtended>(true);
        if (btnExt != null)
        {
            btnExt.onClick.RemoveAllListeners();
            btnExt.interactable = true;

            Action customBuy = () =>
            {
                AddCustomItemToCart(shop, data);
                if (data.OnBuy != null) data.OnBuy.Invoke();
            };

            btnExt.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(customBuy));
        }
        
        if (data.OnUIReady != null)
        {
            try { data.OnUIReady.Invoke(card); }
            catch (System.Exception ex) { MelonLogger.Error($"[ShopLib] UI Callback Error for {data.Name}: {ex}"); }
        }

        card.SetActive(true);
        MelonLogger.Msg($"[ShopLib] Built purely custom Shop Card for: {data.Name}");
    }

    /// <summary>
    /// Safely adds a custom item to the cart. If it already exists, it natively stacks it!
    /// </summary>
    private static void AddCustomItemToCart(ComputerShop shop, CustomShopItem data)
    {
        try 
        {
            int targetID = data.ResultItemID ?? data.TemplateID;

            if (shop.cartUIItems != null)
            {
                foreach (var cartItem in shop.cartUIItems)
                {
                    if (cartItem != null && cartItem.itemID == targetID && cartItem.price == data.Price && cartItem.itemName == data.Name)
                    {
                        try
                        {
                            cartItem.OnAddClicked();
                            shop.UpdateCartTotal();
                            return;
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error($"[ShopLib] Fatal Cart Error: {ex}");
                        }

                        var buttons = cartItem.GetComponentsInChildren<ButtonExtended>(true);
                        foreach (var btn in buttons)
                        {
                            var txtPro = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                            if (txtPro != null && txtPro.text.Contains("+"))
                            {
                                btn.onClick.Invoke();
                                MelonLogger.Msg("[ShopLib] Stacked +1 successfully via ButtonExtended phantom click!");
                                return; 
                            }
                        }
                        return; 
                    }
                }
            }

            // Spawns the very first row using the targetID!
            shop.SpawnNewCartItem(
                targetID, 
                data.Price, 
                data.TemplateType, 
                data.Name, 
                new Il2CppSystem.Nullable<Color>()
            );
                
            shop.UpdateCartTotal();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[ShopLib] Fatal Cart Error: {ex}");
        }
    }
}