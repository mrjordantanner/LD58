using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour, IInitializable
{
    #region Singleton
    public static Config Instance;

    private void Awake()
    {
        if (Application.isEditor)
        {

            Instance = this;
        }
        else
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

        StartCoroutine(Init());
    }


    #endregion

    public string Name { get { return "Configuration"; } }

    [HideInInspector]
    public Setting MasterVolume, MusicVolume, SoundVolume;
    [HideInInspector] public Setting[] allSettings, audioSettings;

    public UserDto currentUserDto = new();

    public IEnumerator Init()
    {
        allSettings = Resources.LoadAll<Setting>("ConfigSettings");
        audioSettings = Resources.LoadAll<Setting>("ConfigSettings/Audio");

        MasterVolume = GetSettingByName(nameof(MasterVolume));
        MusicVolume = GetSettingByName(nameof(MusicVolume));
        SoundVolume = GetSettingByName(nameof(SoundVolume));

        ResetSettingsToDefault(audioSettings);
        //LoadAllSettingsFromPlayerPrefs();

        yield return new WaitForSecondsRealtime(0);
    }

    public Setting GetSettingByName(string settingName)
    {
        foreach (var setting in allSettings)
        {
            if (setting.name == settingName)
            {
                return setting;
            }
        }

        Debug.LogError($"Unable to get Setting by name: {settingName}");
        return null;
    }

    public void LoadAllSettingsFromPlayerPrefs()
    {
        foreach (var setting in allSettings)
        {
            setting.LoadFromPlayerPrefs();
        }
    }

    public void SaveAllSettingsToPlayerPrefs()      
    {
        foreach (var setting in allSettings)
        {
            setting.SaveToPlayerPrefs();
        }
    }

    public void SaveUsernameToPlayerPrefs(string value) 
    {
        PlayerPrefs.SetString("Username", value);
        PlayerPrefs.Save();
        currentUserDto.PlayerName = value;
    }

    public void ResetSettingsToDefault(Setting[] settingsArray)
    { 
        foreach (var setting in settingsArray)
        {
            setting.SetToDefault();
        }
    }

    public static string GetPlayerPrefsString(string key)
    {
        if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetString(key);
        return string.Empty;
    }

    // Button callback
    public void ResetAudioSettingsToDefault()
    {
        ResetSettingsToDefault(audioSettings);
    }

    // Button Callback
    public void SaveAudioSettings()
    {
        foreach (var setting in audioSettings)
        {
            setting.SaveToPlayerPrefs();
        }
    }
}








