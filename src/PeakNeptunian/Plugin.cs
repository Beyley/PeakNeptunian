using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PEAKLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace PeakNeptunian;

[BepInDependency(UIPlugin.Id)]
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    // Plugin stuff
    public static ManualLogSource Log { get; set; } = null!;

    // Config
    private static ConfigEntry<LocalizationType> _localizationType = null!;
    private static ConfigEntry<bool> _enablePingPang = null!;

    // Loaded assets
    private static TMP_FontAsset? _neptunianShpreFont;
    private static readonly Dictionary<string, SFX_Instance> LoadedSoundEffects = new();
    private static readonly Dictionary<string, TexturePatch> TexturePatches = new();

    // Static state
    private static readonly Dictionary<string, string> NahnyaToKey = new();
    private static Action_AskBingBong.BingBongResponse[] _bingBongResponses = [];

    // Dynamic state
    private static readonly HashSet<int> PatchedGameObjects = new();

    private void Awake()
    {
        Log = Logger;

        _localizationType = Config.Bind("General", "LocalizationSetting", LocalizationType.Off);
        _enablePingPang = Config.Bind("General", "EnablePingPang", true);

        try
        {
            LoadTextures();
            LoadFont();
            StartCoroutine(CreateBingBongResponses());

            // We can apply our hooks here.
            // See https://lethal.wiki/dev/fundamentals/patching-code
            Harmony.CreateAndPatchAll(typeof(Plugin));
            
            Localizations.Load();
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to init, got exception {ex}");
        }
        
        // Create a backwards table so we can more performantly detect localized Nahnya in strings
        // (for when things bypass the LocalizedText component)
        foreach (var (key, (nahnya, _)) in Localizations.Neptunian)
            NahnyaToKey[nahnya] = key;

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DevMessageUI), nameof(DevMessageUI.Update))]
    public static void Update(DevMessageUI __instance)
    {
        if (!__instance) return;

        __instance.parent.SetActive(false);
    }

    private string LocalPath(string name)
    {
        return Path.Join(Path.GetDirectoryName(Info.Location), name);
    }

    private void LoadTexture(string name, LocalizationType localizationThreshold)
    {
        var tex = new Texture2D(2, 2, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);

        tex.LoadImage(File.ReadAllBytes(LocalPath($"{name}.png")));

        TexturePatches[name] = new TexturePatch
        {
            Texture = tex,
            LocalizationThreshold = localizationThreshold,
        };

        Log.LogDebug($"Loaded texture {name}");
    }

    private void LoadTextures()
    {
        LoadTexture("Logo", LocalizationType.Roman);
        LoadTexture("LogoBlack", LocalizationType.Roman);
        LoadTexture("Logo_Blurred", LocalizationType.Roman);
    }

    private void LoadFont()
    {
        _neptunianShpreFont =
            TMP_FontAsset.CreateFontAsset(
                LocalPath("NeptunianShpre.ttf"),
                0,
                56,
                1,
                GlyphRenderMode.SDF,
                4096,
                4096);

        _neptunianShpreFont.getFontFeatures = true;
    }
    
    private IEnumerator LoadSoundEffect(string path, string key)
    {
        var sfx = ScriptableObject.CreateInstance<SFX_Instance>();

        var url = "file://" + LocalPath(path);
        
#pragma warning disable CS0618 // Type or member is obsolete
        using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
        
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            var clip = DownloadHandlerAudioClip.GetContent(www);
                
            clip.name = key;

            sfx.clips = [clip];
            sfx.settings = new SFX_Settings
            {
                volume = 0.5f,
                volume_Variation = 0,
                pitch = 1,
                pitch_Variation = 0.1f,
                spatialBlend = 1,
                dopplerLevel = 0.125f,
                range = 60,
                cooldown = 0.02f,
                maxInstances_NOT_IMPLEMENTED = 5
            };
            sfx.name = key;

            LoadedSoundEffects[key] = sfx;
        
            Log.LogDebug($"Loaded sound effect {path} as {key}, Length: {clip.length}");
        }
        else
        {
            Log.LogError("Failed to load: " + www.error);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private IEnumerator LoadBingBongSfx()
    {
        yield return LoadSoundEffect("audio/pingpangVO_AskFriends.ogg", "BB_AskYourFriends");
        yield return LoadSoundEffect("audio/pingpangVO_IfYouWanna.ogg", "BB_IfYouWanna");
        yield return LoadSoundEffect("audio/pingpangVO_IThinkThatsABadIdea.ogg", "BB_BadIdea");
        yield return LoadSoundEffect("audio/pingpangVO_Nah.ogg", "BB_Nah");
        yield return LoadSoundEffect("audio/pingpangVO_NoNoNo.ogg", "BB_NoNoNo");
        yield return LoadSoundEffect("audio/pingpangVO_Sure.ogg", "BB_Sure");
        yield return LoadSoundEffect("audio/pingpangVO_Yes.ogg", "BB_Yes");
        yield return LoadSoundEffect("audio/pingpangVO_DefNot.ogg", "BB_DefinitelyNot");
        yield return LoadSoundEffect("audio/pingpangVO_ImBingBong.ogg", "BB_ImBingBong");
        yield return LoadSoundEffect("audio/pingpangVO_Maybe.ogg", "BB_Maybe");
        yield return LoadSoundEffect("audio/pingpangVO_NoShort.ogg", "BB_No");
        yield return LoadSoundEffect("audio/pingpangVO_TakeMeWith.ogg", "BB_IfISayYes");
        yield return LoadSoundEffect("audio/pingpangVO_Y..Yeah.ogg", "BB_Yeah");
        yield return LoadSoundEffect("audio/pingpangVO_IDunno.ogg", "BB_IDunno");
        yield return LoadSoundEffect("audio/pingpangVO_IThinkItsFine.ogg", "BB_Fine");
        yield return LoadSoundEffect("audio/pingpangVO_MissesWife.ogg", "BB_IMissMyWife");
        yield return LoadSoundEffect("audio/pingpangVO_NOLong.ogg", "BB_INTENSENO");
        yield return LoadSoundEffect("audio/pingpangVO_OK.ogg", "BB_Okay");
        yield return LoadSoundEffect("audio/pingpangVO_YeahDef.ogg", "BB_Definitely");
        yield return LoadSoundEffect("audio/pingpangVO_Neptune.ogg", "BB_MayNeptuneBlessUsAll");
        yield return LoadSoundEffect("audio/pingpangVO_DontDoIt.ogg", "BB_DontDoIt");
        yield return LoadSoundEffect("audio/pingpangVO_IGuess.ogg", "BB_IGuessSo");
        yield return LoadSoundEffect("audio/pingpangVO_NotComfortable.ogg", "BB_NotComfortable");
        yield return LoadSoundEffect("audio/pingpangVO_NotSure.ogg", "BB_ImNotSure");
        yield return LoadSoundEffect("audio/pingpangVO_NuhUh.ogg", "BB_NuhUh");
        yield return LoadSoundEffect("audio/pingpangVO_PleaseDont.ogg", "BB_PleaseDont");
        yield return LoadSoundEffect("audio/pingpangVO_UHHH.ogg", "BB_Uh");
    }
    
    private IEnumerator CreateBingBongResponses()
    {
        var responses = new List<Action_AskBingBong.BingBongResponse>();

        yield return LoadBingBongSfx();
        
        void AddBingBongResponse(string key)
        {
            responses.Add(new Action_AskBingBong.BingBongResponse
            {
                subtitleID = key,
                sfx = LoadedSoundEffects[key],
                mouthCurve = null,
                mouthCurveTime = 0
            });
        }
        
        Log.LogDebug("Adding repsonses");

        try
        {
            AddBingBongResponse("BB_AskYourFriends");
            AddBingBongResponse("BB_IfYouWanna");
            AddBingBongResponse("BB_BadIdea");
            AddBingBongResponse("BB_Nah");
            AddBingBongResponse("BB_NoNoNo");
            AddBingBongResponse("BB_Sure");
            AddBingBongResponse("BB_Yes");
            AddBingBongResponse("BB_DefinitelyNot");
            AddBingBongResponse("BB_ImBingBong");
            AddBingBongResponse("BB_Maybe");
            AddBingBongResponse("BB_No");
            AddBingBongResponse("BB_IfISayYes");
            AddBingBongResponse("BB_Yeah");
            AddBingBongResponse("BB_IDunno");
            AddBingBongResponse("BB_Fine");
            AddBingBongResponse("BB_IMissMyWife");
            AddBingBongResponse("BB_INTENSENO");
            AddBingBongResponse("BB_Okay");
            AddBingBongResponse("BB_Definitely");
            AddBingBongResponse("BB_MayNeptuneBlessUsAll");
            AddBingBongResponse("BB_DontDoIt");
            AddBingBongResponse("BB_IGuessSo");
            AddBingBongResponse("BB_NotComfortable");
            AddBingBongResponse("BB_ImNotSure");
            AddBingBongResponse("BB_NuhUh");
            AddBingBongResponse("BB_PleaseDont");
            AddBingBongResponse("BB_Uh");
        }
        catch (Exception ex)
        {
            Log.LogError($"Got exception adding bing bon response {ex}");
        }

        _bingBongResponses = responses.ToArray();
    }

    [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
    [HarmonyPrefix]
    private static void GameObject_SetActive(GameObject __instance, bool value)
    {
        if (!__instance) return;

        lock (PatchedGameObjects)
        {
            if (!PatchedGameObjects.Add(__instance.GetInstanceID())) return;
        }

        try
        {
            // Manually patch out some strings
            var textComponents = __instance.GetComponentsInChildren<TMP_Text>();
            foreach (var textComponent in textComponents)
            {
                if (!textComponent)
                {
                    continue;
                }

                if (textComponent.text.ToUpperInvariant().Contains("CRABLAND"))
                {
                    Localization.GetLocalizedString(_localizationType.Value, "__NATIONALITY", out var localized);
                    textComponent.SetText(localized!);
                }
            }

            var spriteRenderers = __instance.GetComponentsInChildren<SpriteRenderer>(true);
            var renderers = __instance.GetComponentsInChildren<Renderer>(true).Concat(spriteRenderers);

            foreach (var renderer in renderers)
            {
                if (!renderer) continue;

                var materials = renderer.materials;
                foreach (var material in materials)
                {
                    PatchMaterial(material);
                }
            }

            foreach (var spriteRenderer in spriteRenderers)
            { 
                var sprite = spriteRenderer.sprite;
                var texture2D = sprite.texture;

                if (TexturePatches.TryGetValue(texture2D.name, out var patchedTexture) &&
                    _localizationType.Value >= patchedTexture.LocalizationThreshold)
                    spriteRenderer.sprite = Sprite.Create(patchedTexture.Texture,
                        new Rect(0, 0, patchedTexture.Texture.width, patchedTexture.Texture.height),
                        new Vector2(0.5f, 0.5f));
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to patch textures, got error {ex}");
        }
    }

    private static void PatchMaterial(Material? material)
    {
        if (!material) return;

        var textureNameIds = material.GetTexturePropertyNameIDs();
        foreach (var textureNameId in textureNameIds)
        {
            var texture = material.GetTexture(textureNameId);

            // Wrong texture type, we can't patch non Texture2D
            if (texture is not Texture2D texture2D) continue;

            // Log.LogDebug($"Seen texture {texture2D.name}");

            if (TexturePatches.TryGetValue(texture2D.name, out var patchedTexture) &&
                _localizationType.Value >= patchedTexture.LocalizationThreshold)
                material.SetTexture(textureNameId, patchedTexture.Texture);
        }

    }

    [HarmonyPatch(typeof(Image), "OnEnable")]
    [HarmonyPostfix]
    public static void Image_OnEnable(Image __instance)
    {
        if (!__instance) return;

        // Log.LogDebug($"Seen Image {__instance.mainTexture.name}");

        if (TexturePatches.TryGetValue(__instance.mainTexture.name, out var patchedTexture))
        {
            __instance.sprite = Sprite.Create(patchedTexture.Texture,
                new Rect(0, 0, patchedTexture.Texture.width, patchedTexture.Texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private static void SetFont(GameObject gameObject, bool neptunian)
    {
        // We only modify the font if we are localizing into Nahnya
        if (_localizationType.Value != LocalizationType.Nahnya) return;

        if (neptunian)
        {
            var cacheComponent = gameObject.GetOrAddComponent<FontCacheComponent>();
            var textComponent = gameObject.GetComponent<TMP_Text>();

            if (!cacheComponent.cached)
            {
                cacheComponent.font = textComponent.font;
                cacheComponent.lineSpacing = textComponent.lineSpacing;
                cacheComponent.fontStyle = textComponent.fontStyle;
                cacheComponent.cached = true;
            }

            textComponent.font = _neptunianShpreFont;
            textComponent.lineSpacing = cacheComponent.lineSpacing + 24.0f;
            textComponent.fontStyle &= ~FontStyles.LowerCase;
            if (!textComponent.fontFeatures.Contains(OTL_FeatureTag.liga))
                textComponent.fontFeatures.Add(OTL_FeatureTag.liga);
        }
        else
        {
            var cacheComponent = gameObject.GetComponent<FontCacheComponent>();
            var textComponent = gameObject.GetComponent<TMP_Text>();

            if (!cacheComponent || !cacheComponent.cached) return;

            textComponent.font = cacheComponent.font;
            textComponent.lineSpacing = cacheComponent.lineSpacing;
            textComponent.fontStyle = cacheComponent.fontStyle;
            cacheComponent.cached = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TMP_Text), "PopulateTextBackingArray", typeof(string), typeof(int), typeof(int))]
    [HarmonyPatch(typeof(TMP_Text), "PopulateTextBackingArray", typeof(StringBuilder), typeof(int), typeof(int))]
    [HarmonyPatch(typeof(TMP_Text), "PopulateTextBackingArray", typeof(char[]), typeof(int), typeof(int))]
    public static void TMP_Text_PopulateTextBackingArray(TMP_Text __instance, object[] __args)
    {
        if (!__instance) return;

        // We don't need to do hot-font patching when we aren't doing Nahnya in the first place
        if (_localizationType.Value != LocalizationType.Nahnya) return;

        try
        {
            var text = __args[0].ToString();

            // Set the font if this text is supposed to be Nahnya
            SetFont(__instance.gameObject, NahnyaToKey.ContainsKey(text));
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to patch text mesh font, got exception {ex}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Action_AskBingBong), nameof(Action_AskBingBong.Ask))]
    public static void Action_AskBingBong_OnEnable(Action_AskBingBong __instance, int index, bool spamming)
    {
        // Don't patch Bing Bong's responses if we aren't localizing into Neptunian
        if (!_enablePingPang.Value)
        {
            return;
        }

        __instance.responses = _bingBongResponses;

        if (_localizationType.Value == LocalizationType.Nahnya)
        {
            __instance.subtitles.font = _neptunianShpreFont;
            __instance.subtitles.lineSpacing += 24.0f;
            if (!__instance.subtitles.fontFeatures.Contains(OTL_FeatureTag.liga))
                __instance.subtitles.fontFeatures.Add(OTL_FeatureTag.liga);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizedText), nameof(LocalizedText.GetText), typeof(string), typeof(LocalizedText.Language))]
    [HarmonyPatch(typeof(LocalizedText), nameof(LocalizedText.GetText), typeof(string), typeof(bool))]
    public static void LocalizedText_GetText(object[] __args, ref string __result)
    {
        // Log.LogDebug($"Got arg {__args[0]}");

        if (Localization.GetLocalizedString(_localizationType.Value,
                ((string)__args[0]).ToUpperInvariant(),
                out var localized))
        {
            __result = localized;
        }
    }
}