using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains;
using GameData.Utilities;
using GameData.Common;
using Redzen.Random;
using System;
using System.Collections.Generic;
using Config;
using Character = GameData.Domains.Character.Character;
using LifeSkillType = GameData.Domains.Character.LifeSkillType;
using CombatSkillType = GameData.Domains.CombatSkill.CombatSkillType;

namespace NewTaiwuModifir
{
    [PluginConfig("NewTaiwuModifier", "EveningTwilight", "1.0.3")]
    public class NewTaiwuModifier : TaiwuRemakePlugin
    {
        static Harmony harmony;
        static int lifeSkillGrowthType;
        static int combatSkillGrowthType;
        static int minLifeSkillAvg;
        static int minCombatSkillAvg;
        static int minBasicFeatures;
        static int minGoodBasicFeatures;
        static int maxBadBasicFeatures;
        static bool closeFriendSync;

        public override void Dispose()
        {
            AdaptableLog.Info($"[NewTaiwuModifier] NewTaiwuModifier_Backend Dispose");
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            AdaptableLog.Info($"[NewTaiwuModifier] NewTaiwuModifier_Backend Initialize");
            readSettings();

            harmony = new Harmony(GetGuid());
            harmony.PatchAll(typeof(NewTaiwuModifier));
        }


        public override void OnModSettingUpdate()
        {
            readSettings();
        }

        public void readSettings()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "lifeSkillGrowthType", ref lifeSkillGrowthType);
            DomainManager.Mod.GetSetting(ModIdStr, "combatSkillGrowthType", ref combatSkillGrowthType);
            DomainManager.Mod.GetSetting(ModIdStr, "minLifeSkillAvg", ref minLifeSkillAvg);
            DomainManager.Mod.GetSetting(ModIdStr, "minCombatSkillAvg", ref minCombatSkillAvg);
            DomainManager.Mod.GetSetting(ModIdStr, "minBasicFeatures", ref minBasicFeatures);
            DomainManager.Mod.GetSetting(ModIdStr, "minGoodBasicFeatures", ref minGoodBasicFeatures);
            DomainManager.Mod.GetSetting(ModIdStr, "maxBadBasicFeatures", ref maxBadBasicFeatures);
            DomainManager.Mod.GetSetting(ModIdStr, "closeFriendSync", ref closeFriendSync);

