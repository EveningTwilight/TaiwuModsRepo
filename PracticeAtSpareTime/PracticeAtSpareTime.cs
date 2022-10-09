using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using TaiwuModdingLib.Core.Plugin;

namespace PracticeAtSpareTime
{
    public static class Logger
    {
        public static void Log(string str)
        {
            AdaptableLog.Info(str);
        }

        public static void DebugLog(string str)
        {
            if (PracticeAtSpareTime.debug)
            {
                Log(str);
            }
        }
    }

    [PluginConfig("闲时练功", "EveningTwilight", "1.0.0")]
    public class PracticeAtSpareTime : TaiwuRemakePlugin
    {
        static Harmony harmony;
        /// <summary>
        /// 以下为设置
        /*static bool enablePracticeCombatSkill = false;      // 空闲时间提升战斗技能*/
        static int combatSkillPracticeRate = 50;             // 每天提升战斗技能基础概率
        static bool combatSkillCostExp = false;              // 空闲时间修习技能也消耗历练
        static int combatSkillProbDec = 2;                   // 成功率递减值
        /*static int combatSkillPracticeStep = 1;             // 战斗技能基础提升量*/
        public static bool debug = false;                           // 打印日志
        /// </summary>
        static int practicedDaysInMonth = 0;                // 记录本月修习技能耗时
        public override void Dispose()
        {
            Logger.DebugLog($"[PracticeAtSpareTime] Dispose");
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
        public override void Initialize()
        {
            Logger.DebugLog($"[PracticeAtSpareTime] Initialize");
            readSettings();

            harmony = new Harmony(base.GetGuid());
            harmony.PatchAll(typeof(PracticeAtSpareTime));
        }
        public void readSettings()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "combatSkillPracticeRate", ref combatSkillPracticeRate);
            DomainManager.Mod.GetSetting(ModIdStr, "combatSkillProbDec", ref combatSkillProbDec);
            DomainManager.Mod.GetSetting(ModIdStr, "combatSkillCostExp", ref combatSkillCostExp);
            DomainManager.Mod.GetSetting(ModIdStr, "debug", ref debug);

            Logger.DebugLog(string.Format("[PracticeAtSpareTime] 修习功法概率:{0}", combatSkillPracticeRate));
            Logger.DebugLog(string.Format("[PracticeAtSpareTime] 修习成功率递减:{0}", combatSkillProbDec));
            Logger.DebugLog(string.Format("[PracticeAtSpareTime] {0}启用修习消耗历练", combatSkillCostExp ? "已" : "未"));
        }
        [HarmonyPrefix, HarmonyPatch(typeof(TaiwuDomain), "PracticeCombatSkillWithExp")]
        public static bool TaiwuDomain_PracticeCombatSkillWithExp_Prefix(TaiwuDomain __instance, DataContext context, short skillTemplateId)
        {
            if (DomainManager.World.GetLeftDaysInCurrMonth() > 0)
            {
                CombatSkill skill = DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(__instance.GetTaiwuCharId(), skillTemplateId));
                int costExp = __instance.GetPracticeCostExp(skillTemplateId);
                int taiwuExp = __instance.GetTaiwu().GetExp();
                if (!skill.GetRevoked() && skill.GetPracticeLevel() < 100 && costExp <= taiwuExp)
                {
                    // 说明确实会进行修习，本月练功耗时+1
                    practicedDaysInMonth++;
                }
            }
            return true;    // 只是取一下每个月的记录，不干扰原来的逻辑
        }
        [HarmonyPrefix, HarmonyPatch(typeof(WorldDomain), "PeriAdvanceMonth_CharacterFixedAction_TaiwuGroup")]
        public static bool WorldDomain_PeriAdvanceMonth_CharacterFixedAction_TaiwuGroup_Prefix(DataContext context)
        {
            IRandomSource random = context.Random;
            /*sbyte daysInMonth = DomainManager.World.GetDaysInCurrMonth();*/
            sbyte daysInMonth = 30;
            Logger.DebugLog(string.Format("[PracticeAtSpareTime] 当月练功天数:{0} 当月总天数:{1}", practicedDaysInMonth, daysInMonth));
            if (daysInMonth > practicedDaysInMonth)
            {
                List<short> learnedCombatSkill = DomainManager.Taiwu.GetTaiwu().GetLearnedCombatSkills();
                List<CombatSkill> combatSkillsToPractice = new();
                foreach (short id in learnedCombatSkill)
                {
                    CombatSkill skill = DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(DomainManager.Taiwu.GetTaiwuCharId(), id));
                    if (!skill.GetRevoked() && skill.GetPracticeLevel() < 100)
                    {
                        combatSkillsToPractice.Add(skill);
                    }
                }
                int needPracticeSkillCount = combatSkillsToPractice.Count();
                int leftDays = daysInMonth - practicedDaysInMonth;
                Logger.DebugLog(string.Format("[PracticeAtSpareTime] 有{0}天可能挤出时间修炼，有{1}功法还未练满", leftDays, needPracticeSkillCount));
                if (needPracticeSkillCount > 0)
                {
                    int practiceRate = combatSkillPracticeRate;
                    while (leftDays-- > 0 && needPracticeSkillCount > 0 && practiceRate > 0)
                    {
                        int nextPracticeSkillIdx = random.Next(needPracticeSkillCount);
                        CombatSkill skill = combatSkillsToPractice[nextPracticeSkillIdx];
                        Config.CombatSkillItem item = Config.CombatSkill.Instance.GetItem(skill.GetId().SkillTemplateId);
                        if (!random.CheckPercentProb(combatSkillPracticeRate))   // TODO: EveningTwilight 根据品级降低概率
                        {
                            continue;
                        }
                        int costExp = combatSkillCostExp ? DomainManager.Taiwu.GetPracticeCostExp(skill.GetId().SkillTemplateId) : 0;
                        int taiwuExp = DomainManager.Taiwu.GetTaiwu().GetExp();
                        if (costExp > taiwuExp)
                        {
                            continue;
                        }
                        practiceRate -= combatSkillProbDec;  // 每次修习降低基础概率
                        int practiceResult = DomainManager.Taiwu.CalcPracticeResult(context, skill.GetId().SkillTemplateId);
                        int practiceLevel = Math.Min(skill.GetPracticeLevel() + practiceResult, 100);
                        skill.SetPracticeLevel((sbyte)practiceLevel, context);

                        string skillName = item.Name;
                        if (costExp > 0)
                        {
                            DomainManager.Taiwu.GetTaiwu().ChangeExp(context, -costExp);
                        }
                        Logger.DebugLog(string.Format("[PracticeAtSpareTime] 太吾耗费{0}历练,抽空修习了\t{1}\t取得{2}进展", costExp, skillName, practiceResult));
                        if (skill.GetPracticeLevel() == 100)    // 又练满了一门 好耶!
                        {
                            combatSkillsToPractice.Remove(skill);
                            needPracticeSkillCount = combatSkillsToPractice.Count();
                        }
                    }
                }
            }
            practicedDaysInMonth = 0;   // 清零 下个月重新计算
            return true;    // 过月结算前，处理一下当月没修习功法的日子，补一下各个技能
        }
    }
}
