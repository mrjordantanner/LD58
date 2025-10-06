using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Serializable theme data structure
/// </summary>
[System.Serializable]
public class Theme
{
    [Header("Theme Info")]
    public string themeName = "New Theme";
    
    [Header("Core Colors")]
    public Color backgroundColor = Color.white;
    public Color foregroundColor = Color.black;
    public Color accentColor = Color.blue;
    
    public Theme(string name, Color bg, Color fg, Color accent)
    {
        themeName = name;
        backgroundColor = bg;
        foregroundColor = fg;
        accentColor = accent;
    }
}


/// <summary>
/// Singleton Theme Controller that manages dynamic theme selection based on level progression.
/// Supports multiple themes with level-based automatic switching.
/// </summary>
public class ThemeController : MonoBehaviour, IInitializable
{
    #region Singleton
    public static ThemeController Instance;
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

    public string Name { get { return "Theme Controller"; } }

    [Header("Available Themes")]
    [Tooltip("Array of themes - Level 1 uses themes[0], Level 2 uses themes[1], etc.")]
    public Theme[] themes = new Theme[0];
    
    [Header("Current Theme")]
    [ReadOnly] public string currentThemeName = "Default";
    [ReadOnly] public int currentLevel = 1;
    
    [Header("Fallback Theme")]
    [Tooltip("Theme to use if no theme is found for a level")]
    public Theme fallbackTheme = new Theme("Default", Color.white, Color.black, Color.blue);

    #endregion

    public IEnumerator Init()
    {
        // Initialize with default theme
        ApplyTheme("Default");
        Debug.Log("ThemeController: Initialized with dynamic theme system");
        yield return new WaitForSecondsRealtime(0);
    }

    #region Theme Management

    /// <summary>
    /// Applies a theme by name
    /// </summary>
    /// <param name="themeName">Name of the theme to apply</param>
    public void ApplyTheme(string themeName)
    {
        Theme theme = GetThemeByName(themeName);
        if (theme != null)
        {
            currentThemeName = themeName;
            ApplyThemeColors(theme);
            Debug.Log($"ThemeController: Applied theme '{themeName}'");
        }
        else
        {
            Debug.LogWarning($"ThemeController: Theme '{themeName}' not found, using fallback theme");
            currentThemeName = fallbackTheme.themeName;
            ApplyThemeColors(fallbackTheme);
        }
    }

    /// <summary>
    /// Applies a theme based on the current level (uses array index)
    /// </summary>
    /// <param name="level">The level to get theme for</param>
    public void ApplyThemeForLevel(int level)
    {
        currentLevel = level;
        
        // Level 1 uses themes[0], Level 2 uses themes[1], etc.
        int themeIndex = level - 1;
        
        if (themeIndex >= 0 && themeIndex < themes.Length)
        {
            Theme theme = themes[themeIndex];
            currentThemeName = theme.themeName;
            ApplyThemeColors(theme);
            Debug.Log($"ThemeController: Applied theme '{theme.themeName}' for level {level}");
        }
        else
        {
            currentThemeName = fallbackTheme.themeName;
            ApplyThemeColors(fallbackTheme);
            Debug.LogWarning($"ThemeController: No theme found for level {level}, using fallback theme");
        }
    }

