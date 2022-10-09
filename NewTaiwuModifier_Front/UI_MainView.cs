/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using GuiBaseUI;
using TMPro;

namespace NewTaiwuModifier_Front
{
    internal class UI_MainView : MonoBehaviour
    {
        public static UI_MainView instance;

        public Transform root;
        private Image[] all;
        private int showIndex;
        private Image bg;
        public Transform bgRoot;
        public Transform charMenu;
        public GameObject rootBg;
        public int selectTitleIndex = 0;
        public List<GameObject> titleBgs = new List<GameObject>();
        public List<GameObject> titleSelects = new List<GameObject>();
        private Vector2 dragOffset;
        private float speed = 300f;
        private string uiPath;
        public void Init(GameObject goRoot)
        {
            instance = new UI_MainView();
            root = goRoot.transform;
            Transform charMenu = root.Find("LayerPopUp/UI_CharacterMenu");
            Image charBg = charMenu.GetComponent<Image>();
            GameObject bgGo = CreateUI.NewRawImage(default);
            bgGo.transform.SetParent(transform, false);
            RectTransform tf2 = bgGo.GetComponent<RectTransform>();
            tf2.sizeDelta = new Vector2(1920f, 1080f);
            bgGo.GetComponent<RawImage>().texture = CreateUI.panelBg;
            bgRoot = tf2.transform;
            rootBg = bgGo;

            if (PlayerPrefs.HasKey("yellow.bgPos"))
            {
                string sss = PlayerPrefs.GetString("yellow.bgPos");
                try
                {
                    string[] pp = sss.Split(new char[]
                    {
                        '|'
                    });
                    Vector2 ppp = new Vector2((float)int.Parse(pp[0]), (float)int.Parse(pp[1]));
                    tf2.anchoredPosition = ppp;
                }
                catch (Exception e)
                {
                    Debug.Log("初始化位置错误：" + e.Message + " " + sss);
                    RestUIPosition();
                }
            }
            else
            {
                RestUIPosition();
            }

            GameObject bgTitle = CreateUI.NewImage(null);
            bgTitle.transform.SetParent(bgGo.transform, false);
            RectTransform tf = bgTitle.GetComponent<RectTransform>();
            tf.anchoredPosition = new Vector2(0f, 515f);
            tf.sizeDelta = new Vector2(1920f, 50f);
            bgTitle.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.004f);

