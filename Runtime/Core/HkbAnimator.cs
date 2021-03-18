using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Asyncoroutine;
using System.Threading.Tasks;

namespace UnityBasicUtilities{
    
    public struct ClipMapping
    {
        public AnimationClipPlayable playable;
        public AnimationClip clip;
    }

    public class HkbAnimator : MonoBehaviour
    {
        public Animator animator;
        private AnimationMixerPlayable mixerPlayable;

        private PlayableGraph graph;

        public List<ClipMapping> clipPlayables = new List<ClipMapping>();

        public event Action OnAnimationEnd;

        private bool animationDone;

        //Since the animator gets deleted, we keep a reference of the transform to add it back
        private GameObject animatorGo;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (animator == null){
                animator = animatorGo != null ? animatorGo.EnsureComponent<Animator>() : gameObject.EnsureComponent<Animator>();
            }
        }

        public void SetState(AnimationClip clip)
        {
            SetStates(new List<AnimationClip>() { clip });
        }

        public void SetStates(List<AnimationClip> clips)
        {
            SetAnimations(clips);
            //Go to last frame
            SetProgress(0.99f);
        }

        public void SetAnimations(List<AnimationClip> clips)
        {
            if(animator == null)
                Init();

            clipPlayables = new List<ClipMapping>();

            graph = PlayableGraph.Create();

            var playableOutput = AnimationPlayableOutput.Create(graph, "Animation", animator);

            mixerPlayable = AnimationMixerPlayable.Create(graph, clips.Count);

            playableOutput.SetSourcePlayable(mixerPlayable);

            int i = 0;
            foreach (var clip in clips)
            {
                clipPlayables.Add(new ClipMapping()
                {
                    clip = clip,
                    playable = AnimationClipPlayable.Create(graph, clip)
                });
                graph.Connect(clipPlayables[i].playable, 0, mixerPlayable, i);
                mixerPlayable.SetInputWeight(i, 1);
                clipPlayables[i].playable.Play();
                i++;
            }

            mixerPlayable.SetSpeed(0);
            mixerPlayable.Play();
            graph.Play();
        }

        public void PlayAnimation(AnimationClip clip, float speed = 1, bool continuousAnimation = false)
        {
            PlayAnimations(new List<AnimationClip>() { clip }, speed, continuousAnimation);
        }

        public async Task PlayAnimationAsync(AnimationClip clip, float speed = 1)
        {
            PlayAnimations(new List<AnimationClip>() { clip }, speed, false);
            float time = 0;

            //We wait for animation done or we cancel after waiting for enough time (depending on animation length)
            while(!animationDone && time < clip.length*1.5f){
                time += Time.deltaTime;
                await new WaitForEndOfFrame();
            }
        }

        public void PlayAnimations(List<AnimationClip> clips, float speed = 1, bool continuousAnimation = false)
        {
            if(!continuousAnimation)
                animationDone = false;
            else
                animationDone = true;
                
            SetAnimations(clips);
            mixerPlayable.SetSpeed(speed);
            mixerPlayable.Play();
            graph.Play();
        }

        public void PauseAnimations()
        {
            mixerPlayable.Pause();
        }

        public void SetProgress(float progress)
        {
            foreach (var clip in clipPlayables)
            {
                var time = clip.clip.length * progress;
                clip.playable.SetTime(time);
            }
        }

        public float GetProgress(){
            if(clipPlayables[0].clip == null){
                Debug.LogWarning("Trying to get progress but clip is not set. This should not happen");
                return 0;
            }
            return (float)clipPlayables[0].playable.GetTime() / clipPlayables[0].clip.length;
        }

        public void DisableAnimator()
        {
            animator.enabled = false;
        }

        public void EnableAnimator()
        {
            animator.enabled = true;
        }

        public void CleanupAnimator(){
            if(animator != null){
                animatorGo = animator.gameObject;
                Destroy(animator);
            }
        }

        private void Update(){
            if(!animationDone && clipPlayables.Count > 0 && graph.IsPlaying() && GetProgress() >= 1){
                animationDone = true;
                SetProgress(0.99f);
                OnAnimationEnd?.Invoke();
                mixerPlayable.Pause();
                graph.Stop();
                CleanupAnimator();
            }
        }
    }
}
