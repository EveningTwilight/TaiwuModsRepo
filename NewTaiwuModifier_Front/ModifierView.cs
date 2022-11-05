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
    internal class ModifierView : FeatureContainerMono
    {
        private static readonly string FeatureLineStr = "FeaturelLine";  // 有个拼写错误，所以统一访问，万一官方改了，方便修复
        // Model
        private List<short> _featureIds = new List<short>();
        private GameObject _featureLine;
        private GameObject _lifeSkillLine;
        private GameObject _combatSkillLine;
        private GameObject _MainAttributeLine;

        private bool needRefresh = false;

        public GameObject view;

        /*public ModifierView(GameObject root)
        {
            CreateUI(root);
        }*/

        public void Start()
        {
            ModifierManager.Init();
            if (_featureIds.Count == 0)
            {
                UpdateFeatureList(ModifierManager.instance.selectedFeatures);
            }
        }

        public void Update()
        {
            RefreshFeatures();  // ifNeed
            RefreshMainAttributes();
            RefreshLifeSkills();
            RefreshCombatSkills();
        }
        override public void UpdateFeatureList(List<short> featureIds)
        {
            _featureIds = featureIds;
            _featureIds.Sort();
            needRefresh = true;
        }
        public void RefreshFeatures()
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
                    Debug.LogFormat("[----FeatureItem----] UnselectFeatureId: {0}", featureId);
                    ModifierManager.UnselectFeature(featureId);
                    needRefresh = true;
                });
            }
            for (int i = _featureIds.Count; i < components.Length; ++i)
            {
                components[i].gameObject.SetActive(false);
            }
        }
        public void RefreshMainAttributes()
        {
            if (!ModifierManager.instance.mainAttributesUpdated)
            {
                return;
            }
            ModifierManager.instance.mainAttributesUpdated = false;

            Refers mainAttrRefers = _MainAttributeLine.GetComponent<Refers>();
            List<Refers> mainAttrItemRefers = mainAttrRefers.CGetList<Refers>("AttriItem_");
            for (int i = 0; i < mainAttrItemRefers.Count; i++)
            {
                Refers item = mainAttrItemRefers[i];
                TMP_InputField mainAttrInput = item.gameObject.GetOrAddComponent<TMP_InputField>();
                mainAttrInput.text = ModifierManager.GetMainAttribute(i).ToString();
                mainAttrInput.DeactivateInputField();
            }
        }
        public void RefreshLifeSkills()
        {
            if (!ModifierManager.instance.lifeSkillsUpdated)
            {
                return;
            }
            ModifierManager.instance.lifeSkillsUpdated = false;

            Refers skillRefers = _lifeSkillLine.GetComponent<Refers>();
            List<Refers> skillItemRefers = skillRefers.CGetList<Refers>("SkillItem_");
            for (int i = 0; i < skillItemRefers.Count; i++)
            {
                Refers item = skillItemRefers[i];
                TMP_InputField skillValueInput = item.gameObject.GetOrAddComponent<TMP_InputField>();
                skillValueInput.text = ModifierManager.GetLifeSkill(i).ToString();
                /*skillValueInput.ActivateInputField();*/
                skillValueInput.DeactivateInputField();
            }
        }
        public void RefreshCombatSkills()
        {
            if (!ModifierManager.instance.combatSkillsUpdated)
            {
                return;
            }
            ModifierManager.instance.combatSkillsUpdated = false;

            Refers skillRefers = _combatSkillLine.GetComponent<Refers>();
            List<Refers> skillItemRefers = skillRefers.CGetList<Refers>("SkillItem_");
            for (int i = 0; i < skillItemRefers.Count; i++)
            {
                Refers item = skillItemRefers[i];
                TMP_InputField skillValueInput = item.gameObject.GetOrAddComponent<TMP_InputField>();
                skillValueInput.text = ModifierManager.GetCombatSkill(i).ToString();
                /*skillValueInput.ActivateInputField();*/
                skillValueInput.DeactivateInputField();
            }
        }

        public void CreateUI(GameObject root)
        {
            GameObject WindowRoot = GameObject.Find("Camera_UIRoot/Canvas/LayerMain/UI_NewGame/WindowRoot");
            /// 保留技艺&武学资质+人物特性，因为在右侧，先保持原世界坐标
            GameObject FaceBack = Instantiate(WindowRoot.transform.Find("NewGameBack/ScrollTabs/InscriptionView/FaceBack").gameObject, root.transform, true);
            GameObject AdjustRoot = FaceBack.transform.Find("AdjustRoot").gameObject;
            GameObject Frame = null;
            for (int i = 0; i < AdjustRoot.transform.childCount; i++)
            {
                GameObject obj = AdjustRoot.transform.GetChild(i).gameObject;
                if (obj.transform.Find(FeatureLineStr))
                {
                    Frame = obj;
                    continue;
                }
                if (obj)
                {
                    Destroy(obj);
                }
            }
            view = FaceBack;
            view.gameObject.SetActive(true);

            // 调一下背景图
            GameObject bgImg = Frame.transform.Find("Image").gameObject;
            RectTransform bgImgTF = bgImg.GetComponent<RectTransform>();
            bgImgTF.anchoredPosition += new Vector2(0, 14);

            GameObject LifeSkillLine = null;
            GameObject CombatSkillLine = null;
            GameObject MainAttributeLine = null;
            for (int i = 0; i < Frame.transform.childCount; i++)
            {
                GameObject obj = Frame.transform.GetChild(i).gameObject;
                if (obj.name == "MainAttributeLine")
                {
                    MainAttributeLine = obj;
                }
                else if (obj.name == "LifeSkillLine")
                {
                    LifeSkillLine = obj;
                }
                else if (obj.name == "CombatSkillLine")
                {
                    CombatSkillLine = obj;
                }
                else if (obj.name != "Image" && obj.name != FeatureLineStr && obj.name != "DressLine")
                {
                    Destroy(obj);
                }
            }
            
            /// 每排一个随机按钮、一个输入框(最小均值)、各属性/资质支持编辑
            // 主属性
            {
                // 随机按钮
                GameObject randomMainAttr = Instantiate(WindowRoot.transform.Find("NewGameBack/ScrollTabs/FaceView/FaceHolder/RandomAvatar").gameObject, MainAttributeLine.transform);
                randomMainAttr.transform.name = "randomMainAttr";
                RectTransform rndMainTF = randomMainAttr.GetComponent<RectTransform>();
                CommonDealRandomButton(rndMainTF);
                CButton randomMainBtn = randomMainAttr.GetOrAddComponent<CButton>();
                randomMainBtn.ClearAndAddListener(() =>
                {
                    ModifierManager.RandomMainAttributes();
                });

                // 标签编辑
                Refers mainAttrRefers = MainAttributeLine.GetComponent<Refers>();
                List<Refers> mainAttrItemRefers = mainAttrRefers.CGetList<Refers>("AttriItem_");
                for (int i = 0; i < mainAttrItemRefers.Count; i++)
                {
                    Refers item = mainAttrItemRefers[i];
                    TextMeshProUGUI mainAttrValue = item.CGet<TextMeshProUGUI>("AttrValue");
                    mainAttrValue.text = "0";
                    /*TextMeshProUGUI mainAttrEditText = Instantiate(mainAttrValue, mainAttrValue.transform.parent, true);
                    mainAttrEditText.name = "AttriValueEdited";*/
                    TMP_InputField mainAttrInput = mainAttrValue.transform.parent.gameObject.GetOrAddComponent<TMP_InputField>();
                    mainAttrInput.textViewport = mainAttrValue.transform.parent.gameObject.GetComponent<RectTransform>();
                    mainAttrInput.enabled = false;
                    mainAttrInput.text = "0";
                    mainAttrInput.textComponent = mainAttrValue;
                    mainAttrInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    mainAttrInput.characterLimit = 3;
                    mainAttrInput.onEndEdit.AddListener(value =>
                    {
                        Debug.LogFormat("[EditMainAttr] set:{0} name:{1} value:{2}", i, item.CGet<TextMeshProUGUI>("AttrName").text, value);
                        ModifierManager.SetMainAttribute(i, (short)(value.Length > 0 ? Convert.ToInt16(value) : 0));
                        mainAttrInput.DeactivateInputField();
                    });
                    mainAttrInput.enabled = true;
                }

                // 最小均值输入框
                GameObject mainAttrMinAvgObj = Instantiate(MainAttributeLine.transform.Find("Container/AttriItem_0").gameObject, MainAttributeLine.transform);
                mainAttrMinAvgObj.name = "mainAttrMinAvgInput";
                GameObject minAvgInputName = null;
                GameObject minAvgValue = null;
                RectTransform minAvgTF = mainAttrMinAvgObj.GetComponent<RectTransform>();
                for (int i = 0; i < minAvgTF.childCount; ++i)
                {
                    GameObject obj = minAvgTF.GetChild(i).gameObject;
                    if (obj.name == "AttrName")
                    {
                        obj.name = "MinAvgName";
                        minAvgInputName = obj;
                    }
                    else if (obj.name == "AttrValue")
                    {
                        obj.name = "MinAvgValue";
                        minAvgValue = obj;
                    }
                    else if (obj.name == "Icon")
                    {
                        Destroy(obj);
                    }
                }
                minAvgTF.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                minAvgTF.SetSize(new Vector2(160, 36));
                minAvgTF.localPosition = rndMainTF.localPosition - new Vector3(130, 5, 0);
                RectTransform minAvgInputNameTF = minAvgInputName.GetComponent<RectTransform>();
                minAvgInputNameTF.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                minAvgInputNameTF.anchoredPosition = new Vector2(6, 0);
                TextMeshProUGUI minAvgInputText = minAvgInputName.GetComponent<TextMeshProUGUI>();
                minAvgInputText.alignment = TextAlignmentOptions.Left;
                minAvgInputText.text = "最小均值";
                TextMeshProUGUI minAvgValueText = minAvgValue.GetComponent<TextMeshProUGUI>();
                minAvgValueText.text = "0";
                /*minAvgValueText.text = ModifierManager.min*/
                TMP_InputField minAvgInput = mainAttrMinAvgObj.GetComponent<TMP_InputField>();
                minAvgInput.enabled = false;
                minAvgInput.text = minAvgValueText.text;
                minAvgInput.textComponent = minAvgValueText;
                minAvgInput.textViewport = minAvgTF;
                minAvgInput.onEndEdit.RemoveAllListeners();
                minAvgInput.onEndEdit.AddListener(value =>
                {
                    Debug.LogFormat("[EditMainAttr] 设置主属性最小均值:{0}", value);
                });
                minAvgInput.enabled = true;
            }
            // 技艺资质
            {
                // 随机按钮
                GameObject randomLifeSkill = Instantiate(WindowRoot.transform.Find("NewGameBack/ScrollTabs/FaceView/FaceHolder/RandomAvatar").gameObject, LifeSkillLine.transform);
                randomLifeSkill.transform.name = "randomLifeSkill";
                RectTransform rndLifeTF = randomLifeSkill.GetComponent<RectTransform>();
                CommonDealRandomButton(rndLifeTF);
                CButton randomLifeBtn = randomLifeSkill.GetOrAddComponent<CButton>();
                randomLifeBtn.ClearAndAddListener(() =>
                {
                    ModifierManager.RandomLifeSkills();
                });

                // 标签编辑
                Refers lifeSkillRefers = LifeSkillLine.GetComponent<Refers>();
                List<Refers> skillItemRefers = lifeSkillRefers.CGetList<Refers>("SkillItem_");
                for (int i = 0; i < skillItemRefers.Count; i++)
                {
                    Refers item = skillItemRefers[i];
                    TextMeshProUGUI skillValueText = item.CGet<TextMeshProUGUI>("SkillValue");
                    skillValueText.text = "0";
                    TMP_InputField skillValueInput = skillValueText.transform.parent.gameObject.GetOrAddComponent<TMP_InputField>();
                    skillValueInput.textViewport = skillValueText.transform.parent.gameObject.GetComponent<RectTransform>();
                    skillValueInput.enabled = false;
                    skillValueInput.text = "0";
                    skillValueInput.lineType = TMP_InputField.LineType.SingleLine;
                    skillValueInput.textComponent = skillValueText;
                    skillValueInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    skillValueInput.characterLimit = 3;
                    skillValueInput.onEndEdit.AddListener(value =>
                    {
                        Debug.LogFormat("[EditMainAttr] set:{0} name:{1} value:{2}", i, item.CGet<TextMeshProUGUI>("SkillName").text, value);
                        ModifierManager.SetLifeSkill(i, (short)(value.Length > 0 ? Convert.ToInt16(value) : 0));
                        skillValueInput.DeactivateInputField();
                    });
                    skillValueInput.enabled = true;
                }
            }
            // 功法资质
            {
                // 随机按钮
                GameObject randomCombatSkill = Instantiate(WindowRoot.transform.Find("NewGameBack/ScrollTabs/FaceView/FaceHolder/RandomAvatar").gameObject, CombatSkillLine.transform);
                randomCombatSkill.name = "randomCombatSkill";
                RectTransform rndCombatTF = randomCombatSkill.GetComponent<RectTransform>();
                CommonDealRandomButton(rndCombatTF);
                CButton randomCombatBtn = randomCombatSkill.GetOrAddComponent<CButton>();
                randomCombatBtn.ClearAndAddListener(() =>
                {
                    ModifierManager.RandomCombatSkills();
                });

                // 标签编辑
                Refers lifeSkillRefers = CombatSkillLine.GetComponent<Refers>();
                List<Refers> skillItemRefers = lifeSkillRefers.CGetList<Refers>("SkillItem_");
                for (int i = 0; i < skillItemRefers.Count; i++)
                {
                    Refers item = skillItemRefers[i];
                    TextMeshProUGUI skillValueText = item.CGet<TextMeshProUGUI>("SkillValue");
                    skillValueText.text = "0";
                    TMP_InputField skillValueInput = skillValueText.transform.parent.gameObject.GetOrAddComponent<TMP_InputField>();
                    skillValueInput.textViewport = skillValueText.transform.parent.gameObject.GetComponent<RectTransform>();
                    skillValueInput.enabled = false;
                    skillValueInput.text = "0";
                    skillValueInput.lineType = TMP_InputField.LineType.SingleLine;
                    /*skillValueInput.placeholder = skillValueText;*/
                    skillValueInput.textComponent = skillValueText;
                    skillValueInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    skillValueInput.characterLimit = 3;
                    skillValueInput.onEndEdit.AddListener(value =>
                    {
                        Debug.LogFormat("[EditMainAttr] set:{0} name:{1} value:{2}", i, item.CGet<TextMeshProUGUI>("SkillName").text, value);
                        ModifierManager.SetCombatSkill(i, (short)(value.Length > 0 ? Convert.ToInt16(value) : 0));
                        skillValueInput.DeactivateInputField();
                    });
                    skillValueInput.enabled = true;
                }
            }
            
            _featureLine = Frame.transform.Find(FeatureLineStr).gameObject;
            _lifeSkillLine = LifeSkillLine;
            _combatSkillLine = CombatSkillLine;
            _MainAttributeLine = MainAttributeLine;
        }

        private static void CommonDealRandomButton(RectTransform randomButtonRectTF)
        {
            randomButtonRectTF.localScale = new Vector3(0.5f, 0.5f, 1);
            randomButtonRectTF.anchorMin = randomButtonRectTF.anchorMax = new Vector2(1, 1);  // 以Line的右上角为锚点
            randomButtonRectTF.anchoredPosition = new Vector2(-60, -20);
            if (randomButtonRectTF.Find("LabelRoot"))
            {
                Destroy(randomButtonRectTF.Find("LabelRoot").gameObject);
            }
            if (randomButtonRectTF.Find("AutoWidthLablePreset_30_1"))
            {
                Destroy(randomButtonRectTF.Find("AutoWidthLablePreset_30_1").gameObject);
            }
            MouseTipDisplayer mouseTip = randomButtonRectTF.gameObject.GetOrAddComponent<MouseTipDisplayer>();
            mouseTip.enabled = false;
            mouseTip.Type = TipType.Simple;
            if (mouseTip.PresetParam == null)
            {
                mouseTip.PresetParam = new string[]
                {
                    "随机属性",
                    "根据设置的最低均值，随机生成各个属性的最低值"
                };
            }
            mouseTip.enabled = true;
        }

        private class InputValidator : TMP_InputValidator
        {
            public override char Validate(ref string text, ref int pos, char ch)
            {
                throw new NotImplementedException();
            }
        }
    }
}
