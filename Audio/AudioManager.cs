using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using Unity.Services.CloudSave.Models.Data.Player;
using System.Linq;

public class AudioManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static AudioManager Instance;
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


    }
    public string Name { get { return "Audio"; } }

    //public SoundEffect[] soundEffects;

    public bool audioMuted;
    public float soundCooldown;
    //public float pitchRandomizationAmount = 0.2f;

    public SoundBank soundBank;
    
    // Dictionary for quick sound lookup by name
    private Dictionary<string, SoundEffect> soundLookup = new Dictionary<string, SoundEffect>();

    [Header("Music")]
    public AudioClip titleScreenMusic;
    public AudioClip gameplayMusic;
    public AudioSource musicAudioSource;
    public float masterVolumeCachedValue;

    [Header("Mixer Groups")]
    public AudioMixerGroup masterMixerGroup;
    //public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    [Header("Audio Sliders")]
    public AudioSettingsSlider masterSlider;
    public AudioSettingsSlider musicSlider;
    public AudioSettingsSlider soundSlider;

    public IEnumerator Init()
    {
        //soundEffects = Resources.LoadAll<SoundEffect>("SoundEffects");

        musicAudioSource.loop = true;
        musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
        
        // Build sound lookup dictionary
        BuildSoundLookup();

        yield return new WaitForSecondsRealtime(0);
    }

    /// <summary>
    /// Builds the sound lookup dictionary from the sound bank
    /// </summary>
    private void BuildSoundLookup()
    {
        soundLookup.Clear();
        
        if (soundBank == null)
        {
            Debug.LogError("AudioManager: SoundBank is null - cannot build sound lookup");
            return;
        }
        
        if (soundBank.soundEffects == null)
        {
            Debug.LogError("AudioManager: SoundBank.soundEffects is null - cannot build sound lookup");
            return;
        }
        
        int validSounds = 0;
        int invalidSounds = 0;
        
        foreach (var soundEffect in soundBank.soundEffects)
        {
            if (soundEffect != null && !string.IsNullOrEmpty(soundEffect.name))
            {
                soundLookup[soundEffect.name] = soundEffect;
                validSounds++;
            }
            else
            {
                invalidSounds++;
                if (soundEffect == null)
                {
                    Debug.LogError("AudioManager: Found null SoundEffect in sound bank");
                }
                else if (string.IsNullOrEmpty(soundEffect.name))
                {
                    Debug.LogError("AudioManager: Found SoundEffect with null or empty name");
                }
            }
        }
        
        Debug.Log($"AudioManager: Built sound lookup with {validSounds} valid sounds, {invalidSounds} invalid sounds");
        
        if (invalidSounds > 0)
        {
            Debug.LogError($"AudioManager: {invalidSounds} invalid sound effects found in sound bank - check for null effects or empty names");
        }
    }

    /// <summary>
    /// Play a sound by name using the streamlined API
    /// </summary>
    /// <param name="soundName">Name of the sound to play</param>
    /// <param name="pitchRandomizationAmount">Amount of pitch randomization (0 = no randomization)</param>
    public void PlaySound(string soundName, float pitchRandomizationAmount = 0)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogError("AudioManager: PlaySound called with null or empty sound name");
            return;
        }

        if (soundLookup.TryGetValue(soundName, out SoundEffect soundEffect))
        {
            soundBank.PlaySound(soundEffect, pitchRandomizationAmount);
        }
        else
        {
            Debug.LogError($"AudioManager: Sound '{soundName}' not found in sound bank!");
            Debug.LogError($"Available sounds ({soundLookup.Count}): {string.Join(", ", soundLookup.Keys)}");
            
            // Additional debugging info
            if (soundBank == null)
            {
                Debug.LogError("AudioManager: SoundBank is null!");
            }
            else if (soundBank.soundEffects == null)
            {
                Debug.LogError("AudioManager: SoundBank.soundEffects is null!");
            }
            else
            {
                Debug.LogError($"AudioManager: SoundBank has {soundBank.soundEffects.Length} sound effects");
            }
        }
    }

    /// <summary>
    /// Check if a sound exists in the sound bank
    /// </summary>
    /// <param name="soundName">Name of the sound to check</param>
    /// <returns>True if the sound exists</returns>
    public bool HasSound(string soundName)
    {
        return soundLookup.ContainsKey(soundName);
    }

    /// <summary>
    /// Get all available sound names
    /// </summary>
    /// <returns>Array of sound names</returns>
    public string[] GetAvailableSounds()
    {
        return soundLookup.Keys.ToArray();
    }

    /// <summary>
    /// Refresh the sound lookup dictionary (useful when sounds are added at runtime)
    /// </summary>
    public void RefreshSoundLookup()
    {
        BuildSoundLookup();
    }

    public void SetMixerValue(AudioMixer mixer, string mixerParameter, float percentage)
    {
        var newValue = Utils.ConvertPercentageToDecibels(percentage);
        mixer.SetFloat(mixerParameter, newValue);
    }

    public void ToggleAudio()
    {
        audioMuted = !audioMuted;
        musicAudioSource.mute = audioMuted;
    }

    public void ReduceMusicVolume()
    {
        musicAudioSource.DOFade(0.5f, 0.3f).SetUpdate(UpdateType.Normal, true);
    }

    public void RestoreMusicVolume()
    {
        musicAudioSource.DOFade(1, 0.3f).SetUpdate(UpdateType.Normal, true);
    }

    public IEnumerator InitializeMusic()
    {
        // TODO
        //print("AudioManager: InitializeMusic() not yet implemented");
        //musicAudioSource.mute = true;
        //foreach (var levelTheme in LevelController.Instance.LevelThemeBank)
        //{
        //    musicAudioSource.clip = levelTheme.music;
        //    musicAudioSource.Play();
        //    yield return new WaitForSecondsRealtime(0.02f);
        //    musicAudioSource.Stop();
        //    yield return new WaitForSecondsRealtime(0.02f);
        //}
        //musicAudioSource.mute = false;

        yield return new WaitForSecondsRealtime(0f);
    }

    public IEnumerator SoundCooldown(SoundEffect sound)
    {
        // TODO is this correct?
        soundCooldown = Time.unscaledDeltaTime;

        sound.canPlay = false;
        yield return new WaitForSecondsRealtime(soundCooldown);
        sound.canPlay = true;
    }

    public IEnumerator MusicTransition(AudioClip track, float extraDelayTime = 0)
    {
        yield return FadeMusicOut();
        yield return new WaitForSecondsRealtime(1.5f + extraDelayTime);
        FadeMusicIn(track);
    }

    // Fading in and out modifies the AudioSource's volume directly so as not to interfere with 
    // the user's audio settings/config
    // In contrast, changing the music volume slider in the UI modifies the musicMixerGroup.audioMixer's volume
    public IEnumerator FadeMusicOut(float fadeDuration = 1.5f)
    {
        print($"AudioManager: Fade Music Out: {musicAudioSource.clip.name}");

        musicAudioSource.DOFade(0, fadeDuration).SetUpdate(UpdateType.Normal, true);
        yield return new WaitForSecondsRealtime(fadeDuration);

        musicAudioSource.Stop();
    }

    public void FadeMusicIn(AudioClip track, float fadeDuration = 1f)
    {
        print($"AudioManager: Fade Music In: {track.name}");

        musicAudioSource.volume = 0;
        musicAudioSource.clip = track;
        musicAudioSource.loop = true;

        musicAudioSource.Play();
        musicAudioSource.DOFade(1, fadeDuration).SetUpdate(UpdateType.Normal, true);

    }
}

