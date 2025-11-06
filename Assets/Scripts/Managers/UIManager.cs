using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject drawerPanel;
        
        [SerializeField]
        Vector2 openPosition, closedPosition;
        
        [SerializeField] 
        AnimationCurve animationCurve;

        private RectTransform m_RT;
        private float endTime;
        private bool isOpen = false;

        void Start()
        {
            FindFurthestKeyFrame();
            m_RT = GetComponent<RectTransform>();
            closedPosition = m_RT.anchoredPosition;

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

        private IEnumerator OpenRoutine()
        {
            float elapsedTime = 0f;
            while (elapsedTime < endTime)
            {
                float curveValue = animationCurve.Evaluate(elapsedTime);
                m_RT.anchoredPosition = Vector2.Lerp(closedPosition, openPosition, curveValue); // Interpolate position based on curve
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;

            }
            isOpen = true;
        }

        private IEnumerator CloseRoutine()
        {
            float elapsedTime = 0f;
            while (elapsedTime < endTime)
            {
                float curveValue = animationCurve.Evaluate(elapsedTime);
                m_RT.anchoredPosition = Vector2.Lerp(openPosition, closedPosition, curveValue); // Interpolate position based on curve
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
    }
}
