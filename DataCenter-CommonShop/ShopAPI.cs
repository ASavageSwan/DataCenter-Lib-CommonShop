using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;
using MelonLoader;
using UnityEngine.UI;
using System.Reflection;
using System.Text.Json;
using MelonLoader.Utils;

namespace DataCenter_CommonShop;

public static class ShopAPI
{
    private static List<CustomShopItem> _registeredItems = new();
    internal static List<CustomShopItem> RegisteredItems => _registeredItems;
    private static bool _initialized = false;
    
    private static Dictionary<int, RegistryEntry> _usedCustomIDs = new();
    private static string RegistryFilePath => Path.Combine(MelonEnvironment.UserDataDirectory, "CommonShop_CustomIDs.json");
    
    public static void Initialize(HarmonyLib.Harmony harmony)
    {
        if (_initialized) return;
        
        LoadIDRegistry(); 
        
        harmony.PatchAll(typeof(ShopAPI).Assembly);
        _initialized = true;
    }
    
    public static void RegisterItem(CustomShopItem item)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        string modName = callingAssembly.GetName().Name;
        foreach (var melon in MelonMod.RegisteredMelons)
        {
            if (melon.MelonAssembly?.Assembly == callingAssembly)
            {
                modName = melon.Info.Name;
                break;
            }
        }

        if (_registeredItems.Any(i => i.Name == item.Name))
        {
            MelonLogger.Warning($"[ShopLib] Conflict: An item named '{item.Name}' is already registered! Skipping.");
            return;
        }

        if (item.ResultItemID.HasValue)
        {
            if (_registeredItems.Any((i => i.TemplateType == item.TemplateType && i.ResultItemID == item.ResultItemID)))
            {
                MelonLogger.Warning($"[ShopLib] Conflict: ResultItemID {item.ResultItemID} for {item.TemplateType} is already claimed by another CommonShop mod! Skipping.");
                return;
            }
        }
        
        if (!Enum.IsDefined(typeof(PlayerManager.ObjectInHand), item.TemplateType))
        {
            int customId = (int)item.TemplateType;
            if (_usedCustomIDs.TryGetValue(customId, out RegistryEntry existingEntry))
            {
                if (existingEntry.ModName != modName)
                {
                    MelonLogger.Error($"[ShopLib] FATAL ID COLLISION! Mod '{modName}' tried to register custom ID [{customId}].");
                    MelonLogger.Error($"[ShopLib] -> That ID is permanently registered to '{existingEntry.ModName}' for the item '{existingEntry.ItemName}'. Skipping item to prevent corruption.");
                    return; 
                }
            }
            else
            {
                _usedCustomIDs[customId] = new RegistryEntry { ModName = modName, ItemName = item.Name };
                SaveIDRegistry();
                MelonLogger.Msg($"[ShopLib] Registered new custom ID [{customId}] to mod '{modName}' for '{item.Name}'.");
            }
        }
        
        if (string.IsNullOrEmpty(item.Category))
        {
            item.Category = modName;
        }

        _registeredItems.Add(item);
        MelonLogger.Msg($"[ShopLib] Registered: {item.Name} in {item.Category}");

