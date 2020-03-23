using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace HephaestusForge
{
    /// <summary>
    /// Extensions for the UnityEngine.Object only available in the Editor
    /// </summary>
    public static class UnityEditorObjectExtensions
    {
        //Using reflection to get a method for finding objects by an instance id.
        private static MethodInfo _getObjectByInstanceID = typeof(Object).GetMethod("FindObjectFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static);

        //Using reflection to be able to use values form the inspectorMode, where local file ID is accesible
        private static PropertyInfo _inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Extension method to get a sceneGuid and objec id from a UnityEngine.Object, the sceneGuid will be None if it is an asset or prefab.
        /// The objectID will be the InstanceID if the Object is an asset or prefab, and will be the local file id if it is inside a scene.
        /// </summary>
        /// <param name="source">The source object for the extension.</param>
        /// <param name="sceneGuid">The guid of the scene to get.</param>
        /// <param name="objectID">The id of the object to get.</param>
        public static void GetSceneGuidAndObjectID(this Object source, out string sceneGuid, out int objectID)
        {
            if (AssetDatabase.Contains(source))
            {
                sceneGuid = "None";
                objectID = source.GetInstanceID();
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath);
                var components = prefab.GetComponents(source.GetType()).ToList();
                components.AddRange(prefab.GetComponentsInChildren(source.GetType()));

                sceneGuid = "None";
                int localID = source.GetLocalID();
                objectID = localID;

                for (int i = 0; i < components.Count; i++)
                {
                    if (localID == components[i].GetLocalID())
                    {
                        objectID = components[i].GetInstanceID();
                    }
                }
            }
            else
            {
                var scene = (source as Component).gameObject.scene;
                sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);

                objectID = source.GetLocalID();

                if (objectID == 0)
                {
                    if (EditorSceneManager.SaveScene(scene))
                    {
                        objectID = source.GetLocalID();
                    }
                }
            }
        }

        /// <summary>
        /// Getting the local file id of and object.
        /// </summary>
        /// <param name="source">The source object to get the local file id.</param>
        /// <returns>The local file id.</returns>
        public static int GetLocalID(this Object source)
        {
            SerializedObject serializedObject = new SerializedObject(source);
            _inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");

            return localIdProp.intValue;
        }

        /// <summary>
        /// Get the MonoScript of the object.
        /// </summary>
        /// <param name="source">The source object to get the MonoScript from.</param>
        /// <returns>A MonoScript which contains the class for the sourceobject.</returns>
        public static MonoScript GetScript(this Object source)
        {
            if (source is MonoBehaviour)
            {
                return MonoScript.FromMonoBehaviour((MonoBehaviour)source);
            }
            else if (source is ScriptableObject)
            {
                return MonoScript.FromScriptableObject((ScriptableObject)source);
            }

            return null;
        }

        /// <summary>
        /// Find an object by an instanceID.
        /// </summary>
        /// <param name="instanceID">The instance id of the object you want to find.</param>
        /// <returns>The object found from the instanceID.</returns>
        public static Object GetObjectByInstanceID(int instanceID, string sceneGuid)
        {
            if (AssetDatabase.Contains(instanceID))
            {
                return AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(instanceID));
            }
            else if(sceneGuid != "None")
            {
                for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++)
                {
                    var rootObjs = EditorSceneManager.GetSceneAt(sceneIndex).GetRootGameObjects();

                    for (int i = 0; i < rootObjs.Length; i++)
                    {
                        List<Component> components = new List<Component>(rootObjs[i].GetComponents<Component>());
                        components.AddRange(rootObjs[i].GetComponentsInChildren<Component>());

                        for (int t = 0; t < components.Count; t++)
                        {
                            if(components[t].GetLocalID() == instanceID)
                            {
                                return components[t];
                            }
                        }
                    }
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(sceneGuid));

                return sceneAsset;
            }

            return null;
        }

        public static bool IsAsset(this Object source)
        {
            return AssetDatabase.Contains(source);
        }
    }
}
