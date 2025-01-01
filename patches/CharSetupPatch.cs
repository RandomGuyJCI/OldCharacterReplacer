using HarmonyLib;

namespace OldCharacterReplacer;

#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public partial class OldCharacterReplacer
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    [HarmonyPatch(typeof(scnGame), nameof(scnGame.MakeRow))]
    public class CharSetupPatch
    {
        public static void Prefix(ref Character character, ref string customCharacter)
        {
            if (!OCRUtils.IsCustomLevel())
                return;

            CharacterPlusCustom oldChar = OCRUtils.GetOldCharacter(character);
            if (character == oldChar.character)
                return;

            character = oldChar.character;
            if (character == Character.Custom)
                customCharacter = oldChar.customCharacterName;
        }
    }
}

