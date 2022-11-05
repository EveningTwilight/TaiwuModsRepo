using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// 专门偷Taiwu素材用(bushi
namespace NewTaiwuModifier_Front
{
    internal class TaiwuThief
    {
        // 一般是用偷素材或脚本，缓存也只要有一个实例就行了
        private static Dictionary<UIElement, GameObject> cachedPrefabObjects = new Dictionary<UIElement, GameObject>();
        public static GameObject GetPrefabObject(UIElement element, bool enableCache = true)
        {
            GameObject obj = null;
            // 加载prefab的路径为 (static)rootPrefabPath+(instance)element._path
            string rootPrefabPath = Traverse.Create(element).Field("rootPrefabPath").GetValue<string>();
            string elementPath = Traverse.Create(element).Field("_path").GetValue<string>();
            // readFromCache
            if (enableCache)
            {
                cachedPrefabObjects.TryGetValue(element, out obj);
            }
            // stealFromTaiwu(没缓存可用||禁止缓存 => 偷一个新的)
            if (obj == null)
            {
                ResLoader.Load<GameObject>(Path.Combine(rootPrefabPath, elementPath), gameObject =>
                {
                    obj = gameObject;
                }, null);
                if (obj != null && enableCache)
                {
                    cachedPrefabObjects.Add(element, obj);
                }
            }
            return obj;
        }

        // 批量拿，就直接用缓存了
        public static List<GameObject> GetPrefabObjects(List<UIElement> uIElements)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (UIElement element in uIElements)
            {
                list.Add(GetPrefabObject(element, true));
            }
            return list;
        }

        public static GameObject GlobalRootObject()
        {
            return GameObject.Find("Camera_UIRoot/Canvas");
        }
        /*const List<string> defaultExceptLayer = IList<string>("");*/
        public static Transform GlobalTopLayer()
        {
            GameObject gameObject = GlobalRootObject();
            Transform transform = gameObject.transform.Find("LayerVeryTop");
            if (transform == null)
            {
                transform = gameObject.transform.Find("LayerTips");
            }
            if (transform == null)
            {
                transform = gameObject.transform.Find("LayerPopUp");
            }
            if (transform == null)
            {
                transform = gameObject.transform.Find("LayerPart");
            }
            if (transform == null)
            {
                transform = gameObject.transform.Find("LayerMain");
            }
            if (transform == null)
            {
                transform = gameObject.transform.Find("LayerBack");
            }
            return transform;
        }
    }
}
