#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TTT.Helpers
{
    public static class ExtensionMethods
    {
#if UNITY_EDITOR
        /// <summary>
        /// Finds a <see cref="SerializedProperty"/> C# property with a [field: SerializedField] attribute, because Unity's <see cref="SerializedProperty.FindPropertyRelative"/> does not work with C# properties.
        /// </summary>
        public static SerializedProperty FindRealPropertyRelative(
            this SerializedProperty serializedProp,
            string name
        )
        {
            // credit to https://manuel-rauber.com/2023/02/21/use-serialized-properties-over-serialized-fields-in-unity/

            // This is the serialized name of a C# property.
            var realName = $"<{name}>k__BackingField";

            return serializedProp.FindPropertyRelative(realName);
        }
#endif
    }
}