        if (MainGameManager.instance?.computerShop != null && 
            MainGameManager.instance.computerShop.gameObject.activeInHierarchy)
        {
            MelonLogger.Msg("[ShopLib] Shop is already active, forcing live injection...");
            InjectAll(MainGameManager.instance.computerShop);
        }
    }
    
    internal static void InjectAll(ComputerShop shop)
    {
        if (_registeredItems.Count == 0) return;

        var injectedGrids = new List<Transform>();
        var categoryGroups = _registeredItems.GroupBy(item => item.Category);

        foreach (var catGroup in categoryGroups)
        {
            string mainCategory = catGroup.Key;
            var subGroups = catGroup.GroupBy(item => item.SubCategory ?? "");

            foreach (var subGroup in subGroups)
            {
                string subCategory = subGroup.Key;

                Transform container = ShopUI.EnsureCategoryContainer(shop, mainCategory, subCategory);
                if (container == null) continue;

                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    if (container.GetChild(i).name.StartsWith("ModCard_"))
                        Object.DestroyImmediate(container.GetChild(i).gameObject);
                }

                foreach (var data in subGroup)
                {
                    if (HasExternalModConflict(shop, data))
                    {
                        MelonLogger.Error($"[ShopLib] External Conflict: Another mod is already using the Name '{data.Name}' or custom ID {data.ResultItemID}. Skipping injection.");
                        continue;     
                    }
                    
                    ShopItem template = null;
                    if (shop.shopItems != null)
                    {
                        foreach (var vanillaItem in shop.shopItems)
                        {
                            if (vanillaItem != null && vanillaItem.shopItemSO != null)
                            {
                                if (vanillaItem.shopItemSO.itemType == data.TemplateType && 
                                    vanillaItem.shopItemSO.itemID == data.TemplateID)
                                {
                                    template = vanillaItem;
                                    break;
                                }
                            }
                        }
                    }
                    if (template == null && shop.shopItems != null && shop.shopItems.Length > 0) 
                    {
                        template = shop.shopItems[0];
                        MelonLogger.Warning($"[ShopLib] Could not find template {data.TemplateType} ID {data.TemplateID} for {data.Name}. Using fallback.");
                    }

                    if (template != null) ShopCard.Create(shop, container, template, data);
                }

                injectedGrids.Add(container);
            }
        }

        var sr = shop.shopItemParent.GetComponentInParent<ScrollRect>();
        if (sr?.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);

        foreach (var grid in injectedGrids)
            ShopUI.FixGridHeight(grid);

        ShopUI.UpdateLayoutHeight(shop);
    }
    
    private static bool HasExternalModConflict(ComputerShop shop, CustomShopItem data)
    {
        int targetID = data.ResultItemID ?? data.TemplateID;
        var allUIItems = shop.shopItemParent.GetComponentsInChildren<ShopItem>(true);
        
        foreach (var uiItem in allUIItems)
        {
            if (uiItem?.shopItemSO != null)
            {
                if (uiItem.shopItemSO.itemName == data.Name) return true;

                if (data.ResultItemID.HasValue && 
                    uiItem.shopItemSO.itemType == data.TemplateType && 
                    uiItem.shopItemSO.itemID == targetID)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static void LoadIDRegistry()
    {
        _usedCustomIDs.Clear();
        if (!File.Exists(RegistryFilePath)) return;

        string jsonString = File.ReadAllText(RegistryFilePath);

        try
        {
            // Try to load the new JSON format
            var loadedDict = JsonSerializer.Deserialize<Dictionary<int, RegistryEntry>>(jsonString);
            if (loadedDict != null) _usedCustomIDs = loadedDict;
            
            MelonLogger.Msg($"[ShopLib] Loaded {_usedCustomIDs.Count} claimed custom IDs from JSON registry.");
        }
        catch 
        {
            try
            {
                var legacyDict = JsonSerializer.Deserialize<Dictionary<int, string>>(jsonString);
                if (legacyDict != null)
                {
                    foreach (var kvp in legacyDict)
                    {
                        _usedCustomIDs[kvp.Key] = new RegistryEntry { ModName = kvp.Value, ItemName = "Unknown (Legacy Format)" };
                    }
                    SaveIDRegistry();
                    MelonLogger.Msg($"[ShopLib] Successfully upgraded legacy JSON registry to new format.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ShopLib] Failed to load or upgrade JSON ID Registry: {ex}");
                _usedCustomIDs = new Dictionary<int, RegistryEntry>(); 
            }
        }
    }

    private static void SaveIDRegistry()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_usedCustomIDs, options);
            File.WriteAllText(RegistryFilePath, jsonString);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[ShopLib] Failed to save JSON ID Registry: {ex}");
        }
    }
}