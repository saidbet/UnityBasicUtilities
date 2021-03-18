using System;
using UnityEngine;

namespace UnityBasicUtilities
{
    public static class GameObjectExtensions
    {
        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            T foundComponent = gameObject.GetComponent<T>();
            if (foundComponent == null)
            {
                return gameObject.AddComponent<T>();
            }

            return foundComponent;
        }

        public static void ForEachChild(this GameObject parent, Action<GameObject> callback)
        {
            foreach (Transform child in parent.transform)
            {
                callback(child.gameObject);
            }
        }

        public static void SetLayerRecursively(this GameObject root, int layer)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root", "Root transform can't be null.");
            }

            foreach (var child in root.transform.EnumerateHierarchy())
            {
                child.gameObject.layer = layer;
            }
        }

        public static GameObject GetParentRoot(this GameObject child)
        {
            if (child.transform.parent == null)
            {
                return child;
            }
            else
            {
                return GetParentRoot(child.transform.parent.gameObject);
            }
        }

        public static void ForEachComponent<T>(this GameObject g, Action<T> action)
        {
            foreach (T i in g.GetComponents<T>())
            {
                action(i);
            }
        }

        public static void ForEachComponentsInChildren<T>(this GameObject g, Action<T> action)
        {
            foreach (T i in g.GetComponentsInChildren<T>())
            {
                action(i);
            }
        }

        public static T GetComponentInBrothers<T>(this GameObject gameObject) where T : class
        {
            if (gameObject.transform.parent == null)
                return null;

            T res = null;
            foreach(Transform child in gameObject.transform.parent)
            {
                res = child.GetComponent<T>();
                if(res != null)
                {
                    return res;
                }
            }
            return null;
        }
    }
}