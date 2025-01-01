using HarmonyLib;

namespace OldCharacterReplacer;

#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class OldCharacterReplacer
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    public class ChangeCharPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(scrChar), nameof(scrChar.ChangeCharacter))]
        public static void CCPostfix(scrChar __instance, Character newChar)
        {
            if (!OCRUtils.IsCustomLevel())
                return;
 
            CharacterPlusCustom oldChar = OCRUtils.GetOldCharacter(newChar);
            if (newChar == oldChar.character)
                return;

            __instance.character = oldChar.character;
            __instance.customAnimation.enabled = oldChar.character == Character.Custom;
            if (__instance.customAnimation.enabled)
            {
                __instance.shaderRenderer.material.SetPalette("");
                Level_Custom level_Custom = scnGame.instance.currentLevel as Level_Custom;
                __instance.customAnimation.data = level_Custom.customCharacterData[oldChar.customCharacterName];
                __instance.customAnimation.animationCompleted = __instance.OnCustomAnimEnd;
                __instance.customAnimation.enabled = true;
            }
            else
            {
                __instance.charName = oldChar.character.ToString();
                __instance.SetPaletteNum(0);
            }
            __instance.PlayExpression("neutral");
            return;
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

            __instance.character.shaderRenderer.material.SetPalette("");   
            if (oldChar.character != Character.Custom)
                __instance.character.shaderRenderer.material.SetPalette(oldChar.character.ToString());
            __instance.character.shaderDataSource = __instance.character;
            __instance.character.UpdateShaderDataSource();
            __instance.character.shaderData.CopyFrom(__instance.shaderData);
        }
    }
}

