using HarmonyLib;
using Il2Cpp;

namespace DataCenter_CommonShop.Patches;

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonShopScreen))]
internal class InternalPatches
{
    private static void Postfix(ComputerShop __instance)
    {
        ShopAPI.InjectAll(__instance);
    }
}