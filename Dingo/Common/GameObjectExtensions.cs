using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Dingo.Common
{
    public static class TransformExtensions {

        public static T GetComponentInAncestors<T>(this Transform component) where T : Component
        {
            var result = component.GetComponent<T>();

            if (result != null)
            {
                return result;
            }

            if (component.transform.parent == null)
            {
                return null;
            }
            //TODO: Should this be parent?
            return GetComponentInAncestors<T>(component.transform.parent);
        }

        public static void ApplyTRSMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.GetScale();
            transform.rotation = matrix.GetRotation();
            transform.position = matrix.GetPosition();
        }

        public static Matrix4x4 GetGlobalTRS(this Transform transform)
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }

        public static Matrix4x4 GetLocalTRS(this Transform transform)
        {
            return Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        }

        public static Quaternion GetRotation(this Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        public static Vector3 GetPosition(this Matrix4x4 matrix)
        {
            var x = matrix.m03;
            var y = matrix.m13;
            var z = matrix.m23;

            return new Vector3(x, y, z);
        }

        public static Vector3 GetScale(this Matrix4x4 m)
        {
            return new Vector3(m.GetColumn(0).magnitude,
                                m.GetColumn(1).magnitude,
                                m.GetColumn(2).magnitude);
        }
    }

    public static class GameObjectExtensions
    {
        public static void OptimizeAndFlattenHierarchy(Transform root)
        {
            FlattenHierarchyRecursive(root, root);
            DestroyEmptyObjectsInHierarchy(root);
        }

        public static void DestroyEmptyObjectsInHierarchy(Transform root)
        {
            var childCache = new List<Transform>();
            foreach (Transform child in root)
            {
                childCache.Add(child);
            }
            for (int i = 0; i < childCache.Count; i++)
            {
                DestroyEmptyObjectsInHierarchy(childCache[i]);
                if (childCache[i].GetComponents<Component>().Length == 1)
                {
                    Object.DestroyImmediate(childCache[i].gameObject);
                }
            }
        }
        
        private static void FlattenHierarchyRecursive(Transform transform, Transform root)
        {
            var childCache = new List<Transform>();
            foreach (Transform child in transform)
            {
                childCache.Add(child);
            }
            for (int i = 0; i < childCache.Count; i++)
            {
                childCache[i].SetParent(root);
                FlattenHierarchyRecursive(childCache[i], root);
            }
        }
        
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