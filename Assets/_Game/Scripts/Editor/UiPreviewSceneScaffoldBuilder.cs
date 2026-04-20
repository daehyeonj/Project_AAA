#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UiPreviewSceneScaffoldBuilder
{
    private const string BattleScenePath = "Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity";
    private const string InventoryScenePath = "Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity";
    private const string BattleSkinPath = "Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset";
    private const string InventorySkinPath = "Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset";
    private const string BattleLayoutPath = "Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset";
    private const string InventoryLayoutPath = "Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset";
    private const string BattlePreviewDataPath = "Assets/_Game/Content/UI/Preview/Battle/BattleUiPreview_Default.asset";
    private const string InventoryPreviewDataPath = "Assets/_Game/Content/UI/Preview/Inventory/InventoryUiPreview_Default.asset";

    [MenuItem("Tools/Project AAA/UI/Create Or Update Preview Scene Scaffold")]
    public static void CreateOrUpdatePreviewSceneScaffold()
    {
        EnsureFolders();

        BattleUiSkinDefinition battleSkin = LoadOrCreateAsset<BattleUiSkinDefinition>(BattleSkinPath);
        InventoryUiSkinDefinition inventorySkin = LoadOrCreateAsset<InventoryUiSkinDefinition>(InventorySkinPath);
        BattleUiLayoutProfile battleLayout = LoadOrCreateAsset<BattleUiLayoutProfile>(BattleLayoutPath);
        InventoryUiLayoutProfile inventoryLayout = LoadOrCreateAsset<InventoryUiLayoutProfile>(InventoryLayoutPath);
        BattleUiPreviewData battlePreviewData = LoadOrCreateAsset(BattlePreviewDataPath, CreateBattlePreviewData);
        InventoryUiPreviewData inventoryPreviewData = LoadOrCreateAsset(InventoryPreviewDataPath, CreateInventoryPreviewData);

        CreateOrUpdateBattlePreviewScene(battleSkin, battleLayout, battlePreviewData);
        CreateOrUpdateInventoryPreviewScene(inventorySkin, inventoryLayout, inventoryPreviewData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[UiPreviewSceneScaffoldBuilder] Preview scenes and default assets are ready.");
    }

    public static void RunBatchmode()
    {
        CreateOrUpdatePreviewSceneScaffold();
        EditorApplication.Exit(0);
    }

    private static void CreateOrUpdateBattlePreviewScene(
        BattleUiSkinDefinition skin,
        BattleUiLayoutProfile layout,
        BattleUiPreviewData previewData)
    {
        Scene scene = OpenOrCreateEmptyScene(BattleScenePath);
        EnsureMainCamera(new Color(0.06f, 0.08f, 0.11f, 1f));

        GameObject root = FindOrCreateRoot("BattleUiPreviewRoot");
        BattleUiPreviewSceneController controller = root.GetComponent<BattleUiPreviewSceneController>();
        if (controller == null)
        {
            controller = root.AddComponent<BattleUiPreviewSceneController>();
        }

        controller.SetDependencies(skin, layout, previewData);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, BattleScenePath);
    }

    private static void CreateOrUpdateInventoryPreviewScene(
        InventoryUiSkinDefinition skin,
        InventoryUiLayoutProfile layout,
        InventoryUiPreviewData previewData)
    {
        Scene scene = OpenOrCreateEmptyScene(InventoryScenePath);
        EnsureMainCamera(new Color(0.06f, 0.08f, 0.11f, 1f));

        GameObject root = FindOrCreateRoot("InventoryUiPreviewRoot");
        InventoryUiPreviewSceneController controller = root.GetComponent<InventoryUiPreviewSceneController>();
        if (controller == null)
        {
            controller = root.AddComponent<InventoryUiPreviewSceneController>();
        }

        controller.SetDependencies(skin, layout, previewData);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, InventoryScenePath);
    }

    private static Scene OpenOrCreateEmptyScene(string scenePath)
    {
        return File.Exists(scenePath)
            ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void EnsureMainCamera(Color backgroundColor)
    {
        Camera existing = Object.FindFirstObjectByType<Camera>();
        if (existing == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            existing = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        existing.clearFlags = CameraClearFlags.SolidColor;
        existing.backgroundColor = backgroundColor;
        existing.orthographic = true;
        existing.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        return root != null ? root : new GameObject(name);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/_Game/Content");
        EnsureFolder("Assets/_Game/Content/UI");
        EnsureFolder("Assets/_Game/Content/UI/Skins");
        EnsureFolder("Assets/_Game/Content/UI/Skins/Battle");
        EnsureFolder("Assets/_Game/Content/UI/Skins/Inventory");
        EnsureFolder("Assets/_Game/Content/UI/LayoutProfiles");
        EnsureFolder("Assets/_Game/Content/UI/LayoutProfiles/Battle");
        EnsureFolder("Assets/_Game/Content/UI/LayoutProfiles/Inventory");
        EnsureFolder("Assets/_Game/Content/UI/Preview");
        EnsureFolder("Assets/_Game/Content/UI/Preview/Battle");
        EnsureFolder("Assets/_Game/Content/UI/Preview/Inventory");
        EnsureFolder("Assets/_Game/Scenes/Preview");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
            string name = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && AssetDatabase.IsValidFolder(parent))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        return asset;
    }

    private static T LoadOrCreateAsset<T>(string assetPath, System.Func<T> createAsset) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset != null)
        {
            return asset;
        }

        asset = createAsset != null ? createAsset() : ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        return asset;
    }

    private static BattleUiPreviewData CreateBattlePreviewData()
    {
        BattleUiPreviewData data = ScriptableObject.CreateInstance<BattleUiPreviewData>();
        data.name = "BattleUiPreview_Default";
        return data;
    }

    private static InventoryUiPreviewData CreateInventoryPreviewData()
    {
        InventoryUiPreviewData data = ScriptableObject.CreateInstance<InventoryUiPreviewData>();
        data.name = "InventoryUiPreview_Default";
        return data;
    }
}
#endif
