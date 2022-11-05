using Config;
using FrameWork.Linq;
using GameData.Domains.Character;
using System;
using System.Collections.Generic;
using UICommon.Character;
using UnityEngine;
using LifeSkillType = Config.LifeSkillType;
using Random = System.Random;

namespace NewTaiwuModifier_Front
{
    internal class FeatureContainerMono : MonoBehaviour
    {
        public virtual void UpdateFeatureList(List<short> featureIds)
        {

        }
    }
    internal class ModifierManager
    {
        public List<short> totalFeatureIds = new List<short>();     // 所有特性
        public List<short> basicFeatures = new List<short>();       // 基础特性
        public List<short> selectedFeatures = new List<short>();    // 已选固定特性
        /*public List<short> randomFeaturePool = new List<short>();   // 随机池*/
        public List<short> unselectedFeatures = new List<short>(); // 备选特性

        public static ModifierManager instance;
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        private static int mainAttributesCount;
        private static int lifeSkillsCount;
        private static int combatSkillsCount;
        private static int minAvgMainAttributes;
        private static int minAvgLifeSkills;
        private static int minAvgCombatSkills;
        private List<short> mainAttributes;
        private List<short> lifeSkills;
        private List<short> combatSkills;
        public bool mainAttributesUpdated;
        public bool lifeSkillsUpdated;
        public bool combatSkillsUpdated;

        private FeatureContainerMono unselectedMono;
        private FeatureContainerMono selectedMono;

        /*private static readonly string fileName = "feature.csv";
        private static string filePath = "";
        private static StreamWriter writer;*/

        public static void Clear()
        {
            Init();
            instance.totalFeatureIds.Clear();
            instance.basicFeatures.Clear();
            instance.selectedFeatures.Clear();
            instance.unselectedFeatures.Clear();
            int maxCnt = Math.Max(lifeSkillsCount, combatSkillsCount);
            for (int i = 0; i < maxCnt; i++)
            {
                if (i < mainAttributesCount)
                {
                    instance.mainAttributes[i] = 0;
                }
                if (i < lifeSkillsCount)
                {
                    instance.lifeSkills[i] = 0;
                }
                if (i < combatSkillsCount)
                {
                    instance.combatSkills[i] = 0;
                }
            }
            /*instance.randomFeaturePool.Clear();*/
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new ModifierManager();
                mainAttributesCount = CharacterMajorAttribute.MainAttributeTemplateIdArray.Length;
                lifeSkillsCount = LifeSkillType.Instance.Count;
                combatSkillsCount = CombatSkillType.Instance.Count;
                instance.mainAttributes = new List<short>(mainAttributesCount);
                instance.lifeSkills = new List<short>(lifeSkillsCount);
                instance.combatSkills = new List<short>(combatSkillsCount);
                int maxCnt = Math.Max(lifeSkillsCount, combatSkillsCount);
                for (int i = 0; i < maxCnt; i++)
                {
                    if (i < mainAttributesCount)
                    {
                        instance.mainAttributes.Add(0);
                    }
                    if (i < lifeSkillsCount)
                    {
                        instance.lifeSkills.Add(0);
                    }
                    if (i < combatSkillsCount)
                    {
                        instance.combatSkills.Add(0);
                    }
                }
                minAvgMainAttributes = 80;
                minAvgLifeSkills = 80;
                minAvgCombatSkills = 80;
                Clear();
                CharacterFeature.Instance.Iterate(item =>
                {
                    instance.totalFeatureIds.Add(item.TemplateId);
                    if (item.Basic)
                    {
                        instance.basicFeatures.Add(item.TemplateId);
                    }
                    return true;
                });
            }
        }

        #region Features
        public static void RegisterMono(FeatureContainerMono unselected, FeatureContainerMono selected)
        {
            Init();
            instance.unselectedMono = unselected;
            instance.selectedMono = selected;
        }
        public static void ResetUnselected(List<short> all, List<short> selected)
        {
            instance.unselectedFeatures.Clear();
            foreach (short id in all)
            {
                if (!selected.Contains(id))
                {
                    instance.unselectedFeatures.Add(id);
                }
            }
        }
        public static void Refresh()
        {
            if (instance.selectedMono)
            {
                instance.selectedMono.UpdateFeatureList(instance.selectedFeatures);
                Debug.Log("[ModifierManager] refresh selectedMono");
            }
            if (instance.unselectedMono)
            {
                instance.unselectedMono.UpdateFeatureList(instance.unselectedFeatures);
                Debug.Log("[ModifierManager] refresh unselectedMono");
            }
        }

        public static void SelectFeature(short FeatureId)
        {
            CharacterFeatureItem item = CharacterFeature.Instance[FeatureId];
            short unselectedId = -1;
            foreach (short selectedId in instance.selectedFeatures)
            {
                CharacterFeatureItem selectedItem = CharacterFeature.Instance[selectedId];
                if (selectedItem.MutexGroupId == item.MutexGroupId)
                {
                    unselectedId = selectedId;
                    break;
                }
            }
            instance.selectedFeatures.Add(FeatureId);
            instance.unselectedFeatures.Remove(FeatureId);
            Debug.LogFormat("[ModifierManager] select feature: {0} by unselected:{1}", FeatureId, unselectedId);
            if (unselectedId >= 0)
            {
                UnselectFeature(unselectedId);
            }
            else {
                Refresh();
            }
        }

