using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HirearchyIcons
{
    static HirearchyIcons() 
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHirearchyGUI;
    }

    // hirearchy Color / Icon for gameobjects with specified component

    // ICONS https://github.com/rythwh/unity-editor-icons
    static Dictionary<Type, (Color color, string icon)> typeSettings = new()
    {
        { typeof(Actor_Player), (new Color(0f, 0f, 1f, 0.2f), "Avatar Icon") },
        { typeof(AiActor), (new Color(0f, 1f, 0f, 0.2f), "Avatar Icon") },
        { typeof(CombatArea), (new Color(1f, 0f, 0f, 0.2f), "d_AimConstraint Icon") }

    };
    static void OnHirearchyGUI(int instanceID, Rect selectionRect) 
    {
        GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;
        if (obj == null) return;

        Rect iconRect = new Rect(selectionRect);
        iconRect.x = iconRect.xMax - 18;
        iconRect.width = 16;

        foreach (var kvp in typeSettings)
        {
            Type type = kvp.Key;
            Color color = kvp.Value.color;
            string iconName = kvp.Value.icon;

            if (obj.GetComponent(type) != null)
            {
                EditorGUI.DrawRect(selectionRect, color);

                GUI.Label(iconRect, EditorGUIUtility.IconContent(iconName));

                break;
            }
        }

    }
}
