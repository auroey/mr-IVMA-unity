using System;
using UnityEngine;

public class BootstrapInstaller : MonoBehaviour
{
    [Header("System Prefabs")]
    [SerializeField] private GameObject mrtkXrRigPrefab;
    [SerializeField] private GameObject eventSystemPrefab;

    private const string DefaultMrtkXrRigPrefabPath = "Assets/_Project/Prefabs/Tools/MRTK XR Rig.prefab";
    private const string DefaultEventSystemPrefabPath = "Assets/_Project/Prefabs/Tools/EventSystem.prefab";

    private void Reset()
    {
        AutoAssignDefaultsInEditor();
    }

    private void OnValidate()
    {
        AutoAssignDefaultsInEditor();
    }

    private void Awake()
    {
        if (!HasEventSystem() && eventSystemPrefab != null)
        {
            Instantiate(eventSystemPrefab);
        }

        if (GameObject.Find("MRTK XR Rig") == null && mrtkXrRigPrefab != null)
        {
            Instantiate(mrtkXrRigPrefab);
        }
    }

    private void AutoAssignDefaultsInEditor()
    {
#if UNITY_EDITOR
        if (mrtkXrRigPrefab == null)
        {
            mrtkXrRigPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultMrtkXrRigPrefabPath);
        }

        if (eventSystemPrefab == null)
        {
            eventSystemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultEventSystemPrefabPath);
        }
#endif
    }

    private static bool HasEventSystem()
    {
        // EventSystem is part of Unity UI; this should always be present.
        var type = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
        if (type == null) type = Type.GetType("UnityEngine.EventSystems.EventSystem");
        return type != null && FindFirstObjectOfTypeByType(type) != null;
    }

    private static UnityEngine.Object FindFirstObjectOfTypeByType(Type type)
    {
        var method = typeof(UnityEngine.Object).GetMethod(
            "FindFirstObjectByType",
            new[] { typeof(Type) }
        );
        if (method != null)
            return (UnityEngine.Object)method.Invoke(null, new object[] { type });

        // Fallback for older Unity: FindObjectOfType(Type)
        return UnityEngine.Object.FindObjectOfType(type);
    }
}

