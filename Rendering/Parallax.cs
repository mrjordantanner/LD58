using UnityEngine;
using Cinemachine;

public class Parallax : MonoBehaviour
{
    [HideInInspector]
    public bool isActive = false;
    public float smoothing = 1f;
    public GameObject[] ActiveParallaxElements;

    float[] parallaxScales;
    Vector3[] startingPositions;
    Vector3 previousCamPos;

    public CinemachineVirtualCamera cam;
    //Camera cam;

    private void Start()
    {
        //cam = FindObjectOfType<CinemachineVirtualCamera>();
        InitializeParallax();
    }

    private void Update()
    {
        if (isActive && GameManager.Instance.gameRunning && !GameManager.Instance.gamePaused)
        {
            HandleParallaxMotion();
        }
    }

    void InitializeParallax()
    {
        //cam = Camera.main;
        previousCamPos = cam.transform.position;

        if (ActiveParallaxElements != null && ActiveParallaxElements.Length > 0)
        {
            //foreach (var element in ActiveParallaxElements)
            //{
            //    element.GetComponent<SpriteRenderer>().enabled = true;
            //    //element.GetComponent<Pulse>().enabled = true;
            //}

            parallaxScales = new float[ActiveParallaxElements.Length];
            startingPositions = new Vector3[ActiveParallaxElements.Length];

            for (int i = 0; i < ActiveParallaxElements.Length; i++)
            {
                startingPositions[i] = ActiveParallaxElements[i].transform.position;
                parallaxScales[i] = ActiveParallaxElements[i].transform.position.z * -1;
            }
        }

        isActive = true;

    }

    void HandleParallaxMotion()
    {
        for (int i = 0; i < ActiveParallaxElements.Length; i++)
        {
            if (ActiveParallaxElements[i] == null) continue;

            float parallaxX = (previousCamPos.x - cam.transform.position.x) * parallaxScales[i];
            float parallaxY = (previousCamPos.y - cam.transform.position.y) * parallaxScales[i];

            float backgroundTargetPosX = ActiveParallaxElements[i].transform.position.x + parallaxX;
            float backgroundTargetPosY = ActiveParallaxElements[i].transform.position.y + parallaxY;

            Vector3 backgroundTargetPos = new(backgroundTargetPosX, backgroundTargetPosY, ActiveParallaxElements[i].transform.position.z);

            ActiveParallaxElements[i].transform.position = Vector3.Lerp(ActiveParallaxElements[i].transform.position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        previousCamPos = cam.transform.position;
    }

    public void ResetParallax()
    {
        isActive = false;

        for (int i = 0; i < ActiveParallaxElements.Length; i++)
        {
            ActiveParallaxElements[i].transform.position = startingPositions[i];
            ActiveParallaxElements[i].GetComponent<SpriteRenderer>().enabled = false;
            //ActiveParallaxElements[i].GetComponent<Pulse>().enabled = false;
        }

    }

}

