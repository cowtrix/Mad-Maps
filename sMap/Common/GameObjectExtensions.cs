using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace sMap.Common
{
    public static class GameObjectExtensions
    {
        public static List<T> FindObjectsOfTypeAll<T>(this Object obj)
        {
            List<T> results = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    var allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        var go = allGameObjects[j];
                        results.AddRange(go.GetComponentsInChildren<T>(true));
                    }
                }
            }
            return results;
        }

        public static void SetStatic(this GameObject gameObject)
        {
#if UNITY_EDITOR
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(gameObject,
                    UnityEditor.StaticEditorFlags.BatchingStatic | 
                    UnityEditor.StaticEditorFlags.LightmapStatic | 
                    UnityEditor.StaticEditorFlags.OccludeeStatic | 
                    UnityEditor.StaticEditorFlags.OccluderStatic | 
                    UnityEditor.StaticEditorFlags.OffMeshLinkGeneration | 
                    UnityEditor.StaticEditorFlags.NavigationStatic | 
                    UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
#endif
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T ret = gameObject.GetComponent<T>();
            if (ret == null)
            {
                ret = gameObject.AddComponent<T>();
            }
            return ret;
        }

        public static Component GetOrAddComponent(this GameObject gameObject, Type t)
        {
            Component ret = gameObject.GetComponent(t);
            if (ret == null)
            {
                ret = gameObject.AddComponent(t);
            }
            return ret;
        }

        public static void TryDestroyComponent<T>(this GameObject go) where T : Object
        {
            T comp = go.GetComponent<T>();
            if (comp != null)
            {
                Object.Destroy((Object)(object)comp);
            }
        }
        
        public static T GetComponentByInterface<T>(this GameObject gameObject) where T : class
        {
            if (gameObject == null)
            {
                return null;
            }
            return gameObject.GetComponent(typeof(T)) as T;
        }

        public static T[] GetComponentsByInterface<T>(this GameObject gameObject) where T : class
        {
            var components = gameObject.GetComponents(typeof(T));

            var ret = new T[components.Length];

            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = components[i] as T;
            }

            return ret;
        }

        public static T[] GetComponentsByInterfaceInChildren<T>(this GameObject gameObject) where T : class
        {
            var components = gameObject.GetComponentsInChildren(typeof(T));

            var ret = new T[components.Length];

            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = components[i] as T;
            }

            return ret;
        }

        public static T GetComponentByInterfaceInChildren<T>(this GameObject gameObject) where T : class
        {
            return gameObject.GetComponentInChildren(typeof(T)) as T;
        }

        public static T GetComponentByInterface<T>(this MonoBehaviour component) where T : class
        {
            return component.GetComponent(typeof(T)) as T;
        }

        public static Component[] GetComponentsByInterface<T>(this MonoBehaviour component) where T : class
        {
            return component.GetComponents(typeof(T));
        }

        public static T GetComponentByInterfaceInAncestors<T>(this Transform component) where T : class
        {
            var result = component.GetComponent(typeof(T)) as T;

            if (result != null)
            {
                return result;
            }

            if (component.transform.parent == null)
            {
                return null;
            }

            return GetComponentByInterfaceInAncestors<T>(component.transform.parent);
        }
    }
}