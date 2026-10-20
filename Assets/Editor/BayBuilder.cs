using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// Gera tudo do Fuga na Baía a partir de Assets/Level/baia.txt e da arte em Assets/Art:
// configuração dos sprites, tiles, fontes TMP, prefabs, as cenas MainMenu e Baia e o build WebGL.
// Pelo editor: menu "Fuga na Baía". Pelo terminal: -executeMethod BayBuilder.GenerateAllBatch.
public static class BayBuilder
{
    const string KenneyDir = "Assets/Art/Kenney";
    const string CustomDir = "Assets/Art/Custom";
    const string FontsDir = "Assets/Art/Fonts";
    const string TilesDir = "Assets/Art/Tiles";
    const string PrefabsDir = "Assets/Prefabs";
    const string AudioDir = "Assets/Audio";
    const string LayoutPath = "Assets/Level/baia.txt";
    const string TemplateScene = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";
    const string SpriteMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
    public const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    public const string GameScenePath = "Assets/Scenes/Baia.unity";
    const string WebBuildPath = "WebBuild/FugaNaBaia";

    const int UILayer = 5;
    const int ObstacleLayer = 8;
    const int IslandLayer = 9;

    // Ordem de desenho (sorting order)
    const int OrderShallow = -8, OrderWaves = -6, OrderFoam = -5, OrderIslands = -4, OrderRocks = 0, OrderWalls = 1,
        OrderPalms = 2, OrderTowers = 3, OrderTowerCannon = 4, OrderGlow = 8, OrderItems = 9, OrderShark = 11,
        OrderCannonShip = 14, OrderChaser = 15, OrderShipCannon = 16, OrderPlayer = 20, OrderShield = 21,
        OrderCannonball = 25, OrderEffects = 30, OrderCompass = 35, OrderText = 40;

    static readonly Color WaterColor = new Color32(0x9d, 0xdc, 0xf2, 0xff);
    static readonly Color Ink = new Color32(0x3b, 0x24, 0x12, 0xff);
    static readonly Color Cream = new Color32(0xff, 0xf1, 0xc9, 0xff);
    static readonly Color Gold = new Color32(0xf4, 0xc5, 0x42, 0xff);
    static readonly Color Danger = new Color32(0xc0, 0x39, 0x2b, 0xff);

    const string Charset = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
                           "áàâãéêíóôõúüçÁÀÂÃÉÊÍÓÔÕÚÜÇ–—·•…ºª×";

    static Material spriteMat;
    static TMP_FontAsset titleFont;
    static TMP_FontAsset uiFont;
    static Material titleOutline;
    static Material uiOutline;

    class NineSlice
    {
        public Tile[] tl, t, tr, l, c, r, bl, b, br;
    }

    class TileSet
    {
        public NineSlice sand, grass, shallow;
    }

    class Prefabs
    {
        public GameObject player, chaser, cannonShip, tower, shark, cannonball, chest, coin, hourglass, buoy;
        public GameObject floatingText, explosion, splash, muzzle, spawnFx, compass;
    }

    // ------------------------------------------------------------------ entradas

    [MenuItem("Fuga na Baía/Gerar tudo")]
    public static void GenerateAll()
    {
        var layout = new LevelLayout(File.ReadAllText(LayoutPath));
        List<string> errors = layout.Validate();
        if (errors.Count > 0) throw new Exception("Layout inválido:\n" + string.Join("\n", errors));

        EnsureFolder(TilesDir);
        EnsureFolder(PrefabsDir);
        EnsureLayers();
        ConfigureSprites();
        spriteMat = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
        if (spriteMat == null) throw new Exception("Material de sprite do URP não encontrado: " + SpriteMaterialPath);
        CreateFonts();
        CreateTiles();
        Prefabs prefabs = CreatePrefabs();
        BuildGameScene(layout, prefabs);
        BuildMenuScene(layout, prefabs);
        ConfigureProject();
        AssetDatabase.SaveAssets();
        Debug.Log("[BayBuilder] Cenas, prefabs e assets gerados.");
    }

    [MenuItem("Fuga na Baía/Build WebGL")]
    public static void BuildWebGL()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { MenuScenePath, GameScenePath },
            locationPathName = WebBuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Build WebGL falhou: " + report.summary.result);
        Debug.Log($"[BayBuilder] Build WebGL em {WebBuildPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
    }

    public static void GenerateAllBatch()
    {
        RunBatch(GenerateAll);
    }

    public static void BuildWebGLBatch()
    {
        RunBatch(BuildWebGL);
    }

    public static void CaptureBatch()
    {
        RunBatch(() => CaptureScreens(GetArg("-shotsDir") ?? "Temp/Shots"));
    }

    static void RunBatch(Action action)
    {
        try
        {
            action();
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }

    // ------------------------------------------------------------------ projeto

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static void EnsureLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        layers.GetArrayElementAtIndex(ObstacleLayer).stringValue = "Obstacle";
        layers.GetArrayElementAtIndex(IslandLayer).stringValue = "Island";
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static void ConfigureProject()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        PlayerSettings.productName = "Fuga na Baía";
        PlayerSettings.companyName = "Enzo Rios Perego";
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
    }

    // ------------------------------------------------------------------ sprites, tiles e fontes

    static readonly Dictionary<string, Vector4> SliceBorders = new Dictionary<string, Vector4>
    {
        { "ui_panel", new Vector4(44, 44, 44, 44) },
        { "ui_button", new Vector4(32, 32, 32, 32) },
        { "ui_plaque", new Vector4(30, 30, 30, 30) },
        { "ui_bar_bg", new Vector4(20, 19, 20, 19) },
        { "ui_bar_fill", new Vector4(20, 19, 20, 19) },
    };

