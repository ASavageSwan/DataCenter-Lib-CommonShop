using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(DataCenter_CommonShop.Main), "Test shop Mod", "1.0.0", "ASavageSwan")]
[assembly: MelonGame("Waseku", "Data Center")]


namespace DataCenter_CommonShop;

public class Main : MelonMod
{
    public override void OnInitializeMelon()
    {
        // 1. Initialize the library patches manually
        // This ensures the library only runs when a mod actually needs it. a
        ShopAPI.Initialize(this.HarmonyInstance);
        RunJsonSaveTest();
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Name = "Test Bulk Box (99x)",
            Price = 999,
            TemplateType = PlayerManager.ObjectInHand.SFPBox,
            TemplateID = 3, // QSFP+ 40Gbps Box
            BackgroundColor = new Color(0.2f, 0.5f, 1.0f),
                
            // OnBuy is now just for extra logic. 
            // The base game handles adding it to the cart, updating the total, and stacking it!
            OnBuy = () =>
            {
                MelonLogger.Msg("[TestMod] Native Button clicked!");
            }
        });

        // 3. Register a Native Server
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Name = "Experimental Modded Server",
            Price = 5000,
            TemplateType = PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0,
            BackgroundColor = new Color(1.0f, 0.84f, 1.0f),
            OnBuy = () =>
            {
                MelonLogger.Msg("[TestMod] Native Button clicked!");
            }
        });
        
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Name = "Advanced Server",
            Price = 5000,
            TemplateType = PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0,
    
            // The Library hands the physical card back to the developer right before it spawns!
            OnUIReady = (GameObject cardObj) =>
            {
                // 1. Find the existing text object to use as a template
                var existingText = cardObj.GetComponentInChildren<Il2CppTMPro.TextMeshProUGUI>();
        
                // 2. Clone it to make a new label
                var newLabel = UnityEngine.Object.Instantiate(existingText.gameObject, cardObj.transform);
                newLabel.name = "CustomWarningLabel";
        
                // 3. Move it down and change the text/color using code!
                newLabel.transform.localPosition = new Vector3(0, -60, 0); 
                var tmp = newLabel.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
                tmp.text = "Requires 200W Power!";
                tmp.color = UnityEngine.Color.red;
                tmp.fontSize = 12f;
            }
        });
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Name = "RGB Disco Server",
            Price = 1500,
            TemplateType = PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0,
    
            // The library hands you the raw UI card right before it appears on screen!
            OnUIReady = (GameObject cardObj) =>
            {
                // 1. Find the original 'Buy' button to use as a template
                var originalBtn = cardObj.GetComponentInChildren<ButtonExtended>(true);
                if (originalBtn == null) return;
        
                // 2. Clone the button!
                var extraBtnObj = UnityEngine.Object.Instantiate(originalBtn.gameObject, cardObj.transform);
                extraBtnObj.name = "EffectButton";
        
                // 3. THE FIX: Tell Unity's Auto-Layout engine to LEAVE THIS BUTTON ALONE!
                var layoutElement = extraBtnObj.GetComponent<UnityEngine.UI.LayoutElement>();
                if (layoutElement == null) layoutElement = extraBtnObj.AddComponent<UnityEngine.UI.LayoutElement>();
                layoutElement.ignoreLayout = true; 
        
                // 4. THE FIX: Anchor it perfectly to the bottom-left corner of the card
                var rect = extraBtnObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0); // Bottom-Left anchor
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);     // Set pivot to Bottom-Left
        
                // Move it 10 pixels to the right, and 10 pixels up from the bottom corner
                rect.anchoredPosition = new Vector2(10, 10); 
        
                // Optional: Shrink the button so it doesn't cover the whole card
                rect.sizeDelta = new Vector2(120, 40); 
        
                // 5. Change the text of the new button
                var txt = extraBtnObj.GetComponentInChildren<Il2CppTMPro.TextMeshProUGUI>();
                if (txt != null) 
                {
                    txt.text = "Preview";
                    txt.fontSize = 18f; // Shrink text to fit our smaller button
                }
        
                // 6. Wire up your custom effect!
                var extraBtn = extraBtnObj.GetComponent<ButtonExtended>();
                extraBtn.onClick.RemoveAllListeners(); 
        
                System.Action myCustomEffect = () => 
                {
                    MelonLoader.MelonLogger.Msg("Preview button clicked! Playing RGB light effect...");
                };
        
                extraBtn.onClick.AddListener(Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(myCustomEffect));
            }
        });
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Category = "Bulk Shipments", // <-- This creates a brand new section in the shop!
            Name = "32x SFP+ Module RJ45 10Gbps 1",
            Price = 1620,
            TemplateType = Il2Cpp.PlayerManager.ObjectInHand.SFPBox,
            TemplateID = 0,
            BackgroundColor = new Color(0.7f, 0.8f, 1.0f)
        });

        ShopAPI.RegisterItem(new CustomShopItem
        {
            Category = "Modded Servers", // <-- This creates another new section!
            Name = "RGB Disco Server 2",
            Price = 5020,
            TemplateType = Il2Cpp.PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0
        });
        
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Category = "Modded Servers", // <-- This creates another new section!
            SubCategory = "Bulk Shipments",
            Name = "RGB Disco Server 32",
            Price = 5020,
            TemplateType = Il2Cpp.PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0
        });
        
        ShopAPI.RegisterItem(new CustomShopItem
        {
            SubCategory = "Bulk Shipments",
            Name = "RGB Disco Server 33",
            Price = 5020,
            TemplateType = Il2Cpp.PlayerManager.ObjectInHand.Server1U,
            TemplateID = 0
        });
        
        // --- TEST 1: Internal Conflict Test ---
