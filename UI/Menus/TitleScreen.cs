using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TitleScreen : MenuPanel
{
    //public TextMeshProUGUI pressKeyToStartLabel;

    private void Update()
    {
        if (!isShowing) return;

        //if (Utils.ClickOrTap())
        //{

        //    // TODO play start sound

        //    //pressKeyToStartLabel.gameObject.SetActive(false);
        //    StartCoroutine(GameManager.Instance.InitializeNewRun());
            
        //}
    }

    public void StartTitleSequence()
    {
        Debug.Log("TitleScreen: StartTitleSequence() called");
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        Debug.Log("TitleScreen: IntroSequence() started");
        
        // Start with black screen (instant)
        Debug.Log("TitleScreen: Setting screen to black");
        HUD.Instance.screenFader.SetAlpha(1f);
        HUD.Instance.screenFader.image.color = Color.black;
        
        // Show title screen panel (but it will be hidden behind black screen)
        Debug.Log("TitleScreen: Showing title screen panel");
        Show();
        
        // Start music fade in
        Debug.Log("TitleScreen: Starting music fade in");
        if (AudioManager.Instance != null)
        {
            Debug.Log("TitleScreen: AudioManager found, calling FadeMusicIn");
            if (AudioManager.Instance.gameplayMusic != null)
            {
                AudioManager.Instance.FadeMusicIn(AudioManager.Instance.gameplayMusic);
                Debug.Log("TitleScreen: Music fade in started successfully");
            }
            else
            {
                Debug.LogWarning("TitleScreen: gameplayMusic is not assigned in AudioManager, skipping music fade in");
            }
        }
        else
        {
            Debug.LogError("TitleScreen: AudioManager.Instance is NULL!");
        }
        
        // Play intro sound
        Debug.Log("TitleScreen: Attempting to play Intro sound");
        if (AudioManager.Instance != null)
        {
            Debug.Log("TitleScreen: AudioManager found, calling PlaySound('Intro')");
            
            // Additional debugging for AudioManager state
            Debug.Log($"TitleScreen: AudioManager soundEffects count: {(AudioManager.Instance.soundBank.soundEffects != null ? AudioManager.Instance.soundBank.soundEffects.Length : "NULL")}");
            Debug.Log($"TitleScreen: AudioManager soundBank: {(AudioManager.Instance.soundBank != null ? "Found" : "NULL")}");
            
            // List available sounds for debugging
            if (AudioManager.Instance.soundBank != null && AudioManager.Instance.soundBank.soundEffects != null)
            {
                Debug.Log("TitleScreen: Available sounds:");
                foreach (var sound in AudioManager.Instance.soundBank.soundEffects)
                {
                    if (sound != null)
                    {
                        Debug.Log($"TitleScreen: - {sound.name}");
                    }
                }
            }
            
            AudioManager.Instance.PlaySound("Intro");
            Debug.Log("TitleScreen: PlaySound('Intro') call completed");
        }
        else
        {
            Debug.LogError("TitleScreen: AudioManager.Instance is NULL when trying to play Intro sound!");
        }
        
        // Wait a brief moment for audio to start
        Debug.Log("TitleScreen: Waiting 0.1 seconds for audio to start");
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Fade in from black over 1 second
        Debug.Log("TitleScreen: Starting screen fade in");
        HUD.Instance.screenFader.FadeIn(1f);
        Debug.Log("TitleScreen: IntroSequence() completed");
    }


}
