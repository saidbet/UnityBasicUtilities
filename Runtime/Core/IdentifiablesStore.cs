using System.Collections.Generic;
using UnityEngine;

namespace UnityBasicUtilities
{
    public class IdentifiablesStore : MonoBehaviour
    {
        public static Dictionary<string, GameObject> identifiableGameObjects = new Dictionary<string, GameObject>();

        public static void ResetStore()
        {
            identifiableGameObjects = new Dictionary<string, GameObject>();
        }

        public static void AddElementAndGameObject(string id, GameObject go)
        {
            AddIdentifiableGo(id, go);
        }

        public static GameObject GetIdentifiableGo(string id)
        {
            GameObject go = null;
            string[] fullId = id.Split('|');
            
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("GetIdentifiableGo: " + id + ": id not found.");
            }
            else if (identifiableGameObjects.ContainsKey(fullId[0]))
            {
                go = identifiableGameObjects[fullId[0]];
                if (fullId.Length > 1)
                {
                    var tr = RecursiveFind(go.transform, fullId[1]);
                    go = tr == null ? null : tr.gameObject;
                    if (go == null)
                        Debug.LogWarning("GetIdentifiableGo: " + id + ": path not found.");
                }
            }
            return go;
        }

        public static Transform RecursiveFind(Transform transform, string path)
        {
            Transform res = transform.Find(path);
            int count = transform.childCount;

            while (res == null && count > 0)
                res = RecursiveFind(transform.GetChild(--count), path);

            return res;
        }

        public static void AddIdentifiableGo(string id, GameObject go)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (identifiableGameObjects.ContainsKey(id))
            {
                if (identifiableGameObjects[id] == go)
                    return;
                else
                    identifiableGameObjects[id] = go;
            }
            else
                identifiableGameObjects.Add(id, go);
        }

        public static void RemoveFromStore(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (identifiableGameObjects.ContainsKey(id))
                identifiableGameObjects.Remove(id);
        }

        private void OnDestroy()
        {
            ResetStore();
        }
    }

}

