using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton Theme Controller that manages global color references for the entire game.
/// Allows dynamic theme changes by updating global color values.
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

    [Header("Primary Light Colors")]
    [Tooltip("Main light color (near white)")]
    public Color primaryLight = new Color(0.95f, 0.95f, 0.95f, 1f);
    [Tooltip("Light tint (pure white)")]
    public Color primaryLightTint = Color.white;
    [Tooltip("Light shade (darker than primary light)")]
    public Color primaryLightShade = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("Primary Dark Colors")]
    [Tooltip("Main dark color")]
    public Color primaryDark = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Tooltip("Dark tint (lighter than primary dark)")]
    public Color primaryDarkTint = new Color(0.4f, 0.4f, 0.4f, 1f);
    [Tooltip("Dark shade (darker than primary dark)")]
    public Color primaryDarkShade = new Color(0.1f, 0.1f, 0.1f, 1f);

    [Header("Accent Colors")]
    [Tooltip("Main accent color")]
    public Color accent = new Color(0.2f, 0.6f, 1f, 1f);
    [Tooltip("Accent tint (lighter than accent)")]
    public Color accentTint = new Color(0.4f, 0.7f, 1f, 1f);
    [Tooltip("Accent shade (darker than accent)")]
    public Color accentShade = new Color(0.1f, 0.4f, 0.8f, 1f);

    [Header("Utility Colors")]
    [Tooltip("Success/positive color")]
    public Color success = new Color(0.2f, 0.8f, 0.2f, 1f);
    [Tooltip("Warning/caution color")]
    public Color warning = new Color(1f, 0.8f, 0.2f, 1f);
    [Tooltip("Error/danger color")]
    public Color error = new Color(0.8f, 0.2f, 0.2f, 1f);
    [Tooltip("Info/neutral color")]
    public Color info = new Color(0.2f, 0.6f, 0.8f, 1f);

    #endregion

    public IEnumerator Init()
    {
        Debug.Log("ThemeController: Initialized with color palette");
        yield return new WaitForSecondsRealtime(0);
    }

    #region Color Access Methods

    /// <summary>
    /// Gets a color from the theme by name
    /// </summary>
    /// <param name="colorName">Name of the color to get</param>
    /// <returns>The requested color or white if not found</returns>
    public Color GetColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            // Primary Light
            case "primarylight":
            case "primary_light":
                return primaryLight;
            case "primarylighttint":
            case "primary_light_tint":
                return primaryLightTint;
            case "primarylightshade":
            case "primary_light_shade":
                return primaryLightShade;

            // Primary Dark
            case "primarydark":
            case "primary_dark":
                return primaryDark;
            case "primarydarktint":
            case "primary_dark_tint":
                return primaryDarkTint;
            case "primarydarkshade":
            case "primary_dark_shade":
                return primaryDarkShade;

            // Accent
            case "accent":
                return accent;
            case "accenttint":
            case "accent_tint":
                return accentTint;
            case "accentshade":
            case "accent_shade":
                return accentShade;

            // Utility
            case "success":
                return success;
            case "warning":
                return warning;
            case "error":
                return error;
            case "info":
                return info;

            default:
                Debug.LogWarning($"ThemeController: Unknown color name '{colorName}', returning white");
                return Color.white;
        }
    }

    /// <summary>
    /// Sets a color in the theme by name
    /// </summary>
    /// <param name="colorName">Name of the color to set</param>
    /// <param name="color">The color value to set</param>
    public void SetColor(string colorName, Color color)
    {
        switch (colorName.ToLower())
        {
            // Primary Light
            case "primarylight":
            case "primary_light":
                primaryLight = color;
                break;
            case "primarylighttint":
            case "primary_light_tint":
                primaryLightTint = color;
                break;
            case "primarylightshade":
            case "primary_light_shade":
                primaryLightShade = color;
                break;

            // Primary Dark
            case "primarydark":
            case "primary_dark":
                primaryDark = color;
                break;
            case "primarydarktint":
            case "primary_dark_tint":
                primaryDarkTint = color;
                break;
            case "primarydarkshade":
            case "primary_dark_shade":
                primaryDarkShade = color;
                break;

            // Accent
            case "accent":
                accent = color;
                break;
            case "accenttint":
            case "accent_tint":
                accentTint = color;
                break;
            case "accentshade":
            case "accent_shade":
                accentShade = color;
                break;

            // Utility
            case "success":
                success = color;
                break;
            case "warning":
                warning = color;
                break;
            case "error":
                error = color;
                break;
            case "info":
                info = color;
                break;

            default:
                Debug.LogWarning($"ThemeController: Unknown color name '{colorName}'");
                break;
        }
    }

    #endregion

    #region Theme Presets

    /// <summary>
    /// Applies a predefined theme preset
    /// </summary>
    /// <param name="preset">The theme preset to apply</param>
    public void ApplyThemePreset(ThemePreset preset)
    {
        switch (preset)
        {
            case ThemePreset.Default:
                ApplyDefaultTheme();
                break;
            case ThemePreset.Dark:
                ApplyDarkTheme();
                break;
            case ThemePreset.Bright:
                ApplyBrightTheme();
                break;
            case ThemePreset.Monochrome:
                ApplyMonochromeTheme();
                break;
            case ThemePreset.Warm:
                ApplyWarmTheme();
                break;
            case ThemePreset.Cool:
                ApplyCoolTheme();
                break;
        }
        
        Debug.Log($"ThemeController: Applied {preset} theme");
    }

    private void ApplyDefaultTheme()
    {
        primaryLight = new Color(0.95f, 0.95f, 0.95f, 1f);
        primaryLightTint = Color.white;
        primaryLightShade = new Color(0.8f, 0.8f, 0.8f, 1f);
        primaryDark = new Color(0.2f, 0.2f, 0.2f, 1f);
        primaryDarkTint = new Color(0.4f, 0.4f, 0.4f, 1f);
        primaryDarkShade = new Color(0.1f, 0.1f, 0.1f, 1f);
        accent = new Color(0.2f, 0.6f, 1f, 1f);
        accentTint = new Color(0.4f, 0.7f, 1f, 1f);
        accentShade = new Color(0.1f, 0.4f, 0.8f, 1f);
    }

    private void ApplyDarkTheme()
    {
        primaryLight = new Color(0.3f, 0.3f, 0.3f, 1f);
        primaryLightTint = new Color(0.5f, 0.5f, 0.5f, 1f);
        primaryLightShade = new Color(0.2f, 0.2f, 0.2f, 1f);
        primaryDark = new Color(0.05f, 0.05f, 0.05f, 1f);
        primaryDarkTint = new Color(0.15f, 0.15f, 0.15f, 1f);
        primaryDarkShade = new Color(0.02f, 0.02f, 0.02f, 1f);
        accent = new Color(0.8f, 0.4f, 1f, 1f);
        accentTint = new Color(0.9f, 0.6f, 1f, 1f);
        accentShade = new Color(0.6f, 0.2f, 0.8f, 1f);
    }

    private void ApplyBrightTheme()
    {
        primaryLight = Color.white;
        primaryLightTint = new Color(1f, 1f, 0.95f, 1f);
        primaryLightShade = new Color(0.9f, 0.9f, 0.9f, 1f);
        primaryDark = new Color(0.1f, 0.1f, 0.1f, 1f);
        primaryDarkTint = new Color(0.3f, 0.3f, 0.3f, 1f);
        primaryDarkShade = new Color(0.05f, 0.05f, 0.05f, 1f);
        accent = new Color(1f, 0.6f, 0.2f, 1f);
        accentTint = new Color(1f, 0.8f, 0.4f, 1f);
        accentShade = new Color(0.8f, 0.4f, 0.1f, 1f);
    }

    private void ApplyMonochromeTheme()
    {
        primaryLight = new Color(0.9f, 0.9f, 0.9f, 1f);
        primaryLightTint = Color.white;
        primaryLightShade = new Color(0.7f, 0.7f, 0.7f, 1f);
        primaryDark = new Color(0.1f, 0.1f, 0.1f, 1f);
        primaryDarkTint = new Color(0.3f, 0.3f, 0.3f, 1f);
        primaryDarkShade = new Color(0.05f, 0.05f, 0.05f, 1f);
        accent = new Color(0.5f, 0.5f, 0.5f, 1f);
        accentTint = new Color(0.7f, 0.7f, 0.7f, 1f);
        accentShade = new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    private void ApplyWarmTheme()
    {
        primaryLight = new Color(1f, 0.95f, 0.9f, 1f);
        primaryLightTint = new Color(1f, 1f, 0.95f, 1f);
        primaryLightShade = new Color(0.9f, 0.85f, 0.8f, 1f);
        primaryDark = new Color(0.2f, 0.15f, 0.1f, 1f);
        primaryDarkTint = new Color(0.4f, 0.35f, 0.3f, 1f);
        primaryDarkShade = new Color(0.1f, 0.08f, 0.05f, 1f);
        accent = new Color(1f, 0.5f, 0.2f, 1f);
        accentTint = new Color(1f, 0.7f, 0.4f, 1f);
        accentShade = new Color(0.8f, 0.3f, 0.1f, 1f);
    }

    private void ApplyCoolTheme()
    {
        primaryLight = new Color(0.9f, 0.95f, 1f, 1f);
        primaryLightTint = new Color(0.95f, 1f, 1f, 1f);
        primaryLightShade = new Color(0.8f, 0.85f, 0.9f, 1f);
        primaryDark = new Color(0.1f, 0.15f, 0.2f, 1f);
        primaryDarkTint = new Color(0.3f, 0.35f, 0.4f, 1f);
        primaryDarkShade = new Color(0.05f, 0.08f, 0.1f, 1f);
        accent = new Color(0.2f, 0.5f, 1f, 1f);
        accentTint = new Color(0.4f, 0.7f, 1f, 1f);
        accentShade = new Color(0.1f, 0.3f, 0.8f, 1f);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets a random color from the current theme
    /// </summary>
    /// <returns>A random color from the theme</returns>
    public Color GetRandomThemeColor()
    {
        Color[] themeColors = {
            primaryLight, primaryLightTint, primaryLightShade,
            primaryDark, primaryDarkTint, primaryDarkShade,
            accent, accentTint, accentShade
        };
        
        return themeColors[Random.Range(0, themeColors.Length)];
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
}

/// <summary>
/// Available theme presets
/// </summary>
public enum ThemePreset
{
    Default,
    Dark,
    Bright,
    Monochrome,
    Warm,
    Cool
}
