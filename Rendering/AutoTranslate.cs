using UnityEngine;

public class AutoTranslate : MonoBehaviour
{
    public float moveSpeed = 5;
    public Vector2 direction = Vector3.right;

    public void Update()
    {
        transform.Translate(moveSpeed * direction * Time.deltaTime);
    }
}
