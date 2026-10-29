// Editor authoring for the game audio (Studio audio addition, 2026-10-21), run in the open Editor through Pipeline run_script:
//   unity command --project-path <BuildingDefense> run_script --file ArtSource/UnityAuthoring/AudioAuthoring.cs --entry AudioAuthoring.Run --timeout_ms 180000
// After ArtSource/Audio/make_audio.py: sets the import of Art/Audio clips (SFX mono, decompress on load; ambient streaming),
// creates Resources/Utilities/AudioService.prefab and fills Resources/Configs/Audio/AudioConfig.asset (sounds by
// SFX_<SoundId>[_n], ambient by AMB_<AmbientId>, volumes, pitch spread, cooldowns). Follows FinalProject FpAudioSetup.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets._Project.Develop.Runtime.Configs.Audio;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEditor;
using UnityEngine;

public static class AudioAuthoring
{
    private const string Sfx = "Assets/_Project/Art/Audio/SFX/";
    private const string Ambient = "Assets/_Project/Art/Audio/Ambient/";
    private const string ConfigFolder = "Assets/_Project/Resources/Configs/Audio";
    private const string ConfigPath = ConfigFolder + "/AudioConfig.asset";
    private const string PrefabPath = "Assets/_Project/Resources/Utilities/AudioService.prefab";

    // id, channel, volume, pitch min, pitch max, cooldown (hordes: one bug death or spawn per short window)
    private static readonly (SoundIds id, AudioChannels channel, float volume, float pitchMin, float pitchMax, float cooldown)[] Sounds =
    {
        (SoundIds.UIClick, AudioChannels.UI, 0.8f, 0.97f, 1.03f, 0.03f),
        (SoundIds.UIOpen, AudioChannels.UI, 0.7f, 1f, 1f, 0.1f),
        (SoundIds.Win, AudioChannels.UI, 0.9f, 1f, 1f, 0f),
        (SoundIds.Defeat, AudioChannels.UI, 0.9f, 1f, 1f, 0f),
        (SoundIds.WaveStart, AudioChannels.Gameplay, 0.6f, 1f, 1f, 0.5f),
        (SoundIds.OrbitalStrike, AudioChannels.Gameplay, 0.8f, 0.95f, 1.05f, 0.05f),
        (SoundIds.MinePlace, AudioChannels.Gameplay, 0.7f, 0.97f, 1.03f, 0.05f),
        (SoundIds.MineDenied, AudioChannels.UI, 0.6f, 1f, 1f, 0.15f),
        (SoundIds.MineBlast, AudioChannels.Gameplay, 0.8f, 0.92f, 1.06f, 0.06f),
        (SoundIds.BugSpawn, AudioChannels.Gameplay, 0.35f, 0.9f, 1.15f, 0.12f),
        (SoundIds.BugDeath, AudioChannels.Gameplay, 0.45f, 0.85f, 1.2f, 0.035f),
        (SoundIds.AcidBlast, AudioChannels.Gameplay, 0.55f, 0.9f, 1.1f, 0.08f),
        (SoundIds.DomeHit, AudioChannels.Gameplay, 0.6f, 0.92f, 1.05f, 0.12f),
        (SoundIds.DomeDestroyed, AudioChannels.Gameplay, 1f, 1f, 1f, 0f),
        (SoundIds.TurretPlace, AudioChannels.Gameplay, 0.7f, 0.97f, 1.03f, 0.05f),
        (SoundIds.TurretShot, AudioChannels.Gameplay, 0.4f, 0.92f, 1.08f, 0.07f),
        (SoundIds.PuddlePlace, AudioChannels.Gameplay, 0.6f, 0.95f, 1.05f, 0.05f),
        (SoundIds.SpitterSpit, AudioChannels.Gameplay, 0.45f, 0.9f, 1.12f, 0.08f),
        (SoundIds.AcidSpitSplash, AudioChannels.Gameplay, 0.5f, 0.9f, 1.1f, 0.08f),
    };