    /// <summary>
    /// Gets a theme by name
    /// </summary>
    /// <param name="themeName">Name of the theme</param>
    /// <returns>The theme or null if not found</returns>
    public Theme GetThemeByName(string themeName)
    {
        foreach (Theme theme in themes)
        {
            if (theme.themeName == themeName)
            {
                return theme;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the current active theme
    /// </summary>
    /// <returns>The current theme or fallback if not found</returns>
    public Theme GetCurrentTheme()
    {
        Theme theme = GetThemeByName(currentThemeName);
        return theme != null ? theme : fallbackTheme;
    }

    /// <summary>
    /// Applies theme colors to game objects
    /// </summary>
    /// <param name="theme">The theme to apply</param>
    private void ApplyThemeColors(Theme theme)
    {
        // Apply background color to VFX background
        if (VFX.Instance != null && VFX.Instance.backgroundSpriteRenderer != null)
        {
            VFX.Instance.backgroundSpriteRenderer.color = theme.backgroundColor;
            Debug.Log($"ThemeController: Applied background color to VFX background");
        }
        else
        {
            Debug.LogWarning("ThemeController: VFX.Instance or backgroundSpriteRenderer not found!");
        }

        // Apply accent color to iPhone frame sprite renderer
        if (VFX.Instance != null && VFX.Instance.iphoneFrameSpriteRenderer != null)
        {
            VFX.Instance.iphoneFrameSpriteRenderer.color = theme.accentColor;
            Debug.Log($"ThemeController: Applied accent color to iPhone frame sprite renderer");
        }
        else
        {
            Debug.LogWarning("ThemeController: VFX.Instance or iphoneFrameSpriteRenderer not found!");
        }

        // Apply accent color to all existing balls
        BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
        foreach (BallMovement ball in allBalls)
        {
            SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.color = theme.accentColor;
            }
        }
        Debug.Log($"ThemeController: Applied accent color to {allBalls.Length} balls");

        // Apply foreground color to player sprites (excluding graphicBack)
        PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
        if (playerCharacter != null)
        {
            // Apply to player sprite renderers (excluding graphicBack)
            if (playerCharacter.graphicTopLeft != null)
                playerCharacter.graphicTopLeft.color = theme.foregroundColor;
            if (playerCharacter.graphicTopRight != null)
                playerCharacter.graphicTopRight.color = theme.foregroundColor;
            if (playerCharacter.graphicBottomLeft != null)
                playerCharacter.graphicBottomLeft.color = theme.foregroundColor;
            if (playerCharacter.graphicBottomRight != null)
                playerCharacter.graphicBottomRight.color = theme.foregroundColor;
            
            Debug.Log("ThemeController: Applied foreground color to player sprites (excluding graphicBack)");
        }
        else
        {
            Debug.LogWarning("ThemeController: PlayerCharacter not found!");
        }
    }

    /// <summary>
    /// Applies theme colors with smooth tweening transitions
    /// </summary>
    /// <param name="theme">The theme to apply</param>
    /// <param name="duration">Duration of the color transition in seconds</param>
    /// <param name="ease">Easing type for the transition</param>
    private void ApplyThemeColorsWithTween(Theme theme, float duration = 1f, Ease ease = Ease.OutQuart)
    {
        Debug.Log($"ThemeController: Applying theme '{theme.themeName}' with {duration}s tween transition");
        
        // Tween background color
        if (VFX.Instance != null && VFX.Instance.backgroundSpriteRenderer != null)
        {
            VFX.Instance.backgroundSpriteRenderer.DOColor(theme.backgroundColor, duration).SetEase(ease);
            Debug.Log("ThemeController: Tweening background color");
        }
        else
        {
            Debug.LogWarning("ThemeController: VFX.Instance or backgroundSpriteRenderer not found!");
        }

        // Tween iPhone frame accent color
        if (VFX.Instance != null && VFX.Instance.iphoneFrameSpriteRenderer != null)
        {
            VFX.Instance.iphoneFrameSpriteRenderer.DOColor(theme.accentColor, duration).SetEase(ease);
            Debug.Log("ThemeController: Tweening iPhone frame accent color");
        }
        else
        {
            Debug.LogWarning("ThemeController: VFX.Instance or iphoneFrameSpriteRenderer not found!");
        }

        // Apply accent color to all existing balls (INSTANT - no tweening)
        BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
        foreach (BallMovement ball in allBalls)
        {
            SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.color = theme.accentColor; // Instant application
            }
        }
        Debug.Log($"ThemeController: Applied accent color instantly to {allBalls.Length} balls");

        // Tween foreground color for player sprites
        PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
        if (playerCharacter != null)
        {
            // Tween player sprite renderers (excluding graphicBack)
            if (playerCharacter.graphicTopLeft != null)
                playerCharacter.graphicTopLeft.DOColor(theme.foregroundColor, duration).SetEase(ease);
            if (playerCharacter.graphicTopRight != null)
                playerCharacter.graphicTopRight.DOColor(theme.foregroundColor, duration).SetEase(ease);
            if (playerCharacter.graphicBottomLeft != null)
                playerCharacter.graphicBottomLeft.DOColor(theme.foregroundColor, duration).SetEase(ease);
            if (playerCharacter.graphicBottomRight != null)
                playerCharacter.graphicBottomRight.DOColor(theme.foregroundColor, duration).SetEase(ease);
            
            Debug.Log("ThemeController: Tweening foreground color for player sprites");
        }
        else
        {
            Debug.LogWarning("ThemeController: PlayerCharacter not found!");
        }
    }

    /// <summary>
    /// Applies a theme with smooth color transitions
    /// </summary>
    /// <param name="themeName">Name of the theme to apply</param>
    /// <param name="duration">Duration of the color transition in seconds</param>
    /// <param name="ease">Easing type for the transition</param>
    public void ApplyThemeWithTween(string themeName, float duration = 1f, Ease ease = Ease.OutQuart)
    {
        Theme theme = GetThemeByName(themeName);
        if (theme != null)
        {
            currentThemeName = themeName;
            ApplyThemeColorsWithTween(theme, duration, ease);
            Debug.Log($"ThemeController: Applied theme '{themeName}' with {duration}s tween");
        }
        else
        {
            Debug.LogWarning($"ThemeController: Theme '{themeName}' not found, using fallback theme");
            currentThemeName = fallbackTheme.themeName;
            ApplyThemeColorsWithTween(fallbackTheme, duration, ease);
        }
    }

    /// <summary>
    /// Applies a theme for a level with smooth color transitions
    /// </summary>
    /// <param name="level">The level to get theme for</param>
    /// <param name="duration">Duration of the color transition in seconds</param>
    /// <param name="ease">Easing type for the transition</param>
    public void ApplyThemeForLevelWithTween(int level, float duration = 1f, Ease ease = Ease.OutQuart)
    {
        currentLevel = level;
        
        // Level 1 uses themes[0], Level 2 uses themes[1], etc.
        int themeIndex = level - 1;
        
        if (themeIndex >= 0 && themeIndex < themes.Length)
        {
            Theme theme = themes[themeIndex];
            currentThemeName = theme.themeName;
            ApplyThemeColorsWithTween(theme, duration, ease);
            Debug.Log($"ThemeController: Applied theme '{theme.themeName}' for level {level} with {duration}s tween");
        }
        else
        {
            currentThemeName = fallbackTheme.themeName;
            ApplyThemeColorsWithTween(fallbackTheme, duration, ease);
            Debug.LogWarning($"ThemeController: No theme found for level {level}, using fallback theme with tween");
        }
    }

    /// <summary>
    /// Applies theme colors to a specific player character with smooth tweening
    /// </summary>
    /// <param name="playerCharacter">The player character to theme</param>
    /// <param name="duration">Duration of the color transition in seconds</param>
    /// <param name="ease">Easing type for the transition</param>
    public void ApplyThemeToPlayerWithTween(PlayerCharacter playerCharacter, float duration = 0.5f, Ease ease = Ease.OutQuart)
    {
        if (playerCharacter == null) return;

        Theme currentTheme = GetCurrentTheme();
        
        // Tween foreground color for the 4 player graphics renderers
        if (playerCharacter.graphicTopLeft != null)
            playerCharacter.graphicTopLeft.DOColor(currentTheme.foregroundColor, duration).SetEase(ease);
        if (playerCharacter.graphicTopRight != null)
            playerCharacter.graphicTopRight.DOColor(currentTheme.foregroundColor, duration).SetEase(ease);
        if (playerCharacter.graphicBottomLeft != null)
            playerCharacter.graphicBottomLeft.DOColor(currentTheme.foregroundColor, duration).SetEase(ease);
        if (playerCharacter.graphicBottomRight != null)
            playerCharacter.graphicBottomRight.DOColor(currentTheme.foregroundColor, duration).SetEase(ease);
        
        Debug.Log($"ThemeController: Tweened foreground color for player sprites over {duration}s");
    }

    /// <summary>
    /// Applies theme colors to a specific player character
    /// </summary>
    /// <param name="playerCharacter">The player character to theme</param>
    public void ApplyThemeToPlayer(PlayerCharacter playerCharacter)
    {
        if (playerCharacter == null) return;

        Theme currentTheme = GetCurrentTheme();
        
        // Apply foreground color to the 4 player graphics renderers
        if (playerCharacter.graphicTopLeft != null)
            playerCharacter.graphicTopLeft.color = currentTheme.foregroundColor;
        if (playerCharacter.graphicTopRight != null)
            playerCharacter.graphicTopRight.color = currentTheme.foregroundColor;
        if (playerCharacter.graphicBottomLeft != null)
            playerCharacter.graphicBottomLeft.color = currentTheme.foregroundColor;
        if (playerCharacter.graphicBottomRight != null)
            playerCharacter.graphicBottomRight.color = currentTheme.foregroundColor;
        
        Debug.Log("ThemeController: Applied foreground color to player sprites");
    }

    #endregion

    #region Color Access Methods

    /// <summary>
    /// Gets the background color from the current theme
    /// </summary>
    /// <returns>Background color</returns>
    public Color GetBackgroundColor()
    {
        return GetCurrentTheme().backgroundColor;
    }

    /// <summary>
    /// Gets the foreground color from the current theme
    /// </summary>
    /// <returns>Foreground color</returns>
    public Color GetForegroundColor()
    {
        return GetCurrentTheme().foregroundColor;
    }

    /// <summary>
    /// Gets the accent color from the current theme
    /// </summary>
    /// <returns>Accent color</returns>
    public Color GetAccentColor()
    {
        return GetCurrentTheme().accentColor;
    }


    /// <summary>
    /// Gets a color from the current theme by name
    /// </summary>
    /// <param name="colorName">Name of the color to get</param>
    /// <returns>The requested color or white if not found</returns>
    public Color GetColor(string colorName)
    {
        Theme currentTheme = GetCurrentTheme();
        
        switch (colorName.ToLower())
        {
            case "background":
            case "bg":
                return currentTheme.backgroundColor;
            case "foreground":
            case "fg":
                return currentTheme.foregroundColor;
            case "accent":
                return currentTheme.accentColor;
            default:
                Debug.LogWarning($"ThemeController: Unknown color name '{colorName}', returning white");
                return Color.white;
        }
    }

    /// <summary>
    /// Gets a color with alpha transparency
    /// </summary>
    /// <param name="colorName">Name of the color</param>
    /// <param name="alpha">Alpha value (0-1)</param>
    /// <returns>Color with modified alpha</returns>
    public Color GetColorWithAlpha(string colorName, float alpha)
    {
        Color color = GetColor(colorName);
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets a random color from the current theme
    /// </summary>
    /// <returns>A random color from the theme</returns>
    public Color GetRandomThemeColor()
    {
        Theme currentTheme = GetCurrentTheme();
        Color[] themeColors = {
            currentTheme.backgroundColor,
            currentTheme.foregroundColor,
            currentTheme.accentColor
        };
        
        return themeColors[Random.Range(0, themeColors.Length)];
    }

    /// <summary>
    /// Adds a new theme to the themes array
    /// </summary>
    /// <param name="theme">The theme to add</param>
    public void AddTheme(Theme theme)
    {
        System.Array.Resize(ref themes, themes.Length + 1);
        themes[themes.Length - 1] = theme;
        Debug.Log($"ThemeController: Added theme '{theme.themeName}'");
    }


    #endregion
}
