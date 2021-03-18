using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Asyncoroutine;

namespace UnityBasicUtilities
{
    public class Utilities : MonoBehaviour
    {
        public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            Vector3 vVector1 = vPoint - vA;
            Vector3 vVector2 = (vB - vA).normalized;

            float d = Vector3.Distance(vA, vB);
            float t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return vA;

            if (t >= d)
                return vB;

            Vector3 vVector3 = vVector2 * t;

            return vA + vVector3;
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static bool IsBitSet(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static Bounds GetBoundsRaw(GameObject go)
        {
            Quaternion currentRotation = go.transform.rotation;
            go.transform.rotation = Quaternion.identity;

            Bounds myBounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                if (myBounds.size == Vector3.zero && myBounds.center == Vector3.zero)
                    myBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                else
                    myBounds.Encapsulate(renderer.bounds);
            }

            Vector3 localCenter = myBounds.center - go.transform.position;
            myBounds.center = localCenter;

            go.transform.rotation = currentRotation;
            return myBounds;
        }

        public static Bounds GetBounds(GameObject go)
        {
            Bounds myBounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                if (myBounds.size == Vector3.zero && myBounds.center == Vector3.zero)
                    myBounds = new Bounds(renderer.bounds.center, renderer.bounds.size);
                else
                    myBounds.Encapsulate(renderer.bounds);
            }

            return myBounds;
        }

        public static Vector3 WebToUnity(Vector3 web, Camera cam)
        {
            return new Vector3(web.x, cam.pixelHeight - web.y, 0);
        }

        public static Vector3 WebPosToWorld(Vector3 webPos)
        {
            Plane mousePlane = new Plane(Vector3.up, 0f);
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(webPos.x, Camera.main.pixelHeight - webPos.y, -Camera.main.farClipPlane));
            float rayDistance;
            mousePlane.Raycast(ray, out rayDistance);
            Vector3 position = ray.GetPoint(rayDistance);//Camera.main.ScreenToWorldPoint(new Vector3(payload.x, Camera.main.pixelHeight - payload.y, rayDistance));
            position.y = 0.015f;
            return position;
        }

        public static Vector3 GetRotatedAroundPosition(Vector3 toRotate, Vector3 centerPoint, Vector3 axis, float angle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Vector3 vector2 = toRotate - centerPoint;
            vector2 = rotation * vector2;
            return centerPoint + vector2;
        }

        public static bool nearlyEqual(float a, float b, float epsilon)
        {
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < float.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.MaxValue);
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public static float AngleInRad(Vector3 vec)
        {
            return Mathf.Atan2(vec.z, vec.x);
        }

        //This returns the angle in degress
        public static float AngleInDeg(Vector3 vec)
        {
            return -AngleInRad(vec) * Mathf.Rad2Deg;
        }

        public static bool IsAngleFlat(float angle, float epsilon)
        {
            var absAngle = Math.Abs(angle);
            return (absAngle > 180 - epsilon && absAngle < 180 + epsilon) || (absAngle > 0 - epsilon && absAngle < 0 + epsilon);
        }

        public static string ComputeHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            var hashedBytes = new MD5CryptoServiceProvider().ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes);
        }

        public static void SetPositionAround(GameObject toPosition, GameObject target, GameObject relativeTo, float height = 0)
        {
            Vector3 direction = (relativeTo.transform.position - target.transform.position).normalized;

            direction = Quaternion.AngleAxis(target.transform.rotation.y, Vector3.up) * direction;

            direction = target.transform.InverseTransformDirection(direction);

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
            {
                direction.z = 0;
                if (direction.x > 0)
                    direction.x = 1f;
                else
                    direction.x = -1f;
            }
            else
            {
                direction.x = 0;
                if (direction.z > 0)
                    direction.z = 1f;
                else
                    direction.z = -1f;
            }
            direction.y = 0;

            direction = target.transform.TransformDirection(direction);

            //Debug.DrawRay(target.transform.position, direction * 10, Color.red);

            var colExtents = target.GetComponentInChildren<Collider>().bounds;
            toPosition.transform.position = target.transform.position + (new Vector3(direction.x * 1.1f * colExtents.extents.x, height, direction.z * 1.1f * colExtents.extents.z));
            var rotation = Quaternion.LookRotation(-direction).eulerAngles;
            toPosition.transform.rotation = Quaternion.Euler(new Vector3(0, rotation.y, 0));
        }

        public static void LookAt(Transform toRotate, Transform toLookAt)
        {
            var newRot = Quaternion.LookRotation(toLookAt.forward).eulerAngles;
            newRot.x = 0;
            newRot.z = 0;
            toRotate.rotation = Quaternion.Euler(newRot);
        }

        public static void PositionInFront(Transform toPosition, Transform camera, Transform floorPos, float height, float distance = 1)
        {
            var camPos = camera.position;
            var camFw = camera.forward;
            camFw.y = 0;
            camFw.Normalize();
            toPosition.position = new Vector3(camPos.x + camFw.x, floorPos.position.y + height, camPos.z + (camFw.z * distance));
        }

        public static (bool hasHit, RaycastHit hit) Raycast(Ray ray, LayerMask layers)
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5000, layers))
            {
                return (true, hit);
            }
            else
                return (false, hit);
        }
    }
}