    public static string Run()
    {
        var log = new StringBuilder();

        foreach (string path in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Project/Art/Audio" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            bool ambient = path.StartsWith(Ambient);
            importer.forceToMono = true;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = ambient ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = ambient ? 0.6f : 0.8f;
            settings.preloadAudioData = ambient == false;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = ambient;
            importer.SaveAndReimport();
        }

        var prefabRoot = new GameObject("AudioService");
        prefabRoot.AddComponent<AudioService>();
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);
        log.AppendLine("prefab " + PrefabPath);

        if (AssetDatabase.IsValidFolder(ConfigFolder) == false)
            AssetDatabase.CreateFolder("Assets/_Project/Resources/Configs", "Audio");

        var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AudioConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        var so = new SerializedObject(config);
        SerializedProperty sounds = so.FindProperty("_sounds");
        sounds.arraySize = Sounds.Length;
        for (int i = 0; i < Sounds.Length; i++)
        {
            var s = Sounds[i];
            SerializedProperty item = sounds.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("<Id>k__BackingField").enumValueIndex = (int)s.id;
            item.FindPropertyRelative("<Channel>k__BackingField").enumValueIndex = (int)s.channel;
            item.FindPropertyRelative("<Volume>k__BackingField").floatValue = s.volume;
            item.FindPropertyRelative("<PitchMin>k__BackingField").floatValue = s.pitchMin;
            item.FindPropertyRelative("<PitchMax>k__BackingField").floatValue = s.pitchMax;
            item.FindPropertyRelative("<Cooldown>k__BackingField").floatValue = s.cooldown;
            item.FindPropertyRelative("<Delay>k__BackingField").floatValue = 0f;
            SetClips(item, Clips(Sfx, "SFX_" + s.id));
            log.AppendLine($"{s.id}: {Clips(Sfx, "SFX_" + s.id).Count} clip(s), {s.channel}, vol {s.volume}");
        }

        SerializedProperty ambients = so.FindProperty("_ambients");
        AmbientIds[] ambientIds = (AmbientIds[])System.Enum.GetValues(typeof(AmbientIds));
        ambients.arraySize = ambientIds.Length;
        for (int i = 0; i < ambientIds.Length; i++)
        {
            SerializedProperty item = ambients.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("<Id>k__BackingField").enumValueIndex = (int)ambientIds[i];
            item.FindPropertyRelative("<Volume>k__BackingField").floatValue = 0.6f;
            SetClips(item, Clips(Ambient, "AMB_" + ambientIds[i]));
            log.AppendLine($"ambient {ambientIds[i]}: {Clips(Ambient, "AMB_" + ambientIds[i]).Count} loop(s)");
        }

        so.FindProperty("<UIVolume>k__BackingField").floatValue = 1f;
        so.FindProperty("<GameplayVolume>k__BackingField").floatValue = 1f;
        so.FindProperty("<AmbientVolume>k__BackingField").floatValue = 0.5f;
        // one seamless loop per ambient: the service crossfades it into itself
        so.FindProperty("<AmbientCrossfade>k__BackingField").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    private static void SetClips(SerializedProperty item, List<AudioClip> clips)
    {
        SerializedProperty list = item.FindPropertyRelative("<Clips>k__BackingField");
        list.arraySize = clips.Count;
        for (int k = 0; k < clips.Count; k++)
            list.GetArrayElementAtIndex(k).objectReferenceValue = clips[k];
    }

    private static List<AudioClip> Clips(string folder, string prefix) =>
        AssetDatabase.FindAssets("t:AudioClip", new[] { folder.TrimEnd('/') })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => Path.GetFileNameWithoutExtension(p) == prefix || Path.GetFileNameWithoutExtension(p).StartsWith(prefix + "_"))
            .OrderBy(p => p)
            .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
            .ToList();
}
