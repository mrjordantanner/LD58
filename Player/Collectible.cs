using UnityEngine;

public class Collectible : MonoBehaviour
{
    public void Collect()
    {
        EventManager.Instance.Emit("Collect", this);
    }




}