            AdaptableLog.Info($"[NewTaiwuModifier] OnModSettingUpdate: lifeSkill:{{growth:{lifeSkillGrowthType},minAvg:{minLifeSkillAvg}}},combat:{{growth:{combatSkillGrowthType},minAvg:{minCombatSkillAvg}}}," +
                $"Features:{{minBasic:{minBasicFeatures},minGood:{minGoodBasicFeatures},maxBad:{maxBadBasicFeatures}}}");
        }

        /// <summary>
        /// 将设置里的SkillQualificationGrowthType转换为GameData的常量（SafeCast）
        /// </summary>
        /// <param name="settingsGrowthType"></param>
        /// <returns>SkillQualificationGrowthType</returns>
        private static int TranselateSettingsGrowthType(int settingsGrowthType)
        {
            var growthType = SkillQualificationGrowthType.Average;
            switch (settingsGrowthType)
            {
                case 1:
                    growthType = SkillQualificationGrowthType.Precocious;
                    break;
                case 2:
                    growthType = SkillQualificationGrowthType.Average;
                    break;
                case 3:
                    growthType = SkillQualificationGrowthType.LateBlooming;
                    break;
            }
            return growthType;
        }

        /// <summary>
        /// 检查资质是否满足最低均值条件，不够的话，把差额随机分配到各个技能
        /// </summary>
        /// <param name="skillItems">资质数组</param>
        /// <param name="skillCount">资质数组的技能种类count</param>
        /// <param name="minimumAvgQualifications">最低平均资质</param>
        private static unsafe void CheckMinimunAvgQualificationsAndFixIfNeed(short* skillItems, int skillCount, int minimumAvgQualifications, IRandomSource random)
        {
            int expectedTotalPoints = minimumAvgQualifications * skillCount;
            int curTotalPoints = 0;
            for (int i = 0; i < skillCount; i++)
            {
                curTotalPoints += skillItems[i];
            }

            if (curTotalPoints >= expectedTotalPoints)
            {
                return;
            }

            short[] originSkillItems = new short[16];
            for (int i = 0; i < skillCount; i++)
            {
                originSkillItems[i] = skillItems[i];
            }
            AdaptableLog.Info($"[NewTaiwuModifier] 最低总资质:{expectedTotalPoints} 实际总资质:{curTotalPoints} 开始修正");

            /// 需要补足的资质点数
            int needToFix = expectedTotalPoints - curTotalPoints;
            /// 循环，每次补一点资质，分配到一个随机的技能
            while (needToFix > 0)
            {
                int hitSkill = random.Next(skillCount);
                ++skillItems[hitSkill];
                --needToFix;
            }

            AdaptableLog.Info($"[NewTaiwuModifier] 资质修正:{{");
            for (int i = 0; i < skillCount; i++)
            {
                AdaptableLog.Info($"[NewTaiwuModifier]  {originSkillItems[i]}  =>  {skillItems[i]}");
            }
            AdaptableLog.Info($"[NewTaiwuModifier]          }}");
        }

        public static unsafe void CharacterSetQualifications(Character __instance, DataContext context)
        {
            /// 设置指定成长类型
            if (lifeSkillGrowthType > 0)
            {
                var lifeSkillGrowthType_ = TranselateSettingsGrowthType(lifeSkillGrowthType);
                Traverse.Create(__instance).Field("_lifeSkillQualificationGrowthType").SetValue((sbyte)lifeSkillGrowthType_);
            }
            if (combatSkillGrowthType > 0)
            {
                var combatSkillGrowthType_ = TranselateSettingsGrowthType(combatSkillGrowthType);
                Traverse.Create(__instance).Field("_combatSkillQualificationGrowthType").SetValue((sbyte)combatSkillGrowthType_);
            }

            IRandomSource random = context.Random;

            /// 技艺资质
            int lifeSkillCount = LifeSkillType.Count;
            LifeSkillShorts lifeSkillShorts = Traverse.Create(__instance).Field("_baseLifeSkillQualifications").GetValue<LifeSkillShorts>();
            short* lifeSkillPtr = lifeSkillShorts.Items;
            CheckMinimunAvgQualificationsAndFixIfNeed(lifeSkillPtr, lifeSkillCount, minLifeSkillAvg, random);
            Traverse.Create(__instance).Field("_baseLifeSkillQualifications").SetValue(lifeSkillShorts);

            /// 武学资质
            int combatSkillCount = CombatSkillType.Count;
            CombatSkillShorts combatSkillShorts = Traverse.Create(__instance).Field("_baseCombatSkillQualifications").GetValue<CombatSkillShorts>();
            short* combatSkillPtr = combatSkillShorts.Items;
            CheckMinimunAvgQualificationsAndFixIfNeed(combatSkillPtr, combatSkillCount, minCombatSkillAvg, random);
            Traverse.Create(__instance).Field("_baseCombatSkillQualifications").SetValue(combatSkillShorts);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "OfflineSetCloseFriendFields")]
        public static void Character_OfflineSetCloseFriendFields_Postfix(Character __instance, DataContext context, short morality)
        {
            if (!closeFriendSync)
            {
                return;
            }
            CharacterSetQualifications(__instance, context);
        }

        /// <summary>
        /// Hook创建太吾的方法，在完成默认的创建后，处理mod的逻辑
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "OfflineCreateProtagonist")]
        public static unsafe void Character_OfflineCreateProtagonist_Post(Character __instance, ProtagonistCreationInfo info, DataContext context)
        {
            if (info.InscribedChar != null) // 不是新开局 啥也不干
            {
                return;
            }
            CharacterSetQualifications(__instance, context);
        }

        public static unsafe void GenerateRandomBasicFeatures_Hook(Character instance, DataContext context, Dictionary<short, short> featureGroup2Id, bool isProtagonist = false, bool allGoodBasicFeatures = false)
        {
            AdaptableLog.Info($"[NewTaiwuModifier] GenerateRandomBasicFeatures_Hook Entered:");
            AdaptableLog.Info($"[NewTaiwuModifier] instance:{instance}");
            AdaptableLog.Info($"[NewTaiwuModifier] isProtagonist:{isProtagonist}");
            AdaptableLog.Info($"[NewTaiwuModifier] allGoodBasicFeatures:{allGoodBasicFeatures}");
            IRandomSource random = context.Random;
            int basicFeaturesCount = (allGoodBasicFeatures ? 5 : Traverse.Create(instance).Method("GenerateRandomBasicFeaturesCount", context.Random).GetValue<int>());
            basicFeaturesCount = basicFeaturesCount > minBasicFeatures ? basicFeaturesCount : minBasicFeatures;
            foreach (KeyValuePair<short, short> item in featureGroup2Id)
            {
                short featureId3 = item.Value;
                if (CharacterFeature.Instance[featureId3].Basic)
                {
                    basicFeaturesCount--;
                }
            }
            if (basicFeaturesCount <= 0)
            {
                return;
            }

            int goodFeatureCount = 0;
            int badFeatureCount = 0;
            SByte gender = Traverse.Create(instance).Field("_gender").GetValue<SByte>();
            int goodFeaturesPotential = random.Next(101);
            while (basicFeaturesCount > 0 || goodFeatureCount < minGoodBasicFeatures)   // 总数还不够 || 好的还不够
            {
                if (badFeatureCount >= maxBadBasicFeatures)
                {
                    allGoodBasicFeatures = true;    // 坏的够多了，只生成好的
                }
                if (allGoodBasicFeatures || random.CheckPercentProb(goodFeaturesPotential))
                {
                    var (groupId, featureId) = CharacterDomain.GetRandomBasicFeature(random, isProtagonist, gender, isPositive: true, featureGroup2Id);
                    if (featureId >= 0)
                    {
                        featureGroup2Id.Add(groupId, featureId);
                        goodFeaturesPotential -= 20;
                        ++goodFeatureCount;
                    }
                }
                else
                {
                    var (groupId2, featureId2) = CharacterDomain.GetRandomBasicFeature(random, isProtagonist, gender, isPositive: false, featureGroup2Id);
                    if (featureId2 >= 0)
                    {
                        featureGroup2Id.Add(groupId2, featureId2);
                        goodFeaturesPotential += 20;
                        ++badFeatureCount;
                    }
                }
                --basicFeaturesCount;
            }
            AdaptableLog.Info($"[NewTaiwuModifier] GenerateRandomBasicFeatures_Hook Exit.");
        }

        /*[HarmonyPostfix, HarmonyPatch(typeof(Charact\\\, "OfflineCreateProtagonistRandomFeatures")]
        public static void OfflineCreateProtagonistRandomFeatures()
        {
            AdaptableLog.Info($"[NewTaiwuModifier] OfflineCreateProtagonistRandomFeatures Entered:");
        }*/

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "GenerateRandomBasicFeatures")]
        public static bool Character_GenerateRandomBasicFeatures_Prefix(Character __instance, DataContext context, Dictionary<short, short> featureGroup2Id, bool isProtagonist = false, bool allGoodBasicFeatures = false)
        {
            if (!isProtagonist)
            {
                return true;    // 不做Hook 让系统处理
            }
            AdaptableLog.Info($"[NewTaiwuModifier] GenerateRandomBasicFeatures Entered:");
            GenerateRandomBasicFeatures_Hook(__instance, context, featureGroup2Id, isProtagonist, allGoodBasicFeatures);
            return false;   // Hook 咱们自己处理
        }

        /*[HarmonyTranspiler, HarmonyPatch(typeof(Character), "OfflineCreateProtagonistRandomFeatures")]
        public static IEnumerable<CodeInstruction> Character_OfflineCreateProtagonistRandomFeatures_Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            var methodInfo = AccessTools.Method(typeof(NewTaiwuModifier_Backend), nameof(GenerateRandomBasicFeatures_Hook),
                new System.Type[] { typeof(Character), typeof(DataContext), typeof(Dictionary<short, short>), typeof(bool), typeof(bool) });
            var originMethod = AccessTools.Method(typeof(Character), "GenerateRandomBasicFeatures",
                new System.Type[] { typeof(Character), typeof(DataContext), typeof(Dictionary<short, short>), typeof(bool), typeof(bool) });
            var methodInfo = AccessTools.Method(typeof(GameData.Domains.Character.Character), nameof(GenerateRandomBasicFeatures),
                new System.Type[] { typeof(GameData.Domains.Character.Character), typeof(DataContext), typeof(Dictionary<short, short>), typeof(bool), typeof(bool) });
            instructions = new CodeMatcher(instructions)
                .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(OpCodes.Call, typeof(Character).GetMethod("GenerateRandomBasicFeatures"))
                ).Repeat(matcher => // Do the following for each match
                    matcher.SetInstructionAndAdvance(
                        new CodeInstruction(OpCodes.Call, methodInfo)
                    ).Advance(1)
                ).InstructionEnumeration();
            return instructions;
            var codes = instructions.ToList();
            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Call && code.operand == originMethod)
                {
                    yield return new CodeInstruction(code.opcode, methodInfo);
                }
            }
            yield return (CodeInstruction)codes.AsEnumerable();
        }*/
    }
}