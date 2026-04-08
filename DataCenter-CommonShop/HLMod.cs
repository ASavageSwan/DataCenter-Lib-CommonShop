using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace DataCenter_CommonShop;

public class HLMod
{
    private static float _originalContentHeight = -1f;
    
    internal static void UpdateLayoutHeight(ComputerShop shop, Transform container)
    {
        ScrollRect sr = shop.shopItemParent.GetComponentInParent<ScrollRect>();
        if (sr?.content == null) return;

        RectTransform contentRT = sr.content;
        if (_originalContentHeight < 0f) _originalContentHeight = contentRT.sizeDelta.y;

        var csf = contentRT.GetComponent<ContentSizeFitter>();
        if (csf != null) csf.enabled = false;
        
        float hlNeededHeight = 0f;
        var grid = container.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            int rows = Mathf.CeilToInt((float)container.childCount / 4f);
            hlNeededHeight = rows * (grid.cellSize.y + grid.spacing.y) + grid.padding.top + grid.padding.bottom;
        }
        else
        {
            hlNeededHeight = container.childCount * 160f;
        }

        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, _originalContentHeight + hlNeededHeight + 50f);
    }
    
    internal static Transform EnsureContainer(ComputerShop shop)
    {
        Transform hlMods = FindChildByName(shop.shopItemParent.transform, "HL Mods");
        return hlMods ?? shop.shopItemParent.transform;
    }
    
    private static Transform FindChildByName(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == name) return child;
            Transform found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}