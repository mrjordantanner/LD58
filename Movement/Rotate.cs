using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class Rotate : CustomAction//, IAction
    {
        public GameObject TargetObject;

        public bool startOnAwake = true;
        public bool
            xAxis,
            yAxis,
            zAxis = true;

        public bool accelerate;
        public float accelerationRate = 25f;
        public float rotationSpeed = 200f;

        void Start()
        {
            if (TargetObject == null)
            {
                Debug.LogError($"TargetObject not found for Rotation script on {gameObject.name}");
                return;
            }

            if (startOnAwake)
            {
                PerformAction();
            }

        }

        void Update()
        {
            if (!ActionEnabled || TargetObject == null) return;

            if (accelerate)
            {
                rotationSpeed += accelerationRate;
            }

            if (zAxis) TargetObject.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            if (yAxis) TargetObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            if (xAxis) TargetObject.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);


        }
    }

