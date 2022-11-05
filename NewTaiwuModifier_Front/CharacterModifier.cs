using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NewTaiwuModifier_Front
{
    internal class CharacterModifier : MonoBehaviour
    {
        AllFeatureView allFeatureView;
        ModifierView modifierView;
        GameObject ContainerView;
        public static CharacterModifier instance;

        private CharacterModifier()
        {
            // private 不给外面调
        }

        public void OnDestroy()
        {
            if (ContainerView != null)
            {
                ContainerView.SetActive(false);
                Destroy(ContainerView);
            }
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new CharacterModifier();
                instance.ConstructUI();
            }
        }
        public static void Show()
        {
            Init();
            instance.ContainerView.SetActive(true);
        }
        public static void Hide()
        {
            Init();
            instance.ContainerView.SetActive(false);
        }
        public void ConstructUI()
        {
            GameObject WindowRoot = GameObject.Find("Camera_UIRoot/Canvas/LayerMain/UI_NewGame/WindowRoot");
            ContainerView = Instantiate(WindowRoot.transform.Find("NewGameBack").gameObject, WindowRoot.transform, true);
            ContainerView.name = "NewTaiwuModifierBack";
            for (int i = 0; i < ContainerView.transform.childCount; i++)
            {
                Destroy(ContainerView.transform.GetChild(i).gameObject);
            }
            RectTransform rectTransform = ContainerView.GetComponent<RectTransform>();
            rectTransform.sizeDelta += new Vector2(0, 60);

            allFeatureView = ContainerView.AddComponent<AllFeatureView>();
            allFeatureView.CreateUI(ContainerView);
            allFeatureView.view.name = "AllFeatureFrame";
            modifierView = ContainerView.AddComponent <ModifierView>();
            modifierView.CreateUI(ContainerView);
            modifierView.view.name = "ModifierFrame";
            // 左侧备选特性，对齐右侧面板
            RectTransform allFeatureRect = allFeatureView.view.GetComponent<RectTransform>();
            RectTransform modifierRect = modifierView.view.GetComponent<RectTransform>();
            allFeatureRect.anchorMin = modifierRect.anchorMin;
            allFeatureRect.anchorMax = modifierRect.anchorMax;
            // 高度对齐，x轴左移
            allFeatureRect.localPosition = modifierRect.localPosition - new Vector3(allFeatureRect.sizeDelta.x + 20, 0, 0);

            // 注册到特性管理
            ModifierManager.RegisterMono(allFeatureView, modifierView);
        }
    }
}                                        
