using System;
using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Managers;
using UnityEngine;

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
        private GameObject ShopArea;

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

        /// <summary>
        /// Stores all the SOs we'll need for each category in our shop menu
        /// </summary>
        [SerializeField]
        private Dictionary<
            FeatureCategory,
            Dictionary<string, ScriptableObject>
        > Buildings = new();

        void Awake()
        {
            (this as IOpenable).SetupPositions();
            CreateShopTabs();
            AssetLoader<IList<ScriptableObject>>.LoadGroup(
                "BuildingSO",
                AddToBuildingDictionary
            );
        }

        private void AddToBuildingDictionary(IList<ScriptableObject> handle)
        {
            foreach (ScriptableObject asset in handle)
            {
                FeatureType type = asset as FeatureType;
                if (type != null)
                {
                    Buildings[type.Category][type.UniqueID] = asset;
                }
            }
            CreateShopTabs();
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
                shopTab.TextArea.text = type.ToString();
                shopTab.Button.onClick.AddListener(() =>
                {
                    if (Current != null)
                    {
                        StopCoroutine(Current);
                        IsOpen = !IsOpen;
                    }
                    Current = StartCoroutine(
                        (this as IOpenable).ToggleOpenable()
                    );
                    Debug.Log("Clicked on " + type.ToString());
                });
                newTab.transform.SetParent(ShopTabArea.transform);
            }
        }

        private void CreateShopSlots() { }
    }
}