        public static void UnselectFeature(short FeatureId)
        {
            Debug.LogFormat("[ModifierManager] unselect feature: {0}", FeatureId);
            if (instance.selectedFeatures.Contains(FeatureId))
            {
                instance.selectedFeatures.Remove(FeatureId);
                instance.unselectedFeatures.Add(FeatureId);
            }
            Refresh();
        }
        #endregion

        #region     Attributes
        public static void RandomMainAttributes()
        {
            Init();
            int total = mainAttributesCount * minAvgMainAttributes;
            int p = 0;
            while (p + 1 < mainAttributesCount)
            {
                int restCount = mainAttributesCount - p;
                int min = (int)(0.5 * total / restCount);
                int max = (int)(1.5 * total / restCount);
                instance.mainAttributes[p] = (short)random.Next(min, max);
                total -= instance.mainAttributes[p];
                Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.mainAttributes[p]);
                ++p;
            }
            instance.mainAttributes[p] = (short)total;
            Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.mainAttributes[p]);
            instance.mainAttributesUpdated = true;
            Debug.LogFormat("[ModifierManager] RandomMainAttributes");
        }
        public static void RandomLifeSkills()
        {
            Init();
            int total = lifeSkillsCount * minAvgLifeSkills;
            int p = 0;
            while (p + 1 < lifeSkillsCount)
            {
                int restCount = lifeSkillsCount - p;
                int min = (int)(0.5 * total / restCount);
                int max = (int)(1.5 * total / restCount);
                instance.lifeSkills[p] = (short)random.Next(min, max);
                total -= instance.lifeSkills[p];
                Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.lifeSkills[p]);
                ++p;
            }
            instance.lifeSkills[p] = (short)total;
            Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.lifeSkills[p]);
            instance.lifeSkillsUpdated = true;
            Debug.LogFormat("[ModifierManager] RandomLifeSkills");
        }
        public static void RandomCombatSkills()
        {
            Init();
            int total = combatSkillsCount * minAvgCombatSkills;
            int p = 0;
            while (p + 1 < combatSkillsCount)
            {
                int restCount = combatSkillsCount - p;
                int min = (int)(0.5 * total / restCount);
                int max = (int)(1.5 * total / restCount);
                instance.combatSkills[p] = (short)random.Next(min, max);
                total -= instance.combatSkills[p];
                Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.combatSkills[p]);
                ++p;
            }
            instance.combatSkills[p] = (short)total;
            Debug.LogFormat("[ModifierManager] \t\t\t{0}", instance.combatSkills[p]);
            instance.combatSkillsUpdated = true;
            Debug.LogFormat("[ModifierManager] RandomCombatSkills");
        }
        public static short GetMainAttribute(int mainAttributeType)
        {
            Init();
            Debug.LogFormat("[ModifierManager] GetMainAttribute id:{0} value:{1}", mainAttributeType, (short)(mainAttributeType < mainAttributesCount ? instance.mainAttributes[mainAttributeType] : 0));
            return (short)(mainAttributeType < mainAttributesCount ? instance.mainAttributes[mainAttributeType] : 0);
        }
        public static void SetMainAttribute(int mainAttributeType, short value)
        {
            Init();
            if (mainAttributesCount <= mainAttributeType)
            {
                return;
            }
            instance.mainAttributes[mainAttributeType] = value;
            instance.mainAttributesUpdated = true;
        }
        public static short GetLifeSkill(int skillTemplatedId)
        {
            Init();
            Debug.LogFormat("[ModifierManager] GetMainAttribute id:{0} value:{1}", skillTemplatedId, (short)(skillTemplatedId < lifeSkillsCount ? instance.lifeSkills[skillTemplatedId] : 0));
            return (short)(skillTemplatedId < lifeSkillsCount ? instance.lifeSkills[skillTemplatedId] : 0);
        }
        public static void SetLifeSkill(int skillTemplateId, short value)
        {
            Init();
            if (lifeSkillsCount <= skillTemplateId)
            { 
                return;
            }
            instance.lifeSkills[skillTemplateId] = value;
            instance.lifeSkillsUpdated = true;
        }
        public static short GetCombatSkill(int skillTemplatedId)
        {
            Init();
            Debug.LogFormat("[ModifierManager] GetMainAttribute id:{0} value:{1}", skillTemplatedId, (short)(skillTemplatedId < combatSkillsCount ? instance.combatSkills[skillTemplatedId] : 0));
            return (short)(skillTemplatedId < combatSkillsCount ? instance.combatSkills[skillTemplatedId] : 0);
        }
        public static void SetCombatSkill(int skillTemplateId, short value)
        {
            Init();
            if (combatSkillsCount <= skillTemplateId)
            {
                return;
            }
            instance.combatSkills[skillTemplateId] = value;
            instance.combatSkillsUpdated = true;
        }
        #endregion
        /*public static void SetFilePath(string newDirPath)
        {
            Init();
            string newFilePath = newDirPath + fileName;
            if (Directory.Exists(newDirPath))
            {
                if (!File.Exists(newFilePath))
                {
                    File.Create(newFilePath);
                }
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
                writer = new StreamWriter(newFilePath);
                filePath = newFilePath;
            }
        }

        private static void PrintFeatureItem(CharacterFeatureItem item)
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
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", name, TemplateId, CandidateGroupId, MutexGroupId, level, isBasic, gender, desc));
            }
        }

        public static void WriteToFile()
        {
            Init();
            writer.WriteLine("名称,Id,candidate,mutex,级别,基础特性,性别限制，描述");
            CharacterFeature.Instance.Iterate(item =>
            {
                PrintFeatureItem(item);
                return true;
            });
        }*/
    }
}
