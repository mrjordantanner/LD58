using UnityEngine;

public class MouseAim : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance.inputSuspended || GameManager.Instance.IsGamePaused()) return;

        var mousePosition = Utils.GetMouseWorldPosition();
        var direction = Utils.GetDirection(mousePosition, transform.position).normalized;
        var rotTarget = Quaternion.LookRotation(Vector3.forward, direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget, 1);
    }

}
