using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using RDLevelEditor;
using UnityEngine;
using static tk2dSpriteAnimator;

namespace OldCharacterReplacer;

#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class OldCharacterReplacer
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    public class ChangeCharPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(scrChar), nameof(scrChar.ChangeCharacter))]
        public static bool CCPrefix(scrChar __instance, Character newChar)
        {
            if (!OCRUtils.IsCustomLevel())
                return true;
 
            CharacterPlusCustom oldChar = OCRUtils.GetOldCharacter(newChar);
            if (newChar == oldChar.character)
                return true;

            __instance.animPrefixName = "";
            __instance.charName = "";
            __instance.expressionPrefix = "";
            __instance.neutralAnimName = "neutral";
            __instance.barelyAnimName = "barely";
            __instance.missedAnimName = "missed";
            __instance.happyAnimName = "happy";
            __instance.shaderRenderer.material.SetPalette("");
            __instance.customAnimation.enabled = false;
            __instance.customAnimation.currentClip = null;

            if (oldChar.character == Character.Custom)
                __instance.Setup(oldChar.character, "", oldChar.customCharacterName);
            else
                __instance.Setup(oldChar.character);
            
            return false;
        }

        // messes with palette so gotta mess back
        [HarmonyPostfix]
        [HarmonyPatch(typeof(scrRowEntities), nameof(scrRowEntities.ChangeCharacter))]
        public static void CCRowEntPostfix(scrRowEntities __instance, Character newChar)
        {
            if (!OCRUtils.IsCustomLevel())
                return;
 
            CharacterPlusCustom oldChar = OCRUtils.GetOldCharacter(newChar);
            if (newChar == oldChar.character)
                return;

            if (oldChar.character != Character.Custom)
                __instance.character.shaderRenderer.material.SetPalette(oldChar.character.ToString());
            else
                __instance.character.shaderRenderer.material.SetPalette("");   
            __instance.character.shaderDataSource = __instance.character;
            __instance.character.UpdateShaderDataSource();
            __instance.character.shaderData.CopyFrom(__instance.shaderData);
        }

        // preload...
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelEvent_CallCustomMethod), nameof(LevelEvent_CallCustomMethod.Run))]
        public static void CCMPrefix(LevelEvent_CallCustomMethod __instance)
        {
            string code = __instance.methodName;
            code = RemoveTextBeginning(code, "game.");
            code = RemoveTextBeginning(code, "level.");
            if (!code.StartsWith("ChangeCharacter("))
                return;

            code = RemoveTextBeginning(code, "ChangeCharacter(");
            string characterStr = "";
            if (code.StartsWith("\""))
            {
                Match speechMarks = Regex.Match(code, @"^""([A-Za-z0-9]*)"".*", RegexOptions.Multiline);
                if (speechMarks.Index >= 0 && speechMarks.Groups.Count == 2)
                    characterStr = speechMarks.Groups[1].Value;
            }
            else if (code.StartsWith("str:"))
            {
                code = RemoveTextBeginning(code, "str:");
                characterStr = code[..code.IndexOf(',')];
            }
            else 
                return;
            
            if (!Enum.TryParse(typeof(Character), characterStr, out var uncastedCharacter))
                return;

            Character character = (Character)uncastedCharacter;
            OCRUtils.GetOldCharacter(character); // we just need to get it initially
        }   

        // idk ??
        public static string RemoveTextBeginning(string str, string token)
        {
            if (str.StartsWith(token))
                return str.RemoveTextIfStartsWith(token);
            return str;
        }
    }
}