    static void ConfigureSprites()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { KenneyDir, CustomDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = path.StartsWith(CustomDir) ? 128 : 64;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);

                string name = Path.GetFileNameWithoutExtension(path);
                importer.spriteBorder = SliceBorders.TryGetValue(name, out Vector4 border) ? border : Vector4.zero;
                importer.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }

    static Sprite KenneyTile(int n)
    {
        return LoadSprite($"{KenneyDir}/Tiles/tile_{n:00}.png");
    }

    static Sprite Kenney(string relative)
    {
        return LoadSprite($"{KenneyDir}/{relative}.png");
    }

    static Sprite Custom(string name)
    {
        return LoadSprite($"{CustomDir}/{name}.png");
    }

    static Sprite LoadSprite(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) throw new Exception("Sprite não encontrado: " + path);
        return s;
    }

    static Tile MakeTile(int n, Tile.ColliderType collider)
    {
        string path = $"{TilesDir}/tile_{n:00}.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }
        tile.sprite = KenneyTile(n);
        tile.colliderType = collider;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    static Tile[] Tiles(Tile.ColliderType collider, params int[] numbers)
    {
        var result = new Tile[numbers.Length];
        for (int i = 0; i < numbers.Length; i++) result[i] = MakeTile(numbers[i], collider);
        return result;
    }

    static TileSet CreateTiles()
    {
        var grid = Tile.ColliderType.Grid;
        var none = Tile.ColliderType.None;
        return new TileSet
        {
            sand = new NineSlice
            {
                tl = Tiles(grid, 1), t = Tiles(grid, 2), tr = Tiles(grid, 3),
                l = Tiles(grid, 17), c = Tiles(grid, 68, 69), r = Tiles(grid, 19),
                bl = Tiles(grid, 33), b = Tiles(grid, 34), br = Tiles(grid, 35)
            },
            grass = new NineSlice
            {
                tl = Tiles(grid, 6), t = Tiles(grid, 7, 8), tr = Tiles(grid, 9),
                l = Tiles(grid, 22, 38), c = Tiles(grid, 23, 39, 40, 24), r = Tiles(grid, 25, 41),
                bl = Tiles(grid, 54), b = Tiles(grid, 55, 56), br = Tiles(grid, 57)
            },
            shallow = new NineSlice
            {
                tl = Tiles(none, 10), t = Tiles(none, 11), tr = Tiles(none, 12),
                l = Tiles(none, 26), c = Tiles(none, 27), r = Tiles(none, 28),
                bl = Tiles(none, 42), b = Tiles(none, 43), br = Tiles(none, 44)
            }
        };
    }

    static void CreateFonts()
    {
        titleFont = MakeFontAsset($"{FontsDir}/PirataOne-Regular.ttf", $"{FontsDir}/PirataOne SDF.asset");
        uiFont = MakeFontAsset($"{FontsDir}/LilitaOne-Regular.ttf", $"{FontsDir}/LilitaOne SDF.asset");
        titleOutline = MakeFontMaterial(titleFont, "PirataOne SDF Contorno", 0.22f);
        uiOutline = MakeFontMaterial(uiFont, "LilitaOne SDF Contorno", 0.2f);
    }

    static TMP_FontAsset MakeFontAsset(string ttfPath, string assetPath)
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null) return existing;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font == null) throw new Exception("Fonte não encontrada: " + ttfPath);

        TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        fa.name = Path.GetFileNameWithoutExtension(assetPath);
        if (!fa.TryAddCharacters(Charset, out string missing))
            Debug.LogWarning($"[BayBuilder] {fa.name}: caracteres faltando: {missing}");

        AssetDatabase.CreateAsset(fa, assetPath);
        Texture2D atlas = fa.atlasTexture;
        atlas.name = fa.name + " Atlas";
        AssetDatabase.AddObjectToAsset(atlas, fa);
        fa.material.name = fa.name + " Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        fa.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        return fa;
    }

    static Material MakeFontMaterial(TMP_FontAsset fa, string name, float outline)
    {
        string path = $"{FontsDir}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(fa.material) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetFloat("_FaceDilate", outline * 0.5f);
        mat.SetFloat("_OutlineWidth", outline);
        mat.SetColor("_OutlineColor", Ink);
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.45f));
        mat.SetFloat("_UnderlayOffsetX", 0.25f);
        mat.SetFloat("_UnderlayOffsetY", -0.5f);
        mat.SetFloat("_UnderlaySoftness", 0.2f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ------------------------------------------------------------------ prefabs

    static GameObject NewObject(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    static SpriteRenderer AddSprite(GameObject go, Sprite sprite, int order)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.sharedMaterial = spriteMat;
        return sr;
    }

    static SpriteRenderer SpriteChild(Transform parent, string name, Sprite sprite, int order, Vector3 localPos = default, float scale = 1f)
    {
        GameObject go = NewObject(name, parent);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * scale;
        return AddSprite(go, sprite, order);
    }

    static GameObject SavePrefab(GameObject go)
    {
        string path = $"{PrefabsDir}/{go.name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static Rigidbody2D AddBody(GameObject go, RigidbodyType2D type)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = type;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        return rb;
    }

    static WakeEmitter AddWake(GameObject go, Transform orientation, Vector2 offset, float startScale, float endScale)
    {
        var wake = go.AddComponent<WakeEmitter>();
        wake.foamSprite = Custom("foam");
        wake.material = spriteMat;
        wake.orientation = orientation;
        wake.localOffset = offset;
        wake.startScale = startScale;
        wake.endScale = endScale;
        wake.sortingOrder = OrderFoam;
        return wake;
    }

    static GameObject Effect(string name, Sprite sprite, Sprite[] frames, float duration, float startScale, float endScale, Color color, int order)
    {
        GameObject go = NewObject(name);
        SpriteRenderer sr = AddSprite(go, sprite, order);
        sr.color = color;
        var fx = go.AddComponent<SpriteFlipbook>();
        fx.target = sr;
        fx.frames = frames;
        fx.duration = duration;
        fx.startScale = startScale;
        fx.endScale = endScale;
        return SavePrefab(go);
    }

    static GameObject PickupPrefab(string name, PickupType type, Sprite sprite, float radius, Color? glow)
    {
        GameObject go = NewObject(name);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;
        if (glow.HasValue)
        {
            SpriteRenderer g = SpriteChild(go.transform, "Brilho", Custom("glow"), OrderGlow, Vector3.zero, 1.5f);
            g.color = glow.Value;
        }
        SpriteRenderer visual = SpriteChild(go.transform, "Visual", sprite, OrderItems);
        var pickup = go.AddComponent<Pickup>();
        pickup.type = type;
        pickup.visual = visual.transform;
        return SavePrefab(go);
    }

    static GameObject CannonPrefab(string name, Sprite body, int bodyOrder, bool onShip, Prefabs p)
    {
        GameObject go = NewObject(name);
        SpriteChild(go.transform, "Visual", body, bodyOrder);
        if (onShip)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 1.6f);
        }
        SpriteRenderer cannon = SpriteChild(go.transform, "Canhao", Kenney("Parts/cannon"),
            onShip ? OrderShipCannon : OrderTowerCannon, new Vector3(0f, onShip ? 0.1f : 0f, 0f), 1.5f);
        var shooter = go.AddComponent<CannonShooter>();
        shooter.barrel = cannon.transform;
        shooter.barrelRenderer = cannon;
        shooter.ballPrefab = p.cannonball.GetComponent<Cannonball>();
        shooter.muzzlePrefab = p.muzzle;
        shooter.ignoreIslands = !onShip;
        shooter.range = onShip ? 7f : 7.5f;
        shooter.interval = onShip ? 2.2f : 1.7f;
        return SavePrefab(go);
    }

    static Prefabs CreatePrefabs()
    {
        var p = new Prefabs();
        Sprite[] boom = { Kenney("Effects/explosion3"), Kenney("Effects/explosion2"), Kenney("Effects/explosion1") };
        p.explosion = Effect("Explosao", boom[0], boom, 0.45f, 0.4f, 0.9f, Color.white, OrderEffects);
        p.muzzle = Effect("FumacaCanhao", boom[0], null, 0.25f, 0.2f, 0.45f, new Color(1f, 1f, 1f, 0.9f), OrderEffects);
        p.splash = Effect("Respingo", Custom("ring"), null, 0.5f, 0.15f, 0.7f, new Color(1f, 1f, 1f, 0.9f), OrderFoam + 1);
        p.spawnFx = Effect("EntradaPirata", Custom("ring"), null, 0.8f, 0.3f, 2.4f, new Color(1f, 1f, 1f, 0.9f), OrderFoam + 1);

        // Bala de canhão
        {
            GameObject go = NewObject("BalaDeCanhao");
            Rigidbody2D rb = AddBody(go, RigidbodyType2D.Dynamic);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.14f;
            SpriteChild(go.transform, "Visual", Kenney("Parts/cannonBall"), OrderCannonball, Vector3.zero, 2f);
            var ball = go.AddComponent<Cannonball>();
            ball.explosionPrefab = p.explosion;
            ball.splashPrefab = p.splash;
            go.AddComponent<Hazard>().timePenalty = GameRules.CannonPenalty;
            p.cannonball = SavePrefab(go);
        }

        p.cannonShip = CannonPrefab("NavioCanhoneiro", Kenney("Ships/ship_cannon"), OrderCannonShip, true, p);
        p.tower = CannonPrefab("TorreDoForte", KenneyTile(13), OrderTowers, false, p);

        p.chest = PickupPrefab("Bau", PickupType.Chest, Custom("chest"), 0.45f, new Color(1f, 0.9f, 0.5f, 0.85f));
        p.coin = PickupPrefab("Moeda", PickupType.Coin, Custom("coin"), 0.3f, null);
        p.hourglass = PickupPrefab("Ampulheta", PickupType.Hourglass, Custom("hourglass"), 0.35f, new Color(1f, 0.95f, 0.7f, 0.7f));
        p.buoy = PickupPrefab("Boia", PickupType.Buoy, Custom("buoy"), 0.4f, new Color(0.6f, 0.9f, 1f, 0.8f));

        // Tubarão
        {
            GameObject go = NewObject("Tubarao");
            AddBody(go, RigidbodyType2D.Kinematic);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            SpriteRenderer visual = SpriteChild(go.transform, "Visual", Custom("shark"), OrderShark);
            var patrol = go.AddComponent<SharkPatrol>();
            patrol.visual = visual.transform;
            go.AddComponent<Hazard>().timePenalty = GameRules.SharkPenalty;
            WakeEmitter wake = AddWake(go, visual.transform, new Vector2(0f, 0.05f), 0.2f, 0.6f);
            wake.alpha = 0.5f;
            wake.interval = 0.08f;
            p.shark = SavePrefab(go);
        }

        // Navio pirata perseguidor
        {
            GameObject go = NewObject("NavioPirata");
            Rigidbody2D rb = AddBody(go, RigidbodyType2D.Dynamic);
            rb.mass = 4f;
            go.AddComponent<CircleCollider2D>().radius = 0.55f;
            SpriteRenderer visual = SpriteChild(go.transform, "Visual", Kenney("Ships/ship_pirate"), OrderChaser);
            var chaser = go.AddComponent<EnemyChaser>();
            chaser.visual = visual.transform;
            go.AddComponent<Hazard>().timePenalty = GameRules.ChaserPenalty;
            AddWake(go, visual.transform, new Vector2(0f, -0.9f), 0.4f, 1.1f);
            p.chaser = SavePrefab(go);
        }

        // Surfista
        {
            GameObject go = NewObject("Surfista");
            go.tag = "Player";
            Rigidbody2D rb = AddBody(go, RigidbodyType2D.Dynamic);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = 0.32f;
            SpriteRenderer visual = SpriteChild(go.transform, "Visual", Custom("surfer"), OrderPlayer);
            SpriteRenderer shield = SpriteChild(go.transform, "Escudo", Custom("shield"), OrderShield);
            shield.gameObject.SetActive(false);
            var player = go.AddComponent<PlayerMovement>();
            player.visual = visual.transform;
            player.shieldVisual = shield.gameObject;
            player.blinkRenderers = new[] { visual };
            AddWake(go, visual.transform, new Vector2(0f, -0.6f), 0.3f, 0.8f);
            p.player = SavePrefab(go);
        }

        // Bússola
        {
            GameObject go = NewObject("Bussola");
            SpriteRenderer arrow = AddSprite(go, Custom("arrow"), OrderCompass);
            go.AddComponent<Compass>().arrow = arrow;
            p.compass = SavePrefab(go);
        }

        // Texto flutuante
        {
            GameObject go = NewObject("TextoFlutuante");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = uiFont;
            tmp.fontSharedMaterial = uiOutline;
            tmp.fontSize = 5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
            tmp.text = "+4s";
            go.GetComponent<MeshRenderer>().sortingOrder = OrderText;
            go.AddComponent<FloatingText>().text = tmp;
            p.floatingText = SavePrefab(go);
        }

        return p;
    }

    // ------------------------------------------------------------------ mapa

    static int Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 73856093 ^ y * 19349663;
            return h & 0x7fffffff;
        }
    }

    static Tilemap MakeTilemap(Transform grid, string name, int order, bool solid)
    {
        GameObject go = NewObject(name, grid);
        var map = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = order;
        renderer.sharedMaterial = spriteMat;
        if (solid)
        {
            go.layer = IslandLayer;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            var cc = go.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }
        return map;
    }

    static void Paint(Tilemap map, RectInt r, NineSlice ns)
    {
        for (int y = r.yMin; y < r.yMax; y++)
        {
            for (int x = r.xMin; x < r.xMax; x++)
            {
                bool left = x == r.xMin, right = x == r.xMax - 1, bottom = y == r.yMin, top = y == r.yMax - 1;
                Tile[] set = top ? (left ? ns.tl : right ? ns.tr : ns.t)
                    : bottom ? (left ? ns.bl : right ? ns.br : ns.b)
                    : (left ? ns.l : right ? ns.r : ns.c);
                map.SetTile(new Vector3Int(x, y, 0), set[Hash(x, y) % set.Length]);
            }
        }
    }

    static Vector3 Center(Vector2Int cell)
    {
        return LevelLayout.CellCenter(cell);
    }

    static GameObject BuildMap(LevelLayout layout, TileSet tiles, Prefabs p, bool gameplay)
    {
        GameObject root = NewObject("Mapa");

        // Ilhas e água rasa
        Transform grid = NewObject("Grid", root.transform).transform;
        grid.gameObject.AddComponent<Grid>();
        Tilemap shallow = MakeTilemap(grid, "AguaRasa", OrderShallow, false);
        Tilemap islands = MakeTilemap(grid, "Ilhas", OrderIslands, true);
        List<RectInt> islandRects = layout.FindIslands();
        foreach (RectInt r in islandRects)
        {
            Paint(shallow, new RectInt(r.x - 1, r.y - 1, r.width + 2, r.height + 2), tiles.shallow);
            Paint(islands, r, r.width >= 4 && r.height >= 4 ? tiles.grass : tiles.sand);
        }
        if (islands.GetUsedTilesCount() == 0) throw new Exception("Nenhum tile de ilha foi pintado.");

        // Decoração das ilhas
        Transform decor = NewObject("Decoracao", root.transform).transform;
        Sprite[] palms = { KenneyTile(71), KenneyTile(71), KenneyTile(70), KenneyTile(72) };
        foreach (Vector2Int c in layout.Find(LevelLayout.Palm))
        {
            SpriteRenderer palm = SpriteChild(decor, "Palmeira", palms[Hash(c.x, c.y) % palms.Length], OrderPalms, Center(c), 1.15f);
            palm.transform.rotation = Quaternion.Euler(0f, 0f, Hash(c.y, c.x) % 360);
        }
        foreach (Vector2Int c in layout.Find(LevelLayout.Wall))
            SpriteChild(decor, "Muralha", KenneyTile(16), OrderWalls, Center(c));

        // Pedras
        Transform rocks = NewObject("Pedras", root.transform).transform;
        int[] rockTiles = { 49, 50, 51, 65, 66, 67 };
        foreach (Vector2Int c in layout.Find(LevelLayout.Rock))
        {
            SpriteRenderer rock = SpriteChild(rocks, "Pedra", KenneyTile(rockTiles[Hash(c.x, c.y) % rockTiles.Length]), OrderRocks, Center(c));
            rock.gameObject.layer = ObstacleLayer;
            rock.gameObject.AddComponent<CircleCollider2D>().radius = 0.42f;
        }

        // Ondas decorativas
        Transform waves = NewObject("Ondas", root.transform).transform;
        waves.gameObject.AddComponent<OceanDecor>();
        var rng = new System.Random(7);
        Sprite waveSprite = Custom("wave_line");
        for (int i = 0, placed = 0; i < 2000 && placed < 90; i++)
        {
            int x = rng.Next(1, layout.width - 1), y = rng.Next(1, layout.height - 1);
            bool open = true;
            for (int dy = -2; dy <= 2 && open; dy++)
                for (int dx = -2; dx <= 2 && open; dx++)
                    if (layout.IsIsland(x + dx, y + dy)) open = false;
            if (!open || layout.IsBlocked(x, y)) continue;
            Vector3 pos = new Vector3(x + (float)rng.NextDouble(), y + (float)rng.NextDouble(), 0f);
            SpriteRenderer w = SpriteChild(waves, "Onda", waveSprite, OrderWaves, pos, 0.6f + 0.4f * (float)rng.NextDouble());
            w.color = new Color(1f, 1f, 1f, 0.3f);
            placed++;
        }

        // Canhões, navios ancorados e tubarões
        Transform enemies = NewObject("Perigos", root.transform).transform;
        foreach (Vector2Int c in layout.Find(LevelLayout.Tower))
            Place(p.tower, enemies, Center(c), 0f);
        foreach (Vector2Int c in layout.Find(LevelLayout.CannonShip))
            Place(p.cannonShip, enemies, Center(c), Hash(c.x, c.y) % 360);
        foreach ((Vector2Int a, Vector2Int b) in layout.SharkRoutes())
        {
            GameObject shark = Place(p.shark, enemies, Center(a), 0f);
            var patrol = shark.GetComponent<SharkPatrol>();
            patrol.pointA = Center(a);
            patrol.pointB = Center(b);
        }

        // Bordas invisíveis
        if (gameplay)
        {
            Transform bounds = NewObject("Bordas", root.transform).transform;
            float w = layout.width, h = layout.height;
            AddWall(bounds, new Vector2(-0.5f, h / 2f), new Vector2(1f, h + 2f));
            AddWall(bounds, new Vector2(w + 0.5f, h / 2f), new Vector2(1f, h + 2f));
            AddWall(bounds, new Vector2(w / 2f, -0.5f), new Vector2(w + 2f, 1f));
            AddWall(bounds, new Vector2(w / 2f, h + 0.5f), new Vector2(w + 2f, 1f));
        }
        return root;
    }

    static GameObject Place(GameObject prefab, Transform parent, Vector3 position, float angle)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        return go;
    }

    static void AddWall(Transform parent, Vector2 center, Vector2 size)
    {
        GameObject go = NewObject("Borda", parent);
        go.layer = ObstacleLayer;
        go.transform.position = center;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    // ------------------------------------------------------------------ cenas

    static Scene NewSceneFromTemplate(string path)
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        if (!AssetDatabase.CopyAsset(TemplateScene, path)) throw new Exception("Não consegui copiar a cena modelo para " + path);
        return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    static Camera SetupCamera()
    {
        Camera cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = WaterColor;
        cam.tag = "MainCamera";
        return cam;
    }

    static AudioManager MakeAudio(Transform parent)
    {
        GameObject go = NewObject("Audio", parent);
        var music = go.AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/Music/bitbound-run.mp3");
        music.loop = true;
        music.playOnAwake = true;
        music.volume = 0.45f;
        var sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        var audio = go.AddComponent<AudioManager>();
        audio.musicSource = music;
        audio.sfxSource = sfx;
        audio.hitClip = Clip("elevenlabs-blade.mp3");
        audio.coinClip = Clip("chieuk-coin-257878.mp3");
        audio.chestClip = Clip("chest.wav");
        audio.powerUpClip = Clip("powerup.wav");
        audio.shieldUpClip = Clip("shield_up.wav");
        audio.shieldBreakClip = Clip("shield_break.wav");
        audio.cannonClip = Clip("cannon.wav");
        audio.explosionClip = Clip("explosion.wav");
        audio.splashClip = Clip("splash.wav");
        audio.waveStartClip = Clip("wave_start.wav");
        audio.gameOverClip = Clip("game_over.wav");
        audio.clickClip = Clip("click.wav");
        return audio;
    }

    static AudioClip Clip(string file)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/Sfx/{file}");
        if (clip == null) throw new Exception("Áudio não encontrado: " + file);
        return clip;
    }

    static void MakeEventSystem(GameObject firstSelected)
    {
        GameObject go = NewObject("EventSystem");
        var es = go.AddComponent<EventSystem>();
        es.firstSelectedGameObject = firstSelected;
        go.AddComponent<StandaloneInputModule>();
    }

    static void BuildGameScene(LevelLayout layout, Prefabs p)
    {
        Scene scene = NewSceneFromTemplate(GameScenePath);
        // Abrir uma cena descarrega assets sem referência; os tiles precisam ser recarregados depois disso.
        TileSet tiles = CreateTiles();
        Camera cam = SetupCamera();

        GameObject map = BuildMap(layout, tiles, p, true);
        var gameMap = map.AddComponent<GameMap>();
        gameMap.layoutFile = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath);
        gameMap.obstacleMask = (1 << ObstacleLayer) | (1 << IslandLayer);

        Vector3 start = Center(layout.FindStart());
        var player = (GameObject)PrefabUtility.InstantiatePrefab(p.player);
        player.transform.position = start;
        gameMap.player = player.transform;

        var compass = (GameObject)PrefabUtility.InstantiatePrefab(p.compass);
        compass.GetComponent<Compass>().player = player.transform;

        var follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        follow.bounds = new Rect(0f, 0f, layout.width, layout.height);
        float halfW = cam.orthographicSize * 16f / 9f;
        cam.transform.position = new Vector3(
            Mathf.Clamp(start.x, halfW, layout.width - halfW),
            Mathf.Clamp(start.y, cam.orthographicSize, layout.height - cam.orthographicSize), -10f);

        GameObject systems = NewObject("Sistemas");
        MakeAudio(systems.transform);
        var spawner = systems.AddComponent<WaveSpawner>();
        spawner.map = gameMap;
        spawner.player = player.transform;
        spawner.chestPrefab = p.chest.GetComponent<Pickup>();
        spawner.coinPrefab = p.coin.GetComponent<Pickup>();
        spawner.hourglassPrefab = p.hourglass.GetComponent<Pickup>();
        spawner.buoyPrefab = p.buoy.GetComponent<Pickup>();
        spawner.chaserPrefab = p.chaser.GetComponent<EnemyChaser>();
        spawner.spawnEffectPrefab = p.spawnFx;
        var popups = systems.AddComponent<TimePopups>();
        popups.textPrefab = p.floatingText.GetComponent<FloatingText>();
        popups.player = player.transform;

        GameObject first = BuildGameUI(cam, systems);
        MakeEventSystem(first);

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    static void BuildMenuScene(LevelLayout layout, Prefabs p)
    {
        Scene scene = NewSceneFromTemplate(MenuScenePath);
        TileSet tiles = CreateTiles();
        Camera cam = SetupCamera();
        cam.gameObject.AddComponent<MenuCameraPan>().bounds = new Rect(0f, 0f, layout.width, layout.height);
        cam.transform.position = new Vector3(layout.width / 2f, layout.height / 2f, -10f);

        BuildMap(layout, tiles, p, false);
        GameObject systems = NewObject("Sistemas");
        MakeAudio(systems.transform);

        GameObject first = BuildMenuUI(cam);
        MakeEventSystem(first);

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    // ------------------------------------------------------------------ interface

    static RectTransform UI(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = UILayer;
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static RectTransform Stretch(string name, Transform parent, float inset = 0f)
    {
        RectTransform rt = UI(name, parent, Vector2.zero, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    static Image Img(RectTransform rt, Sprite sprite, Color color, float ppuMultiplier = 1f)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        if (sprite != null && sprite.border != Vector4.zero)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppuMultiplier;
        }
        else
        {
            img.preserveAspect = true;
        }
        return img;
    }

    static TextMeshProUGUI Label(RectTransform rt, string text, TMP_FontAsset font, Material mat, float size, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        if (mat != null) t.fontSharedMaterial = mat;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    static TextMeshProUGUI TextUI(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string text,
        TMP_FontAsset font, Material mat, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        return Label(UI(name, parent, anchor, pos, size), text, font, mat, fontSize, color, align);
    }

    static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction action, MenuAction clickSound)
    {
        RectTransform rt = UI("Botao " + label, parent, new Vector2(0.5f, 0.5f), pos, size);
        Image img = Img(rt, Custom("ui_button"), Color.white, 0.8f);
        img.raycastTarget = true;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.92f, 0.72f);
        colors.selectedColor = new Color(1f, 0.85f, 0.5f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.58f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        Label(Stretch("Texto", rt), label, uiFont, uiOutline, 46f, Cream);
        UnityEventTools.AddPersistentListener(btn.onClick, clickSound.Clique);
        UnityEventTools.AddPersistentListener(btn.onClick, action);
        return btn;
    }

    static Canvas MakeCanvas(Camera cam)
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = UILayer;
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static RectTransform Overlay(Transform parent, string name, Vector2 panelSize)
    {
        RectTransform root = Stretch(name, parent);
        Img(root, null, new Color(0f, 0f, 0f, 0.55f)).raycastTarget = true;
        RectTransform panel = UI("Painel", root, new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);
        Img(panel, Custom("ui_panel"), Color.white, 0.7f);
        return panel;
    }

    static GameObject IconText(Transform parent, Sprite icon, Vector2 pos, float iconSize, out TextMeshProUGUI text, float fontSize)
    {
        RectTransform row = UI("Linha", parent, new Vector2(0f, 0.5f), pos, new Vector2(260f, iconSize));
        row.pivot = new Vector2(0f, 0.5f);
        RectTransform iconRt = UI("Icone", row, new Vector2(0f, 0.5f), new Vector2(iconSize / 2f, 0f), new Vector2(iconSize, iconSize));
        Img(iconRt, icon, Color.white);
        RectTransform textRt = UI("Texto", row, new Vector2(0f, 0.5f), new Vector2(iconSize + 14f, 0f), new Vector2(200f, iconSize));
        textRt.pivot = new Vector2(0f, 0.5f);
        text = Label(textRt, "", uiFont, uiOutline, fontSize, Cream, TextAlignmentOptions.Left);
        return row.gameObject;
    }

    static GameObject BuildGameUI(Camera cam, GameObject systems)
    {
        Canvas canvas = MakeCanvas(cam);
        Transform c = canvas.transform;
        var menu = canvas.gameObject.AddComponent<MenuAction>();

        // HUD
        RectTransform hud = Stretch("HUD", c);

        RectTransform timerPlaque = UI("Relogio", hud, new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(360f, 132f));
        Img(timerPlaque, Custom("ui_plaque"), Color.white, 0.6f);
        Img(UI("Ampulheta", timerPlaque, new Vector2(0f, 0.5f), new Vector2(62f, 0f), new Vector2(64f, 92f)), Custom("hourglass"), Color.white);
        TextMeshProUGUI timerText = TextUI("Tempo", timerPlaque, new Vector2(0.5f, 0.5f), new Vector2(36f, 0f), new Vector2(240f, 120f),
            "00:30", uiFont, uiOutline, 92f, Cream);

        RectTransform left = UI("Onda", hud, new Vector2(0f, 1f), new Vector2(184f, -100f), new Vector2(320f, 160f));
        Img(left, Custom("ui_plaque"), Color.white, 0.6f);
        IconText(left, Custom("icon_wave"), new Vector2(22f, 34f), 60f, out TextMeshProUGUI waveText, 52f);
        IconText(left, Custom("chest"), new Vector2(22f, -38f), 60f, out TextMeshProUGUI chestsText, 52f);

        RectTransform right = UI("Pontuacao", hud, new Vector2(1f, 1f), new Vector2(-184f, -100f), new Vector2(320f, 160f));
        Img(right, Custom("ui_plaque"), Color.white, 0.6f);
        TextUI("Rotulo", right, new Vector2(0.5f, 0.5f), new Vector2(0f, 52f), new Vector2(280f, 40f), "Sobrevivido", uiFont, uiOutline, 30f, Cream);
        TextMeshProUGUI survivedText = TextUI("Sobrevivido", right, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(280f, 64f),
            "00:00", uiFont, uiOutline, 58f, Cream);
        IconText(right, Custom("icon_trophy"), new Vector2(60f, -48f), 44f, out TextMeshProUGUI recordText, 36f);

        var score = systems.AddComponent<ScoreUI>();
        score.waveText = waveText;
        score.chestsText = chestsText;
        score.recordText = recordText;

        // Barra de stamina
        RectTransform staminaPlaque = UI("PlacaStamina", hud, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(600f, 116f));
        Img(staminaPlaque, Custom("ui_plaque"), Color.white, 0.6f);
        TextUI("RotuloRemada", staminaPlaque, new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(560f, 40f), "Remada forte (Shift)", uiFont, uiOutline, 30f, Cream);
        RectTransform bar = UI("Stamina", staminaPlaque, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(520f, 40f));
        Img(Stretch("Fundo", bar), Custom("ui_bar_bg"), Color.white, 0.5f);
        RectTransform fillArea = Stretch("Area", bar, 6f);
        RectTransform fill = Stretch("Preenchimento", fillArea);
        Image fillImg = Img(fill, Custom("ui_bar_fill"), Color.white, 0.5f);
        var slider = bar.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.minValue = 0f;
        slider.maxValue = PlayerMovement.maxStamina;
        slider.value = PlayerMovement.maxStamina;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        bar.gameObject.AddComponent<StaminaBar>().fill = fillImg;

        TextUI("DicaPausa", hud, new Vector2(1f, 0f), new Vector2(-150f, 40f), new Vector2(260f, 40f), "ESC: pausar", uiFont, uiOutline, 28f,
            new Color(1f, 1f, 1f, 0.8f));

        var timer = systems.AddComponent<GameTimer>();
        timer.timerText = timerText;
        timer.survivedText = survivedText;

        // Aviso de onda
        RectTransform banner = UI("AvisoOnda", c, new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(1200f, 260f));
        var group = banner.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        var waveBanner = banner.gameObject.AddComponent<WaveBanner>();
        waveBanner.group = group;
        waveBanner.title = TextUI("Titulo", banner, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1200f, 180f), "Onda 1!",
            uiFont, uiOutline, 140f, Cream);
        waveBanner.subtitle = TextUI("Subtitulo", banner, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(1200f, 60f), "",
            uiFont, uiOutline, 46f, Cream);

        // Pausa
        RectTransform pausePanel = Overlay(c, "Pausa", new Vector2(640f, 620f));
        TextUI("Titulo", pausePanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(560f, 140f), "Pausado", titleFont, null, 110f, Ink);
        var pause = canvas.gameObject.AddComponent<PauseMenu>();
        Button resume = MakeButton(pausePanel, "Continuar", new Vector2(0f, 50f), new Vector2(420f, 104f), pause.Continuar, menu);
        MakeButton(pausePanel, "Reiniciar", new Vector2(0f, -76f), new Vector2(420f, 104f), menu.IniciarJogo, menu);
        MakeButton(pausePanel, "Menu", new Vector2(0f, -202f), new Vector2(420f, 104f), menu.MenuPrincipal, menu);
        pause.panel = pausePanel.parent.gameObject;
        pause.firstSelected = resume;

        // Fim de jogo
        RectTransform endPanel = Overlay(c, "FimDeJogo", new Vector2(880f, 820f));
        TextUI("Titulo", endPanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(800f, 150f), "Fim de jogo!", titleFont, null, 120f, Danger);
        var ui = canvas.gameObject.AddComponent<UIManager>();
        ui.hud = hud.gameObject;
        ui.endGamePanel = endPanel.parent.gameObject;
        TextMeshProUGUI badge = TextUI("NovoRecorde", endPanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(600f, 70f),
            "Novo recorde!", uiFont, uiOutline, 54f, Gold);
        badge.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -4f);
        ui.newRecordBadge = badge.gameObject;
        ui.finalTimeText = TextUI("TempoFinal", endPanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(800f, 80f), "", uiFont, null, 64f, Ink);
        ui.waveText = TextUI("OndaFinal", endPanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(800f, 60f), "", uiFont, null, 44f, Ink);
        ui.recordText = TextUI("Recorde", endPanel, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(800f, 60f), "", uiFont, null, 44f, Ink);
        Button again = MakeButton(endPanel, "Jogar de novo", new Vector2(0f, -150f), new Vector2(460f, 104f), menu.IniciarJogo, menu);
        MakeButton(endPanel, "Menu", new Vector2(0f, -280f), new Vector2(460f, 104f), menu.MenuPrincipal, menu);
        ui.firstSelected = again;

        pause.panel.SetActive(false);
        ui.endGamePanel.SetActive(false);
        return null;
    }

    static GameObject BuildMenuUI(Camera cam)
    {
        Canvas canvas = MakeCanvas(cam);
        Transform c = canvas.transform;
        var menu = canvas.gameObject.AddComponent<MenuAction>();

        Img(Stretch("Escurecer", c), null, new Color(0.02f, 0.1f, 0.2f, 0.22f));
        TextUI("Titulo", c, new Vector2(0.5f, 0.76f), Vector2.zero, new Vector2(1600f, 260f), "Fuga na Baía", titleFont, titleOutline, 200f, Cream);
        TextUI("Subtitulo", c, new Vector2(0.5f, 0.76f), new Vector2(0f, -140f), new Vector2(1600f, 70f),
            "Surfe, pegue os tesouros e fuja dos piratas!", uiFont, uiOutline, 50f, Cream);

        Button play = MakeButton(c, "Jogar", new Vector2(0f, -40f), new Vector2(460f, 116f), menu.IniciarJogo, menu);
        Button howTo = MakeButton(c, "Como jogar", new Vector2(0f, -176f), new Vector2(460f, 116f), menu.AbrirComoJogar, menu);
        menu.recordText = TextUI("Recorde", c, new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(1200f, 60f), "",
            uiFont, uiOutline, 42f, Gold);
        TextUI("Creditos", c, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1600f, 50f),
            "Enzo Rios Perego · Jogos Digitais · Insper", uiFont, uiOutline, 30f, Cream);

        RectTransform panel = Overlay(c, "ComoJogar", new Vector2(1240f, 860f));
        TextUI("Titulo", panel, new Vector2(0.5f, 0.5f), new Vector2(0f, 340f), new Vector2(1000f, 130f), "Como jogar", titleFont, null, 100f, Ink);
        TextMeshProUGUI body = TextUI("Texto", panel, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1080f, 520f),
            "• O relógio é a sua vida: sobreviva o máximo que puder.\n" +
            "• Baú +4s · Moeda +1s · Ampulheta +5s · Boia = escudo\n" +
            "• Pegue todos os baús para começar a próxima onda (+5s).\n" +
            "• Navio pirata -5s · Bala de canhão -3s · Tubarão -3s\n" +
            "• A seta amarela aponta o baú mais próximo.\n\n" +
            "Mover: WASD, setas ou analógico\n" +
            "Remada forte: Shift ou X (gasta stamina)\n" +
            "Pausar: ESC ou Start",
            uiFont, null, 38f, Ink, TextAlignmentOptions.Left);
        body.textWrappingMode = TextWrappingModes.Normal;
        body.lineSpacing = 12f;
        Button back = MakeButton(panel, "Voltar", new Vector2(0f, -330f), new Vector2(400f, 104f), menu.FecharComoJogar, menu);

        menu.howToPanel = panel.parent.gameObject;
        menu.mainFirst = play;
        menu.howToFirst = back;
        menu.howToPanel.SetActive(false);
        return play.gameObject;
    }

    // ------------------------------------------------------------------ screenshots (verificação)

    public static void CaptureScreens(string outDir)
    {
        Directory.CreateDirectory(outDir);

        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        Render(UnityEngine.Object.FindFirstObjectByType<Camera>(), Path.Combine(outDir, "menu.png"), 1280, 720, null, null);

        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        Camera cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
        Render(cam, Path.Combine(outDir, "jogo_inicio.png"), 1280, 720, null, null);

        var layout = new LevelLayout(File.ReadAllText(LayoutPath));
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.gameObject.SetActive(false);
        Render(cam, Path.Combine(outDir, "mapa_completo.png"), 1600, 1000, layout.height / 2f + 0.3f,
            new Vector3(layout.width / 2f, layout.height / 2f, -10f));
        Debug.Log("[BayBuilder] Screenshots em " + outDir);
    }

    static void Render(Camera cam, string file, int width, int height, float? orthoSize, Vector3? position)
    {
        Canvas.ForceUpdateCanvases();
        float oldSize = cam.orthographicSize;
        Vector3 oldPos = cam.transform.position;
        if (orthoSize.HasValue) cam.orthographicSize = orthoSize.Value;
        if (position.HasValue) cam.transform.position = position.Value;

        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(file, tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(tex);

        cam.orthographicSize = oldSize;
        cam.transform.position = oldPos;
    }
}
