using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace DataCenter_CommonShop.Patches;

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonCheckOut))]
internal class Patch_Checkout
{
    private static void Prefix(ComputerShop __instance)
    {
        if (__instance.cartUIItems == null) return;

        foreach (var cartItem in __instance.cartUIItems)
        {
            if (cartItem == null) continue;

            foreach (var customItem in ShopAPI.RegisteredItems)
            {
                int targetID = customItem.ResultItemID ?? customItem.TemplateID;

                // The library handles the robust string matching (checking both itemName and displayName)
                if (cartItem.itemID == targetID && 
                    (cartItem.itemName == customItem.Name || cartItem.itemName == customItem.Name))
                {
                    if (customItem.OnCheckout != null)
                    {
                        try 
                        { 
                            // Use .quantity or .Quantity depending on what Il2Cpp exposed
                            customItem.OnCheckout.Invoke(cartItem.Quantity); 
                        }
                        catch (System.Exception ex) 
                        { 
                            MelonLogger.Error($"[ShopLib] Error firing OnCheckout for {customItem.Name}: {ex}"); 
                        }
                    }
                }
            }
        }
    }
}