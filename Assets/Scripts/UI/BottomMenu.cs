using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Managers;
using UnityEditor.Graphs;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace TTT.UI
{
    /// <summary>
    /// Class for specifically handling the bottom menu for the in-game UI.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BottomMenu : MonoBehaviour, IOpenable
    {
        /* #region IOpenable requirements */
        [Header("IOpenable")]
        [field: SerializeField]
        public RectTransform ToOpen { get; set; }

        [field: SerializeField]
        public bool IsOpen { get; set; }
        public Vector2 EndPosition { get; set; }
        public Vector2 StartPosition { get; set; }

        [field: SerializeField]
        public AnimationCurve MovementCurve { get; set; }

        [field: SerializeField]
        public float MovementSeconds { get; set; }

        [field: SerializeField]
        public ShiftType ShiftDirection { get; set; }

        [field: SerializeField]
        public Vector2 ShiftPadding { get; set; }

        /* #endregion*/

        /// <summary>
        /// Event to be raised when we change to Inspect mode.
        /// </summary>
        [SerializeField]
        private GameEvent startInspectMode;

        [Header("Shop Slots")]
        /// <summary>
        /// GameObject that will hold the Shop Slots as its children.
        /// </summary>
        [SerializeField]
        private GameObject ShopSlotArea;

        /// <summary>
        /// The prefab for a shop slot.
        /// </summary>
        [Tooltip("The prefab for a shop slot.")]
        [SerializeField]
        private GameObject ShopSlotPrefab;

        /// <summary>
        /// Spacing between Shop Slots
        /// </summary>
        [SerializeField]
        private float SlotSpacing;

        [Header("Shop Tabs")]
        /// <summary>
        /// Game Object that will hold the Shop Tabs as its children.
        /// </summary>
        [SerializeField]
        private GameObject ShopTabArea;

        /// <summary>
        /// Prefab for a Shop Tab
        /// </summary>
        [SerializeField]
        private GameObject ShopTabPrefab;

        /// <summary>
        /// Holds the currently running Shift coroutine.
        /// </summary>
        private Coroutine CurrentShift { get; set; }

        private Dictionary<GameObject, List<GameObject>> tabSlots = new();

        private GameObject currentTab;

        /// <summary>
        /// Stores all the SOs we'll need for each category in our shop menu
        /// </summary>
        [SerializeField]
        private Dictionary<
            FeatureCategory,
            Dictionary<string, ScriptableObject>
        > Buildings = new();

        /// <summary>
        /// Setup the start/end positions for the IOpenable and create the tabs.
        /// </summary>
        public void Awake()
        {
            (this as IOpenable).SetupPositions();
            CreateShopTabs();
        }

        /// <summary>
        /// Asynchronously load all the building prefabs and add setup the dictionaries.
        /// </summary>
        public IEnumerator Start()
        {
            var buildingRoutine = AssetLoader<FeatureType>.LoadGroup(
                "building",
                AddToDictionaries
            );
            yield return buildingRoutine;
        }

        /// <summary>
        /// Cancels the current shift if one is running,
        /// then runs the movement function as per the IOpenable
        /// </summary>
        public void Toggle()
        {
            if (CurrentShift != null)
            {
                StopCoroutine(CurrentShift);
            }
            CurrentShift = StartCoroutine((this as IOpenable).ToggleOpenable());
        }

        /// <summary>
        /// Helper function to be run when a Shop tab is clicked.
        /// Checks if its the current tab, running the IOpenable
        /// functions if required.
        /// </summary>
        /// <param name="tab">A reference to the tab that was clicked.</param>
        private void TabClicked(GameObject tab)
        {
            if (!IsOpen || currentTab.Equals(tab))
            {
                Toggle();
                currentTab = tab;
            }
            if (IsOpen)
            {
                foreach (var slots in tabSlots.Values)
                {
                    foreach (
                        var slot in slots.Where(slot =>
                            slot.activeSelf.Equals(true)
                        )
                    )
                    {
                        slot.SetActive(false);
                    }
                }
                FeatureCategory type = (FeatureCategory)
                    Enum.Parse(typeof(FeatureCategory), tab.name, true);
                tabSlots[tab]
                    .ForEach(slot =>
                    {
                        slot.SetActive(true);
                    });
                currentTab = tab;
            }
        }

        private void AddToDictionaries(FeatureType feature)
        {
            if (feature != null)
            {
                //Add to Buildings dictionary
                Buildings[feature.Category][feature.UniqueID] = feature;

                AddToTabSlots(feature);
            }
        }

        private void AddToTabSlots(FeatureType feature)
        {
            var tab = tabSlots.Keys.First(key =>
                key.name.Equals(feature.Category.ToString())
            );
            // Create the slot and insert into dictionary
            List<GameObject> slotList = tabSlots[tab];
            float slotWidth = ShopSlotPrefab
                .transform.GetComponent<RectTransform>()
                .rect.width;
            float xOffset = SlotSpacing + slotList.Count * (slotWidth * 2);
            Vector3 parentPosition = ShopSlotArea.transform.position;

            GameObject slotObject = Instantiate(
                ShopSlotPrefab,
                new Vector3(xOffset, parentPosition.y, parentPosition.z),
                Quaternion.identity,
                ShopSlotArea.transform
            );
            BuildingShopSlot slot = slotObject.GetComponent<BuildingShopSlot>();
            slot.feature = feature;

            tabSlots[tab].Add(slotObject);
        }

        private void CreateShopTabs()
        {
            var types = Enum.GetValues(typeof(FeatureCategory))
                .Cast<FeatureCategory>();
            foreach (var type in types)
            {
                Buildings[type] = new();
                var newTab = Instantiate(ShopTabPrefab);
                var shopTab = newTab.GetComponent<ShopTab>();
                newTab.name = type.ToString();
                shopTab.TextArea.text = type.ToString();
                shopTab.Button.onClick.AddListener(() =>
                {
                    TabClicked(newTab);
                });
                newTab.transform.SetParent(ShopTabArea.transform);
                tabSlots[newTab] = new();
            }
        }
    }
}
