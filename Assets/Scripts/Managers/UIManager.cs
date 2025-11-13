using System.Collections;
using System.Collections.Generic;
using TTT.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace TTT.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject drawerPanel;

        [SerializeField]
        Vector2 openPosition,
            closedPosition;

        [SerializeField]
        AnimationCurve animationCurve;

        public string currentFeatureType;

        private RectTransform m_RT;
        private float endTime;
        public bool isOpen = false;

        private int currentTabID = -1;
        public GameObject[] Tabs;
        public Image[] TabButtons;
        public Color activeTabColor, inactiveTabColor;
        public Vector2 InactiveTabSize, activeTabSize;

        void Start()
        {
            FindFurthestKeyFrame();
            m_RT = GetComponent<RectTransform>();
            closedPosition = m_RT.anchoredPosition;
        }

        public void TabClicked(int TabID)
        {
            if (currentTabID == TabID && isOpen)
            {
                ToggleDrawer();
                currentTabID = -1;

            }
            else if (isOpen == false)
            {
                ToggleDrawer();
                // Emit toggle event
                currentTabID = TabID;
                Debug.Log("Tab Clicked: " + TabID);
            }
            else
            {
                SwitchToTab(TabID);
                currentTabID = TabID;
                Debug.Log("Tab Clicked: " + TabID);
            }
        }


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

        // event emitter goes here.
        public void SwitchToTab(int TabID)
        {

            Tabs[TabID].SetActive(true);

            Debug.Log("Switched to Tab: " + TabID);
        }

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
            HideUnopenedButtons();
        }

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

        private void HideUnopenedButtons()
        {
            // Placeholder for future implementation
            foreach (Transform child in transform)
            {
                Button button = child.GetComponent<Button>();
                if (button != null && button.gameObject.activeSelf != isOpen)
                {
                    button.interactable = false;
                }
            }
        }
    }
}