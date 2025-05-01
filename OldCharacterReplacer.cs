using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RDLevelEditor;
using System.Reflection;

namespace OldCharacterReplacer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
// i don't know what
#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class OldCharacterReplacer : BaseUnityPlugin
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    public static string path = AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\ocrData";
    // illegal characters
    public static string dictionaryPrefix = @"|?#<OCR>#?|\/";
    public static MethodInfo loadTexture2D;

    internal static ManualLogSource logger;

    private ConfigEntry<bool> charReplacement;

    private void Awake()
    {
        logger = Logger;
        
        loadTexture2D = typeof(RDEditorUtils).GetMethod("LoadTextureFileToCache", BindingFlags.Static | BindingFlags.NonPublic);

        charReplacement = Config.Bind("General", "ReplaceCharacters", true, "Whether to replace characters or not with older versions if the level version matches.");
        logger.LogMessage($"Old Character Replacer plugin loaded! Will {(charReplacement.Value ? "" : "not ")}replace characters with older versions when appropriate!");

        Harmony instance = new("patcher");
        if (charReplacement.Value)
        {
            instance.PatchAll(typeof(CharSetupPatch));
            instance.PatchAll(typeof(ChangeCharPatch));
            instance.PatchAll(typeof(DialoguePatch));
        }
    }
}