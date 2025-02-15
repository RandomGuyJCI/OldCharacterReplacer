using System;
using System.Collections.Generic;
using System.IO;
using RDLevelEditor;
using UnityEngine;

namespace OldCharacterReplacer;

public class OCRUtils
{
    // TODO: Get a proper date on:
    // TODO: - Miner -> 43 -> olderMiner
    public static List<object[]> chars = [
        // these are just for dialogue portraits
        [Character.HoodieBoy, 30, Character.Custom, "olderCole"],
        [Character.Farmer, 30, Character.Custom, "oldFarmer"],
        
        // this is when early access happened so massive spike here in changes
        [Character.Samurai, 43, Character.Custom, "oldSamurai"],
        [Character.SamuraiBoss, 43, Character.Custom, "oldInsomniac"],
        [Character.Boy, 43, Character.Custom, "oldLogan"],
        [Character.Girl, 43, Character.Custom, "oldHailey"],
        [Character.HoodieBoy, 43, Character.Custom, "oldCole"],
        [Character.HoodieBoyBlue, 43, Character.Custom, "oldColeBlue"],
        [Character.Miner, 43, Character.Custom, "olderMiner"],
        [Character.Ian, 43, Character.Custom, "oldIan"],
        
        [Character.Bodybuilder, 55, Character.Custom, "oldBodybuilder"],
        
        [Character.Miner, 56, Character.Custom, "oldMiner"],
        [Character.Paige, 56, Character.Custom, "oldPaige"],
    ];

    public static CharacterPlusCustom GetOldCharacter(Character character)
    {
        if (!IsCustomLevel())
            return new(character, null);

        Character oldChar = character;
        string customPath = null;
        int version = RDLevelData.current.settings.version;

        foreach (var list in chars)
        {
            if (character != (Character)list[0])
                continue;

            if (version >= (int)list[1])
                continue;

            oldChar = (Character)list[2];
            customPath = (string)list[3];
            break;
        }

        if (customPath != null)
            CreateCustomCharacter(customPath);
        return new(oldChar, OldCharacterReplacer.dictionaryPrefix + customPath);
    }

    // LevelEvent_MakeRow.UpdateCustomCharacter
    public static void CreateCustomCharacter(string name)
    {
        if (!IsCustomLevel())
            return;

        var path = OldCharacterReplacer.path + $@"\{name}\";
        var dictionaryPrefix = OldCharacterReplacer.dictionaryPrefix;
        var ccd = (scnGame.instance.currentLevel as Level_Custom).customCharacterData;
        var key = dictionaryPrefix + name;
        if (ccd.ContainsKey(key))
            return;

        Texture2D img = LoadTex2D($@"{path}\{name}.png", key + "normal");
        Texture2D outline = LoadTex2D($@"{path}\{name}_outline.png", key + "outline");
        Texture2D glow = LoadTex2D($@"{path}\{name}_glow.png", key + "glow");
        Texture2D freeze = LoadTex2D($@"{path}\{name}_freeze.png", key + "freeze");

        string jsonTxt = File.ReadAllText($"{path}\\{name}.json");
        img.filterMode = FilterMode.Point;
        img.wrapMode = TextureWrapMode.Clamp;

        Level_Custom.LoadCustomCharacter(ccd, key, jsonTxt, img, outline, glow, freeze);
    }

    private static Texture2D LoadTex2D(string path, string key)
    {
        if (scrVfxControl.textureCache.TryGetValue(key, out CachedTexture tex))
            return tex.texture;
            
        return (Texture2D)OldCharacterReplacer.loadTexture2D.Invoke(null, [scrVfxControl.textureCache, path, key]);
    }

    public static bool IsCustomLevel()
    {
        return scnGame.levelToLoadSource == LevelSource.ExternalPath;
    }
}

public struct CharacterPlusCustom(Character chr, string ccp)
{
    public Character character = chr;
    public string customCharacterName = ccp;
}