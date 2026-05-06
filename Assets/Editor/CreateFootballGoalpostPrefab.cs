#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a simple American football goalpost prefab.
/// Usage: Put this file in Assets/Editor, then run:
/// Tools > Create Football Goalpost Prefab
/// </summary>
public class CreateFootballGoalpostPrefab : EditorWindow
{
    [MenuItem("Tools/Create Football Goalpost Prefab")]
    public static void CreatePrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject root = new GameObject("Football_Goalpost_Prefab");

        // Materials
        Material yellow = new Material(Shader.Find("Standard"));
        yellow.name = "Goalpost_Yellow";
        yellow.color = new Color(1.0f, 0.84f, 0.05f);

        Material black = new Material(Shader.Find("Standard"));
        black.name = "Goalpost_Base_Black";
        black.color = new Color(0.05f, 0.05f, 0.05f);

        AssetDatabase.CreateAsset(yellow, "Assets/Prefabs/Goalpost_Yellow.mat");
        AssetDatabase.CreateAsset(black, "Assets/Prefabs/Goalpost_Base_Black.mat");

        // Dimensions: Unity units, roughly meters
        float postRadius = 0.08f;
        float baseRadius = 0.25f;

        // Single support post
        CreateCylinder(root.transform, "Support_Post", new Vector3(0, 1.75f, 0), new Vector3(postRadius, 1.75f, postRadius), yellow);

        // Curved-looking neck simplified as forward arm
        GameObject arm = CreateCylinder(root.transform, "Forward_Arm", new Vector3(0, 3.45f, 0.65f), new Vector3(postRadius, 0.65f, postRadius), yellow);
        arm.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Crossbar
        GameObject crossbar = CreateCylinder(root.transform, "Crossbar", new Vector3(0, 3.45f, 1.3f), new Vector3(postRadius, 2.2f, postRadius), yellow);
        crossbar.transform.rotation = Quaternion.Euler(0, 0, 90);

        // Uprights
        CreateCylinder(root.transform, "Left_Upright", new Vector3(-2.2f, 5.0f, 1.3f), new Vector3(postRadius, 1.55f, postRadius), yellow);
        CreateCylinder(root.transform, "Right_Upright", new Vector3(2.2f, 5.0f, 1.3f), new Vector3(postRadius, 1.55f, postRadius), yellow);

        // Base pad
        CreateCylinder(root.transform, "Base_Pad", new Vector3(0, 0.35f, 0), new Vector3(baseRadius, 0.35f, baseRadius), black);

        // Invisible collider around scoring area, optional
        GameObject trigger = new GameObject("Goal_Area_Trigger");
        trigger.transform.parent = root.transform;
        trigger.transform.localPosition = new Vector3(0, 4.2f, 1.3f);
        BoxCollider box = trigger.AddComponent<BoxCollider>();
        box.size = new Vector3(4.2f, 2.4f, 0.25f);
        box.isTrigger = true;

        string prefabPath = "Assets/Prefabs/Football_Goalpost_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created prefab at: " + prefabPath);
    }

    private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }
}
#endif