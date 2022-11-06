using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Utilities;
using TaiwuModdingLib.Core.Plugin;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System;
using Redzen.Random;
using GameData.Common;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.ParallelModifications;
using GameData.Domains.Organization;

namespace LesLegends
{
    public static class Logger
    {
        public static void Log(string str)
        {
            AdaptableLog.Info(str);
        }

        public static void DebugLog(string str)
        {
            if (LesLegends.debug)
            {
                Log(str);
            }
        }
    }

    [PluginConfig("姬友传说", "EveningTwilight", "1.1.1")]
    public class LesLegends : TaiwuRemakePlugin
    {
        static Harmony harmony;
        static Random random;
        /// <summary>
        /// 以下为设置
        static bool closeFriendFixBisexual = false;     // 太吾和谷中密友同性且为双性恋
        static bool presetNPCFixBisexual = false;       // 隐秘小村NPC为双性恋(仅太吾与太吾性别相同时)
        static bool keepVirginity = false;              // 保持纯洁
        static bool forceSexualRateForAll = false;      // 新生儿概率调整对所有NPC生效
        static int fixMotherType = 0;                   // 固定太吾为母亲? 0:随机 1:太吾 2:!太吾
        static bool sameSexualFromParents = false;      // 同性父母子女性别相同
        static bool bisexualFromParents = false;        // 双性恋子女必双性恋
        static int newBornsFemaleRate = 50;             // 新生儿女性概率
        static int newBornsBisexualRate = 20;           // 新生儿双性恋概率
        static int fixCricketLuckType = 0;              // 太吾生蛐蛐调整
        static int minSexHitProb = 20;                  // 同性春宵最低命中率
        static int maxSexHitProb = 20;                  // 同性春宵最高命中率
        public static bool debug = false;               // debug模式 打印日志
        /// </summary>

        // static bool forceBisexualAsSame = false;        // 强制双性恋为同性恋(暂不支持)(好像现在就是同，不考虑x生y相的话，没有双[那可真是太棒了])

        public override void Dispose()
        {
            Logger.DebugLog($"[LesLegends] Dispose");
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            Logger.DebugLog($"[LesLegends] Initialize");
            ReadSettings();

            harmony ??= new Harmony(base.GetGuid());
            harmony.PatchAll(typeof(LesLegends));
            harmony.PatchAll(typeof(BiSCloseFriendTranspiler));
            harmony.PatchAll(typeof(BisexualLovePatch));
            random = new Random(Environment.TickCount);
        }

        public override void OnModSettingUpdate()
        {
            Logger.DebugLog($"[LesLegends] OnModSettingUpdate");
            ReadSettings();
        }

        public void ReadSettings()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "closeFriendFixBisexual", ref closeFriendFixBisexual);
            DomainManager.Mod.GetSetting(ModIdStr, "presetNPCFixBisexual", ref presetNPCFixBisexual);
            DomainManager.Mod.GetSetting(ModIdStr, "keepVirginity", ref keepVirginity);
            DomainManager.Mod.GetSetting(ModIdStr, "forceSexualRateForAll", ref forceSexualRateForAll);
            DomainManager.Mod.GetSetting(ModIdStr, "fixMotherType", ref fixMotherType);
            DomainManager.Mod.GetSetting(ModIdStr, "sameSexualFromParents", ref sameSexualFromParents);
            DomainManager.Mod.GetSetting(ModIdStr, "bisexualFromParents", ref bisexualFromParents);
            DomainManager.Mod.GetSetting(ModIdStr, "newBornsFemaleRate", ref newBornsFemaleRate);
            DomainManager.Mod.GetSetting(ModIdStr, "newBornsBisexualRate", ref newBornsBisexualRate);
            DomainManager.Mod.GetSetting(ModIdStr, "fixCricketLuckType", ref fixCricketLuckType);
            DomainManager.Mod.GetSetting(ModIdStr, "minSexHitProb", ref minSexHitProb);
            DomainManager.Mod.GetSetting(ModIdStr, "maxSexHitProb", ref maxSexHitProb);
            DomainManager.Mod.GetSetting(ModIdStr, "debug", ref debug);
            
