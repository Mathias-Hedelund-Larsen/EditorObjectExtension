using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace HephaestusForge
{
    /// <summary> 
    /// Extensions for the SerializedProperty class
    /// </summary>
    public static class SerializedProperyExtensions
    {
        /// <summary>
        /// Gets all children of SerializedProperty at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent SerializedProperty.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.Next(false);
            }

            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.Next(false));
            }
        }

        /// <summary>
        /// Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent SerializedProperty.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (currentProperty.propertyPath.Contains(serializedProperty.propertyPath))
                    {
                        yield return currentProperty.Copy();
                    }
                }
                while (currentProperty.NextVisible(false));
            }
        }

        /// <summary>
        /// Search through an SerializedProperty array to find a specific child property defined by the predicate,
        /// you will also get the index of the child property inside the array.
        /// </summary>
        /// <param name="array">The source SerializedProperty which needs to be an array.</param>
        /// <param name="predicate">Definitions of the value to search for in the array.</param>
        /// <param name="index">The index of the found value, -1 if the value couldnt be found.</param>
        /// <returns>The found SerializedProperty, will return the array if the child property couldnt be found.</returns>
        public static SerializedProperty FindInArray(this SerializedProperty array, Predicate<SerializedProperty> predicate, out int index)
        {
            if (array.isArray)
            {
                for (int i = array.arraySize - 1; i >= 0; i--)
                {
                    if (predicate.Invoke(array.GetArrayElementAtIndex(i)))
                    {
                        index = i;
                        return array.GetArrayElementAtIndex(i);
                    }
                }

                Debug.LogWarning("Couldnt find what you were searching for in array");
                index = -1;
                return array;
            }
            else
            {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("The serialized property was not an array");
#endif

                index = -1;
                return array;
            }
        }
    }
}
