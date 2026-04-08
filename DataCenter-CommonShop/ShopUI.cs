using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace DataCenter_CommonShop;

public class ShopUI
{
    internal static void FixGridHeight(Transform gridContainer)
    {
        var grid = gridContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        var rt = gridContainer.GetComponent<RectTransform>();
        var le = gridContainer.GetComponent<UnityEngine.UI.LayoutElement>();
        if (le == null) le = gridContainer.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();

        if (grid != null)
        {
            int leftPad = grid.padding.left;
            int rightPad = grid.padding.right;
            grid.padding = new UnityEngine.RectOffset { left = leftPad, right = rightPad, top = 10, bottom = 20 };
            if (grid.spacing.y > 50f) grid.spacing = new Vector2(grid.spacing.x, 15f);
        }

        int activeCards = 0;
        for (int i = 0; i < gridContainer.childCount; i++)
            if (gridContainer.GetChild(i).gameObject.activeSelf) activeCards++;

        if (activeCards == 0)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
            le.minHeight = 0;
            le.preferredHeight = 0;
            le.flexibleHeight = 0;
            return;
        }

        int cols;
        if (grid != null && grid.constraint == UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount)
        {
            cols = grid.constraintCount;
        }
        else if (grid != null)
        {
            // After ForceRebuildLayoutImmediate, rt.rect.width is the real container width.
            // Use it to work out how many cells actually fit — avoids hardcoding 4 columns.
            float usableWidth = rt.rect.width - grid.padding.left - grid.padding.right;
            cols = Mathf.Max(1, Mathf.FloorToInt((usableWidth + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x)));
        }
        else cols = 4;

        int rows = Mathf.CeilToInt((float)activeCards / cols);
        float height = grid.padding.top + grid.padding.bottom
                                        + (rows * grid.cellSize.y)
                                        + (Mathf.Max(0, rows - 1) * grid.spacing.y);

        MelonLogger.Msg($"[ShopLib] FixGridHeight '{gridContainer.name}': width={rt.rect.width} cols={cols} cards={activeCards} rows={rows} height={height}");

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleHeight = 0;
    }
    
    internal static Transform EnsureCategoryContainer(ComputerShop shop, string mainCat, string subCat)
    {
        Transform parent = shop.shopItemParent.transform;

        // Prevent Unity from stretching children to fill all available space (causes massive gaps)
        var vlg = parent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vlg != null) vlg.childForceExpandHeight = false;

        string gridName = string.IsNullOrEmpty(subCat) ? $"Grid_{mainCat}" : $"Grid_{mainCat}_{subCat}";
        Transform existing = parent.Find(gridName);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
        }

        Transform hlTemplate = parent.Find("HL Mods");
        bool safeToInsertBefore = (hlTemplate != null); 
        if (hlTemplate == null) hlTemplate = parent.GetComponentsInChildren<UnityEngine.UI.GridLayoutGroup>(true).FirstOrDefault()?.transform;
        if (hlTemplate == null) return parent;

        int insertIdx = -1; 
        if (safeToInsertBefore)
        {
            int hlIdx = hlTemplate.GetSiblingIndex();
            insertIdx = hlIdx > 0 ? hlIdx - 1 : 0;
        }

        Transform textTemplate = null;
        float defaultFontSize = 32f; 
        foreach (var textItem in parent.GetComponentsInChildren<Il2CppTMPro.TextMeshProUGUI>(true))
        {
            if (textItem.transform.parent == parent)
            {
                textTemplate = textItem.transform;
                defaultFontSize = textItem.fontSize;
                break;
            }
        }

        string mainLabelName = $"Label_Main_{mainCat}";
        if (parent.Find(mainLabelName) == null && textTemplate != null)
        {
            GameObject mainLabel = UnityEngine.Object.Instantiate(textTemplate.gameObject, parent);
            mainLabel.name = mainLabelName;
            mainLabel.SetActive(true);
            
            // Delete padlocks/icons
            for (int i = mainLabel.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(mainLabel.transform.GetChild(i).gameObject);
            
            // Destroy any leftover LayoutElements from previous tests so it acts natively
            var le = mainLabel.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le != null) UnityEngine.Object.DestroyImmediate(le);
            
            var tmp = mainLabel.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = mainCat;
                tmp.fontSize = defaultFontSize;
                // Reset margin: the cloned template has left margin for its icon, which we deleted.
                // Labels without icons (like vanilla "SFPs") use margin=(0,0,0,0).
                tmp.margin = new Vector4(0, 0, 0, 0);
            }

            if (insertIdx != -1) mainLabel.transform.SetSiblingIndex(insertIdx++);
            else mainLabel.transform.SetAsLastSibling();
        }

        string subLabelName = $"Label_Sub_{mainCat}_{subCat}";
        if (!string.IsNullOrEmpty(subCat) && parent.Find(subLabelName) == null && textTemplate != null)
        {
            GameObject subLabel = UnityEngine.Object.Instantiate(textTemplate.gameObject, parent);
            subLabel.name = subLabelName;
            subLabel.SetActive(true);

            for (int i = subLabel.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(subLabel.transform.GetChild(i).gameObject);

            var le = subLabel.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le != null) UnityEngine.Object.DestroyImmediate(le);

            var tmp = subLabel.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = subCat;
                tmp.fontSize = defaultFontSize * 0.75f;
                tmp.margin = new Vector4(0, 0, 0, 0);
            }
            
            if (insertIdx != -1) subLabel.transform.SetSiblingIndex(insertIdx++);
            else subLabel.transform.SetAsLastSibling();
        }

        GameObject newGrid = UnityEngine.Object.Instantiate(hlTemplate.gameObject, parent);
        newGrid.name = gridName;
        newGrid.SetActive(true);
        
        var gridLe = newGrid.GetComponent<UnityEngine.UI.LayoutElement>();
        if (gridLe == null) gridLe = newGrid.AddComponent<UnityEngine.UI.LayoutElement>();
        gridLe.flexibleHeight = 0;

        if (insertIdx != -1) newGrid.transform.SetSiblingIndex(insertIdx);
        else newGrid.transform.SetAsLastSibling();

        for (int i = newGrid.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(newGrid.transform.GetChild(i).gameObject);

        return newGrid.transform;
    }
    
    internal static void UpdateLayoutHeight(ComputerShop shop)
    {
        var sr = shop.shopItemParent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (sr == null || sr.content == null) return;

        // THE GAP ASSASSIN: Force Unity to snap the UI together BEFORE we calculate height!
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);

        float totalHeight = 0f;
        var layout = shop.shopItemParent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        int activeChildren = 0;

        for (int i = 0; i < shop.shopItemParent.transform.childCount; i++)
        {
            var child = shop.shopItemParent.transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;

            var le = child.GetComponent<UnityEngine.UI.LayoutElement>();
            var rt = child.GetComponent<RectTransform>();

            // Measure true height
            float childHeight = rt.rect.height;
            if (le != null && le.preferredHeight > 0) childHeight = le.preferredHeight;
            
            if (childHeight > 0)
            {
                totalHeight += childHeight;
                activeChildren++;
            }
        }

        if (layout != null)
        {
            totalHeight += layout.padding.top + layout.padding.bottom;
            if (activeChildren > 1) totalHeight += (activeChildren - 1) * layout.spacing;
        }

        totalHeight += 50f; // Give a nice bumper at the very bottom

        sr.content.sizeDelta = new Vector2(sr.content.sizeDelta.x, totalHeight);
        Canvas.ForceUpdateCanvases();
    }
    
}