using System.Collections;
using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// Interface that defines required variables
    /// and functionality for a moveable UI Element.
    /// </summary>
    public interface IOpenable
    {
        /// <summary>
        /// The direction you want to move the element.
        /// </summary>
        abstract ShiftType ShiftDirection { get; set; }

        /// <summary>
        /// An extra amount of distance to move. The default is the width
        /// or height of the ToOpen object, so this allows for larger movements.
        /// </summary>
        abstract Vector2 ShiftPadding { get; set; }

        /// <summary>
        /// The object we want to move.
        /// </summary>
        abstract RectTransform ToOpen { get; set; }

        /// <summary>
        /// The end position you want.
        /// <para>Defaults to the StartPosition +/- the height/width of the
        /// ToOpen, depending on the ShiftDirection.</para>
        /// </summary>
        abstract Vector2 EndPosition { get; set; }

        /// <summary>
        /// Where the movement "starts" and "returns" to. Defaults to wherever
        /// the object is on the screen at creation.
        /// </summary>
        abstract Vector2 StartPosition { get; set; }

        abstract bool IsOpen { get; set; }

        /// <summary>
        /// A curve for making the movement lerp.
        /// </summary>
        abstract AnimationCurve MovementCurve { get; set; }

        /// <summary>
        /// How long you want the movement to take.
        /// </summary>
        abstract float MovementSeconds { get; set; }

        /// <summary>
        /// Runs either the Open or Close routine based on the IsOpen variable.
        /// </summary>
        /// <returns>An IEnumerator of the shift function. </returns>
        public virtual IEnumerator ToggleOpenable()
        {
            float currentTime = 0f;
            Vector2 start = ToOpen.anchoredPosition;
            Vector2 end = IsOpen ? StartPosition : EndPosition;
            IsOpen = !IsOpen;

            while (currentTime < MovementSeconds)
            {
                float normalizedTime = currentTime / MovementSeconds;

                ToOpen.anchoredPosition = Vector2.Lerp(
                    start,
                    end,
                    MovementCurve.Evaluate(normalizedTime)
                );
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// Establishes the Start and End position based on the given data.
        /// </summary>
        public virtual void SetupPositions()
        {
            StartPosition = ToOpen.anchoredPosition;
            var openPos = StartPosition + ShiftPadding;

            switch (ShiftDirection)
            {
                case ShiftType.Up:
                    openPos.y += ToOpen.rect.height;
                    break;
                case ShiftType.Down:
                    openPos.y -= ToOpen.rect.height;
                    break;
                case ShiftType.Left:
                    openPos.x -= ToOpen.rect.width;
                    break;
                case ShiftType.Right:
                    openPos.x += ToOpen.rect.width;
                    break;
            }
            EndPosition = openPos;
        }
    }
}
