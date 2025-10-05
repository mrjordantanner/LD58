using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public enum GameState
{
    Initializing,
    MainMenu,
    Starting,
    Running,
    Paused,
    Ending,
    GameOver,
    Restarting
}

public class GameManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static GameManager Instance;
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
        #endregion

        StartCoroutine(Init());
    }

    #region Declarations

    public string Name { get { return "Game Manager"; } }

    [Header("Game Configuration")]
    public Dialogue tutorialDialogue;
    public bool showIntroDialogue = true;
    public bool cloudLogging;

    [Header("Game State")]
    [ReadOnly] public GameState currentState = GameState.Initializing;
    [ReadOnly] public bool inputSuspended;
    [ReadOnly] public float gameTimer;
    public bool gameTimerEnabled = true;

    [Header("Time Management")]
    [ReadOnly] public float timeScale;

    #endregion

    #region Game State Management

    /// <summary>
    /// Sets the current game state and triggers appropriate events
    /// </summary>
    /// <param name="newState">The new game state to transition to</param>
    private void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"GameManager: State changed from {previousState} to {newState}");
        
        // Trigger state-specific events
        switch (newState)
        {
            case GameState.Running:
                EventManager.Instance.TriggerEvent(EventManager.GAME_STARTED);
                break;
            case GameState.Paused:
                EventManager.Instance.TriggerEvent(EventManager.GAME_PAUSED);
                break;
            case GameState.GameOver:
                EventManager.Instance.TriggerEvent(EventManager.GAME_ENDED);
                break;
        }
    }

    /// <summary>
    /// Gets the current game state
    /// </summary>
    public GameState GetGameState() => currentState;

    /// <summary>
    /// Checks if the game is currently running
    /// </summary>
    public bool IsGameRunning() => currentState == GameState.Running;

    /// <summary>
    /// Checks if the game is currently paused
    /// </summary>
    public bool IsGamePaused() => currentState == GameState.Paused;

    /// <summary>
    /// Checks if input should be suspended based on current game state
    /// </summary>
    public bool ShouldSuspendInput() => inputSuspended || currentState != GameState.Running;

    #endregion

    public IEnumerator Init()
    {
        // Initialize game state
        SetGameState(GameState.Initializing);
        inputSuspended = true;
        Time.timeScale = 0;
        gameTimer = 0;

        Debug.Log("GameManager: Initialized");
        yield return new WaitForSecondsRealtime(0f);
    }


    void Update()
    {
        timeScale = Time.timeScale;
        
        // Update game timer when running
        if (gameTimerEnabled && currentState == GameState.Running)
        {
            gameTimer += Time.deltaTime;
        }
        
        // Update total time elapsed
        if (PlayerData.Instance) 
        {
            PlayerData.Instance.Data.TotalTimeElapsed += Time.unscaledDeltaTime;
        }
    }

    // Button callback
    public void RestartFromPauseMenu()
    {
        PlayerManager.Instance.DespawnPlayer();
        Menu.Instance.BackToGame();
        ReplayGame();
    }

    public void ReplayGame()
    {
        Debug.Log("GameManager: Replaying game");
        
        PlayerData.Instance.Data.Replays++;
        HUD.Instance.objectivesUI.alpha = 0;

        showIntroDialogue = false;
        StartCoroutine(InitializeNewRun(true));
    }

    public IEnumerator InitializeNewRun(bool isReplay = false)
    {
        Debug.Log("GameManager: InitializeNewRun called");
        
        // Handle screen fade if it's player's first playthrough
        if (!isReplay)
        {
            HUD.Instance.screenFader.FadeToWhite(1f);
            yield return new WaitForSecondsRealtime(1f);

            Menu.Instance.FullscreenMenuBackground.SetActive(false);
            Menu.Instance.NameEntryPanel.Hide();
        }

        gameTimer = 0;
        //PlayerManager.Instance.RefillLives();

        PlayerData.Instance.Data.ResetGameSessionData();

        Debug.Log("GameManager: About to start StartRun()");
        StartCoroutine(StartRun());
    }


    public IEnumerator StartRun()
    {
        Debug.Log("GameManager: Starting game run");
        
        SetGameState(GameState.Starting);
        gameTimerEnabled = true;
        gameTimer = 0;

        // Fade in screen
        HUD.Instance.screenFader.FadeIn(1f);
        yield return new WaitForSecondsRealtime(1f);

        // Set up time scale
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;

        // Handle intro dialogue or start directly
        if (showIntroDialogue && tutorialDialogue != null)
        {
            Debug.Log("GameManager: Starting intro dialogue");
            StartCoroutine(DialogueManager.Instance.StartDialogue(tutorialDialogue));
        }
        else
        {
            Debug.Log("GameManager: Skipping intro dialogue, calling OnIntroDialogueComplete directly");
            StartCoroutine(OnIntroDialogueComplete());
        }
    }

    public IEnumerator OnIntroDialogueComplete()
    {
        Debug.Log("GameManager: Intro dialogue complete, starting gameplay");
        
        // Show HUD and enable input
        HUD.Instance.Show();
        inputSuspended = false;
        
        // Set game state to running
        SetGameState(GameState.Running);
        
        Debug.Log("GameManager: Waiting 1 seconds before spawning player...");
        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("GameManager: About to spawn player and initialize progression");
        PlayerManager.Instance.SpawnPlayer();
        Progression.Instance.InitializeLevel(1);
        Debug.Log("GameManager: Player spawning and progression initialization complete");
    }

    public void EndRunCallback()
    {
        StartCoroutine(EndRun());
    }

    public IEnumerator EndRun()
    {
        Debug.Log("GameManager: Ending game run");
        
        SetGameState(GameState.Ending);
        
        // Hide UI elements
        HUD.Instance.Hide();
        HUD.Instance.objectivesUI.alpha = 0;
        PauseMenu.Instance.Hide();
        Menu.Instance.ActiveMenuPanel.Hide();

        // Save data and cleanup
        PlayerData.Instance.SaveAllAsync();
        PlayerManager.Instance.DespawnPlayer();

        // Fade out and return to menu
        HUD.Instance.screenFader.FadeOut(1);
        StartCoroutine(AudioManager.Instance.FadeMusicOut(1));
        yield return new WaitForSecondsRealtime(1);

        // Reset game state
        gameTimerEnabled = false;
        gameTimer = 0;
        inputSuspended = true;
        Time.timeScale = 0;
        SetGameState(GameState.MainMenu);

        StartCoroutine(Menu.Instance.ReturnToTitleScreen());
    }

    public void GameOver()
    {
        Debug.Log("GameManager: Game Over");
        
        SetGameState(GameState.GameOver);
        
        // Hide UI elements
        HUD.Instance.Hide();
        HUD.Instance.objectivesUI.alpha = 0;

        // Save data and update leaderboard
        PlayerData.Instance.SaveAllAsync();
        LeaderboardService.Instance.OnPlaySessionEnd();

        // Display results
        Menu.Instance.ResultsPanel.Show();
        StartCoroutine(Menu.Instance.ResultsPanel.ShowResults());
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: Restarting game");
        SetGameState(GameState.Restarting);
        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        if (currentState != GameState.Running) return;

        SetGameState(GameState.Paused);
        
        AudioManager.Instance.ReduceMusicVolume();
        inputSuspended = true;
        Time.timeScale = 0;
        Physics2D.simulationMode = SimulationMode2D.Script;
        HUD.Instance.screenFlash.SetActive(false);
        
    }

    public void Unpause()
    {
        if (currentState != GameState.Paused) return;

        SetGameState(GameState.Running);
        
        AudioManager.Instance.RestoreMusicVolume();
        inputSuspended = false;
        Time.timeScale = 1;
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        HUD.Instance.screenFlash.SetActive(true);
        
    }


    public void Quit()
    {
        PlayerData.Instance.SaveAllAsync();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
