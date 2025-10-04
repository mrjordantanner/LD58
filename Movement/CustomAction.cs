using UnityEngine;
using DG.Tweening;
using System.Collections;

    public class CustomAction : MonoBehaviour
    {
        public bool ActionEnabled { get; set; }
        public bool hasCompleted { get; set; }

        public virtual void DisableAction()
        {
            ActionEnabled = false;
        }

        public virtual void PerformAction()
        {
            ActionEnabled = true;
        }



    }


