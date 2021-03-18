using System.Collections.Generic;
using UnityEngine;

namespace UnityBasicUtilities
{
    [ExecuteInEditMode]
    public class EditorAnimationSetter : MonoBehaviour
    {
        public List<AnimationClip> clips;
        public bool update;

        [Range(0, 1)]
        public float progress;

        public void Update()
        {
            if (update)
            {
                update = false;
                var anim = gameObject.EnsureComponent<HkbAnimator>();
                anim.SetStates(clips);
            }

            gameObject.GetComponent<HkbAnimator>().SetProgress(progress);
        }
    }
}
