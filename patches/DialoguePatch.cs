using HarmonyLib;
using System;
using RDLevelEditor;
using System.Text.RegularExpressions;

namespace OldCharacterReplacer;

#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class OldCharacterReplacer
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    [HarmonyPatch(typeof(LevelEvent_ShowDialogue), nameof(LevelEvent_ShowDialogue.Prepare))]
    public class DialoguePatch
    {
        public static void Prefix(LevelEvent_ShowDialogue __instance)
        {
            if (!OCRUtils.IsCustomLevel())
                return;

            string[] array = __instance.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in array)
            {
                int colonIndex = line.IndexOf(":");
                if (colonIndex == -1)
                    continue;

                string nameWithMaybeExpr = line[..colonIndex].Trim();
                int exprIndex = nameWithMaybeExpr.IndexOf("_");
                string charName = nameWithMaybeExpr;

                // does using discard _ modify the variable "in place" ?
                if (exprIndex != -1)
                    charName = charName[..exprIndex];

                if (!Enum.TryParse(typeof(Character), charName, out var uncastedCharacter))
                    continue;

                var character = (Character)uncastedCharacter;
                var oldChar = OCRUtils.GetOldCharacter(character);
                if (character == oldChar.character)
                    continue;

                var replacedCharName = oldChar.character.ToString();
                if (oldChar.character == Character.Custom)
                    replacedCharName = oldChar.customCharacterName;

                Regex regex = new($"^{character}(_[A-Za-z0-9]*)?:", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                string replaceText = $"{replacedCharName}$1:";
                // this must be old paige and have no expression, if so, we set to conversing as to not look weird
                if (character == Character.Paige && exprIndex == -1)
                    replaceText = replaceText.Replace("$1", "_conversing");
                __instance.text = regex.Replace(__instance.text, replaceText);
            }
            return;
        }
    }
}