            /// 拖拽事件
            EventTrigger.Entry BeginDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.BeginDrag
            };
            BeginDrag.callback.AddListener(delegate (BaseEventData s)
            {
                dragOffset = tf2.anchoredPosition - (Vector2)Input.mousePosition;
            });
            EventTrigger.Entry Drag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };
            Drag.callback.AddListener(delegate (BaseEventData s)
            {
                tf2.anchoredPosition = (Vector2)Input.mousePosition + dragOffset;
                Vector2 anchoredPosition = tf2.anchoredPosition;
                string str = anchoredPosition.x.ToString();
                string str2 = "|";
                anchoredPosition = bgGo.GetComponent<RectTransform>().anchoredPosition;
                string bgPos = str + str2 + anchoredPosition.y.ToString();
                PlayerPrefs.SetString("yellow.bgPos", bgPos);
            });
            EventTrigger.Entry EndDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.EndDrag
            };
            EndDrag.callback.AddListener(delegate (BaseEventData s)
            {
            });
            EventTrigger eventTrigger = bgTitle.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(BeginDrag);
            eventTrigger.triggers.Add(Drag);
            eventTrigger.triggers.Add(EndDrag);

            InitTitle(charMenu);
            GameObject closeBg = CreateUI.NewImage(CreateUI.closeBg);
            closeBg.transform.SetParent(bgGo.transform, false);
            closeBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(850f, -450f);
            Button btn = closeBg.AddComponent<Button>();
            btn.onClick.AddListener(delegate ()
            {
                gameObject.SetActive(false);
            });
            GameObject closeIcon = CreateUI.NewImage(CreateUI.closeBtn);
            closeIcon.transform.SetParent(closeBg.transform, false);
            TestUI("LayerPopUp/UI_CharacterMenu");
        }

        public void InitTitle(Transform charMenu)
        {
            string[] titleArray = new string[]
            {
                "开局设置",
                "指定特性",
            };
            for (int i = 0; i < titleArray.Length; i++)
            {
                int index = i;
                GameObject titleBg = CreateUI.NewImage(CreateUI.btn2Bg);
                titleBg.transform.SetParent(rootBg.transform, false);
                titleBg.GetComponent<RectTransform>().anchoredPosition = new Vector2((float)(-750 + i * 180), 450f);
                titleBgs.Add(titleBg);
                GameObject titleSelect = CreateUI.NewImage(CreateUI.btn2Select);
                titleSelect.transform.SetParent(titleBg.transform, false);
                titleSelect.GetComponent<Image>().raycastTarget = false;
                titleSelects.Add(titleSelect);
                GameObject titleText = CreateUI.NewTextPro(titleArray[i], default, 0);
                titleText.transform.SetParent(titleBg.transform, false);
                titleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                titleText.GetComponent<TextMeshProUGUI>().raycastTarget = false;
                titleBg.AddComponent<Button>().onClick.AddListener(delegate ()
                {
                    UpdateTitle(index);
                });
            }
            UpdateTitle(0);
        }

        private void OnGUI1()
        {
            try
            {
                GUILayout.Space(100f);
                GUILayout.Label("打开 " + showIndex.ToString() + "/" + all.Length.ToString(), Array.Empty<GUILayoutOption>());
                if (all[showIndex])
                {
                    GUILayout.Label(all[showIndex].name, Array.Empty<GUILayoutOption>());
                }
                GUILayout.Space(20f);
                uiPath = GUILayout.TextField(uiPath, Array.Empty<GUILayoutOption>());
                if (GUILayout.Button("切换", Array.Empty<GUILayoutOption>()))
                {
                    Transform go = root.Find(uiPath);
                    if (go != null)
                    {
                        go.gameObject.SetActive(!go.gameObject.activeSelf);
                    }
                }
                if (GUILayout.Button("测试UI", Array.Empty<GUILayoutOption>()))
                {
                    TestUI(uiPath);
                }
            }
            catch (Exception)
            {
            }
        }

        public void RestUIPosition()
        {
            bool flag = Screen.width > 2000;
            if (flag)
            {
                rootBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-600f, -400f);
            }
            else
            {
                rootBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            }
        }

        private void SetImage()
        {
            bool flag = all[showIndex];
            if (flag)
            {
                bg.sprite = all[showIndex].sprite;
                bg.SetNativeSize();
            }
        }

        private void TestUI(string uiPath)
        {
            Transform go = root.Find(uiPath);
            if (go)
            {
                GameObject bg = CreateUI.NewImage(null);
                bg.transform.SetParent(transform, false);
                all = go.GetComponentsInChildren<Image>();
                bg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
                this.bg = bg.GetComponent<Image>();
                SetImage();
                bg.SetActive(false);
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                showIndex--;
                if (showIndex < 0)
                {
                    showIndex = all.Length - 1;
                }
                SetImage();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                showIndex++;
                if (showIndex >= all.Length)
                {
                    showIndex = 0;
                }
                SetImage();
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    rootBg.GetComponent<RectTransform>().anchoredPosition += new Vector2(-1f, 0f) * Time.deltaTime * speed;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    rootBg.GetComponent<RectTransform>().anchoredPosition += new Vector2(1f, 0f) * Time.deltaTime * speed;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    rootBg.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 1f) * Time.deltaTime * speed;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    rootBg.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, -1f) * Time.deltaTime * speed;
                }
            }
        }

        public void UpdatePage(int index, Component com, Func<GameObject> newAction)
        {
            if (selectTitleIndex == index)
            {
                GameObject go = com == null ? newAction() : com.gameObject;
                go.gameObject.SetActive(true);
            }
            else if (com != null)
            {
                com.gameObject.SetActive(false);
            }
        }

        private void UpdateTitle(int index)
        {
            for (int i = 0; i < titleSelects.Count; i++)
            {
                titleSelects[i].SetActive(index == i);
            }
            selectTitleIndex = index;
            *//*            this.UpdatePage(0, this.ui_GMItems, delegate
                        {
                            GameObject go = new GameObject();
                            go.transform.SetParent(this.bgRoot, false);
                            this.ui_GMItems = go.AddComponent<UI_GMItems>();
                            this.ui_GMItems.Init();
                            return go;
                        });
                        this.UpdatePage(1, this.ui_GMHouse, delegate
                        {
                            GameObject go = new GameObject();
                            go.transform.SetParent(this.bgRoot, false);
                            this.ui_GMHouse = go.AddComponent<UI_GMHouse>();
                            this.ui_GMHouse.Init();
                            return go;
                        });*//*
                    }
                }

                public class UI_ModifierView : MonoBehaviour
                {
                    public List<int> availableFeatureIds;

                    public void InitFeatures()
                    {
                        for (int i = 0; i < 1000; ++i)
                        {

                        }
                    }
                }*//*
        }
    }
}
*/