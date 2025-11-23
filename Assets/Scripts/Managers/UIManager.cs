using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;

namespace TTT.Managers
{
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// Manager class that handles UI elements and state switching.
    /// <author> Rodrigo, Richard </author>
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject drawerPanel;

        [SerializeField]
        Vector2 openPosition,
            closedPosition;

        [SerializeField]
        private AnimationCurve animationCurve;

        public string currentFeatureType;

        private RectTransform m_RT;
        private float endTime;
        public bool isOpen = false;

        private int currentTabID = -1;
        private GameObject[] Tabs;

        private Color activeTabColor;
        private Color inactiveTabColor;

        /// <summary>
        /// The prefab for a shop slot.
        /// </summary>
        [Tooltip("The prefab for a shop slot.")]
        public GameObject ShopSlotPrefab;
        public GameObject ShopSlotParent;
        public const float SlotSpacing = 20;

        [SerializeField]
        private List<FeatureType> featureTypesHouses;

        [SerializeField]
        private List<FeatureType> featureTypesIndustry;

        [SerializeField]
        private List<FeatureType> featureTypesEnergy;

        [SerializeField]
        private List<FeatureType> featureTypesRenewable;

        [SerializeField]
        private List<FeatureType> featureTypesNature;

        [SerializeField]
        private GameEvent startInspectMode;

        private readonly List<List<GameObject>> shopTabContents = new();

        void Start()
        {
            FindFurthestKeyFrame();
            m_RT = GetComponent<RectTransform>();
            closedPosition = m_RT.anchoredPosition;

            // dynamically populate shop
            InitializeShopTabs();
        }

        /// <summary>
        /// Initializes all of the building shop's tabs with the given lists of tile features to display.
        /// </summary>
        private void InitializeShopTabs()
        {
            List<List<FeatureType>> shopCategories = new()
            {
                featureTypesHouses,
                featureTypesIndustry,
                featureTypesEnergy,
                featureTypesRenewable,
                featureTypesNature,
            };

            for (int tabIndex = 0; tabIndex < shopCategories.Count; tabIndex++)
            {
                var tab = InitializeShopTab(shopCategories[tabIndex]);
                shopTabContents.Add(tab);
            }
        }

        /// <summary>
        /// Initializes one tab in the building shop with the given list of tile features.
        /// </summary>
        private List<GameObject> InitializeShopTab(
            List<FeatureType> featureList
        )
        {
            List<GameObject> tabShopSlots = new();

            for (int index = 0; index < featureList.Count; index++)
            {
                float slotWidth = ShopSlotPrefab
                    .transform.GetComponent<RectTransform>()
                    .rect.width;

                float xOffset = SlotSpacing + index * (slotWidth * 2);

                Vector3 parentPosition = ShopSlotParent.transform.position;
                Vector3 slotPosition = new(
                    parentPosition.x + xOffset,
                    parentPosition.y,
                    parentPosition.z
                );

                GameObject slotGameObject = Instantiate(
                    ShopSlotPrefab,
                    slotPosition,
                    Quaternion.identity,
                    ShopSlotParent.transform
                );
                slotGameObject.SetActive(false);

                tabShopSlots.Add(slotGameObject);

                BuildingShopSlot slot =
                    slotGameObject.GetComponent<BuildingShopSlot>();

                slot.feature = featureList[index];
                slot.UpdateText();
            }

            return tabShopSlots;
        }

        /// <summary>
        /// Handles logic for when a tab is clicked.
        /// </summary>
        /// <param name="tabID"></param>
        public void TabClicked(int tabID)
        {
            if (currentTabID == tabID && isOpen)
            {
                ToggleDrawer();

                shopTabContents[tabID].ForEach(obj => obj.SetActive(false));

                startInspectMode.Raise();

                currentTabID = -1;
            }
            else if (isOpen == false)
            {
                ToggleDrawer();
                // Emit toggle event

                shopTabContents[tabID].ForEach(obj => obj.SetActive(true));

                currentTabID = tabID;

                Debug.Log("Tab Clicked: " + tabID);
            }
            else
            {
                shopTabContents[currentTabID]
                    .ForEach(obj => obj.SetActive(false));

                SwitchToTab(tabID);
                shopTabContents[tabID].ForEach(obj => obj.SetActive(true));

                currentTabID = tabID;

                Debug.Log("Tab Clicked: " + tabID);
            }
        }

        /// <summary>
        /// Toggles the drawer open or closed.
        /// </summary>
        public void ToggleDrawer()
        {
            if (isOpen)
            {
                StartCoroutine(CloseRoutine());
            }
            else
            {
                StartCoroutine(OpenRoutine());
            }
        }

        /// <summary>
        /// Handles switching to a different tab.
        /// </summary>
        /// <param name="tabID"></param>
        /// event emitter goes here.
        public void SwitchToTab(int tabID)
        {
            Tabs[tabID].SetActive(true);

            Debug.Log("Switched to Tab: " + tabID);
        }

        /// <summary>
        /// Coroutine to open the drawer with animation.
        /// </summary>
        /// <returns></returns>
        private IEnumerator OpenRoutine()
        {
            float elapsedTime = 0;
            while (elapsedTime < endTime)
            {
                m_RT.anchoredPosition = Vector2.Lerp(
                    closedPosition,
                    openPosition,
                    animationCurve.Evaluate(elapsedTime)
                ); // Interpolate position based on curve
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }
            isOpen = true;
        }

        /// <summary>
        /// Coroutine to close the drawer with animation.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CloseRoutine()
        {
            float elapsedTime = 0;
            while (elapsedTime < endTime)
            {
                m_RT.anchoredPosition = Vector2.Lerp(
                    openPosition,
                    closedPosition,
                    animationCurve.Evaluate(elapsedTime)
                ); // Interpolate position based on curve
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }
            isOpen = false;
        }

        /// <summary>
        /// Finds the furthest keyframe in the animation curve to determine the duration of the animation.
        /// </summary>
        private void FindFurthestKeyFrame()
        {
            float maxTime = Mathf.NegativeInfinity;
            foreach (Keyframe frame in animationCurve.keys)
            {
                if (frame.time > maxTime)
                {
                    maxTime = frame.time;
                }
            }
            endTime = maxTime;
        }
    }
}
