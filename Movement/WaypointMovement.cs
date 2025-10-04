using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

    public class WaypointMovement : CustomAction
    {
        public override void DisableAction()
        {
            base.DisableAction();

            if (motion != null)
            {
                motion.Pause();
            }

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
                StartCoroutine(StartWaypointMovement());
            }
        }

        public enum Type { Reverse, Loop }

        public bool canMove = true;
        public bool startOnAwake = true;
        public float startDelay = 0;
        public float durationBetweenWaypoints = 3f;
        public float pauseAtWaypointDuration = 1f;

        public GameObject WaypointContainer;
        public Type type;
        public Ease easing = Ease.InOutSine;

        Marker[] waypoints;
        Transform targetWaypoint;
        Tween motion;

        private void Start()
        {
            waypoints = WaypointContainer.GetComponentsInChildren<Marker>();

            ActionEnabled = true;
            if (startOnAwake)
            {
                StartCoroutine(StartWaypointMovement());
            }
        }

        List<Marker> BuildWaypointList()
        {
            List<Marker> waypointList = new();

            foreach (var waypoint in waypoints)
            {
                waypointList.Add(waypoint);
            }

            return waypointList;
        }

        public IEnumerator StartWaypointMovement()
        {
            yield return new WaitForSeconds(startDelay);

            var waypointList = BuildWaypointList();

            while (canMove)
            {
                foreach (var waypoint in waypointList)
                {
                    if (type == Type.Reverse)
                    {
                        if (waypoint == waypointList[0]) continue;
                    }

                    targetWaypoint = waypoint.transform;
                    motion = transform.DOMove(targetWaypoint.position, durationBetweenWaypoints).SetEase(easing);
                    yield return new WaitForSeconds(durationBetweenWaypoints + pauseAtWaypointDuration);

                }

                if (type == Type.Reverse)
                {
                    waypointList.Reverse();
                }

            }

        }

    }