// We register an item, then immediately try to register another with the exact same Name.
        ShopAPI.RegisterItem(new CustomShopItem { 
            Name = "Internal Conflict Server", 
            Price = 100, 
            TemplateType = PlayerManager.ObjectInHand.Server1U, 
            TemplateID = 0 
        });
        ShopAPI.RegisterItem(new CustomShopItem { 
            Name = "Internal Conflict Server", // Identical Name
            Price = 200, 
            TemplateType = PlayerManager.ObjectInHand.SFPBox, 
            TemplateID = 0 
        });

        // --- TEST 2: External Name Conflict Test ---
        ShopAPI.RegisterItem(new CustomShopItem {
            Name = "5x QSFP-DD 400Gbps", 
            Price = 10,
            TemplateType = PlayerManager.ObjectInHand.SFPBox,
            TemplateID = 0
        });

        // --- TEST 3: External ID Conflict Test ---
        // It uses SFPBox (which is TemplateType PlayerManager.ObjectInHand.SFPBox).
        ShopAPI.RegisterItem(new CustomShopItem {
            Name = "My Custom Box Mod",
            Price = 500,
            TemplateType = PlayerManager.ObjectInHand.SFPBox,
            TemplateID = 0,
            ResultItemID = 100 
        });
    }
    
    private void RunJsonSaveTest()
    {
        MelonLoader.MelonLogger.Msg("=========================================");
        MelonLoader.MelonLogger.Msg("      RUNNING JSON FILE SAVE TEST        ");
        MelonLoader.MelonLogger.Msg("=========================================");

        // Step 1: Trigger the save by registering a brand new custom ID
        int fakeCustomID = 7777; // Use a random high number so it doesn't conflict with real items
        
        ShopAPI.RegisterItem(new CustomShopItem
        {
            Name = "Automated Test Item",
            Price = 1,
            Category = "Testing",
            TemplateType = (Il2Cpp.PlayerManager.ObjectInHand)fakeCustomID, // This triggers the JSON save!
            TemplateID = 0
        });

        // Step 2: Locate the expected file path
        string filePath = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "CommonShop_CustomIDs.json");
        MelonLoader.MelonLogger.Msg($"Checking path: {filePath}");

        // Step 3: Verify the file exists
        if (!System.IO.File.Exists(filePath))
        {
            MelonLoader.MelonLogger.Error("[TEST FAIL] The JSON file was NOT created on the disk!");
            return;
        }

        // Step 4: Read the file and verify the contents
        try
        {
            string jsonContent = System.IO.File.ReadAllText(filePath);
            
            // Check if our specific fake ID and Item Name made it into the file
            if (jsonContent.Contains("\"7777\"") && jsonContent.Contains("Automated Test Item"))
            {
                MelonLoader.MelonLogger.Msg("[TEST PASS] The file exists AND the data was written perfectly!");
                MelonLoader.MelonLogger.Msg($"\n--- LIVE FILE CONTENTS ---\n{jsonContent}\n--------------------------");
            }
            else
            {
                MelonLoader.MelonLogger.Error("[TEST FAIL] The file exists, but our test data is missing!");
                MelonLoader.MelonLogger.Error($"\n--- LIVE FILE CONTENTS ---\n{jsonContent}\n--------------------------");
            }
        }
        catch (System.Exception ex)
        {
            MelonLoader.MelonLogger.Error($"[TEST FAIL] Could not read the file due to an error: {ex.Message}");
        }
        
        MelonLoader.MelonLogger.Msg("=========================================");
    }
}