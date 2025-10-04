using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;


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

    //public bool startRoundsOnStartup = true;
    public Dialogue tutorialDialogue;


    [Header("Logging")]
    public bool cloudLogging;

    [Header("Game State")]
    [ReadOnly]
    public bool gameRunning;
    [ReadOnly]
    public bool gamePaused;
    [ReadOnly]
    public bool inputSuspended;
    public bool showIntroDialogue = true;

    [Header("Time")]
    [ReadOnly] public float timeScale;
    public float velocityThreshold = 0.1f; // Buffer to determine if the character is moving
    public float timeAccelerationDuration = 0.2f; // Duration for time scale to reach 1.0
    public float timeDecelerationDuration = 0.5f; // Duration for time scale to reach slowTimeAmount
    public float slowTimeAmount = 0.25f; // Time scale value when character is at rest
    [ReadOnly] public float gameTimer;
    public bool gameTimerEnabled = true;




    #endregion


    public IEnumerator Init()
    {
        gameRunning = false;
        inputSuspended = true;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0f);
    }


    void Update()
    {
        timeScale = Time.timeScale;
        if (gameTimerEnabled && gameRunning) gameTimer += Time.deltaTime;
        if (PlayerData.Instance) PlayerData.Instance.Data.TotalTimeElapsed += Time.unscaledDeltaTime;
    }

    // TIME
    public IEnumerator TimeAcceleration()
    {
        float elapsedTime = 0f;
        float startValue = Time.timeScale;
        while (elapsedTime < timeAccelerationDuration)
        {
            Time.timeScale = Mathf.Lerp(startValue, 1.0f, elapsedTime / timeAccelerationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Time.timeScale = 1.0f;
    }

    public IEnumerator TimeDeceleration()
    {
        float elapsedTime = 0f;
        float startValue = Time.timeScale;
        while (elapsedTime < timeDecelerationDuration)
        {
            Time.timeScale = Mathf.Lerp(startValue, slowTimeAmount, elapsedTime / timeDecelerationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Time.timeScale = slowTimeAmount;
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
        PlayerData.Instance.Data.Replays++;
        HUD.Instance.objectivesUI.alpha = 0;

        showIntroDialogue = false;
        StartCoroutine(InitializeNewRun(true));
    }

    public IEnumerator InitializeNewRun(bool isReplay = false)
    {
        // Handle screen fade if it's player's first playthrough
        if (!isReplay)
        {
            HUD.Instance.screenFader.FadeToWhite(1f);
            yield return new WaitForSecondsRealtime(1f);

            Menu.Instance.FullscreenMenuBackground.SetActive(false);
            Menu.Instance.NameEntryPanel.Hide();
        }

        gameTimer = 0;
        PlayerManager.Instance.RefillLives();

        PlayerData.Instance.Data.ResetGameSessionData();

        StartCoroutine(StartRun());
    }


    public IEnumerator StartRun()
    {
        //print("IF YOU'RE READING THIS, THANKS FOR PLAYING MY GAME");
        //print("GameManager: Start Run");

        gameRunning = true;
        gameTimerEnabled = true;

        //PlayerManager.Instance.SpawnPlayer(PlayerManager.Instance.playerSpawnPoint.transform.position);

        HUD.Instance.screenFader.FadeIn(1f);
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;

        if (showIntroDialogue)
        {
            StartCoroutine(DialogueManager.Instance.StartDialogue(tutorialDialogue));
        }
        else
        {
            StartCoroutine(OnIntroDialogueComplete());
        }
    }

    public IEnumerator OnIntroDialogueComplete()
    {
        HUD.Instance.Show();
        inputSuspended = false;
        yield return new WaitForSecondsRealtime(2f);

        //LevelController.Instance.LoadLevel(1);
        yield return new WaitForSecondsRealtime(1f);

        //LevelController.Instance.CurrentLevel.StartLevel();
    }

    public void EndRunCallback()
    {
        StartCoroutine(EndRun());
    }

    public IEnumerator EndRun()
    {
        //print("GameManager: End Run");

        HUD.Instance.Hide();
        //HUD.Instance.ShowPointerCursor();
        HUD.Instance.objectivesUI.alpha = 0;

        PlayerData.Instance.SaveAllAsync();
        PlayerManager.Instance.DespawnPlayer();

        PauseMenu.Instance.Hide();
        //DevTools.Instance.gameplayDevToolsWindow.Hide();
        Menu.Instance.ActiveMenuPanel.Hide();

        HUD.Instance.screenFader.FadeOut(1);
        StartCoroutine(AudioManager.Instance.FadeMusicOut(1));
        yield return new WaitForSecondsRealtime(1);

        gameRunning = false;
        gameTimerEnabled = false;
        gameTimer = 0;
        inputSuspended = true;
        Time.timeScale = 0;

        StartCoroutine(Menu.Instance.ReturnToTitleScreen());
    }

    public void GameOver()
    {
        HUD.Instance.Hide();
        //HUD.Instance.ShowPointerCursor();
        HUD.Instance.objectivesUI.alpha = 0;

        // Stop game running and score calculation, then calculate one final time
        gameRunning = false;

        // Update cloud, push player score up, get all scores back down
        PlayerData.Instance.SaveAllAsync();
        LeaderboardService.Instance.OnPlaySessionEnd();

        // Display results
        Menu.Instance.ResultsPanel.Show();
        StartCoroutine(Menu.Instance.ResultsPanel.ShowResults());
    }

    public void RestartGame()
    {
        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        AudioManager.Instance.ReduceMusicVolume();

        inputSuspended = true;
        gamePaused = true;
        Time.timeScale = 0;
        Physics2D.simulationMode = SimulationMode2D.Script;
        HUD.Instance.screenFlash.SetActive(false);
    }

    public void Unpause()
    {
        AudioManager.Instance.RestoreMusicVolume();

        inputSuspended = false;
        gamePaused = false;
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