            // DomainManager.Mod.GetSetting(ModIdStr, "forceSexualRateForAll", ref forceSexualRateForAll);  // 新生儿概率调整对所有NPC生效
            // DomainManager.Mod.GetSetting(ModIdStr, "forceBisexualAsSame", ref forceBisexualAsSame);      // 强制双性恋为同性恋(暂不支持)

            // 约束一下范围，虽然其实无所谓？
            newBornsFemaleRate = MathUtils.Clamp(newBornsFemaleRate, 0, 100);
            newBornsBisexualRate = MathUtils.Clamp(newBornsBisexualRate, 0, 100);

            Logger.DebugLog(string.Format("[LesLegends] {0}启用谷中密友与太吾同性别", closeFriendFixBisexual ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] {0}启用隐秘小村同性别NPC为双性恋", presetNPCFixBisexual ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] {0}启用不破神功绝不破身", keepVirginity ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] {0}启用新生儿概率调整对所有NPC生效", forceSexualRateForAll ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] 怀孕对象指定:{0}", fixMotherType));
            Logger.DebugLog(string.Format("[LesLegends] {0}启用同性生殖固定性别", sameSexualFromParents ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] {0}启用双性恋取向继承", bisexualFromParents ? "已" : "未"));
            Logger.DebugLog(string.Format("[LesLegends] 新生儿为女性概率:{0}%", newBornsFemaleRate));
            Logger.DebugLog(string.Format("[LesLegends] 新生儿为双性恋概率:{0}%", newBornsBisexualRate));
            Logger.DebugLog(string.Format("[LesLegends] 太吾同性生蛐蛐类型:{0}%", fixCricketLuckType));
            Logger.DebugLog(string.Format("[LesLegends] 同性生殖最低命中率:{0}%", minSexHitProb));
            Logger.DebugLog(string.Format("[LesLegends] 同性生殖最高命中率:{0}%", maxSexHitProb));
        }

        // 1.太吾和谷中密友同性且为双性恋
        // Transpiler
        public static class BiSCloseFriendTranspiler
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return typeof(CharacterDomain).GetMethod("CreateProtagonist", (BindingFlags)(-1));
                yield return typeof(EventHelper).GetMethod("CreateCloseFriend", (BindingFlags)(-1));
                yield break;
            }
            public static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
            {
                if (!closeFriendFixBisexual)
                {
                    return instructions;
                }

                instructions = new CodeMatcher(instructions, null).MatchForward(false, new CodeMatch[]
                {
                    new CodeMatch(new OpCode?(OpCodes.Call), typeof(Gender).GetMethod("Flip"), null)
                }).Repeat(delegate (CodeMatcher matcher)
                {
                    matcher.RemoveInstruction();
                    Logger.DebugLog(string.Format("[LesLegends] {0}_Transpiler removeFlip", __originalMethod.Name));
                }, null).InstructionEnumeration();
                return instructions;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CharacterDomain), "CreateCloseFriend")]
        public static Character CharacterDomain_CreateCloseFriend_Postfix(Character __result)
        {
            if (!closeFriendFixBisexual)
            {
                return __result;
            }

            sbyte taiwuGender = DomainManager.Taiwu.GetTaiwu().GetGender();
            sbyte cfGender = __result.GetGender();
            /*Logger.DebugLog(string.Format("[LesLegends] CharacterDomain_CreateCloseFriend_Postfix didEnter: taiwu:{0}, friend:{{gender:{1}, displayGender:{2},transGender:{3},",
                taiwuGender, cfGender, __result.GetDisplayingGender(), __result.GetTransgender()));*/

            if (taiwuGender != cfGender)
            {
                Traverse.Create(__result).Field("_gender").SetValue(taiwuGender);
                Logger.DebugLog(string.Format("[LesLegends] CharacterDomain_CreateCloseFriend_Postfix: closeFriend: {0} => {1}", cfGender, taiwuGender));
            }
            Traverse.Create(__result).Field("_bisexual").SetValue(true);

            /*Logger.DebugLog(string.Format("[LesLegends] CharacterDomain_CreateCloseFriend_Postfix willExit: taiwu:{0}, friend:{{gender:{1}, displayGender:{2},transGender:{3},",
                taiwuGender, __result.GetGender(), __result.GetDisplayingGender(), __result.GetTransgender()));*/
            return __result;
        }

        //2.同性可结婚生子
        //3.固定太吾为母亲? 0:随机 1:太吾 2:!太吾
        //4.生蛐蛐的策略
        public static class BisexualLovePatch
        {
            public static bool CalcIsTaiwuMother(IRandomSource random, Character mother)
            {
                if (fixMotherType == 0) // 随机
                {
                    return random.CheckPercentProb(50);
                }
                else if (fixMotherType == 1)    // 固定太吾
                {
                    return true;
                }
                else if (fixMotherType == 2)    // 固定非太吾
                {
                    return false;
                }
                return false;
            }

            public static bool CheckPregnant(IRandomSource random, Character father, Character mother, bool isRape)
            {
                int baseFertility = (isRape ? 20 : 60);
                int value;
                if (mother.GetFeatureIds().Contains(Config.CharacterFeature.DefKey.Pregnant) || DomainManager.Character.TryGetElement_PregnancyLockEndDates(mother.GetId(), out value))
                {
                    return false;   // 已经怀孕||有怀孕锁
                }
                // 原始命中率
                int originProb = (int)Math.Round(DomainManager.World.GetProbAdjustOfCreatingCharacter()
                        * baseFertility
                        * father.GetFertility()
                        * mother.GetFertility()
                        / 10000f);

                int finalProb = Math.Clamp(originProb, minSexHitProb, maxSexHitProb);
                return random.CheckPercentProb(finalProb);
            }

            [HarmonyPrefix, HarmonyPatch(typeof(Character), "OfflineMakeLove")]
            public static bool Character_OfflineMakeLove_Prefix(IRandomSource random, Character father, Character mother, bool isRape, ref bool __result, ref bool __state)
            {
                /*__state = __result; // 即便不执行相关逻辑，也记录一下__result状态，如果__result出现过true的，应该不能再变为false了*/
                __state = false;    // 还是先初始化为False吧，想太多干啥
                int taiwuId = DomainManager.Taiwu.GetTaiwuCharId();
                if (father.GetGender() != mother.GetGender())   // 为同性时，才需要修改，尽早返回
                {
                    Logger.DebugLog(string.Format("[LesLegends] Ori Fa:{0} Mo:{1} taiwu:{2}", father.GetId(), mother.GetId(), taiwuId));
                    return true;
                }

                if (father.GetId() == taiwuId || mother.GetId() == taiwuId) // 父母里有太吾，需要看谁做父母
                {
                    bool taiwuToBeMother = CalcIsTaiwuMother(random, mother);
                    bool taiwuIsNowFather = father.GetId() == taiwuId;
                    bool taiwuIsNowMother = mother.GetId() == taiwuId;
                    if ((taiwuToBeMother && taiwuIsNowFather) || (!taiwuToBeMother && taiwuIsNowMother))   // 太吾身份位与设置/随机值不匹配，攻守易势
                    {
                        Logger.Log(string.Format("[LesLegends] Swap Fa:{0} Mo:{1} taiwu:{2}, result:{3}", father.GetId(), mother.GetId(), taiwuId, __result));
                        (mother, father) = (father, mother);
                    }
                }
                // 只有太吾不破身
                if (father.GetId() != taiwuId || !keepVirginity)
                {
                    Traverse.Create(father).Method("OfflineAddFeature", new Type[] { typeof(short), typeof(bool) }).GetValue(Config.CharacterFeature.DefKey.VirginityFalse, true);
                }
                if (mother.GetId() != taiwuId || !keepVirginity)
                { 
                    Traverse.Create(mother).Method("OfflineAddFeature", new Type[] { typeof(short), typeof(bool) }).GetValue(Config.CharacterFeature.DefKey.VirginityFalse, true);
                }
                // 本来没怀上 现在怀上了
                if ((__result = CheckPregnant(random, father, mother, isRape)))
                {
                    Logger.DebugLog(string.Format("[LesLegends] Fin Fa:{0} Mo:{1} taiwu:{2}, result:{3}", father.GetId(), mother.GetId(), taiwuId, __result));
                    Traverse.Create(mother).Method("OfflineAddFeature", new Type[] { typeof(short), typeof(bool) }).GetValue(Config.CharacterFeature.DefKey.Pregnant, true);
                }
                __state = __result;
                return false;    // 同性不能ML，用自己的函数处理
            }

            // 有其他mod对OfflineMakeLove做Patch可能导致返回值被修改，在Postfix里做个检查和修正，(不排除有Postfix在更后面执行，先尽人事
            [HarmonyPostfix, HarmonyPatch(typeof(Character), "OfflineMakeLove")]
            public static void Character_OfflineMakeLove_Postfix(IRandomSource random, Character father, Character mother, bool isRape, ref bool __result, ref bool __state)
            {
                if (father.GetGender() == mother.GetGender())   // 为同性时，才需要修改
                {
                    if (__result != __state)
                    {
                        int taiwuId = DomainManager.Taiwu.GetTaiwuCharId();
                        Logger.Log(string.Format("[LesLegends] Error!!! FatherId:{0} MotherId:{1} taiwuId:{2} Found OfflineMakeLove result modified!", father.GetId(), mother.GetId(), taiwuId));
                    }
                    __result |= __state;    // 安全一点，用或而非直接赋值，万一是其他mod改为true，这边又改成false就尴尬了
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(Character), "OfflineExecuteFixedAction_MakeLove_Mutual")]
            public static void Character_OfflineExecuteFixedAction_MakeLove_Mutual_PostFix(Character __instance, IRandomSource random, int targetCharId, bool allowRape, PeriAdvanceMonthFixedActionModification mod)
            {
                if (targetCharId == DomainManager.Taiwu.GetTaiwuCharId() && debug)  // debug下启用，就不注释了
                {
                    if (mod != null && mod.MakeLoveTargetList != null)
                    {
                        foreach (var (target, makeLoveState, isPregnant) in mod.MakeLoveTargetList)
                        {
                            Logger.DebugLog(string.Format("[MakeLove_Mutual] target:{0} makeLoveState:{1} isPregnant:{2}", target.GetId(), makeLoveState, isPregnant));
                        }
                    }
                    else
                    {
                        Logger.DebugLog(string.Format("[MakeLove_Mutual] self:{0}, target:{1} mod.MakeLoveTargetList is null", __instance.GetId(), targetCharId));
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch(typeof(CharacterDomain), "CreatePregnantState")]
            public static bool CharacterDomain_CreatePregnantState_Prefix(CharacterDomain __instance, DataContext context, Character mother, Character father, bool isRaped)
            {
                int currDate = DomainManager.World.GetCurrDate();
                int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
                bool hasTaiwu = taiwuCharId == mother.GetId() || taiwuCharId == father.GetId();
                bool isSameSexual = mother.GetGender() == father.GetGender();
                if (!hasTaiwu || !isSameSexual)
                {
                    Logger.DebugLog(string.Format("[LesLegends] Ori Fa:{0} Mo:{1} taiwu:{2}", father.GetId(), mother.GetId(), taiwuCharId));
                    return true;   // 没有太吾 || 非同性 走原来的处理
                }
                // 因为太吾&&同性 怀孕时可能修改受孕者，这里要判断一下mother是否真的怀孕&&是否已有怀孕状态
                bool isMotherPregnant = mother.GetFeatureIds().Contains(Config.CharacterFeature.DefKey.Pregnant);
                Dictionary<int, PregnantState> pregnantStates = Traverse.Create(__instance).Field("_pregnantStates").GetValue<Dictionary<int, PregnantState>>();
                bool motherHasPregnantState = pregnantStates.ContainsKey(mother.GetId());
                bool isFatherPregnant = father.GetFeatureIds().Contains(Config.CharacterFeature.DefKey.Pregnant);
                bool fatherHasPregnantState = pregnantStates.ContainsKey(father.GetId());
                if (hasTaiwu && isFatherPregnant && !fatherHasPregnantState)    // 当前认为的父亲有怀胎但是没有怀孕状态，交换角色
                {
                    Logger.DebugLog(string.Format("[LesLegends] OriFa:{0} OriMo:{1} taiwu:{2}", father.GetId(), mother.GetId(), taiwuCharId));
                    Logger.DebugLog(string.Format("[LesLegends] Mother:{{pregnant:{0}, hasState:{1}}}", isMotherPregnant, motherHasPregnantState));
                    Logger.DebugLog(string.Format("[LesLegends] Father:{{pregnant:{0}, hasState:{1}}}", isFatherPregnant, fatherHasPregnantState));
                    Logger.Log("[LesLegends] warning:当前认为的父亲有怀胎但是没有怀孕状态，交换角色");
                    (mother, father) = (father, mother);
                    Logger.DebugLog(string.Format("[LesLegends] FactFa:{0} FactMo:{1} taiwu:{2}", father.GetId(), mother.GetId(), taiwuCharId));
                }

                PregnantState state = new PregnantState(mother, father, isRaped);
                /*fixCricketLuckType{[1]="默认",[2]="不生蛐蛐",[3]="只生蛐蛐"}*/
                /*if (fixCricketLuckType == 1)
                {*/
                state.IsHuman = !context.Random.CheckPercentProb(DomainManager.Taiwu.GetCricketLuckPoint() / 100);
                /*}*/
                if (fixCricketLuckType == 2)    // 不生蛐蛐
                {
                    state.IsHuman = true;
                }
                else if (fixCricketLuckType == 3)   // 只生蛐蛐
                {
                    state.IsHuman = false;
                }
                state.ExpectedBirthDate = currDate + (state.IsHuman ? context.Random.Next(6, 10) : 42);
                Logger.DebugLog(string.Format("[LesLegends] Fin Fa:{0} Mo:{1} taiwu:{2}", father.GetId(), mother.GetId(), taiwuCharId));
                Traverse.Create(__instance).Method("AddElement_PregnantStates",
                    new Type[] { typeof(int), typeof(PregnantState), typeof(DataContext) })
                    .GetValue(mother.GetId(), state, context);
                return false;
            }
        }

        public static sbyte GetRandomGender(IRandomSource random, CharacterDomain domain, int fatherId, Character mother)
        {
            sbyte originGender = random.CheckPercentProb(newBornsFemaleRate) ? Gender.Female : Gender.Male; // 新生儿性别
            if (sameSexualFromParents && fatherId >= 0) // 有父亲 且 性别遗传
            {
                Dictionary<int, Character> dic = Traverse.Create(domain).Field("_objects").GetValue<Dictionary<int, Character>>();
                Character father;
                if (dic != null && dic.TryGetValue(fatherId, out father) && father.GetGender() == mother.GetGender())
                {
                    /*Logger.DebugLog(string.Format("[LesLegends] changeMainChildGender"));*/
                    originGender = father.GetGender();
                }
            }
            return originGender;
        }

        //4.同性父母子女性别相同
        //5.新生儿女性概率
        [HarmonyTranspiler, HarmonyPatch(typeof(CharacterDomain), "ParallelCreateNewbornChildren")]
        public static IEnumerable<CodeInstruction> CharacterDomain_ParallelCreateNewbornChildren_Transpiler(
            MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            /*var __result = new List<CodeInstruction>(instructions);*/
            /// 第一步，找checkPercentProb，改参数 多胞胎性别变化概率与设置的新生儿性别概率有关，越是偏向某种性别，变化概率越小
            instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_I4_S, 25),
                    new CodeMatch(OpCodes.Call, typeof(RedzenHelper).GetMethod("CheckPercentProb"))  // 找CheckPercentProb，改概率参数
                )
                .Repeat(matcher => // Do the following for each match
                    matcher
                    .SetInstructionAndAdvance(
                        new CodeInstruction(OpCodes.Ldc_I4_S, (int)(25 * (1 - Math.Abs(newBornsFemaleRate - 50) / 50.0)))
                    )
                ).InstructionEnumeration();

            /// 第二步，加上同性别父母时固定子女性别的代码
            MethodInfo GetRandomGender = AccessTools.Method(typeof(LesLegends), nameof(GetRandomGender));
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    // 匹配 mainChildGender = Gender.GetRandom()
                    new CodeMatch(OpCodes.Call, typeof(Gender).GetMethod("GetRandom"))
                )
                .Repeat(matcher => // Do the following for each match
                    matcher
                    .RemoveInstruction()
                    // 先不能移除，因为GetRandom要给后面的逻辑定位用，直接插入
                    .InsertAndAdvance(
                        new CodeInstruction[]
                        {                                                                   // 前面已经传了 random
                            new CodeInstruction(OpCodes.Ldarg_0, null),                     // CharacterDomain
                            new CodeInstruction(OpCodes.Ldloc_S, 4),                        // fatherId
                            new CodeInstruction(OpCodes.Ldarg_2, null),                     // mother
                            new CodeInstruction(OpCodes.Call, GetRandomGender),             // 替换随机生成性别的方法
                        }
                    )
                ).InstructionEnumeration();
            return instructions;

            /*int i = 0;
            foreach (var instruction in instructions)
            {
                Logger.DebugLog(String.Format("{0} {1} {2}", i++, instruction.opcode, instruction.operand));
            }
            return __result;*/
        }

        //6.双性恋子女必双性恋
        //7.新生儿双性恋概率
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "OfflineCreateIntelligentCharacter")]
        public static void Character_OfflineCreateIntelligentCharacter_Postfix(
            Character __instance,
            DataContext context,
            CreateIntelligentCharacterModification mod,
            ref IntelligentCharacterCreationInfo info
            )
        {
            if (info.Age != 0 && !forceSexualRateForAll)  // 不是儿童 || 新生儿修改全局生效
            {
                return;
            }
            IRandomSource random = context.Random;
            bool fatherBisexual = info.Father != null ? info.Father.GetBisexual() : false;
            bool motherBisexual = info.Mother != null ? info.Mother.GetBisexual() : false;
            bool parentSameSexual = (info.Father != null && info.Mother != null) ? info.Father.GetGender() == info.Mother.GetGender() : false;

            int bisexualProb = newBornsBisexualRate;  // 默认双性恋概率
            if (bisexualFromParents)    // 继承双性恋打开
            {
                if ((motherBisexual && fatherBisexual) || parentSameSexual)
                {
                    bisexualProb = 100;
                }
            }
            else
            {
                if (motherBisexual)
                {
                    bisexualProb += 20;
                }
                if (fatherBisexual)
                {
                    bisexualProb += 20;
                }
                if (parentSameSexual)
                {
                    bisexualProb += 20;
                }
            }

            if (!__instance.GetBisexual() && random.CheckPercentProb(bisexualProb))
            {
                Traverse.Create(__instance).Field("_bisexual").SetValue(true);
            }
        }

        //8.隐秘小村 NPC与太吾同性时为双性恋
        [HarmonyPostfix, HarmonyPatch(typeof(CharacterDomain), "CreatePresetCharacter")]
        public static void CharacterDomain_CreatePresetCharacter_Postfix(DataContext context, short templateId, ref Character __result)
        {
            if (templateId == Config.Character.DefKey.StoryBigWig           // 大人物=>郭彦
                || templateId == Config.Character.DefKey.StoryLittleUrchin  // 小男孩
                || templateId == Config.Character.DefKey.StoryStrongMan     // 壮男=>阿牛
                || templateId == Config.Character.DefKey.StoryHuanyue)      // 还月
            {
                Logger.DebugLog(string.Format("[CreatePresetCharacter] {0}\t\t{1}是双性恋", __result.GetFullName().ToString(), __result.GetBisexual() ? "" : "不"));
                if (presetNPCFixBisexual && __result.GetGender() == DomainManager.Taiwu.GetTaiwu().GetGender())
                {
                    Traverse.Create(__result).Field("_bisexual").SetValue(true);
                    Logger.DebugLog(string.Format("[CreatePresetCharacter] {0}\t\t现在{1}是双性恋", __result.GetFullName().ToString(), __result.GetBisexual() ? "" : "不"));
                }
            }
        }

        //9.新生儿概率调整对所有NPC生效
        [HarmonyPostfix, HarmonyPatch(typeof(Gender), "GetRandom")]
        public static void Gender_GetRandom_Postfix(IRandomSource random, ref sbyte __result)
        {
            if (forceSexualRateForAll)
            {
                __result = random.CheckPercentProb(newBornsFemaleRate) ? Gender.Female : Gender.Male;
            }
        }

        /*[HarmonyPrefix, HarmonyPatch(typeof(OrganizationDomain), "GetCharacterTemplateId")]
        public static void OrganizationDomain_GetCharacterTemplateId_Prefix(sbyte orgTemplateId, sbyte mapStateTemplateId, ref sbyte gender)
        {
            if (forceSexualRateForAll)
            {
                gender = random.Next(100) < newBornsFemaleRate ? Gender.Female : Gender.Male;
            }
        }*/
    }
}
