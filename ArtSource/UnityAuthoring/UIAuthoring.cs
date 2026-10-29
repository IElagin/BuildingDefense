// Editor authoring for the L8 UI (stage 4b), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/UIAuthoring.cs --entry UIAuthoring.Run --timeout_ms 180000
// Idempotent: imports the UI sprites (ArtSource/UI/make_ui_sprites.py), makes static TMP font assets with
// Latin and Cyrillic from Russo One and Exo 2 (only when missing, so their GUIDs stay), restyles the menu,
// loading screen, currency view and gameplay HUD, builds the course result popups and HP bars, and puts
// a health bar point on the bomber and building prefabs. Prefabs keep their roots; children are rebuilt.
// Entry UIAuthoring.RunGameplayHud (final assignment, 2026-10-29) rebuilds only the gameplay HUD with the defense
// cards and the placement ring prefab.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.Defense;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.UI.MainMenu;
using Assets._Project.Develop.Runtime.UI.Statistics;
using Assets._Project.Develop.Runtime.Utilities.LoadingScreen;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class UIAuthoring
{
    private const string Fonts = "Assets/_Project/Art/Fonts/";
    private const string Sprites = "Assets/_Project/Art/Sprites/UI/";
    private const string Backgrounds = "Assets/_Project/Art/Sprites/Backgrounds/";
    private const string UI = "Assets/_Project/Resources/UI/";
    private const string Entities = "Assets/_Project/Resources/Entities/";
    private const string ParticleMaterial = "Assets/_Project/Art/Materials/ParticleFlat.mat";

    private const string Charset =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
        "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
        " !\"#$%&'()*+,-./:;<=>?@[\\]^_{|}~«»—–…№×•°";

    // 9-slice borders (left, bottom, right, top), the same as BORDERS in make_ui_sprites.py
    private static readonly Dictionary<string, Vector4> Borders = new()
    {
        ["Panel"] = new Vector4(22, 22, 22, 22),
        ["PanelHeader"] = new Vector4(22, 22, 22, 22),
        ["ButtonTeal"] = new Vector4(20, 20, 20, 20),
        ["ButtonOrange"] = new Vector4(20, 20, 20, 20),
        ["ButtonDark"] = new Vector4(20, 20, 20, 20),
        ["BarBack"] = new Vector4(10, 10, 10, 10),
        ["BarFill"] = new Vector4(8, 8, 8, 8),
        ["Banner"] = new Vector4(44, 0, 44, 0),
    };

    private static readonly Color TextWhite = new(0.95f, 0.98f, 0.98f);
    private static readonly Color TextMuted = new(0.62f, 0.74f, 0.78f);
    private static readonly Color TextWarm = new(1f, 0.88f, 0.56f);
    private static readonly Color Teal = new(0.18f, 0.83f, 0.78f);
    private static readonly Color Gold = new(1f, 0.83f, 0.3f);
    private static readonly Color Danger = new(0.9f, 0.28f, 0.3f);

    private static TMP_FontAsset _russo;
    private static TMP_FontAsset _exo;
    private static TMP_FontAsset _exoSemiBold;
    private static Material _russoOutline;

    public static string Run()
    {
        var log = new StringBuilder();

        ImportSprites(log);
        _russo = FontAsset("RussoOne-Regular.ttf", "RussoOne SDF", log);
        _exo = FontAsset("Exo2-Regular.ttf", "Exo2 SDF", log);
        _exoSemiBold = FontAsset("Exo2-SemiBold.ttf", "Exo2 SemiBold SDF", log);
        _russoOutline = OutlineMaterial(_russo, "RussoOne SDF Outline");

        SetCurrencyIcon();
        BuildCurrencyView();
        BuildMainMenuScreen();
        AddMainMenuBackground();
        BuildLoadingScreen();
        BuildGameplayScreen();
        BuildHealthBar(UI + "Gameplay/HealthBars/SimpleHealthBar.prefab", new Vector2(64, 11), 3, 2, false, true);
        BuildHealthBar(UI + "Gameplay/HealthBars/BuildingHealthBar.prefab", new Vector2(300, 40), 1, 5, true, false);
        BuildWinPopup();
        BuildDefeatPopup();
        AddHealthBarPoint(Entities + "Bomber.prefab", 0.25f, log);
        AddHealthBarPoint(Entities + "Building.prefab", 0.4f, log);

        AssetDatabase.SaveAssets();
        log.AppendLine("UI authored");
        return log.ToString();
    }

    public static string RunGameplayHud()
    {
        var log = new StringBuilder();

        ImportSprites(log);
        _russo = FontAsset("RussoOne-Regular.ttf", "RussoOne SDF", log);
        _exo = FontAsset("Exo2-Regular.ttf", "Exo2 SDF", log);
        _exoSemiBold = FontAsset("Exo2-SemiBold.ttf", "Exo2 SemiBold SDF", log);
        _russoOutline = OutlineMaterial(_russo, "RussoOne SDF Outline");

        BuildGameplayScreen();
        BuildPlacementPreview();

        AssetDatabase.SaveAssets();
        log.AppendLine("gameplay HUD with defense cards, placement preview");
        return log.ToString();
    }

    // ---------- assets ----------

    private static void ImportSprites(StringBuilder log)
    {
        // sprites freshly drawn by make_ui_sprites.py have no importer until the asset database sees them
        AssetDatabase.Refresh();

        foreach (string path in Directory.GetFiles(Sprites, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            string name = Path.GetFileNameWithoutExtension(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteBorder = Borders.TryGetValue(name, out Vector4 border) ? border : Vector4.zero;
            importer.SaveAndReimport();
        }

        var background = (TextureImporter)AssetImporter.GetAtPath(Backgrounds + "MenuBackground.jpg");
        background.textureType = TextureImporterType.Sprite;
        background.spriteImportMode = SpriteImportMode.Single;
        background.mipmapEnabled = false;
        background.maxTextureSize = 2048;
        background.wrapMode = TextureWrapMode.Clamp;
        background.SaveAndReimport();
        log.AppendLine("sprites imported");
    }

    private static Sprite S(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + name + ".png")
        ?? throw new InvalidOperationException("missing sprite " + name);

    private static Sprite MenuBackground => AssetDatabase.LoadAssetAtPath<Sprite>(Backgrounds + "MenuBackground.jpg");

    private static TMP_FontAsset FontAsset(string ttf, string assetName, StringBuilder log)
    {
        string path = Fonts + assetName + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null)
        {
            log.AppendLine($"{assetName}: {existing.characterTable.Count} characters, {existing.atlasTextures.Length} atlas");
            return existing;
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(Fonts + ttf);
        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, 60, 6, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, false);
        asset.name = assetName;
        asset.TryAddCharacters(Charset, out string missing);
        asset.atlasPopulationMode = AtlasPopulationMode.Static;

        AssetDatabase.CreateAsset(asset, path);
        asset.atlasTexture.name = assetName + " Atlas";
        AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
        asset.material.name = assetName + " Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        log.AppendLine($"{assetName}: {asset.characterTable.Count} characters, missing '{missing}'");
        return asset;
    }

    private static Material OutlineMaterial(TMP_FontAsset font, string name)
    {
        string path = Fonts + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(font.material) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }

        material.EnableKeyword("OUTLINE_ON");
        material.SetFloat("_OutlineWidth", 0.22f);
        material.SetColor("_OutlineColor", new Color(0.04f, 0.07f, 0.09f));
        material.EnableKeyword("UNDERLAY_ON");
        material.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.55f));
        material.SetFloat("_UnderlayOffsetX", 0.4f);
        material.SetFloat("_UnderlayOffsetY", -0.9f);
        material.SetFloat("_UnderlayDilate", 0.2f);
        material.SetFloat("_UnderlaySoftness", 0.3f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetCurrencyIcon()
    {
        var config = Resources.Load<CurrencyIconsConfig>("Configs/Meta/Wallet/CurrencyIconsConfig");
        var so = new SerializedObject(config);
        SerializedProperty configs = so.FindProperty("_configs");
        for (int i = 0; i < configs.arraySize; i++)
            configs.GetArrayElementAtIndex(i).FindPropertyRelative("<Sprite>k__BackingField").objectReferenceValue = S("IconGold");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
    }

    // ---------- screens ----------

    private static void BuildCurrencyView()
    {
        Rebuild(UI + "Wallet/CurrencyView.prefab", root =>
        {
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(290, 84);

            AddImage(Stretch(root.transform, "Background"), S("Panel"), Color.white);
            Image icon = AddImage(Place(root.transform, "Icon", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 0), new Vector2(68, 68)), S("IconGold"), Color.white);
            icon.preserveAspect = true;
            TMP_Text value = AddText(Stretch(root.transform, "Value", 98, 0, 20, 0), "200", _russo, 44, TextWhite, TextAlignmentOptions.MidlineLeft, _russoOutline);

            var view = root.GetComponent<IconTextView>();
            SetRef(view, "_text", value);
            SetRef(view, "_icon", icon);
        });
    }

    private static void BuildMainMenuScreen()
    {
        Rebuild(UI + "MainMenu/MainMenuScreen.prefab", root =>
        {
            RectTransform safeArea = Stretch(root.transform, "SafeArea");
            safeArea.gameObject.AddComponent<SafeAreaContainer>();

            IconTextListView wallet = WalletList(Place(safeArea, "WalletView", new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(700, 96)), TextAnchor.UpperLeft);

            RectTransform statsRect = Place(safeArea, "Statistics", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(560, 84));
            AddImage(Stretch(statsRect, "Background"), S("Panel"), Color.white);
            TMP_Text wins = AddText(Anchored(statsRect, "Wins", new Vector2(0, 0), new Vector2(0.5f, 1)), "Победы: 0", _exoSemiBold, 34, TextWhite, TextAlignmentOptions.Center);
            TMP_Text losses = AddText(Anchored(statsRect, "Losses", new Vector2(0.5f, 0), new Vector2(1, 1)), "Поражения: 0", _exoSemiBold, 34, TextMuted, TextAlignmentOptions.Center);
            var statistics = statsRect.gameObject.AddComponent<StatisticsView>();
            SetRef(statistics, "_winsText", wins);
            SetRef(statistics, "_lossesText", losses);

            TMP_Text title = AddText(Place(safeArea, "Title", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1500, 160)),
                "Защита купола", _russo, 116, TextWhite, TextAlignmentOptions.Center, _russoOutline);
            title.fontStyle = FontStyles.UpperCase;

            Button play = MakeButton(safeArea, "PlayButton", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(560, 150), S("ButtonTeal"), "Играть", 72);

            var view = root.GetComponent<MainMenuScreenView>();
            SetRef(view, "<WalletView>k__BackingField", wallet);
            SetRef(view, "<StatisticsView>k__BackingField", statistics);
            SetRef(view, "_playButton", play);
        });
    }

    // Course lesson 33/35: background image under all layers, AspectRatioFitter EnvelopeParent, vignette on top.
    private static void AddMainMenuBackground()
    {
        string path = UI + "MainMenu/MainMenuUIRoot.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Transform old = root.transform.Find("Background");
        if (old != null)
            Object.DestroyImmediate(old.gameObject);

        RectTransform background = Stretch(root.transform, "Background");
        background.SetSiblingIndex(0);
        EnvelopeImage(background, MenuBackground);
        AddImage(Stretch(background, "Vignette"), S("Vignette"), new Color(0, 0, 0, 0.85f));

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void BuildLoadingScreen()
    {
        Rebuild("Assets/_Project/Resources/Utilities/StandardLoadingScreen.prefab", root =>
        {
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform background = Stretch(root.transform, "Background");
            EnvelopeImage(background, MenuBackground);
            AddImage(Stretch(background, "Dim"), null, new Color(0.03f, 0.05f, 0.07f, 0.62f));

            RectTransform bottom = Anchored(root.transform, "LoadingText", new Vector2(0, 0), new Vector2(1, 0));
            bottom.pivot = new Vector2(0.5f, 0);
            bottom.anchoredPosition = new Vector2(0, 90);
            bottom.sizeDelta = new Vector2(-80, 140);
            AddText(bottom, "Загрузка…", _russo, 72, TextWhite, TextAlignmentOptions.Center, _russoOutline).fontStyle = FontStyles.UpperCase;
        });
    }

    private static void BuildGameplayScreen()
    {
        Rebuild(UI + "Gameplay/GameplayScreen.prefab", root =>
        {
            // HP bars sit at entity screen positions, so their container stays outside the safe area.
            RectTransform healthBars = Stretch(root.transform, "HealthBars");
            var healthDisplay = healthBars.gameObject.AddComponent<EntitiesHealthDisplay>();
            SetRef(healthDisplay, "_parent", healthBars);

            RectTransform safeArea = Stretch(root.transform, "SafeArea");
            safeArea.gameObject.AddComponent<SafeAreaContainer>();

            RectTransform phasePanel = Place(safeArea, "PhasePanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(640, 128));
            AddImage(Stretch(phasePanel, "Background"), S("Panel"), Color.white);
            TMP_Text phase = AddText(Inset(Anchored(phasePanel, "Phase", new Vector2(0, 0.5f), new Vector2(1, 1)), 28, 0, 24, 10),
                "Волна 1", _exoSemiBold, 40, TextWhite, TextAlignmentOptions.MidlineLeft);
            TMP_Text hint = AddText(Inset(Anchored(phasePanel, "Hint", new Vector2(0, 0), new Vector2(1, 0.5f)), 28, 12, 24, 0),
                "Клик по земле — орбитальный удар", _exoSemiBold, 28, TextWarm, TextAlignmentOptions.MidlineLeft);

            RectTransform stageRect = Place(safeArea, "StageView", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(280, 96));
            AddImage(Stretch(stageRect, "Background"), S("Panel"), Color.white);
            Image stageIcon = AddImage(Place(stageRect, "Icon", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(56, 0), new Vector2(76, 76)), S("IconBug"), Color.white);
            stageIcon.preserveAspect = true;
            TMP_Text stageText = AddText(Stretch(stageRect, "Text", 104, 0, 20, 0), "1 / 3", _russo, 48, TextWhite, TextAlignmentOptions.MidlineLeft, _russoOutline);
            var stageView = stageRect.gameObject.AddComponent<IconTextView>();
            SetRef(stageView, "_text", stageText);
            SetRef(stageView, "_icon", stageIcon);

            IconTextListView wallet = WalletList(Place(safeArea, "WalletView", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(600, 96)), TextAnchor.UpperRight);

            RectTransform restPanel = Place(safeArea, "RestPanel", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(820, 136));
            AddImage(Stretch(restPanel, "Background"), S("Panel"), Color.white);
            AddImage(Place(restPanel, "TimerIcon", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66, 0), new Vector2(72, 72)), S("IconTimer"), Color.white).preserveAspect = true;
            AddText(Place(restPanel, "Label", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(120, 24), new Vector2(230, 40)),
                "До волны", _exoSemiBold, 28, TextMuted, TextAlignmentOptions.MidlineLeft);
            TMP_Text restTime = AddText(Place(restPanel, "Time", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(120, -18), new Vector2(230, 64)),
                "12 с", _russo, 52, TextWhite, TextAlignmentOptions.MidlineLeft, _russoOutline);
            Button startWave = MakeButton(restPanel, "StartWaveButton", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-22, 0), new Vector2(430, 104),
                S("ButtonOrange"), "Начать волну\n<size=65%>Space</size>", 38);

            // defense cards to the left of the rest panel, keys 1-3 in DefenseTypes order
            RectTransform defensePanel = Place(safeArea, "DefensePanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 36), new Vector2(474, 136));
            var defensePanelView = defensePanel.gameObject.AddComponent<DefensePanelView>();
            SetRef(defensePanelView, "<MineCard>k__BackingField", DefenseCard(defensePanel, 0, "IconMine"));
            SetRef(defensePanelView, "<TurretCard>k__BackingField", DefenseCard(defensePanel, 1, "IconTurret"));
            SetRef(defensePanelView, "<PuddleCard>k__BackingField", DefenseCard(defensePanel, 2, "IconPuddle"));

            var view = root.GetComponent<GameplayScreenView>();
            SetRef(view, "<DefensePanel>k__BackingField", defensePanelView);
            SetRef(view, "_phaseText", phase);
            SetRef(view, "_hintText", hint);
            SetRef(view, "_restPanel", restPanel.gameObject);
            SetRef(view, "_restTimeText", restTime);
            SetRef(view, "_startWaveButton", startWave);
            SetRef(view, "<StageNumberView>k__BackingField", stageView);
            SetRef(view, "<EntitiesHealthDisplay>k__BackingField", healthDisplay);
            SetRef(view, "<WalletView>k__BackingField", wallet);
        });
    }

    private static DefenseCardView DefenseCard(RectTransform panel, int index, string icon)
    {
        RectTransform rect = Place(panel, "Card" + (index + 1), new Vector2(0, 0), new Vector2(0, 0), new Vector2(index * 162, 0), new Vector2(150, 136));
        Image background = AddImage(rect, S("ButtonDark"), Color.white, true);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.93f, 0.97f, 1f);
        colors.pressedColor = new Color(0.78f, 0.82f, 0.85f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        rect.gameObject.AddComponent<ButtonPressFeedback>();

        RectTransform content = Stretch(rect, "Content");
        var group = content.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        AddImage(Place(content, "Icon", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(76, 76)), S(icon), Color.white).preserveAspect = true;
        AddText(Place(content, "Key", new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, -8), new Vector2(30, 34)),
            (index + 1).ToString(), _russo, 28, TextMuted, TextAlignmentOptions.TopLeft);
        AddImage(Place(content, "GoldIcon", new Vector2(0.5f, 0), new Vector2(1, 0.5f), new Vector2(-4, 28), new Vector2(30, 30)), S("IconGold"), Color.white).preserveAspect = true;
        TMP_Text price = AddText(Place(content, "Price", new Vector2(0.5f, 0), new Vector2(0, 0.5f), new Vector2(0, 28), new Vector2(70, 36)),
            "25", _russo, 30, Gold, TextAlignmentOptions.MidlineLeft, _russoOutline);

        RectTransform locked = Stretch(rect, "Locked");
        AddImage(locked, S("Panel"), new Color(1, 1, 1, 0.75f));
        AddText(Stretch(locked, "Label"), "скоро", _exoSemiBold, 30, TextWarm, TextAlignmentOptions.Center);
        locked.gameObject.SetActive(false);

        var card = rect.gameObject.AddComponent<DefenseCardView>();
        SetRef(card, "_button", button);
        SetRef(card, "_background", background);
        SetRef(card, "_normalSprite", S("ButtonDark"));
        SetRef(card, "_selectedSprite", S("ButtonTeal"));
        SetRef(card, "_content", group);
        SetRef(card, "_priceText", price);
        SetRef(card, "_lockedLabel", locked.gameObject);
        return card;
    }

    // A world ring for the placement preview (unlit particle material, coloured by the view).
    private static void BuildPlacementPreview()
    {
        string path = UI + "Gameplay/PlacementPreview.prefab";
        var root = new GameObject("PlacementPreview");
        var ring = root.AddComponent<LineRenderer>();
        ring.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterial);
        ring.widthMultiplier = 0.14f;
        ring.numCornerVertices = 2;
        ring.alignment = LineAlignment.View;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 0;

        var view = root.AddComponent<PlacementPreviewView>();
        SetRef(view, "_ring", ring);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    // Course lesson 55: slider bar with a dark back and a tintable filler; the simple variant hides the text.
    // Enemy bars (L8 hordes) are compact and stay hidden while the enemy is at full health.
    private static void BuildHealthBar(string path, Vector2 size, float spriteScale, float inset, bool withText, bool hiddenWhenFull)
    {
        Rebuild(path, root =>
        {
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = size;

            RectTransform barRect = Stretch(root.transform, "Bar");
            AddImage(Stretch(barRect, "Background"), S("BarBack"), Color.white).pixelsPerUnitMultiplier = spriteScale;
            RectTransform fillArea = Stretch(barRect, "FillArea", inset, inset, inset, inset);
            RectTransform fill = Stretch(fillArea, "Fill");
            Image filler = AddImage(fill, S("BarFill"), Color.white);
            filler.pixelsPerUnitMultiplier = spriteScale;

            var slider = barRect.gameObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.fillRect = fill;
            slider.value = 1;

            var bar = barRect.gameObject.AddComponent<Bar>();
            SetRef(bar, "_slider", slider);
            SetRef(bar, "_filler", filler);

            TMP_Text value = AddText(Stretch(root.transform, "Value", 0, 2, 0, 0), "150", _russo, 28, TextWhite, TextAlignmentOptions.Center, _russoOutline);
            value.gameObject.SetActive(withText);

            BarWithText view = GetOrAdd<BarWithText>(root);
            SetRef(view, "_text", value);
            SetRef(view, "_bar", bar);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_hiddenWhenFull").boolValue = hiddenWhenFull;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        });
    }

    // Course lesson 43: anticlicker, full-screen body with a click trigger, glow, header plate across the
    // screen with 20 px margins, three stars (slot + coloured star), reward and a tap hint.
    private static void BuildWinPopup()
    {
        Rebuild(UI + "Gameplay/ResultsPopup/WinPopup.prefab", root =>
        {
            WinPopupView view = GetOrAdd<WinPopupView>(root);
            RectTransform body = PopupFrame(root, view, Gold, Teal, "Победа!");

            var stars = new List<Transform>();
            RectTransform starsRect = Place(body, "Stars", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(660, 200));
            for (int i = 0; i < 3; i++)
                stars.Add(Star(starsRect, i, true));

            TMP_Text reward = AddText(Place(body, "Reward", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1400, 70)),
                "Купол выстоял. Награда: +50", _exoSemiBold, 44, TextWarm, TextAlignmentOptions.Center);
            AddText(Place(body, "Hint", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -262), new Vector2(1000, 50)),
                "Нажмите, чтобы продолжить", _exo, 32, new Color(0.8f, 0.88f, 0.9f, 0.85f), TextAlignmentOptions.Center);

            // the whole body continues (course: EventTrigger on the body calls OnContinueClick)
            AddImage(body, null, new Color(0, 0, 0, 0), true);
            var trigger = body.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            UnityEventTools.AddVoidPersistentListener(entry.callback, new UnityAction(view.OnContinueClick));
            trigger.triggers.Add(entry);

            SetRef(view, "_reward", reward);
            var so = new SerializedObject(view);
            SerializedProperty list = so.FindProperty("_stars");
            list.arraySize = stars.Count;
            for (int i = 0; i < stars.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        });
    }

    // Course lesson 46: the same frame in red, empty star slots, restart and menu buttons.
    private static void BuildDefeatPopup()
    {
        Rebuild(UI + "Gameplay/ResultsPopup/DefeatPopup.prefab", root =>
        {
            DefeatPopupView view = GetOrAdd<DefeatPopupView>(root);
            RectTransform body = PopupFrame(root, view, Danger, Danger, "Поражение");

            RectTransform starsRect = Place(body, "Stars", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(660, 200));
            for (int i = 0; i < 3; i++)
                Star(starsRect, i, false);

            AddText(Place(body, "Message", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(1400, 70)),
                "Купол разрушен", _exoSemiBold, 44, new Color(1f, 0.8f, 0.78f), TextAlignmentOptions.Center);

            Button menu = MakeButton(body, "MenuButton", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-215, -265), new Vector2(390, 120), S("ButtonDark"), "В меню", 46);
            Button restart = MakeButton(body, "RestartButton", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(215, -265), new Vector2(390, 120), S("ButtonOrange"), "Заново", 46);

            SetRef(view, "_continueButton", menu);
            SetRef(view, "_restartButton", restart);
        });
    }

    private static RectTransform PopupFrame(GameObject root, Component view, Color glowColor, Color headerColor, string title)
    {
        var rect = (RectTransform)root.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        CanvasGroup mainGroup = GetOrAdd<CanvasGroup>(root);

        Image anticlicker = AddImage(Stretch(root.transform, "Anticlicker"), null, new Color(0.02f, 0.04f, 0.06f, 0.8f), true);

        RectTransform body = Stretch(root.transform, "Body");
        CanvasGroup bodyGroup = body.gameObject.AddComponent<CanvasGroup>();

        glowColor.a = 0.4f;
        AddImage(Place(body, "Glow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1300, 1300)), S("Glow"), glowColor);

        RectTransform header = Anchored(body, "Header", new Vector2(0, 0.5f), new Vector2(1, 0.5f));
        header.anchoredPosition = new Vector2(0, 230);
        header.sizeDelta = new Vector2(-40, 170);
        AddImage(header, S("Banner"), headerColor);
        TMP_Text titleText = AddText(Stretch(header, "Title", 60, 0, 60, 8), title, _russo, 100, TextWhite, TextAlignmentOptions.Center, _russoOutline);
        titleText.fontStyle = FontStyles.UpperCase;

        SetRef(view, "_mainGroup", mainGroup);
        SetRef(view, "_anticlicker", anticlicker);
        SetRef(view, "_body", bodyGroup);
        SetRef(view, "_title", titleText);
        var so = new SerializedObject(view);
        SerializedProperty animation = so.FindProperty("_animationType");
        animation.enumValueIndex = Array.IndexOf(animation.enumNames, "Fade");
        so.ApplyModifiedPropertiesWithoutUndo();
        return body;
    }

    private static Transform Star(RectTransform parent, int index, bool filled)
    {
        bool middle = index == 1;
        float size = middle ? 190 : 160;
        RectTransform slot = Place(parent, "Star" + (index + 1), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2((index - 1) * 215, middle ? 18 : 0), new Vector2(size, size));
        AddImage(slot, S("StarSlot"), Color.white);
        RectTransform star = Stretch(slot, "Star");
        AddImage(star, S("Star"), Color.white);
        star.gameObject.SetActive(filled);
        return star;
    }

    private static void AddHealthBarPoint(string path, float offset, StringBuilder log)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>().Where(r => r is not ParticleSystemRenderer).ToArray();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        Transform point = root.transform.Find("HealthBarPoint");
        if (point == null)
        {
            point = new GameObject("HealthBarPoint").transform;
            point.SetParent(root.transform, false);
        }

        point.localPosition = root.transform.InverseTransformPoint(new Vector3(bounds.center.x, bounds.max.y + offset, bounds.center.z));
        SetRef(GetOrAdd<HealthBarPointRegistrator>(root), "_point", point);

        log.AppendLine($"{Path.GetFileName(path)}: health bar point at {point.localPosition}");
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // ---------- helpers ----------

    private static void Rebuild(string path, Action<GameObject> build)
    {
        EnsureFolder(Path.GetDirectoryName(path));
        bool exists = File.Exists(path);
        GameObject root = exists
            ? PrefabUtility.LoadPrefabContents(path)
            : new GameObject(Path.GetFileNameWithoutExtension(path), typeof(RectTransform)) { layer = 5 };

        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

        build(root);
        PrefabUtility.SaveAsPrefabAsset(root, path);

        if (exists)
            PrefabUtility.UnloadPrefabContents(root);
        else
            Object.DestroyImmediate(root);
    }

    private static void EnsureFolder(string folder)
    {
        folder = folder.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(folder))
            return;

        EnsureFolder(Path.GetDirectoryName(folder));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component =>
        go.TryGetComponent(out T component) ? component : go.AddComponent<T>();

    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field)
            ?? throw new InvalidOperationException($"{target.GetType().Name}.{field} not found");
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform Node(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static RectTransform Stretch(Transform parent, string name, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        RectTransform rect = Node(parent, name);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        return Inset(rect, left, bottom, right, top);
    }

    private static RectTransform Inset(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        return rect;
    }

    private static RectTransform Anchored(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = Node(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static RectTransform Place(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        RectTransform rect = Node(parent, name);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image AddImage(RectTransform rect, Sprite sprite, Color color, bool raycast = false)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycast;
        if (sprite != null && sprite.border != Vector4.zero)
            image.type = Image.Type.Sliced;
        return image;
    }

    private static TMP_Text AddText(RectTransform rect, string text, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Material material = null)
    {
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        if (material != null)
            label.fontSharedMaterial = material;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        return label;
    }

    private static void EnvelopeImage(RectTransform parent, Sprite sprite)
    {
        RectTransform rect = Node(parent, "Image");
        AddImage(rect, sprite, Color.white);
        var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private static IconTextListView WalletList(RectTransform rect, TextAnchor alignment)
    {
        var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = alignment;
        layout.spacing = 16;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var list = rect.gameObject.AddComponent<IconTextListView>();
        SetRef(list, "_parent", rect);
        return list;
    }

    private static Button MakeButton(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Sprite sprite, string label, float fontSize)
    {
        RectTransform rect = Place(parent, name, anchor, pivot, position, size);
        Image image = AddImage(rect, sprite, Color.white, true);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.93f, 0.97f, 1f);
        colors.pressedColor = new Color(0.78f, 0.82f, 0.85f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        rect.gameObject.AddComponent<ButtonPressFeedback>();

        TMP_Text text = AddText(Stretch(rect, "Label", 16, 8, 16, 12), label, _russo, fontSize, TextWhite, TextAlignmentOptions.Center, _russoOutline);
        text.fontStyle = FontStyles.UpperCase;
        text.lineSpacing = -18;
        return button;
    }
}
