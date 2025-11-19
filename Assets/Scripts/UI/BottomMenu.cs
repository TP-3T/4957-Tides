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
    [RequireComponent(typeof(RectTransform))]
    public class BottomMenu : MonoBehaviour, IOpenable
    {
        /* #region IOpenable requirements */
        [field: SerializeField]
        public RectTransform ToHide { get; set; }

        [field: SerializeField]
        public bool IsOpen { get; set; }
        public Vector2 OpenPosition { get; set; }
        public Vector2 ClosedPosition { get; set; }

        [field: SerializeField]
        public AnimationCurve MovementCurve { get; set; }

        [field: SerializeField]
        public float MovementSeconds { get; set; }

        [field: SerializeField]
        public ShiftType ShiftDirection { get; set; }

        [field: SerializeField]
        public Vector2 ShiftPadding { get; set; }

        /* #endregion*/

        [SerializeField]
        private GameEvent startInspectMode;

        [SerializeField]
        private GameObject SlotArea;

        [SerializeField]
        private GameObject ShopTabArea;

        [SerializeField]
        private GameObject ShopTab;

        private Coroutine Current { get; set; }

        /// <summary>
        /// The prefab for a shop slot.
        /// </summary>
        [Tooltip("The prefab for a shop slot.")]
        [SerializeField]
        private GameObject ShopSlotPrefab;

        [SerializeField]
        private float SlotSpacing = 20;

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

        public void Awake()
        {
            (this as IOpenable).SetupPositions();
            CreateShopTabs();
        }

        public IEnumerator Start()
        {
            var buildingRoutine = AssetLoader<FeatureType>.LoadGroup(
                "building",
                AddToDictionaries
            );
            yield return buildingRoutine;
        }

        public void TabClicked(GameObject tab)
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
                Buildings[feature.Category][feature.UniqueID] = feature;
                var tab = tabSlots.Keys.First(key =>
                    key.name.Equals(feature.Category.ToString())
                );
                // Create the slot and insert into dictionary
                List<GameObject> slotList = tabSlots[tab];
                float slotWidth = ShopSlotPrefab
                    .transform.GetComponent<RectTransform>()
                    .rect.width;
                float xOffset = SlotSpacing + slotList.Count * (slotWidth * 2);
                Vector3 parentPosition = SlotArea.transform.position;

                GameObject slotObject = Instantiate(
                    ShopSlotPrefab,
                    new Vector3(xOffset, parentPosition.y, parentPosition.z),
                    Quaternion.identity,
                    SlotArea.transform
                );
                BuildingShopSlot slot =
                    slotObject.GetComponent<BuildingShopSlot>();
                slot.feature = feature;

                tabSlots[tab].Add(slotObject);
            }
        }

        private void CreateShopTabs()
        {
            var types = Enum.GetValues(typeof(FeatureCategory))
                .Cast<FeatureCategory>();
            foreach (var type in types)
            {
                Buildings[type] = new();
                var newTab = Instantiate(ShopTab);
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

        public void Toggle()
        {
            if (Current != null)
            {
                StopCoroutine(Current);
            }
            Current = StartCoroutine((this as IOpenable).ToggleOpenable());
        }
    }
}
