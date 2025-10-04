using UnityEngine;

public class Collector : MonoBehaviour
{
    public bool isExpanded = false;

    private void Update()
    {
        if (GameManager.Instance.inputSuspended || GameManager.Instance.gamePaused) return;

        if (Input.GetKeyDown(InputManager.Instance.shootButton) || 
            Input.GetKeyDown(InputManager.Instance.shootKey))
        {
            transform.localScale = Vector3.one * 1.5f;
            isExpanded = true;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExpanded)
        {
            if (collision.CompareTag("Collectible"))
            {
                collision.GetComponent<Collectible>().Collect();
            }
        }
    }


}
