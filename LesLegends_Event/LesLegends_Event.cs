using GameData.Domains;
using HarmonyLib;
using Redzen.Random;
using GameData.Domains.Character;
using GameData.Domains.Character.Relation;
using ConchShip.EventConfig.Taiwu;
using TaiwuModdingLib.Core.Plugin;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.World;
using GameData.Utilities;
using Config;
using GameData.Domains.Character.Ai;
using System;
using Character = GameData.Domains.Character.Character;
using Config.EventConfig;
using GameData.Domains.TaiwuEvent;

namespace LesLegends_Event
{
    public static class Logger
    {
        public static void Log(string str)
        {
            AdaptableLog.Info(str);
        }

        public static void DebugLog(string str)
        {
            if (LesLegends_Event.debug)
            {
                Log(str);
            }
        }
    }

    [PluginConfig("姬友传说", "EveningTwilight", "1.1.1")]
    public class LesLegends_Event : TaiwuRemakePlugin
    {
        static Harmony harmony;
        static Harmony eventHarmony;
        static bool eventPatched = false;
        /// <summary>
        /// 以下为设置
        static bool bisexualMarray = false;         // 比武招亲检查性取向而非单纯性别
        static bool unlimitMarray = false;          // 允许重婚
        static bool unlimitMonk = false;            // 无视门派限制
        static bool unlimitBlood = false;           // 无视血缘关系
        static bool unlimitMentor = false;          // 无视师徒关系(仅倾诉爱意)
        static bool unlimitFavor = false;           // 无视太吾对目标好感(仅共结连理)
        static bool modifyForDifSexual = false;     // 修改对异性恋生效
        static int minMarrayAge = 16;               // 最小结婚年龄(含比武招亲入场判断)
        public static bool debug;
        /// </summary>

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            ReadSettings();

            harmony ??= new Harmony(base.GetGuid());
            harmony.PatchAll(typeof(LesLegends_Event));
            eventPatched = false;
            eventHarmony ??= new Harmony(base.GetGuid()+"EventPatch");
        }

        public override void OnModSettingUpdate()
        {
            ReadSettings();
        }

