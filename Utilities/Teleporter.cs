using UnityEngine;
using DG.Tweening;
using System.Collections;


public class Teleporter : MonoBehaviour
{
	public Transform destination;
	public float teleportDuration = 2f;
	public Ease teleportEasing = Ease.InOutQuad;
	public bool keepXPosition = true;

    void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			StartCoroutine(Teleport(other.gameObject));
		}
	}

	IEnumerator Teleport(GameObject obj)
	{
		// Keep object's Z position, optionally set new X position, and always set new Y position
		var posX = keepXPosition ? obj.transform.position.x : destination.position.x;
		var posY = destination.position.y;
		var posZ = obj.transform.position.z;

		obj.transform.DOMove(new Vector3(posX, posY, posZ), teleportDuration).SetEase(teleportEasing);

		PlayerManager.Instance.invulnerable = true;
        yield return new WaitForSeconds(teleportDuration);

        PlayerManager.Instance.State = PlayerState.Idle;
        PlayerManager.Instance.invulnerable = false;
    }

}
