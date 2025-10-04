using UnityEngine;



    public class Marker : MonoBehaviour
    {
        public float gizmoSize = 2f;
        public Color gizmoColor = Color.blue;

        void OnDrawGizmos()
        {

            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position - Vector3.right * gizmoSize, transform.position + Vector3.right * gizmoSize);
            Gizmos.DrawLine(transform.position - Vector3.up * gizmoSize, transform.position + Vector3.up * gizmoSize);
        }



    }


