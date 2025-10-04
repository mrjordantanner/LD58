using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public enum LaunchSetting { Normal, QuickPlay }

/// <summary>
/// Loads all dependencies on startup before allowing program flow to continue.  Displays loading info on Loading screen.
/// </summary>
public class Loader : MonoBehaviour
{
    #region Singleton
    public static Loader Instance;
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

    public LaunchSetting launchSetting;

    [ReadOnly] public bool isLoading;
    public bool offlineMode;

    List<object> Instances = new();
    List<IInitializable> Dependencies = new();

    public LoadingScreen loadingScreen;
    public GameObject UIObject;

    //public GameObject SceneObject;  // All 'gameplay' objects

    IEnumerator Init()
    {
        UIObject.SetActive(true);
        //SceneObject.SetActive(false);

        isLoading = true;

        loadingScreen.UpdateProgress(0, "");
        loadingScreen.Show(0, false);

        yield return StartCoroutine(LoadDependencies());
        print("Loader: Dependencies loaded & initialized.");

        yield return new WaitForSecondsRealtime(1);
        yield return StartCoroutine(StartUp());

        //loadingScreen.Hide();
        loadingScreen.gameObject.SetActive(false);
        isLoading = false;
    }

    IEnumerator LoadDependencies()
    {
        Instances = new()
        {
            Authentication.Instance,
            Config.Instance,
            GameManager.Instance,
            Menu.Instance,
            AudioManager.Instance,
            PlayerManager.Instance,
            HUD.Instance,
            LeaderboardService.Instance,
        };

        //print($"Loader: Waiting for {Instances.Count} Instances");

        int instancesLoaded = 0;
        foreach (var instance in Instances)
        {
            yield return StartCoroutine(Utils.WaitFor(instance != null, 3));
            instancesLoaded++;

            float loadingProgress = Utils.CalculateSliderValue(instancesLoaded, Instances.Count);
            loadingScreen.UpdateProgress(loadingProgress, $"Waiting for {Instances.Count - instancesLoaded} Components.");
            //print($"Instances loaded: {instancesLoaded}, Progress; {loadingProgress}");
        }

        if (offlineMode)
        {
            Dependencies = new()
            {
                //Authentication.Instance,
                Config.Instance,
                GameManager.Instance,
                Menu.Instance,
                AudioManager.Instance,
                PlayerManager.Instance,
                HUD.Instance,
            };
        }
        else
        {
            Dependencies = new()
            {
                Authentication.Instance,
                GameManager.Instance,
                Menu.Instance,
                AudioManager.Instance,
                PlayerManager.Instance,
                HUD.Instance,
                LeaderboardService.Instance,
            };
        }

        // print($"Loader: Initializing {Dependencies.Count} Dependencies");

        int dependenciesInitialized = 0;
        foreach (var dependency in Dependencies)
        {
            if (dependency != null)
            {
                yield return StartCoroutine(dependency.Init());
                dependenciesInitialized++;

                float loadingProgress = Utils.CalculateSliderValue(dependenciesInitialized, Dependencies.Count);
                loadingScreen.UpdateProgress(loadingProgress, dependency.Name);
                //print($"dependenciesInitialized: {dependenciesInitialized} - {dependency.Name}");
            }
        }

        StartCoroutine(AudioManager.Instance.InitializeMusic());
    }

    IEnumerator StartUp()
    {
        switch (launchSetting)
        {
            case LaunchSetting.Normal:
                Menu.Instance.TitleScreenPanel.StartTitleSequence();
                break;

            case LaunchSetting.QuickPlay:
                GameManager.Instance.showIntroDialogue = false;
                StartCoroutine(GameManager.Instance.InitializeNewRun(true));
                break;
        }

        yield return new WaitForSecondsRealtime(0.2f);
    }

}
