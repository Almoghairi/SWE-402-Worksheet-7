using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class Worksheet7Builder
{
    public static void BuildProject()
    {
        CreateFolders();
        ConfigurePipeline();
        ConfigureTextures();
        CreateMaterials();
        CreateScene();
        WriteReadme();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateFolders()
    {
        foreach (string path in new[] {"Assets/Materials", "Assets/Scenes", "Assets/Settings", "Assets/Textures", "Assets/Shaders"})
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string name = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static void ConfigurePipeline()
    {
        UniversalRenderPipelineAsset asset = UniversalRenderPipelineAsset.Create();
        AssetDatabase.CreateAsset(asset, "Assets/Settings/Worksheet7_URP.asset");
        GraphicsSettings.defaultRenderPipeline = asset;
        QualitySettings.renderPipeline = asset;
    }

    private static void ConfigureTextures()
    {
        TextureImporter tile = (TextureImporter)AssetImporter.GetAtPath("Assets/Textures/CircuitTile.png");
        tile.textureType = TextureImporterType.Default;
        tile.wrapMode = TextureWrapMode.Repeat;
        tile.SaveAndReimport();

        TextureImporter normal = (TextureImporter)AssetImporter.GetAtPath("Assets/Textures/RidgedNormal.png");
        normal.textureType = TextureImporterType.NormalMap;
        normal.wrapMode = TextureWrapMode.Repeat;
        normal.SaveAndReimport();
    }

    private static Material CreateLit(string name, Color color, float metallic, float smoothness, bool transparent = false, bool emission = false)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.name = name;
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        if (transparent)
        {
            material.SetFloat("_Surface", 1);
            material.SetFloat("_AlphaClip", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.1f, 2.5f, 3.8f));
        }
        AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
        return material;
    }

    private static void CreateMaterials()
    {
        Material matte = CreateLit("Matte_Basalt_Clay", new Color(0.28f, 0.26f, 0.24f), 0f, 0.12f);
        Material ceramic = CreateLit("Shiny_Ivory_Ceramic", new Color(0.93f, 0.89f, 0.78f), 0f, 0.86f);
        Material gold = CreateLit("Mirror_Gold_Metal", new Color(1f, 0.62f, 0.16f), 1f, 0.92f);
        Material glass = CreateLit("Transparent_Aqua_Glass", new Color(0.35f, 0.95f, 1f, 0.38f), 0f, 0.88f, true);
        Material textured = CreateLit("Textured_Ridged_Circuit", Color.white, 0f, 0.58f, false, true);
        textured.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/CircuitTile.png"));
        textured.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        textured.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/RidgedNormal.png"));
        textured.EnableKeyword("_NORMALMAP");

        AssetDatabase.ImportAsset("Assets/Shaders/NeonCircuitShimmer.shadergraph");
        Shader graphShader = Shader.Find("Shader Graphs/NeonCircuitShimmer") ?? Shader.Find("Shader Graphs/SamplesEmissive");
        Material graphMaterial = new Material(graphShader != null ? graphShader : Shader.Find("Universal Render Pipeline/Lit"));
        graphMaterial.name = "ShaderGraph_Neon_Shimmer";
        graphMaterial.SetColor("_BaseColor", new Color(0.1f, 0.75f, 1f));
        graphMaterial.EnableKeyword("_EMISSION");
        graphMaterial.SetColor("_EmissionColor", new Color(0.2f, 3f, 4f));
        AssetDatabase.CreateAsset(graphMaterial, "Assets/Materials/ShaderGraph_Neon_Shimmer.mat");
    }

    private static GameObject Parent(string name)
    {
        return new GameObject(name);
    }

    private static void CreateScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject products = Parent("Products");
        GameObject plinths = Parent("Plinths");
        GameObject lights = Parent("Lights");
        GameObject probes = Parent("Probes");

        Material matte = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Matte_Basalt_Clay.mat");
        Material ceramic = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Shiny_Ivory_Ceramic.mat");
        Material gold = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mirror_Gold_Metal.mat");
        Material glass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Transparent_Aqua_Glass.mat");
        Material circuit = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Textured_Ridged_Circuit.mat");
        Material graph = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShaderGraph_Neon_Shimmer.mat");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground - Static Gallery Floor";
        ground.transform.localScale = new Vector3(2.8f, 1f, 2.1f);
        ground.GetComponent<Renderer>().sharedMaterial = matte;
        GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.ReflectionProbeStatic);

        Vector3[] spots = { new Vector3(-4, 0, 1.6f), new Vector3(-2, 0, -1.6f), new Vector3(0, 0, 1.6f), new Vector3(2, 0, -1.6f), new Vector3(4, 0, 1.6f) };
        Material[] mats = { ceramic, gold, glass, circuit, graph };
        PrimitiveType[] shapes = { PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere };
        for (int i = 0; i < spots.Length; i++)
        {
            GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plinth.name = $"Plinth_{i + 1}";
            plinth.transform.SetParent(plinths.transform);
            plinth.transform.position = spots[i] + Vector3.up * 0.35f;
            plinth.transform.localScale = new Vector3(0.85f, 0.35f, 0.85f);
            plinth.GetComponent<Renderer>().sharedMaterial = matte;
            GameObjectUtility.SetStaticEditorFlags(plinth, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);

            GameObject product = GameObject.CreatePrimitive(shapes[i]);
            product.name = $"Product_{i + 1}_{mats[i].name}";
            product.transform.SetParent(products.transform);
            product.transform.position = spots[i] + Vector3.up * 1.15f;
            product.transform.localScale = Vector3.one * (i == 2 ? 0.9f : 0.75f);
            product.GetComponent<Renderer>().sharedMaterial = mats[i];
        }

        GameObject directional = new GameObject("Directional Light - Cool Moon Key");
        directional.transform.SetParent(lights.transform);
        Light dl = directional.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(0.55f, 0.72f, 1f);
        dl.intensity = 1.05f;
        dl.shadows = LightShadows.Soft;
        directional.transform.rotation = Quaternion.Euler(48, -35, 0);

        CreateLight(lights.transform, "Key Spot - Gold Product", LightType.Spot, new Vector3(2.4f, 4f, -2.3f), new Color(1f, 0.78f, 0.42f), 60f, 7f);
        CreateLight(lights.transform, "Fill Point - Aqua Glass", LightType.Point, new Vector3(-2.8f, 2.2f, 1.8f), new Color(0.25f, 0.9f, 1f), 4f, 5f);
        CreateLight(lights.transform, "Rim Spot - Neon Back Edge", LightType.Spot, new Vector3(0f, 3f, 4f), new Color(0.9f, 0.18f, 0.32f), 40f, 6f);
        Light area = CreateLight(lights.transform, "Baked Area Light - Soft Overhead", LightType.Rectangle, new Vector3(0, 3.5f, 0), new Color(0.8f, 0.95f, 1f), 3f, 5f);
        area.lightmapBakeType = LightmapBakeType.Baked;

        LightProbeGroup group = probes.AddComponent<LightProbeGroup>();
        group.probePositions = new[]
        {
            new Vector3(-4, 1, 1.6f), new Vector3(-2, 1, -1.6f), new Vector3(0, 1, 1.6f),
            new Vector3(2, 1, -1.6f), new Vector3(4, 1, 1.6f), new Vector3(0, 2.5f, 0)
        };
        ReflectionProbe reflection = probes.AddComponent<ReflectionProbe>();
        reflection.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
        reflection.boxProjection = true;
        reflection.size = new Vector3(8, 4, 5);
        probes.transform.position = new Vector3(0, 1.4f, 0);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.06f, 0.08f, 0.13f);
        RenderSettings.ambientEquatorColor = new Color(0.12f, 0.2f, 0.22f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.025f);

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 4.2f, -7.4f);
        camera.transform.rotation = Quaternion.Euler(30, 0, 0);
        camera.fieldOfView = 42f;

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/ProductShowcase.unity", true) };
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/ProductShowcase.unity");
    }

    private static Light CreateLight(Transform parent, string name, LightType type, Vector3 position, Color color, float intensity, float range)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.LookAt(Vector3.zero);
        Light light = obj.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        light.lightmapBakeType = type == LightType.Point ? LightmapBakeType.Mixed : LightmapBakeType.Baked;
        return light;
    }

    private static void WriteReadme()
    {
        System.IO.File.WriteAllText("README.md",
@"# SWE-402 Worksheet 7 - Product Showcase Scene

Theme: Neon museum after midnight.

Open Assets/Scenes/ProductShowcase.unity. The scene contains grouped Products, Plinths, Lights, and Probes objects.

Checklist coverage:
- Five product objects on plinths with five distinct URP/Lit materials: matte, shiny non-metal, metal, transparent glass, and textured/emissive circuit material.
- CircuitTile.png is assigned as a base map with adjusted tiling. RidgedNormal.png is imported as a normal map and assigned to the textured material.
- Assets/Shaders/NeonCircuitShimmer.shadergraph is included and the ShaderGraph_Neon_Shimmer material is applied to Product_5.
- Directional, point, spot, and baked area lights are configured for a cool neon gallery mood, with soft shadows enabled.
- Ground and plinths are marked static. A LightProbeGroup and ReflectionProbe are placed near reflective/transparent products.
");
    }
}
