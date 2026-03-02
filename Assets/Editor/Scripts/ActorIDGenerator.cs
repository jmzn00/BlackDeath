#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class ActorIDGenerator
{
    [MenuItem("Tools/Generate Actor IDs")]
    public static void GenerateIDs()
    {
        // Find all MonoBehaviours in the scene (including inactive objects)
        MonoBehaviour[] allBehaviours = GameObject.FindObjectsByType<MonoBehaviour>(
            FindObjectsSortMode.None);

        int count = 0;
        foreach (var mb in allBehaviours)
        {
            // Check if the component implements IActor
            if (mb is IActor actor)
            {
                SerializedObject so = new SerializedObject(mb);
                var idProp = so.FindProperty("m_actorID");

                if (idProp != null && string.IsNullOrEmpty(idProp.stringValue))
                {
                    idProp.stringValue = Guid.NewGuid().ToString();
                    so.ApplyModifiedProperties();
                    Debug.Log($"Generated ActorID for {mb.gameObject.name}: {idProp.stringValue}");
                    count++;
                }
            }
        }

        if (count == 0)
            Debug.Log("No actors without IDs found in the scene.");
        else
            Debug.Log($"Generated {count} Actor IDs in the scene.");
    }
}
#endif