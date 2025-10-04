using UnityEngine;
using DG.Tweening;
using System.Collections;

    public class ExpandRetract : CustomAction
    {
        public bool useTimer = true;
        public Vector3 targetPosition = new();
        public float moveDuration = 1f;
        public float waitDuration = 4f;

        public Ease easing = Ease.OutElastic;

        Vector3 startPosition;
        float waitTimer;

        bool isExpanded;

        public override void PerformAction()
        {
            base.PerformAction();
            if (isExpanded)
            {
                StartCoroutine(Retract());
            }
            else
            {
                StartCoroutine(Expand());
            }
        }

        private void Start()
        {
            waitTimer = waitDuration;
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            if (useTimer)
            {
                HandleWaitTimer();
            }

        }

        void HandleWaitTimer()
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                PerformAction();
                waitTimer = waitDuration;
            }
        }

        public IEnumerator Expand()
        {
            transform.DOLocalMove(targetPosition, moveDuration).SetEase(easing);
            yield return new WaitForSeconds(moveDuration);
            isExpanded = true;
        }

        public IEnumerator Retract()
        {
            transform.DOLocalMove(startPosition, moveDuration).SetEase(easing);
            yield return new WaitForSeconds(moveDuration);
            isExpanded = false;
        }

    }


