using UnityEngine;
using DG.Tweening;
using System.Collections;


    public class BackAndForth : CustomAction//, IAction
    {
        public override void DisableAction()
        {
            base.DisableAction();
            motion.Pause();
        }

        public override void PerformAction()
        {
            base.PerformAction();

            if (motion != null)
            {
                motion.Play();
            }
            else
            {
                StartMoving();
            }
        }

        bool startOnAwake = true;
        public float duration = 1f;
        public Vector2 amount = new(0, 3);
        public Ease easing = Ease.InOutSine;

        Tween motion;

        private void Start()
        {
            if (startOnAwake)
            {
                PerformAction();
            }
        }

        public void StartMoving()
        {
            if (!ActionEnabled) return;

            motion = transform.DOMove(transform.position + (Vector3)amount, duration).SetEase(easing).SetLoops(-1, LoopType.Yoyo);
        }


    }


