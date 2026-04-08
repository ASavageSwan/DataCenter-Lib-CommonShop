# DataCenter-CommonShop

A mod library for the game **Data Center** that lets developers add custom items to the in-game shop.

---

## ✨ Features

- **Add custom shop items** — Register items with a name, price, template type, and background color. The library handles spawning them into the shop UI automatically.
- **Custom categories & subcategories** — Organise your items into new shop sections that appear alongside the vanilla categories.
- **UI customisation hook (`OnUIReady`)** — Get direct access to the card's `GameObject` just before it renders, so you can add labels, clone buttons, or change layout however you like.
- **Purchase callback (`OnBuy`)** — Run any logic you need when a player buys your item. The base game handles cart logic and item stacking automatically.
- **Duplicate detection** — The library validates new items against both registered custom items and existing game items by name and ID, skipping conflicts and logging a warning.
- **Persistent custom ID tracking** — Any new enum-range IDs your mod introduces are saved to `UserData/CommonShop_CustomIDs.json` and reloaded between sessions.

---

## 🛠 Requirements

- **Data Center** (Steam version)
- **MelonLoader (Il2Cpp)** — v0.7.0 or higher

---

## 🚀 Installation

1. **Install MelonLoader**
   - Download the [MelonLoader Installer](https://github.com/LavaGang/MelonLoader/releases).
   - Run it, select your `Data Center.exe`, set game type to **Il2Cpp**, and click **Install**.

2. **Run the game once**
   - Launch the game and let MelonLoader generate the Il2Cpp assemblies, then close it at the main menu.

3. **Install the library**
   - Download `DataCenter-CommonShop.dll` from the [Releases](https://github.com/ASavageSwan/DataCenter-Lib-CommonShop/releases) page.
   - Place it in the `UserLibs` folder inside your game directory.

4. **Verify**
   - Launch the game. The MelonLoader console should show:
     ```
     Melon Assembly loaded: .\UserLibs\DataCenter-CommonShop.dll
     ```

---

## 🎮 How to Use

### 1. Reference the library

Add `DataCenter-CommonShop.dll` as a reference in your mod project.

### 2. Initialise the library

Call `ShopAPI.Initialize` once when your mod loads, passing in your mod's Harmony instance:

```csharp
public override void OnInitializeMelon()
{
    ShopAPI.Initialize(this.HarmonyInstance);
}
```

### 3. Register items

Call `ShopAPI.RegisterItem` with a `CustomShopItem` object:

```csharp
ShopAPI.RegisterItem(new CustomShopItem
{
    Name            = "Test Bulk Box (99x)",
    Price           = 999,
    TemplateType    = PlayerManager.ObjectInHand.SFPBox,
    TemplateID      = 3,
    BackgroundColor = new Color(0.2f, 0.5f, 1.0f),
    OnBuy = () =>
    {
        MelonLogger.Msg("Item purchased!");
    }
});
```

### CustomShopItem properties

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Display name shown on the shop card. Must be unique. |
| `Price` | `int` | Cost of the item in-game. |
| `TemplateType` | `PlayerManager.ObjectInHand` | The base game object type this item clones. |
| `TemplateID` | `int` | The variant ID of the template object. |
| `ResultItemID` | `int` | *(Optional)* Override the spawned item's ID. |
| `BackgroundColor` | `Color` | *(Optional)* Tint applied to the shop card background. |
| `Category` | `string` | *(Optional)* Creates / adds to a named top-level shop section. |
| `SubCategory` | `string` | *(Optional)* Creates / adds to a sub-section within a category. |
| `OnBuy` | `Action` | *(Optional)* Callback fired when the player purchases the item. |
| `OnUIReady` | `Action<GameObject>` | *(Optional)* Callback fired just before the card GameObject appears, giving you full access to customise the UI. |

### Custom categories

Set `Category` and/or `SubCategory` to automatically create new sections in the shop:

```csharp
ShopAPI.RegisterItem(new CustomShopItem
{
    Category    = "Bulk Shipments",
    SubCategory = "SFP Modules",
    Name        = "32x SFP+ 10Gbps",
    Price       = 1620,
    TemplateType = PlayerManager.ObjectInHand.SFPBox,
    TemplateID  = 0
});
```

### UI customisation with `OnUIReady`

Use `OnUIReady` to modify the card's `GameObject` directly before it is shown:

```csharp
ShopAPI.RegisterItem(new CustomShopItem
{
    Name         = "Advanced Server",
    Price        = 5000,
    TemplateType = PlayerManager.ObjectInHand.Server1U,
    TemplateID   = 0,
    OnUIReady    = (GameObject cardObj) =>
    {
        var existingText = cardObj.GetComponentInChildren<Il2CppTMPro.TextMeshProUGUI>();
        var newLabel     = UnityEngine.Object.Instantiate(existingText.gameObject, cardObj.transform);

        newLabel.transform.localPosition = new Vector3(0, -60, 0);
        var tmp   = newLabel.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
        tmp.text  = "Requires 200W Power!";
        tmp.color = UnityEngine.Color.red;
        tmp.fontSize = 12f;
    }
});
```

See [`TestShopUsage/Main.cs`](https://github.com/ASavageSwan/DataCenter-Lib-CommonShop/blob/main/TestShopUsage/Main.cs) for a full set of working examples.

---

## 📂 Technical Details

- **Custom ID file:** `<GameFolder>/UserData/CommonShop_CustomIDs.json`
  Stores any non-vanilla enum IDs introduced by registered items. Delete individual entries to unregister them — takes effect on the next game launch.
- Duplicate detection runs on both item `Name` and `TemplateType`/`TemplateID` pairs, skipping any item that would conflict with an already-registered or vanilla item.

---

## 🏗 Building from Source

1. Clone the repo.
2. Copy `LocalConfig.props.examples` → `LocalConfig.props` and `LocalConfigLib.props.exmaples` → `LocalConfigLib.props`.
3. Edit both files to point at your game directory and MelonLoader assemblies.
4. Build the solution in Visual Studio.

---

## 📜 License

[MIT](LICENSE) © 2026 ASavageSwan

---

## 💙 Credits

- Powered by [MelonLoader](https://github.com/LavaGang/MelonLoader) and [HarmonyLib](https://github.com/pardeike/Harmony).
