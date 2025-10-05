using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