        public void ReadSettings()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "bisexualMarray", ref bisexualMarray);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitMarray", ref unlimitMarray);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitMonk", ref unlimitMonk);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitBlood", ref unlimitBlood);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitMentor", ref unlimitMentor);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitMentor", ref unlimitMentor);
            DomainManager.Mod.GetSetting(ModIdStr, "unlimitFavor", ref unlimitFavor);
            DomainManager.Mod.GetSetting(ModIdStr, "minMarrayAge", ref minMarrayAge);
            DomainManager.Mod.GetSetting(ModIdStr, "debug", ref debug);
        }

        /// <summary>
        /// 启动时进行事件的patch会有问题，应该是还未加载进来
        /// 改成在世界加载时再进行，所以单独拿类封装一下patch的方法，等加载的时候对这个class进行PatchAll即可
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(WorldDomain), "OnLoadWorld")]
        public static bool WorldDomain_OnLoadWorld_Prefix()
        {
            if (!eventPatched)
            {
                eventHarmony.PatchAll(typeof(LoveEventPatch));
                MarrayEventPatcher.PatchAllMarrayEvent(eventHarmony);
                eventPatched = true;
            }
            return true;
        }

        // 亲近-倾诉爱意 + 亲近-共结连理
        public static class LoveEventPatch
        {
            // 倾诉爱意
            [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption9VisibleCheck")]
            public static void TaiwuEventAdore_OnOption9VisibleCheck_Postfix(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance, ref bool __result)
            {
                Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                Character target = __instance.ArgBox.GetCharacter("CharacterId");
                if ((modifyForDifSexual || taiwu.GetGender() == target.GetGender()) && (unlimitMarray || unlimitMonk || unlimitBlood || unlimitMentor))   // (修改对异性恋生效 || 为同性) 设置需要修改
                {
                    if (!EventHelper.CheckMainStoryLineProgress(6))   // 应该是出谷和隐秘小村，先保持一致
                    {
                        return;
                    }
                    if (EventHelper.GetRoleAge(taiwu) >= minMarrayAge && EventHelper.GetRoleAge(target) >= minMarrayAge    // 年龄限制
                        && EventHelper.CheckHasRelationship(taiwu, target, RelationType.Friend) && !EventHelper.CheckHasRelationship(taiwu, target, RelationType.SwornBrotherOrSister)    // 朋友 && 没有义结金兰
                        && (!EventHelper.CheckHasRelationship(taiwu, target, RelationType.Adored) || !EventHelper.CheckHasRelationship(target, taiwu, RelationType.Adored)))    // 还没有相互倾慕
                    {
                        if (!unlimitMentor && (EventHelper.CheckHasRelationship(taiwu, target, RelationType.Mentor) || EventHelper.CheckHasRelationship(target, taiwu, RelationType.Mentor))) // 不允许师徒 && 有师徒关系
                        {
                            return;
                        }
                        if (!unlimitBlood && (EventHelper.HasNominalBloodRelation(taiwu.GetId(), target.GetId()) || EventHelper.HasBloodExclusionRelation(taiwu.GetId(), target.GetId())))  // 血缘限制 && 存在血缘关系(这个判断包含已经是夫妻，但前面有过滤互相倾慕了)
                        {
                            return;
                        }
                        __result = true;
                    }
                }
            }

            // 共结连理
            [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption10VisibleCheck")]
            public static void TaiwuEventMarray_OnOption10VisibleCheck_Postfix(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance, ref bool __result)
            {
                Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                Character target = __instance.ArgBox.GetCharacter("CharacterId");
                if ((modifyForDifSexual || taiwu.GetGender() == target.GetGender()) && (unlimitMarray || unlimitMonk || unlimitBlood))   // (修改对异性恋生效 || 为同性) 设置需要修改
                {
                    if (!EventHelper.CheckMainStoryLineProgress(6))   // 应该是出谷和隐秘小村，先保持一致
                    {
                        return;
                    }
                    if (EventHelper.GetRoleAge(taiwu) >= minMarrayAge && EventHelper.GetRoleAge(target) >= minMarrayAge    // 年龄限制
                        && EventHelper.CheckHasRelationship(taiwu, target, RelationType.Adored) && EventHelper.CheckHasRelationship(target, taiwu, RelationType.Adored))    // 互相倾慕
                    {
                        if (!unlimitMarray && (EventHelper.RoleHasAliveSpouse(taiwu.GetId()) || EventHelper.RoleHasAliveSpouse(target.GetId()))) // 不允许重婚 && 至少一方已婚
                        {
                            return;
                        }
                        if (!unlimitBlood && (EventHelper.HasNominalBloodRelation(taiwu.GetId(), target.GetId()) || EventHelper.HasBloodExclusionRelation(taiwu.GetId(), target.GetId())))  // 血缘限制 && 存在血缘关系(包含已经是夫妻)
                        {
                            return;
                        }
                        if (!unlimitMonk && !target.OrgAndMonkTypeAllowMarriage())   // 未解除门派限制 && 门派限制(里面还有个childGrade 应该是孩子阶级，先不管)
                        {
                            return;
                        }
                        if (unlimitFavor)   // 无视太吾对目标好感
                        {
                            __result = true;
                            return;
                        }
                        // 判断一下好感
                        RelatedCharacter taiwuToTarget = DomainManager.Character.GetRelation(taiwu.GetId(), target.GetId());
                        sbyte favorabilityTypeToTarget = FavorabilityType.GetFavorabilityType(taiwuToTarget.Favorability);
                        AiRelationsItem relationsCfg = AiHelper.Relation.GetAiRelationConfig(AiRelations.DefKey.StartHusbandOrWifeRelation);
                        short minSelfFavorabilityReq = relationsCfg.MinFavorability[taiwu.GetBehaviorType()];
                        __result |= (favorabilityTypeToTarget >= minSelfFavorabilityReq);    // 防止有其他mod改为true后被覆盖
                        /*__result = true;*/
                    }
                }
            }

            // 共结连理-远走高飞
            [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption11VisibleCheck")]
            public static void TaiwuEventMarray_OnOption11VisibleCheck_Postfix(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance, ref bool __result)
            {
                Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                Character target = __instance.ArgBox.GetCharacter("CharacterId");
                if ((modifyForDifSexual || taiwu.GetGender() == target.GetGender()) && (unlimitMarray || unlimitBlood))   // (修改对异性恋生效 || 为同性) 设置需要修改
                {
                    if (!EventHelper.CheckMainStoryLineProgress(6))   // 应该是出谷和隐秘小村，先保持一致
                    {
                        return;
                    }
                    if (EventHelper.GetRoleAge(taiwu) >= minMarrayAge && EventHelper.GetRoleAge(target) >= minMarrayAge    // 年龄限制
                        && EventHelper.CheckHasRelationship(taiwu, target, RelationType.Adored) && EventHelper.CheckHasRelationship(target, taiwu, RelationType.Adored))    // 互相倾慕
                    {
                        if (!unlimitMarray && (EventHelper.RoleHasAliveSpouse(taiwu.GetId()) || EventHelper.RoleHasAliveSpouse(target.GetId()))) // 不允许重婚 && 至少一方已婚
                        {
                            return;
                        }
                        if (!unlimitBlood && (EventHelper.HasNominalBloodRelation(taiwu.GetId(), target.GetId()) || EventHelper.HasBloodExclusionRelation(taiwu.GetId(), target.GetId())))  // 血缘限制 && 存在血缘关系(包含已经是夫妻)
                        {
                            return;
                        }
                        __result |= EventHelper.GetCharacterChildGrade(target) < 0; // 防止有其他mod改为true后被覆盖
                    }
                }
            }
        }

        // 比武招亲事件
        public static class MarrayEventPatcher
        {
            public static void PatchAllMarrayEvent(Harmony harmony)
            {
                harmony.PatchAll(typeof(ChengDuMarryEvent));
                harmony.PatchAll(typeof(DaLiMarryEvent));
                harmony.PatchAll(typeof(FuZhouMarryEvent));
                harmony.PatchAll(typeof(GuangZhouMarryEvent));
                harmony.PatchAll(typeof(GuiZhouMarryEvent));
                harmony.PatchAll(typeof(HangZhouMarryEvent));
                harmony.PatchAll(typeof(JiangLingMarryEvent));
                harmony.PatchAll(typeof(JingChengMarryEvent));
                harmony.PatchAll(typeof(LiaoYangMarryEvent));
                harmony.PatchAll(typeof(QingZhouMarryEvent));
                harmony.PatchAll(typeof(QinZhouMarryEvent));
                harmony.PatchAll(typeof(ShouChunMarryEvent));
                harmony.PatchAll(typeof(TaiYuanMarryEvent));
                harmony.PatchAll(typeof(XiangYangMarryEvent));
                harmony.PatchAll(typeof(YangZhouMarryEvent));
            }
            // 比武招亲入口事件统一处理
            static void CommonMarrayEventEntryHandler(ref string __result, Character taiwu, Character target, string entrance, string tooYougn, string femaleNotFit, string notFemaleNotFit)
            {
                if (minMarrayAge == 16 && !bisexualMarray)  // 无需修改
                {
                    return;
                }
                bool genderCheckValid = true;   // 如果无targetCharactor则跳过性别检查
                if (target != null)
                {
                    sbyte targetGender = EventHelper.GetRoleGender(target);
                    sbyte availableGender = target.GetBisexual() ? Gender.Flip(targetGender) : targetGender;  // 考虑目标的性取向决定检查的性别
                    genderCheckValid = EventHelper.IsMeetGender(taiwu, availableGender);   // 起点check 真实性别 || 外貌性别
                }
                // 检查太吾性别是否满足
                if (genderCheckValid)
                {
                    __result = (EventHelper.GetRoleAge(taiwu) >= minMarrayAge) ? entrance : tooYougn;   // 检查年龄
                }
                // 性别不满足，走原来的逻辑吧
                else
                {
                    __result = EventHelper.GetRoleGender(taiwu) == Gender.Female ? femaleNotFit : notFemaleNotFit;
                }
            }
            // 比武招亲结束事件统一处理
            static void CommonMarrayEventFinishHandler(ref string __result, Character taiwu, Character target, string success, string genderFailure, string married)
            {
                if (!unlimitMarray && !bisexualMarray)  // 无需修改
                {
                    return;
                }
                bool genderCheckValid = true;   // 如果无targetCharactor则跳过性别检查
                if (target != null)
                {
                    sbyte targetGender = EventHelper.GetRoleGender(target);
                    sbyte availableGender = target.GetBisexual() ? Gender.Flip(targetGender) : targetGender;  // 考虑目标的性取向决定检查的性别
                    genderCheckValid = taiwu.GetGender() == availableGender;
                }
                if (!genderCheckValid)   // 终点只看真实性别
                {
                    __result = genderFailure;  // 失败-性别
                }
                else if (!unlimitMarray && EventHelper.RoleHasAliveSpouse(EventArgBox.TaiwuCharacterId))    // 已婚限制 && 太吾已婚
                {
                    __result = married;  // 失败-已婚
                }
                else
                {
                    __result = success;  // 成功
                }

            }
            
            /// <summary>
            /// 成都比武招亲事件
            /// </summary>
            static class ChengDuMarryEvent
            {
                // 成都比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_e0c68df999df4308bc7bd9b16bf90a5c), "OnOption1Select")]
                public static void TaiwuEventMarray_ChengDu_Entry_OnOption1Select_Postfix(TaiwuEvent_e0c68df999df4308bc7bd9b16bf90a5c __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R3");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "b14cde62-2ae0-49c7-a356-9bacdfdfb319", "aaeede7f-dcce-4796-ae89-31b482a30d1a", "f3bfb41a-fa9b-45fd-997c-008729dad673", "9ccb2902-4975-49a4-ab41-bf873b58aa71");
                }
                // 成都比武招亲成功
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_5e37867cf725454382b7d7f43da3e98f), "OnOption1Select")]
                public static void TaiwuEventMarray_ChengDu_Suc_OnOption1Select_Postfix(TaiwuEvent_5e37867cf725454382b7d7f43da3e98f __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R3");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "488ca8df-d02f-40c7-9ecb-36af88a5fb23", "f9eaa256-a69c-41a9-ba07-ab40cf3a96a2", "ee77f3f1-52f8-4bce-82aa-20830a3a9d6b");
                }
            }

            /// <summary>
            /// 大理比武招亲事件
            /// </summary>
            static class DaLiMarryEvent
            {
                // 大理比武招亲入口    
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_78dd2fb39443482aa114fdb2f920dc59), "OnOption1Select")]
                public static void TaiwuEventMarray_DaLi_Entry_OnOption1Select_Postfix(TaiwuEvent_78dd2fb39443482aa114fdb2f920dc59 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "050a383c-e4b4-47ee-aaf5-b56e601ed339", "497592ec-95c1-4083-a7c0-87b2bbfb6cb1", "7801bce2-098c-44ab-bd43-1f8ed5c1bdd5", "bb832bde-bc53-4c86-a2e9-8fd28b70fb71");
                }
                // 大理比武招亲-成功是新娘-无视已婚-判定取向
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_207a1d9c40874872812823e39b6bcb88), "OnOption1Select")]
                public static void TaiwuEventMarray_DaLi_SucTrue_OnOption1Select_Postfix(TaiwuEvent_207a1d9c40874872812823e39b6bcb88 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "5fa7a020-4046-4c8e-b715-aceb929f07ac", "d7bcafac-9d22-4a2d-98ff-ec62df7e5ebf", "cfb67373-96b2-4507-847e-fd8640d44f91");
                }
                // 大理比武招亲-成功不是新娘-无视已婚-判定取向
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_156989198abb4e66b8d6c97287953e98), "OnOption1Select")]
                public static void TaiwuEventMarray_DaLi_SucFalse_OnOption1Select_Postfix(TaiwuEvent_156989198abb4e66b8d6c97287953e98 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "5c3dfc3b-89dc-42bd-8239-cc84626da977", "d4132187-d540-4407-81d9-88c851a84e90", "d4132187-d540-4407-81d9-88c851a84e90");
                }
            }

            /// <summary>
            /// 福州比武招亲事件
            /// </summary>
            static class FuZhouMarryEvent
            {
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_fc7a9145c1764a5ba0300312957baebc), "OnOption1Select")]
                public static bool TaiwuEventMarray_FuZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_fc7a9145c1764a5ba0300312957baebc __instance, ref string __result)
                {
                    if (minMarrayAge == 16 && !bisexualMarray)  // 无需修改
                    {
                        return true;
                    }
                    // 福州的特殊处理下，不走common了
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, string.Empty, "ba09482a-6eaa-47ea-ad27-9e44bfe76261", "ba09482a-6eaa-47ea-ad27-9e44bfe76261", "132b9847-4a91-47bc-8cc8-ff6c8f615575");
                    if (__result == string.Empty)
                    {
                        EventHelper.SelectAdventureBranch("1");
                        EventHelper.FinishAdventureEvent();
                    }
                    return false;   // 不走原有逻辑
                }
                // 福州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_d6797b52725c4fbcbc2f05e8d5caff2c), "OnEventEnter")]
                public static void TaiwuEventMarray_FuZhou_Finish_OnEventEnter_Postfix(TaiwuEvent_d6797b52725c4fbcbc2f05e8d5caff2c __instance)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    int adventureParameter = EventHelper.GetAdventureParameter("acceptance");
                    int adventureParameter2 = EventHelper.GetAdventureParameter("acceptanceNum");
                    string nextEvent = "f48f7897-e610-4069-b133-c05f12730055";
                    if (adventureParameter >= adventureParameter2)
                    {
                        CommonMarrayEventFinishHandler(ref nextEvent, taiwu, target, "39645c71-0d13-4184-922e-ee2b697897e4", "0b60e276-8980-4677-a528-72ef10ace6e2", "1771dde6-b437-4848-a9b7-5448926153c5");
                    }
                    EventHelper.ToEvent(nextEvent);
                }
            }

            /// <summary>
            /// 广州比武招亲
            /// </summary>
            static class GuangZhouMarryEvent
            {
                // 广州比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_dc8d1d05cb3848a0852e292acc6322d1), "OnOption1Select")]
                public static void TaiwuEventMarray_GuangZhou_Entry_OnOption1Select_Postfix(TaiwuEvent_dc8d1d05cb3848a0852e292acc6322d1 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "4300df02-7a77-4970-a185-e4ff007c08fe", "d3d16711-f388-47f0-8c6b-2d0ee0a3d571", "b9952407-0f26-4f60-a854-875579c6f6e3", "5eaab08f-5d1d-4549-afc0-5dac9f2ab747");
                }
                // 广州比武招亲终点(送完三束花)
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_a22b78003d9542108023b4642749acd7), "OnOption1Select")]
                public static void TaiwuEventMarray_GuangZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_a22b78003d9542108023b4642749acd7 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "6e95869f-aab4-419c-adc2-b03f21f50caa", "974d0521-708f-4575-b630-06dbf48b27f7", "974d0521-708f-4575-b630-06dbf48b27f7");
                }
            }

            /// <summary>
            /// 桂州比武招亲
            /// </summary>
            static class GuiZhouMarryEvent
            {
                static bool CommonGuiZhouMarryEventEntryHandler(TaiwuEventItem __instance, ref string __result)
                {
                    if (minMarrayAge == 16 && !bisexualMarray)  // 无需修改
                    {
                        return true;
                    }
                    int charId = -1;
                    if (__instance.ArgBox.Get("R1", ref charId))
                    {
                        bool characterVeilShowState = EventHelper.GetCharacterVeilShowState(charId);
                        if (characterVeilShowState)
                        {
                            EventHelper.SetCharacterVeilShowState(charId, false);
                        }
                    }
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = EventHelper.GetCharacterById(charId);
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "b7f15b23-39bf-4966-89c8-1c7ca0e06a02", "448bb9ba-efbf-41db-9a4b-ff63cd85b741", "18372562-7c91-4991-ba4c-15f2a7749a7d", "6548dcc2-366b-4fde-9009-9b62ff2bad3c");
                    return false;
                }
                #region 桂州比武招亲入口: 刚正 仁善 中庸 叛逆 唯我
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_e575d05cadc4496c9075fd95f3964898), "OnOption1Select")]
                public static bool TaiwuEventMarray_GuiZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_e575d05cadc4496c9075fd95f3964898 __instance, ref string __result)
                {
                    return CommonGuiZhouMarryEventEntryHandler(__instance, ref __result);
                }
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_d9d83e016d264fbd9f285e45fecad783), "OnOption1Select")]
                public static bool TaiwuEventMarray_GuiZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_d9d83e016d264fbd9f285e45fecad783 __instance, ref string __result)
                {
                    return CommonGuiZhouMarryEventEntryHandler(__instance, ref __result);
                }
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_2a3513408920425195a256407f92d54d), "OnOption1Select")]
                public static bool TaiwuEventMarray_GuiZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_2a3513408920425195a256407f92d54d __instance, ref string __result)
                {
                    return CommonGuiZhouMarryEventEntryHandler(__instance, ref __result);
                }
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_6cc0f841371e490cb61ce9d257a6264d), "OnOption1Select")]
                public static bool TaiwuEventMarray_GuiZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_6cc0f841371e490cb61ce9d257a6264d __instance, ref string __result)
                {
                    return CommonGuiZhouMarryEventEntryHandler(__instance, ref __result);
                }
                [HarmonyPrefix, HarmonyPatch(typeof(TaiwuEvent_c13ad364e77b4e768742218d3231af21), "OnOption1Select")]
                public static bool TaiwuEventMarray_GuiZhou_Entry_OnOption1Select_Prefix(TaiwuEvent_c13ad364e77b4e768742218d3231af21 __instance, ref string __result)
                {
                    return CommonGuiZhouMarryEventEntryHandler(__instance, ref __result);
                }
                #endregion
                // 桂州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_e99e645eaeca4503ba38283bd98d56b2), "OnOption1Select")]
                public static void TaiwuEventMarray_GuiZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_e99e645eaeca4503ba38283bd98d56b2 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "9693dc1d-8117-498a-9f0f-7971acee80d7", "d4f46532-61eb-4b68-a90d-420b7227b322", "29426835-16ff-4779-8245-1a2482f0f085");
                }
            }

            /// <summary>
            /// 杭州比武招亲
            /// </summary>
            static class HangZhouMarryEvent
            {
                // 杭州比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_77578fba74bf409f8acc25cbfef2f3b0), "OnOption1Select")]
                public static void TaiwuEventMarray_HangZhou_Entry_OnOption1Select_Postfix(TaiwuEvent_77578fba74bf409f8acc25cbfef2f3b0 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "f83680db-d392-4b11-946a-db6d0e41886c", "4b1a91c0-d317-4cd0-be10-bfda13ed7b0a", "7c6e2f83-3c83-455f-8b44-0af807942afc", "132b9847-4a91-47bc-8cc8-ff6c8f615575");
                }
                // 杭州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_04d44869ac5c4927b41f7e6635fc9820), "OnOption1Select")]
                public static void TaiwuEventMarray_HangZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_04d44869ac5c4927b41f7e6635fc9820 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    int favorability = EventHelper.GetFavorability(target, taiwu);
                    int adventureParameter = EventHelper.GetAdventureParameter("love");
                    if (favorability >= adventureParameter)
                    {
                        CommonMarrayEventFinishHandler(ref __result, taiwu, target, "317d057e-796d-4e61-a11c-a5ac91c7147e", "71d5a2dd-7908-4faa-af8f-3e33950573c8", "6c457da3-abd3-42ff-b2fd-abb78a08141d");
                    }
                    else
                    {
                        __result = "31e0309a-3cad-4652-9955-007616e2daad";
                    }
                }
            }
            
            /// <summary>
            /// 江陵比武招亲
            /// </summary>
            static class JiangLingMarryEvent
            {
                // 江陵比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_2cc3ae0382c24a88b628ac62ae533dce), "OnOption1Select")]
                public static void TaiwuEventMarray_JiangLing_Entry_OnOption1Select_Postfix(TaiwuEvent_2cc3ae0382c24a88b628ac62ae533dce __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R6");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "a6031685-72c9-4c8b-a958-bddcb5fea2da", "b47f59dc-8443-47cd-8929-e2ddadc12601", "c9b53ac7-8898-4c1a-b5fc-4703ede154d5", "f6b99a54-e1d0-4013-9db6-694a847d791f");
                }
                // 江陵比武招亲终点选对高好感
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_e97189e78a2d42db88e6b7cbe0b3ddd9), "OnOption1Select")]
                public static void TaiwuEventMarray_JiangLing_SucHigh_OnOption1Select_Postfix(TaiwuEvent_e97189e78a2d42db88e6b7cbe0b3ddd9 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R6");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "353cf4f9-0388-4af8-b877-311607c3079b", "1339846f-6760-48c0-84d8-cd07cc0c715e", "f4b46456-2c27-427d-9cb2-1fa3d041b740");
                }
                // 江陵比武招亲终点选对中好感
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_e136425a59ea41419dd1ac5d2b9bb86a), "OnOption1Select")]
                public static void TaiwuEventMarray_JiangLing_SucMid_OnOption1Select_Postfix(TaiwuEvent_e136425a59ea41419dd1ac5d2b9bb86a __instance, ref string __result)
                {

                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R6");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "1318403c-d4af-4b37-b932-fd0472de0097", "1339846f-6760-48c0-84d8-cd07cc0c715e", "f4b46456-2c27-427d-9cb2-1fa3d041b740");
                }
                // 江陵比武招亲终点选对低好感
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_1bffcb5f7c9f44bf83328b91e07b93a8), "OnOption1Select")]
                public static void TaiwuEventMarray_JiangLing_SucLow_OnOption1Select_Postfix(TaiwuEvent_1bffcb5f7c9f44bf83328b91e07b93a8 __instance, ref string __result)
                {

                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R6");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "40f6bd27-fcae-47cf-9a0d-0e4c28f7340e", "3833d025-bef9-4cd6-bee2-c9a2b24e56f1", "82cb884e-e6ca-41da-b61c-fa5b1260f082");
                }
            }

            /// <summary>
            /// 京城比武招亲
            /// </summary>
            static class JingChengMarryEvent
            {
                // 京城比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_d944505d10b5450fa7c1a854caa17949), "OnOption1Select")]
                public static void TaiwuEventMarray_JingCheng_Entry_OnOption1Select_Postfix(TaiwuEvent_d944505d10b5450fa7c1a854caa17949 __instance, ref string __result)
                {
                    if (__instance.ArgBox.GetAdventureParticipateCharacterCount(0) < 7)
                    {
                        __result = "0906956c-e06a-40d0-975a-2856378d6275";
                    }
                    else
                    {
                        Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                        Character target = __instance.ArgBox.GetCharacter("R1");
                        CommonMarrayEventEntryHandler(ref __result, taiwu, target, "7778ebee-5a59-4bf3-acd2-26669ada7936", "a51e1bb4-eca3-46f0-a555-0284634fd2a7", "007b6719-14dd-43aa-8002-28dab1910b61", "007b6719-14dd-43aa-8002-28dab1910b61");
                    }
                }
                // 京城比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_f943e03f02c245dba5ac0988b6058d45), "OnOption1Select")]
                public static void TaiwuEventMarray_JingCheng_Finish_OnOption1Select_Postfix(TaiwuEvent_f943e03f02c245dba5ac0988b6058d45 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "38bb4940-7070-4460-98c9-16a75f0d9d66", "7a3571e1-cfc5-41aa-acd0-80a24ef7b0bc", "68681a54-1bfe-4937-87b6-c8a1f1780039");
                }
            }

            /// <summary>
            /// 辽阳比武招亲
            /// </summary>
            static class LiaoYangMarryEvent
            {
                // 辽阳比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_7183650b056a4dc7b980bf3afbe9a094), "OnOption1Select")]
                public static void TaiwuEventMarray_LiaoYang_Entry_OnOption1Select_Postfix(TaiwuEvent_7183650b056a4dc7b980bf3afbe9a094 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "c3f1bd68-d6c7-4837-98fc-f036c396a40d", "cc57cc60-e616-4aa0-afdc-28707aad9a63", "cc57cc60-e616-4aa0-afdc-28707aad9a63", "132b9847-4a91-47bc-8cc8-ff6c8f615575");
                }
                // 辽阳比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_3abddbc2218549828a585d815a118844), "OnOption1Select")]
                public static void TaiwuEventMarray_LiaoYang_Finish_OnOption1Select_Postfix(TaiwuEvent_3abddbc2218549828a585d815a118844 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    int favorability = EventHelper.GetFavorability(target, taiwu);
                    int adventureParameter = EventHelper.GetAdventureParameter("love");
                    if (favorability >= adventureParameter)
                    {
                        CommonMarrayEventFinishHandler(ref __result, taiwu, target, "b9555076-578d-42bb-b6d5-fde808aa9c5c", "bf7fb8a5-3f14-4ab4-bb73-cb312a0baa34", "3275a298-3ec7-454b-8447-84c96a56dcdb");
                    }
                    else
                    {
                        __result = "dc825221-b0b6-408c-bc5f-99f68ce551ee";
                    }
                }
            }

            /// <summary>
            /// 青州比武招亲
            /// </summary>
            static class QingZhouMarryEvent
            {
                // 青州比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_9b6ba5cd662b4f61a37da8d4671430ec), "OnOption1Select")]
                public static void TaiwuEventMarray_QingZhou_Entry_OnOption1Select_Postfix(TaiwuEvent_9b6ba5cd662b4f61a37da8d4671430ec __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "90df6d51-fd70-4f08-afae-311c0d500af7", "63373769-a546-4c91-bf02-ec3128e58fdf", "127fe103-b00c-4a8f-be77-e6859f73a55e", "0c84daba-30a5-441f-bef8-ac1177edb2f9");
                }
                // 青州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_0e8205f06be64c7fa2c6c5f1a4846c09), "OnOption1Select")]
                public static void TaiwuEventMarray_QingZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_0e8205f06be64c7fa2c6c5f1a4846c09 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    string successPlaceHolder = "4a258ee1-d0c3-4c4a-b532-387249335c8a"; // 先用最低好感的结果，如果得到是这个result，再根据好感度升级
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, successPlaceHolder, "bb82935d-e2c9-4a1a-ba74-b84a24a54d98", "e60c9ad5-c6d0-482e-9b38-909330a1226d");
                    if (__result == successPlaceHolder)
                    {
                        int favorabilityType = EventHelper.GetFavorabilityType(taiwu, target);
                        if (favorabilityType >= FavorabilityType.Favorite5)
                        {
                            __result = "9e41183e-3271-4eb1-a79e-d641230c0f8d";
                        }
                        else if (favorabilityType >= FavorabilityType.Unfamiliar)
                        {
                            __result = "e1ac06ab-6a2d-4178-9975-4cb76b8c522b";
                        }
                    }
                }
            }

            /// <summary>
            /// 秦州比武招亲
            /// </summary>
            static class QinZhouMarryEvent
            {
                // 秦州比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_ed953a154e8342f588b84635c8bcd621), "OnOption1Select")]
                public static void TaiwuEventMarray_QinZhou_Entry_OnOption1Select_Postfix(TaiwuEvent_ed953a154e8342f588b84635c8bcd621 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "85812b1e-ae01-4253-8662-6bf38f906c71", "92d5a7b8-4c33-4f61-991a-041915d2b17e", "4e4ad014-a580-411e-bd17-97770715152e", "7feaff96-19df-4a95-a77d-41ec0cbd68a1");
                }
                // 秦州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_8700d8379c3b4ead8871bf68a67c07ed), "OnOption1Select")]
                public static void TaiwuEventMarray_QinZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_8700d8379c3b4ead8871bf68a67c07ed __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "861963b3-f6f9-4cd7-a988-6672a0749562", "e4441c86-8c16-45a2-b510-22e1befb3dcb", "7afb72b1-2487-484c-8620-e9d5fb727a06");
                }
            }

            /// <summary>
            /// 寿春比武招亲
            /// </summary>
            static class ShouChunMarryEvent
            {
                // 寿春比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_137191a07a534ce39285cd73c10233d0), "OnOption1Select")]
                public static void TaiwuEventMarray_ShouChun_Entry_OnOption1Select_Postfix(TaiwuEvent_137191a07a534ce39285cd73c10233d0 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "602c18a0-81df-4f04-9f2d-c912657b4463", "a6c66d72-d364-4588-9df8-847c74cb584c", "97884777-67c3-46f3-ad1f-499e4e859473", "9d2ae125-5e18-48ac-bc03-2f95d9551301");
                }
                // 寿春比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_bfa8bd980c0e44f3a6778b04af895c13), "OnOption1Select")]
                public static void TaiwuEventMarray_ShouChun_Finish_OnOption1Select_Postfix(TaiwuEvent_bfa8bd980c0e44f3a6778b04af895c13 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    string successPlaceHolder = "dbd33e99-fed7-4f05-b284-f2fce04e1a1a"; // 先用最低好感的结果，如果得到是这个result，再根据好感度升级
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, successPlaceHolder, "3161b48e-b6b3-48a4-99c3-99da2c60e342", "90d23b20-ec91-4ef0-9b13-e127c82ee54a");
                    if (__result == successPlaceHolder)
                    {
                        int favorabilityType = EventHelper.GetFavorabilityType(taiwu, target);
                        if (favorabilityType >= FavorabilityType.Favorite5)
                        {
                            __result = "e92dd80c-9f14-495f-a73c-86875d4100d6";
                        }
                        else if (favorabilityType >= FavorabilityType.Unfamiliar)
                        {
                            __result = "c514ca3e-c230-4808-a591-ae0a33d70612";
                        }
                    }
                }
            }

            /// <summary>
            /// 太原比武招亲
            /// </summary>
            static class TaiYuanMarryEvent
            {
                // 太原比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_77218701d69b45fa99d2729441773dd9), "OnOption1Select")]
                public static void TaiwuEventMarray_TaiYuan_Entry_OnOption1Select_Postfix(TaiwuEvent_77218701d69b45fa99d2729441773dd9 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    // 原代码里 非男性角色，gender!=Female，又走到判定成功的事件了，还绕过了年龄check，可能是写错了，但是保持一致先……(是不是看不起百合！？)
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "508507a9-72cd-4681-8aec-8930e62f4eb8", "a76d7d7d-a4c1-411e-b60a-0f5686212bdf", "a346e78b-47a5-439d-a77f-b55f25c71027", "508507a9-72cd-4681-8aec-8930e62f4eb8");
                }
                // 太原比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_fa86099d919843fc98fc9d499853c1fa), "OnOption1Select")]
                public static void TaiwuEventMarray_TaiYuan_Finish_OnOption1Select_Postfix(TaiwuEvent_fa86099d919843fc98fc9d499853c1fa __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    string successPlaceHolder = "18d306e9-8364-47db-96c3-a5166f96943a"; // 先用最低好感的结果，如果得到是这个result，再根据好感度升级
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, successPlaceHolder, "b941ce08-f1d3-4929-813a-8c7fa208bd75", "6423542e-d2f8-417d-b8f3-e48e57b9aedc");
                    if (__result == successPlaceHolder)
                    {
                        int favorabilityType = EventHelper.GetFavorabilityType(taiwu, target);
                        if (favorabilityType >= FavorabilityType.Favorite5)
                        {
                            __result = "f5a41fb3-ee88-40e1-bec1-fcc29372a8c8";
                        }
                        else if (favorabilityType >= FavorabilityType.Unfamiliar)
                        {
                            __result = "8b79727a-756d-4edf-8164-93eb2076541d";
                        }
                    }
                }
            }

            /// <summary>
            /// 襄阳比武招亲
            /// </summary>
            static class XiangYangMarryEvent
            {
                // 襄阳比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_acfb81d89a094da5b9fea25c5d5280b1), "OnOption1Select")]
                public static void TaiwuEventMarray_XiangYang_Entry_OnOption1Select_Postfix(TaiwuEvent_acfb81d89a094da5b9fea25c5d5280b1 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, "aa9610a2-a559-4c32-afbe-d38f7f16f2cb", "0fda66a0-49ba-42de-bcd6-511fa763db29", "0fda66a0-49ba-42de-bcd6-511fa763db29", "291ed057-e378-4eb4-af15-04143851525e");
                }
                // 襄阳比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_499a24092a9241b7b5ba27d3a83bb5b7), "OnOption1Select")]
                public static void TaiwuEventMarray_XiangYang_Finish_OnOption1Select_Postfix(TaiwuEvent_499a24092a9241b7b5ba27d3a83bb5b7 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R0");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "fc7ce259-bc70-417d-bf8f-ee9a8b25ceb9", "f0a3b1c9-a4e1-4f31-a382-a65d52436f5f", "3d85bee0-f77e-4993-86ca-3f49b40257b2");
                }
            }

            /// <summary>
            /// 扬州比武招亲
            /// </summary>
            static class YangZhouMarryEvent
            {
                // 扬州比武招亲入口
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_2ed5796cd79449a9b02eeec6e75b8c36), "OnOption1Select")]
                public static void TaiwuEventMarray_YangZhou_Entry_OnOption1Select_Postfix(TaiwuEvent_2ed5796cd79449a9b02eeec6e75b8c36 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    string entranceEvent = "e813882d-3f24-4980-8eb7-9eb74b766204";
                    CommonMarrayEventEntryHandler(ref __result, taiwu, target, entranceEvent, "2c695b8a-5bc6-4cd8-b41a-21b3cdbfb881", "f1771846-5928-4b19-a198-ba3e1fa11550", "132b9847-4a91-47bc-8cc8-ff6c8f615575");
                    if (__result == entranceEvent)
                    {
                        // 收500银钱
                        EventHelper.ChangeRoleResource(taiwu, 6, -500, true);
                    }
                }
                // 扬州比武招亲终点
                [HarmonyPostfix, HarmonyPatch(typeof(TaiwuEvent_d105760379134167aa27a2d761fa0a93), "OnOption1Select")]
                public static void TaiwuEventMarray_YangZhou_Finish_OnOption1Select_Postfix(TaiwuEvent_d105760379134167aa27a2d761fa0a93 __instance, ref string __result)
                {
                    Character taiwu = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    Character target = __instance.ArgBox.GetCharacter("R1");
                    CommonMarrayEventFinishHandler(ref __result, taiwu, target, "068f219e-df1d-4bb6-9c90-3ec8e4a14e82", "43abbdf9-e513-47f8-bc9b-840996455165", "43abbdf9-e513-47f8-bc9b-840996455165");
                }
            }
        }
    }
}
