using UnityEngine;
using System.Linq;
using System.Collections;


public class InputManager : MonoBehaviour
{
    #region Singleton
    public static InputManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("Keyboard")]
    public KeyCode shootKey;
    public KeyCode leftKey, rightKey, upKey, downKey;
    //public KeyCode mobilityKey;

    [Header("Gamepad")]
    public KeyCode shootButton;
    //public KeyCode mobilityButton;


}
