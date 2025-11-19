using System.Collections;
using UnityEngine;

namespace TTT.UI
{
    public interface IOpenable
    {
        abstract ShiftType ShiftDirection { get; set; }

        abstract Vector2 ShiftPadding { get; set; }
        abstract RectTransform ToHide { get; set; }

        abstract Vector2 OpenPosition { get; set; }

        /// <summary>
        /// Assumption is that the starting position is the 'closed' position
        /// </summary>
        ///
        abstract Vector2 ClosedPosition { get; set; }

        abstract bool IsOpen { get; set; }

        abstract AnimationCurve MovementCurve { get; set; }
        abstract float MovementSeconds { get; set; }
        public virtual IEnumerator ToggleOpenable()
        {
            if (IsOpen)
            {
                yield return CloseRoutine();
            }
            else
            {
                yield return OpenRoutine();
            }
        }

        public virtual IEnumerator OpenRoutine()
        {
            float currentTime = 0f;
            Vector2 start = ToHide.anchoredPosition;
            IsOpen = !IsOpen;
            while (currentTime < MovementSeconds)
            {
                float normalizedTime = currentTime / MovementSeconds;

                ToHide.anchoredPosition = Vector2.Lerp(
                    start,
                    OpenPosition,
                    MovementCurve.Evaluate(normalizedTime)
                ); // Interpolate position based on curve
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }
        }

        public virtual IEnumerator CloseRoutine()
        {
            float currentTime = 0f;
            Vector2 start = ToHide.anchoredPosition;
            IsOpen = !IsOpen;
            while (currentTime < MovementSeconds)
            {
                float normalizedTime = currentTime / MovementSeconds;

                ToHide.anchoredPosition = Vector2.Lerp(
                    start,
                    ClosedPosition,
                    MovementCurve.Evaluate(normalizedTime)
                );
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }
        }

        public virtual void SetupPositions()
        {
            ClosedPosition = ToHide.anchoredPosition;
            var openPos = ClosedPosition + ShiftPadding;

            switch (ShiftDirection)
            {
                case ShiftType.Up:
                    openPos.y += ToHide.rect.height;
                    break;
                case ShiftType.Down:
                    openPos.y -= ToHide.rect.height;
                    break;
                case ShiftType.Left:
                    openPos.x += ToHide.rect.width;
                    break;
                case ShiftType.Right:
                    openPos.x += ToHide.rect.height;
                    break;
            }
            OpenPosition = openPos;
        }
    }
}
