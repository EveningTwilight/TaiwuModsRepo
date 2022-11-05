using Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UICommon.Character.Elements;
using UnityEngine;

namespace NewTaiwuModifier_Front
{
    internal class AllFeatureView : FeatureContainerMono
    {
        private static readonly string FeatureLineStr = "FeaturelLine";  // 有个拼写错误，所以统一访问，万一官方改了，方便修复
        // Model
        private List<short> _featureIds = new List<short>();
        private GameObject _featureLine;

        private bool needRefresh = false;

        public GameObject view;

        /*public AllFeatureView(GameObject root)
        {
            CreateUI(root);
        }*/

        private static int FeatureComparison(short a, short b)  // TODO: EveningTwilight 完善备选特性部分UI及排序
        {
            CharacterFeatureItem item1 = CharacterFeature.Instance[a];
            CharacterFeatureItem item2 = CharacterFeature.Instance[b];
            if (item1.MutexGroupId < item2.MutexGroupId)
            {
                return -1;
            }
            if (item2.MutexGroupId < item1.MutexGroupId)
            {
                return 1;
            }
            if (item1.Level > item2.Level)
            {
                return -1;
            }
            return a == b ? 0 : a > b ? 1 : -1;
        }

        public void Start()
        {
            ModifierManager.Init();
            if (_featureIds.Count == 0)
            {
                ModifierManager.ResetUnselected(ModifierManager.instance.basicFeatures, ModifierManager.instance.selectedFeatures);
                UpdateFeatureList(ModifierManager.instance.unselectedFeatures);
            }
        }

        public void Update()
        {
            Refresh();  // ifNeed
        }
        override public void UpdateFeatureList(List<short> featureIds)
        {
            _featureIds = featureIds;
            _featureIds.Sort();
            needRefresh = true;
        }
        public void Refresh()
        {
            if (!needRefresh)
            {
                return;
            }
            needRefresh = false;
            RectTransform featureContainer = (RectTransform)_featureLine.transform.Find("FeatureContainer");
            Refers[] components = featureContainer.GetComponentsInTopChildren<Refers>(true);
            for (int i = 0; i < _featureIds.Count; i++)
            {
                Refers featureRefers;
                if (components.CheckIndex(i))
                {
                    featureRefers = components[i];
                }
                else
                {
                    featureRefers = Instantiate(components[0], components[0].transform.parent, false);
                    featureRefers.transform.SetAsLastSibling();
                }
                FeatureItem featureItem;
                if (featureRefers.UserObject != null && (featureItem = (featureRefers.UserObject as FeatureItem)) != null)
                {
                    featureItem.Refresh(_featureIds[i]);
                }
                else
                {
                    featureRefers.UserObject = new FeatureItem(featureRefers, _featureIds[i]);
                }
                featureRefers.gameObject.SetActive(true);
                CButton featureButton = featureRefers.gameObject.GetOrAddComponent<CButton>();
                featureButton.transform.SetParent(featureRefers.gameObject.transform, false);
                featureButton.transform.SetAsLastSibling();
                featureButton.name = "featureButton";
                featureButton.enabled = true;
                featureButton.gameObject.SetActive(true);
                short featureId = _featureIds[i];
                featureButton.ClearAndAddListener(() =>
                {
                    Debug.LogFormat("[----FeatureItem----] SelectFeatureId: {0}", featureId);
                    ModifierManager.SelectFeature(featureId);
                    needRefresh = true;
                });
            }
            for (int i = _featureIds.Count; i < components.Length; ++i)
            {
                components[i].gameObject.SetActive(false);
            }
        }
        public void CreateUI(GameObject root)
        {
            GameObject WindowRoot = GameObject.Find("Camera_UIRoot/Canvas/LayerMain/UI_NewGame/WindowRoot");
            /// 仅保留人物特性+滑动条的FaceBack
            GameObject FaceBack = Instantiate(WindowRoot.transform.Find("NewGameBack/ScrollTabs/InscriptionView/FaceBack").gameObject, root.transform);
            GameObject AdjustRoot = FaceBack.transform.Find("AdjustRoot").gameObject;
            /*bool foundFirstFrame = false;*/
            GameObject Frame = null;   // 有两个Frame，第二个是带FeatureLine的，也就是要保留的那个
            for (int i = 0; i < AdjustRoot.transform.childCount; i++)
            {
                GameObject obj = AdjustRoot.transform.GetChild(i).gameObject;
                if (obj.transform.Find(FeatureLineStr))
                {
                    Frame = obj;
                    continue;
                }
                Destroy(obj);
            }
            if (Frame != null)
            {
                for (int i = 0; i < Frame.transform.childCount; i++)
                {
                    GameObject obj = Frame.transform.GetChild(i).gameObject;
                    if (obj && obj.name != "Image" && obj.name != FeatureLineStr && obj.name != "DressLine")
                    {
                        Destroy(obj);
                    }
                }
                _featureLine = Frame.transform.Find(FeatureLineStr).gameObject;
            }
            view = FaceBack;
            view.gameObject.SetActive(true);

            // 修改标题
            GameObject FeatureLineText = _featureLine.transform.Find("Text").gameObject;
            TextMeshProUGUI FeatureLineTitle = FeatureLineText.GetComponent<TextMeshProUGUI>();
            FeatureLineTitle.SetText("备选特性");

            // 调一下背景图
            GameObject bgImg = Frame.transform.Find("Image").gameObject;
            RectTransform bgImgTF = bgImg.GetComponent<RectTransform>();
            bgImgTF.anchoredPosition += new Vector2(0, 14);

            // 需要1.基础特性 2.正面特性 3.三级正面特性 4.全特性
            // 偷一下toggleGroup方便切换特性池子
            /*"Camera_UIRoot/Canvas/LayerPopUp/UI_CharacterMenu/AnimationRoot/ChildPages/CharacterAttributeView/TabToggleGroup/"*/
        }
    }
}
