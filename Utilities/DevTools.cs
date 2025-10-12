using UnityEngine;
using TMPro;
using System.Collections;


public class DevTools : MonoBehaviour
{
    #region Singleton
    public static DevTools Instance;
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

    #region Declarations
    [ReadOnly] public bool gameToolsActive, menuToolsActive;
    public float devToolsOpacity = 0.9f;
    public float statsWindowOpacity = 0.75f;

    [Header("Dev Message")]
    public float uiMessageFadeInDuration = 0.25f;
    public float uiMessageDisplayDuration = 2f,
        uiMessageFadeOutDuration = 0.5f;

    [Header("Dev Features")]
    public bool levelSkippingEnabled = false;

    bool devInputBufferOn;
    [HideInInspector]
    public bool devToolsWereUsed;
    #endregion

    private void Update()
    {
        if (!devInputBufferOn)
        {
            HandleGameplayDevInput();
        }
    }

    void HandleGameplayDevInput()
    {
        if (Menu.Instance.enteringText) return;

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (levelSkippingEnabled)
            {
                StartPreviousLevel();
            }
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            if (levelSkippingEnabled)
            {
                StartNextLevel();
            }
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            ToggleLevelSkipping();
            StartDevInputBuffer();
        }

        // 9: Toggle Freeze-frame
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
           ToggleFreezeframe();
           StartDevInputBuffer();
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            StartDevInputBuffer();
        }
    }

    #region DevTools Methods

    public void ToggleFreezeframe()
    {
        if (GameManager.Instance.IsGamePaused())
        {
            GameManager.Instance.Unpause();
        }
        else
        {
            GameManager.Instance.Pause();
        }

        StartDevInputBuffer();
    }

    public void ToggleLevelSkipping()
    {
        levelSkippingEnabled = !levelSkippingEnabled;
        HUD.Instance.ShowAlertMessage($"Level Skipping {(levelSkippingEnabled ? "Enabled" : "Disabled")}", 0.1f, 0.2f, 0.5f, Color.yellow);
    }
    
    /// <summary>
    /// Start the previous level (decrement level and restart)
    /// </summary>
    public void StartPreviousLevel()
    {
        if (Progression.Instance == null)
        {
            Debug.LogWarning("DevTools: Progression.Instance is null! Cannot start previous level.");
            return;
        }
        
        Debug.Log("DevTools: Starting previous level...");
        
        // End current round and level cleanly
        EndCurrentRoundAndLevel();
        
        // Decrement level (ensure we don't go below 1)
        int newLevel = Mathf.Max(1, Progression.Instance.currentLevel - 1);
        
        // Start the new level
        StartLevel(newLevel);
        
        HUD.Instance.ShowAlertMessage($"Started Level {newLevel}", 0.3f, 1.5f, 0.5f, Color.cyan);
    }
    
    /// <summary>
    /// Start the next level (increment level and restart)
    /// </summary>
    public void StartNextLevel()
    {
        if (Progression.Instance == null)
        {
            Debug.LogWarning("DevTools: Progression.Instance is null! Cannot start next level.");
            return;
        }
        
        Debug.Log("DevTools: Starting next level...");
        
        // End current round and level cleanly
        EndCurrentRoundAndLevel();
        
        // Increment level
        int newLevel = Progression.Instance.currentLevel + 1;
        
        // Start the new level
        StartLevel(newLevel);
        
        HUD.Instance.ShowAlertMessage($"Started Level {newLevel}", 0.3f, 1.5f, 0.5f, Color.cyan);
    }
    
    /// <summary>
    /// Helper method to cleanly end current round and level
    /// </summary>
    private void EndCurrentRoundAndLevel()
    {
        Debug.Log("DevTools: Ending current round and level...");
        
        // End SpawnerController round if active
        if (SpawnerController.Instance != null)
        {
            SpawnerController.Instance.EndRound();
        }
        
        // Clear all collectibles/balls
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible collectible in collectibles)
        {
            if (collectible != null)
            {
                collectible.DestroyMe();
            }
        }
        
        // Despawn player
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // Reset progression state
        if (Progression.Instance != null)
        {
            Progression.Instance.isLevelActive = false;
            Progression.Instance.isRoundActive = false;
        }
        
        Debug.Log("DevTools: Current round and level ended");
    }
    
    /// <summary>
    /// Helper method to start a specific level
    /// </summary>
    private void StartLevel(int levelNumber)
    {
        Debug.Log($"DevTools: Starting level {levelNumber}...");
        
        if (Progression.Instance == null)
        {
            Debug.LogWarning("DevTools: Progression.Instance is null! Cannot start level.");
            return;
        }
        
        // Use Progression's existing functionality to start the level
        Progression.Instance.InitializeLevel(levelNumber);
        
        Debug.Log($"DevTools: Level {levelNumber} started successfully");
    }
    #endregion

    void StartDevInputBuffer()
    {
        StartCoroutine(DevInputBuffer());
    }

    IEnumerator DevInputBuffer()
    {
        devToolsWereUsed = true;

        if (devInputBufferOn) yield return null;
        devInputBufferOn = true;
        yield return new WaitForSecondsRealtime(0.2f);
        devInputBufferOn = false;
    }

}
