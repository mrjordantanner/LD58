using UnityEngine;
using Unity.Services.Core;

public class NetworkCheck : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("?  UGS network OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError("?  " + e);
        }
    }
}
