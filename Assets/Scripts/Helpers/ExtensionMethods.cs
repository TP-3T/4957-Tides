using System;
using System.Collections.Generic;
using System.Linq;
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

        public static List<List<T>> ChunkBy<T>(
            this List<T> source,
            int chunkSize
        )
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static T NextEnumValue<T>(this T src)
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(
                    String.Format(
                        "Argument {0} is not an Enum",
                        typeof(T).FullName
                    )
                );

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}
