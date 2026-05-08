#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a cute, rounded, wavy banner style finish line prefab.
/// Usage: Put this file in Assets/Editor, then run:
/// Tools > Create Cute Wavy Finish Line Prefab
/// </summary>
public class CreateCuteWavyFinishLinePrefab
{
    [MenuItem("Tools/Create Cute Wavy Finish Line Prefab")]
    public static void CreatePrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject root = new GameObject("CuteWavyFinishLine_Prefab");

        // Materials
        Material whiteMat = CreateMaterial("CuteFinish_White", Color.white);
        Material redMat = CreateMaterial("CuteFinish_CoralRed", new Color(1.0f, 0.32f, 0.32f));
        Material blueMat = CreateMaterial("CuteFinish_SkyBlue", new Color(0.22f, 0.70f, 1.0f));
        Material darkMat = CreateMaterial("CuteFinish_DarkText", new Color(0.06f, 0.08f, 0.13f));
        Material ropeMat = CreateMaterial("CuteFinish_Rope", new Color(0.72f, 0.58f, 0.42f));
        Material blackMat = CreateMaterial("CuteFinish_Black", Color.black);

        // Poles
        CreateCylinder(root.transform, "Left_Rounded_Pole", new Vector3(-2.5f, 2.45f, 0), new Vector3(0.16f, 2.35f, 0.16f), whiteMat);
        CreateCylinder(root.transform, "Right_Rounded_Pole", new Vector3(2.5f, 2.45f, 0), new Vector3(0.16f, 2.35f, 0.16f), whiteMat);

        // Rounded caps
        CreateSphere(root.transform, "Left_Top_Round_Cap", new Vector3(-2.5f, 4.95f, 0), new Vector3(0.45f, 0.45f, 0.45f), redMat);
        CreateSphere(root.transform, "Right_Top_Round_Cap", new Vector3(2.5f, 4.95f, 0), new Vector3(0.45f, 0.45f, 0.45f), redMat);

        // Blue bands
        CreateCylinder(root.transform, "Left_Blue_Band", new Vector3(-2.5f, 2.0f, 0), new Vector3(0.17f, 0.12f, 0.17f), blueMat);
        CreateCylinder(root.transform, "Right_Blue_Band", new Vector3(2.5f, 2.0f, 0), new Vector3(0.17f, 0.12f, 0.17f), blueMat);

        // Rounded bases
        CreateCylinder(root.transform, "Left_Base_Red", new Vector3(-2.5f, 0.18f, 0), new Vector3(0.48f, 0.18f, 0.48f), redMat);
        CreateCylinder(root.transform, "Right_Base_Red", new Vector3(2.5f, 0.18f, 0), new Vector3(0.48f, 0.18f, 0.48f), redMat);
        CreateCylinder(root.transform, "Left_Base_White_Rim", new Vector3(-2.5f, 0.40f, 0), new Vector3(0.38f, 0.07f, 0.38f), whiteMat);
        CreateCylinder(root.transform, "Right_Base_White_Rim", new Vector3(2.5f, 0.40f, 0), new Vector3(0.38f, 0.07f, 0.38f), whiteMat);

        // Wavy hanging banner mesh
        GameObject banner = CreateWavyBanner(root.transform, "Wavy_FINISH_Banner", whiteMat);
        banner.transform.localPosition = new Vector3(0, 3.72f, -0.03f);

        // Slight dark raised text using TextMesh
        GameObject textObj = new GameObject("FINISH_Text");
        textObj.transform.parent = root.transform;
        textObj.transform.localPosition = new Vector3(0, 3.70f, -0.09f);
        textObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        textObj.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = "FINISH";
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 80;
        tm.characterSize = 1f;
        tm.color = darkMat.color;

        MeshRenderer textRenderer = textObj.GetComponent<MeshRenderer>();
        textRenderer.sharedMaterial = darkMat;

        // Ropes
        GameObject topRope = CreateCylinder(root.transform, "Top_Rope", new Vector3(0, 4.38f, -0.05f), new Vector3(0.04f, 2.45f, 0.04f), ropeMat);
        topRope.transform.localRotation = Quaternion.Euler(0, 0, 90);

        GameObject bottomRope = CreateCylinder(root.transform, "Bottom_Rope", new Vector3(0, 3.07f, -0.05f), new Vector3(0.04f, 2.45f, 0.04f), ropeMat);
        bottomRope.transform.localRotation = Quaternion.Euler(0, 0, 90);

        // Corner rings
        CreateTorusLikeRing(root.transform, "Left_Top_Ring", new Vector3(-1.95f, 4.18f, -0.08f), darkMat);
        CreateTorusLikeRing(root.transform, "Right_Top_Ring", new Vector3(1.95f, 4.18f, -0.08f), darkMat);
        CreateTorusLikeRing(root.transform, "Left_Bottom_Ring", new Vector3(-1.95f, 3.23f, -0.08f), darkMat);
        CreateTorusLikeRing(root.transform, "Right_Bottom_Ring", new Vector3(1.95f, 3.23f, -0.08f), darkMat);

        // Small checkered finish strip
        for (int i = 0; i < 12; i++)
        {
            Material mat = (i % 2 == 0) ? whiteMat : blackMat;
            CreateCube(root.transform, "Checker_Tile_" + i, new Vector3(-2.75f + i * 0.5f, 0.025f, 0.55f), new Vector3(0.5f, 0.05f, 0.55f), mat);
        }

        // Finish trigger
        GameObject trigger = new GameObject("Finish_Trigger");
        trigger.transform.parent = root.transform;
        trigger.transform.localPosition = new Vector3(0, 1.7f, 0.15f);

        BoxCollider box = trigger.AddComponent<BoxCollider>();
        box.size = new Vector3(5.3f, 3.4f, 1.2f);
        box.isTrigger = true;

        string prefabPath = "Assets/Prefabs/CuteWavyFinishLine_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        GameObject.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created cute wavy finish line prefab at: " + prefabPath);
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = "Assets/Prefabs/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.name = name;
            mat.color = color;
            mat.SetFloat("_Glossiness", 0.35f);
            AssetDatabase.CreateAsset(mat, path);
        }

        return mat;
    }

    private static GameObject CreateWavyBanner(Transform parent, string name, Material mat)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = parent;

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        int segments = 16;
        float width = 4.0f;
        float height = 1.15f;
        float wave = 0.15f;

        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float x = Mathf.Lerp(-width / 2f, width / 2f, t);
            float yOffset = Mathf.Sin(t * Mathf.PI * 2f) * wave;

            vertices[i * 2] = new Vector3(x, -height / 2f + yOffset * 0.65f, 0);
            vertices[i * 2 + 1] = new Vector3(x, height / 2f + yOffset, 0);

            uv[i * 2] = new Vector2(t, 0);
            uv[i * 2 + 1] = new Vector2(t, 1);
        }

        int tri = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            triangles[tri++] = a;
            triangles[tri++] = b;
            triangles[tri++] = c;

            triangles[tri++] = c;
            triangles[tri++] = b;
            triangles[tri++] = d;
        }

        Mesh mesh = new Mesh();
        mesh.name = "WavyBannerMesh";
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;

        return obj;
    }

    private static void CreateTorusLikeRing(Transform parent, string name, Vector3 position, Material mat)
    {
        GameObject ring = CreateCylinder(parent, name, position, new Vector3(0.14f, 0.025f, 0.14f), mat);
        ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static GameObject CreateSphere(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}
#endif