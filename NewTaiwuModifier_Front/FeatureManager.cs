using Config;
using System.Collections.Generic;
using System.IO;

namespace NewTaiwuModifier_Front
{
    internal class FeatureManager
    {
        public List<short> totalFeatureIds = new List<short>();   // 所有特性
        public List<short> basicFeatures = new List<short>();     // 基础特性
        public List<short> selectedFeatures = new List<short>();  // 已选固定特性
        public List<short> randomFeaturePool = new List<short>(); // 随机池
        public static FeatureManager instance;

        private static readonly string fileName = "feature.csv";
        private static string filePath = "";
        private static StreamWriter writer;

        public static void Clear()
        {
            if (instance == null)
            {
                return;
            }
            instance.totalFeatureIds.Clear();
            instance.basicFeatures.Clear();
            instance.selectedFeatures.Clear();
            instance.randomFeaturePool.Clear();
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new FeatureManager();
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

        public static void SetFilePath(string newDirPath)
        {
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

        private static bool PrintFeatureItem(CharacterFeatureItem item)
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
            return true;
        }

        public static void WriteToFile()
        {
            writer.WriteLine("名称,Id,candidate,mutex,级别,基础特性,性别限制，描述");
            CharacterFeature.Instance.Iterate(PrintFeatureItem);
        }
    }
}
