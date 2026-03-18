using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IVMASceneScaffolder
{
    private const string ScenesRoot = "Assets/_Project/Scenes";
    private const string PrefabsRoot = "Assets/_Project/Prefabs";
    private const string ScriptsRoot = "Assets/_Project/Scripts";

    [MenuItem("Tools/IVMA/Scaffold Scenes")]
    public static void Scaffold()
    {
        EnsureFolder("Assets", "_Project");
        EnsureFolder("Assets/_Project", "Scenes");
        EnsureFolder("Assets/_Project", "Prefabs");
        EnsureFolder("Assets/_Project", "Scripts");

        // Scripts organization (keep existing folders if you have them; scaffold the recommended map)
        EnsureFolder(ScriptsRoot, "Backend");
        EnsureFolder(ScriptsRoot, "UI");
        EnsureFolder(ScriptsRoot, "Movement");
        EnsureFolder(ScriptsRoot, "Selection");
        EnsureFolder(ScriptsRoot, "EyeTracking");
        EnsureFolder(ScriptsRoot, "CrossSections");
        EnsureFolder(ScriptsRoot, "Lexicon");
        EnsureFolder(ScriptsRoot, "Pencil");
        EnsureFolder(ScriptsRoot, "Notes");
        EnsureFolder(ScriptsRoot, "Common");
        EnsureFolder($"{ScriptsRoot}/Common", "Events");

        EnsureFolder(PrefabsRoot, "Anatomy");
        EnsureFolder(PrefabsRoot, "UI");
        EnsureFolder(PrefabsRoot, "Tools");
        EnsureFolder(ScenesRoot, "90_Tests");
        EnsureFolder(ScenesRoot, "99_Demo");

        // Prefab templates (keep scenes clean: place prefabs, not loose objects)
        CreatePrefabIfMissing($"{PrefabsRoot}/Anatomy/AnatomyRoot.prefab", root =>
        {
            CreateChild(root.transform, "Models");
            CreateChild(root.transform, "Colliders");
            CreateChild(root.transform, "Anchors");
        });

        CreatePrefabIfMissing($"{PrefabsRoot}/Anatomy/System_Skeletal.prefab", root =>
        {
            CreateChild(root.transform, "Bones");
            CreateChild(root.transform, "Joints");
        });

        CreatePrefabIfMissing($"{PrefabsRoot}/UI/InfoPanel.prefab", root =>
        {
            CreateChild(root.transform, "Canvas (placeholder)");
            CreateChild(root.transform, "Title");
            CreateChild(root.transform, "Body");
        });

        CreatePrefabIfMissing($"{PrefabsRoot}/UI/HandMenu.prefab", root =>
        {
            CreateChild(root.transform, "MenuRoot (placeholder)");
        });

        CreatePrefabIfMissing($"{PrefabsRoot}/Tools/CrossSectionRig.prefab", root =>
        {
            CreateChild(root.transform, "CutPlane");
            CreateChild(root.transform, "Gizmo");
        });

        CreatePrefabIfMissing($"{PrefabsRoot}/Tools/NotesRig.prefab", root =>
        {
            CreateChild(root.transform, "NotesRoot");
        });

        CreateSceneIfMissing($"{ScenesRoot}/00_Bootstrap.unity", scene =>
        {
            var systems = new GameObject("_SYSTEMS");
            CreateChild(systems.transform, "MRTK XR Rig (placeholder)");
            CreateChild(systems.transform, "SceneLoader (placeholder)");
        });

        CreateSceneIfMissing($"{ScenesRoot}/10_Anatomy_Main.unity", scene =>
        {
            // Root groups (recommended: stable 5 roots)
            var systems = new GameObject("_SYSTEMS");
            var env = new GameObject("_ENV");
            var content = new GameObject("_CONTENT");
            var ui = new GameObject("_UI");
            var managers = new GameObject("_MANAGERS");

            // _SYSTEMS
            CreateChild(systems.transform, "MRTK XR Rig (or XR Origin)");
            CreateChild(systems.transform, "MRTKInputSimulator (Editor only)");
            CreateChild(systems.transform, "XR Interaction Manager");
            CreateChild(systems.transform, "EventSystem");

            // _ENV
            var dirLight = CreateChild(env.transform, "Directional Light");
            var light = dirLight.AddComponent<Light>();
            light.type = LightType.Directional;
            CreateChild(env.transform, "Reflection Probe (optional)").AddComponent<ReflectionProbe>();
            CreateChild(env.transform, "Volume (URP, optional)");

            // _CONTENT
            CreateChild(content.transform, "AnatomyRoot (prefab)");
            CreateChild(content.transform, "AnatomySystems (optional)");

            // _UI
            CreateChild(ui.transform, "HandMenu / NearMenu (MRTK3 prefab)");
            CreateChild(ui.transform, "InfoPanel (Lexicon/Description)");
            CreateChild(ui.transform, "Dialogs (keyboard, dialogs, etc.)");

            // _MANAGERS
            CreateChild(managers.transform, "SceneController");
            CreateChild(managers.transform, "SelectionManager");
            CreateChild(managers.transform, "LexiconController");
            CreateChild(managers.transform, "CrossSectionController");
            CreateChild(managers.transform, "NotesManager");
            CreateChild(managers.transform, "BackendManager (optional)");
        });

        CreateSceneIfMissing($"{ScenesRoot}/20_Anatomy_Sandbox.unity", scene =>
        {
            new GameObject("SandboxRoot");
        });

        CreateSceneIfMissing($"{ScenesRoot}/90_Tests/InteractionTest.unity", scene =>
        {
            new GameObject("InteractionTestRoot");
        });

        CreateSceneIfMissing($"{ScenesRoot}/90_Tests/PerformanceTest.unity", scene =>
        {
            new GameObject("PerformanceTestRoot");
        });

        CreateSceneIfMissing($"{ScenesRoot}/99_Demo/Demo.unity", scene =>
        {
            new GameObject("DemoRoot");
        });

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("IVMA", "Scenes scaffolded (folders + scenes).", "OK");
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        var path = $"{parent}/{folderName}";
        if (AssetDatabase.IsValidFolder(path)) return;

        if (!AssetDatabase.IsValidFolder(parent))
        {
            Directory.CreateDirectory(parent);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static void CreateSceneIfMissing(string sceneAssetPath, System.Action<Scene> populate)
    {
        if (File.Exists(sceneAssetPath)) return;

        var directory = Path.GetDirectoryName(sceneAssetPath)?.Replace("\\", "/");
        if (!string.IsNullOrWhiteSpace(directory) && !AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        populate?.Invoke(scene);

        if (!EditorSceneManager.SaveScene(scene, sceneAssetPath))
            throw new IOException($"Failed to save scene: {sceneAssetPath}");
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void CreatePrefabIfMissing(string prefabAssetPath, System.Action<GameObject> populate)
    {
        if (File.Exists(prefabAssetPath)) return;

        var directory = Path.GetDirectoryName(prefabAssetPath)?.Replace("\\", "/");
        if (!string.IsNullOrWhiteSpace(directory) && !AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        var root = new GameObject(Path.GetFileNameWithoutExtension(prefabAssetPath));
        try
        {
            populate?.Invoke(root);
            PrefabUtility.SaveAsPrefabAsset(root, prefabAssetPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}