using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityBasicUtilities
{
    public static class TransformExtensions
    {

        public static Transform[] GetChildren(this Transform parent)
        {
            Transform[] res = new Transform[parent.transform.childCount];
            int index = 0;
            foreach (Transform child in parent)
            {
                res[index] = child;
            }
            return res;
        }

        public static string GetFullPath(this Transform transform, string delimiter = ".", string prefix = "/")
        {
            StringBuilder stringBuilder = new StringBuilder();
            GetFullPath(stringBuilder, transform, delimiter, prefix);
            return stringBuilder.ToString();
        }

        private static void GetFullPath(StringBuilder stringBuilder, Transform transform, string delimiter, string prefix)
        {
            if (transform.parent == null)
            {
                stringBuilder.Append(prefix);
            }
            else
            {
                GetFullPath(stringBuilder, transform.parent, delimiter, prefix);
                stringBuilder.Append(delimiter);
            }
            stringBuilder.Append(transform.name);
        }

        public static string GetPathToWithPrefix(this Transform transform, Transform targetParent, string delimiter = "/", string prefix = "|", bool includeParent = true){
            var stringBuilder = new StringBuilder();
            GetPathToWithPrefix(stringBuilder, transform, targetParent, delimiter, prefix, includeParent);
            var res = stringBuilder.ToString();
            if(res == "|")
                res = "";
            return res;
        }

        public static string GetPathTo(this Transform transform, Transform targetParent)
        {
            var stringBuilder = new StringBuilder();
            GetPathTo(stringBuilder, transform, targetParent);
            return stringBuilder.ToString();
        }

        public static void GetPathToWithPrefix(StringBuilder stringBuilder, Transform transform, Transform targetParent, string delimiter, string prefix, bool includeParent){
            if(transform == null)
                return;
                
            if (transform.parent == null || transform == targetParent)
            {
                if(!includeParent){
                    return;
                }
                stringBuilder.Append(prefix);
            }
            else
            {
                GetPathToWithPrefix(stringBuilder, transform.parent, targetParent, delimiter, prefix, includeParent);
                if(!includeParent && (transform.parent.parent == null || transform.parent == targetParent))
                    stringBuilder.Append(prefix);
                else
                    stringBuilder.Append(delimiter);
            }

            stringBuilder.Append(transform.name);
        }

        private static void GetPathTo(StringBuilder stringBuilder, Transform transform, Transform targetParent)
        {
            if (transform.parent == null || transform == targetParent)
            {
                return;
            }
            else
            {
                GetPathTo(stringBuilder, transform.parent, targetParent);
                stringBuilder.Append("/");
            }

            stringBuilder.Append(transform.name);
        }

        public static IEnumerable<Transform> EnumerateHierarchy(this Transform root)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            return root.EnumerateHierarchyCore(new List<Transform>(0));
        }

        public static IEnumerable<Transform> EnumerateHierarchy(this Transform root, ICollection<Transform> ignore)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            if (ignore == null)
            {
                throw new ArgumentNullException("ignore", "Ignore collection can't be null, use EnumerateHierarchy(root) instead.");
            }
            return root.EnumerateHierarchyCore(ignore);
        }

        private static IEnumerable<Transform> EnumerateHierarchyCore(this Transform root, ICollection<Transform> ignore)
        {
            var transformQueue = new Queue<Transform>();
            transformQueue.Enqueue(root);

            while (transformQueue.Count > 0)
            {
                var parentTransform = transformQueue.Dequeue();

                if (!parentTransform || ignore.Contains(parentTransform)) { continue; }

                for (var i = 0; i < parentTransform.childCount; i++)
                {
                    transformQueue.Enqueue(parentTransform.GetChild(i));
                }

                yield return parentTransform;
            }
        }

        public static bool IsParentOrChildOf(this Transform transform1, Transform transform2)
        {
            return transform1.IsChildOf(transform2) || transform2.IsChildOf(transform1);
        }

        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }

        public static Vector3 GetBoundsPos(this Transform trans, Vector3 direction)
        {
            var bounds = Utilities.GetBoundsRaw(trans.gameObject);
            return trans.position + bounds.center + trans.TransformDirection(Vector3.Scale(direction, bounds.extents));
        }
    }
}