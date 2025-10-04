using UnityEngine;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    #region Singleton
    public static CameraController Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    public CinemachineVirtualCamera cam;
    public CinemachineFramingTransposer transposer;
    public CinemachineConfiner2D confiner;

    [ReadOnly] public Vector3 cameraPosition;


    private void Start()
    {
        if (!cam) cam = FindObjectOfType<CinemachineVirtualCamera>();
        if (!transposer) transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (!confiner) confiner = cam.GetComponent<CinemachineConfiner2D>();

        cam.Follow = PlayerManager.Instance.playerSpawnPoint;
    }

    private void LateUpdate()
    {
        if (cam) cameraPosition = cam.transform.position;
    }

    public void SetCameraFollow(Transform transformToFollow)
    {
        var followTarget = transformToFollow != null ? transformToFollow : null;
        cam.Follow = followTarget;
    }

}
