using Config;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TaiwuModdingLib.Core.Plugin;
using Debug = UnityEngine.Debug;
using UnityEngine;

namespace NewTaiwuModifier_Front
{
    [PluginConfig("NewTaiwuModifier_Front", "EveningTwilight", "1.0.0")]
    public class NewTaiwuModifier_Front : TaiwuRemakePlugin
    {
        private static Harmony harmony;
        public static bool freeTrait = false;      // 免费特质

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
        public override void Initialize()
        {
            readSettings();

            harmony = new Harmony(GetGuid());
            /*harmony.PatchAll(typeof(NewTaiwuModifier_Front));*/
            harmony.PatchAll(typeof(TraitCostToZero));
            harmony.PatchAll(typeof(TraitCostNoZeroCheck));
            harmony.PatchAll(typeof(TaiwuFeatureLazyModifer));
        }
        public override void OnModSettingUpdate()
        {
            readSettings();
        }
        public void readSettings()
        {
            ModManager.GetSetting(ModIdStr, "freeTrait", ref freeTrait);
        }

    }

    public static class TraitCostToZero
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(UI_NewGame).GetMethod("OnAbilityClick", (BindingFlags)(-1));
            yield return typeof(UI_NewGame).GetMethod("OnCellItemRender", (BindingFlags)(-1));
            yield return typeof(UI_NewGame).GetMethod("UpdatePoints", (BindingFlags)(-1));
        }
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            if (NewTaiwuModifier_Front.freeTrait)
            {
                Debug.Log("start Transpiler freeTrait");
                instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, typeof(ProtagonistFeatureItem).GetField("Cost"))
                ).Repeat(matcher =>
                    matcher
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Pop)
                    )
                    .SetAndAdvance(
                        OpCodes.Ldc_I4_0, null
                    )
                ).InstructionEnumeration();
                instructions = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldfld, typeof(ProtagonistFeatureItem).GetField("PrerequisiteCost"))
                    ).Repeat(matcher =>
                        matcher
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Pop)
                        )
                        .SetAndAdvance(
                            OpCodes.Ldc_I4_0, null
                        )
                    ).InstructionEnumeration();
            }
            return instructions;
        }
    }
    public static class TraitCostNoZeroCheck
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(UI_NewGame).GetMethod("OnStartNewGame", (BindingFlags)(-1));
        }
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            if (NewTaiwuModifier_Front.freeTrait)
            {
                Debug.Log("start Transpiler OnStartNewGame");
                instructions = new CodeMatcher(instructions)
                    .MatchForward(false,
                        /*new CodeMatch(OpCodes.Ldc_I4_S, 10)*/
                        new CodeMatch(OpCodes.Clt, null)
                    )
                    .Repeat(matcher => // Do the following for each match
                        matcher
                        .SetAndAdvance(
                            OpCodes.Cgt, null
                        /*OpCodes.Ldc_I4_0, null*/
                        )
                    ).InstructionEnumeration();
                /*int i = 0;
                foreach (CodeInstruction instruction in instructions)
                {
                    Debug.Log(string.Format("Transpiler: {0} {1} {2}", i++, instruction.opcode, instruction.operand));
                }*/
            }
            return instructions;
        }
    }

    public static class TaiwuFeatureLazyModifer
    {
        [HarmonyPostfix, HarmonyPatch(typeof(UI_NewGame), "Awake")]
        public static void UI_NewGame_Awake_Postfix()
        {
            /*CharacterModifier.Init();*/
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_NewGame), "OnClickConfirmInscribedChar")]
        public static void UI_NewGame_OnClickConfirmInscribedChar_Postfix()
        {
            CharacterModifier.Show();
        }
    }
}