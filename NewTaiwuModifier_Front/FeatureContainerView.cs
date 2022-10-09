/*using Config;
using FrameWork;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UICommon.Character.Elements;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NewTaiwuModifier_Front
{

    internal class FeatureContainerView : UIBase
    {
        private List<short> _featureIds = new List<short>();
        private Refers _featureLine;

        public Transform root;
        public void UpdateFeatureList(List<short> featureIds)
        {
            _featureIds = featureIds;
            _featureIds.Sort();
            Refresh();
        }
        public void Refresh()
        {
            RectTransform featureContainer = _featureLine.CGet<RectTransform>("FeatureContainer");
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
            }
            for (int i = _featureIds.Count; i < components.Length; ++i)
            {
                components[i].gameObject.SetActive(false);
            }
        }

        public void Init(GameObject goRoot)
        {
            root = goRoot.transform;
            Refers refers = CGet<Refers>("InscriptionView");   // 借用一下铭刻页面的资源
            _featureLine = Instantiate(refers.CGet<Refers>("FeatureLine"), transform, false);   // 偷一份FeatureLine出来,放这里面
            FeatureManager.Init();
            UpdateFeatureList(FeatureManager.instance._basicFeatures);
        }

        public override void OnInit(ArgumentBox argsBox)
        {
        }
    }
}
*/