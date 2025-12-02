using UnityEngine;

namespace TTT.Helpers
{
    public static partial class Extensions
    {
        /// <summary>
        /// Selects what destroy method to use, depending on if we're in editor or not.
        /// <para>
        /// Because we have live-editor functionality, its important to use this when you want
        /// to delete objects, but can't be sure if the functions will be called at runtime
        /// or in edit mode.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Warning: Do not call this function from any MonoBehaviors "Awake" method.
        ///         This can have unforeseen side effects as you can't destroy objects
        ///         during Awake.
        /// </remarks>
        /// <param name="otherObject"></param>
        public static void SmartDestroy(GameObject otherObject)
        {
            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(otherObject);
            }
            else if (Application.isPlaying)
            {
                GameObject.Destroy(otherObject);
            }
            else
            {
                throw new UnityException(
                    "Smart Destroy does not contain logic for the current play mode."
                );
            }
        }
    }
}
