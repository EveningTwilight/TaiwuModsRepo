using Config;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;
using Debug = UnityEngine.Debug;
using GuiBaseUI;
using UnityEngine.UI;

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
            /*harmony.PatchAll(typeof(NewGameModifierUI));*/
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

/*    public static class TaiwuFeatureLazyModifer
    {
        [HarmonyPostfix, HarmonyPatch(typeof(UI_NewGame), "Awake")]
        public static void UI_NewGame_Awake_Postfix()
        {
            FeatureManager.Init();
            FeatureManager.SetFilePath();
        }
    }*/

    /*public static class NewGameModifierUI
    {
        private static UI_MainView mainView;
        private static CButton entryButton;
        private static FeatureContainerView featureContainerView;

        public static bool PrintFeatureItem(CharacterFeatureItem item)
        {
            if (item != null)
            {
                string name = item.Name;
                int TemplateId = item.TemplateId;
                int CandidateGroupId = item.CandidateGroupId;
                int MutexGroupId = item.MutexGroupId;
                int level = item.Level;
                bool isBasic = item.Basic;
                int gender = item.Gender;
                string desc = item.Desc;
                Debug.Log(string.Format("[CharacterFeatureItem]\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", name, TemplateId, CandidateGroupId, MutexGroupId, level, isBasic, gender, desc));
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_NewGame), "Awake")]
        public static void UI_NewGame_Awake_Postfix()
        {
            Debug.Log("UI_NewGame_Awake_Postfix");
            *//*if (featureContainerView == null)
            {
                Debug.Log("Create featureContainerView");
                GameObject root = GameObject.Find("Camera_UIRoot/Canvas");
                GameObject go = CreateUI.NewCanvas(int.MinValue);
                go.layer = root.layer;
                go.transform.SetParent(root.transform.Find("LayerVeryTop"), false);
                featureContainerView = go.AddComponent<FeatureContainerView>();
                featureContainerView.Init(root);

            }
            featureContainerView.gameObject.SetActive(true);*/
            /*CharacterFeature.Instance.Iterate(PrintFeatureItem);*//*
            if (mainView == null)
            {
                GameObject root = GameObject.Find("Camera_UIRoot/Canvas");
                GameObject go = CreateUI.NewCanvas(int.MinValue);
                go.layer = root.layer;
                go.transform.SetParent(root.transform.Find("LayerVeryTop"), false);
                mainView = go.AddComponent<UI_MainView>();
                mainView.Init(root);
            }
            mainView.gameObject.SetActive(true);
        }
    }*/

            /*public class UI_EntryButton : MonoBehaviour
            {
                private Image bg;

                public void Init()
                {
                    CToggle toggle = GetComponent<CToggle>();
                }

                private void OnGUI()
                {

                }
            }*/
}