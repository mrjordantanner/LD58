using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ThemeController))]
public class ThemeControllerEditor : Editor
{
    private ThemeController themeController;
    private bool showThemes = true;
    private bool showCurrentTheme = true;

    private void OnEnable()
    {
        themeController = (ThemeController)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // Current Theme Display
        showCurrentTheme = EditorGUILayout.Foldout(showCurrentTheme, "Current Theme", true);
        if (showCurrentTheme)
        {
            EditorGUI.indentLevel++;
            
            if (Application.isPlaying)
            {
                Theme currentTheme = themeController.GetCurrentTheme();
                if (currentTheme != null)
                {
                    EditorGUILayout.LabelField("Theme Name:", currentTheme.themeName);
                    EditorGUILayout.LabelField("Background:", ColorUtility.ToHtmlStringRGB(currentTheme.backgroundColor));
                    EditorGUILayout.LabelField("Foreground:", ColorUtility.ToHtmlStringRGB(currentTheme.foregroundColor));
                    EditorGUILayout.LabelField("Accent:", ColorUtility.ToHtmlStringRGB(currentTheme.accentColor));
                }
                else
                {
                    EditorGUILayout.HelpBox("No current theme found", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Current theme info available only during play mode", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // Theme Management
        showThemes = EditorGUILayout.Foldout(showThemes, "Theme Management", true);
        if (showThemes)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Default Theme"))
            {
                AddDefaultTheme();
            }
            if (GUILayout.Button("Add Dark Theme"))
            {
                AddDarkTheme();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Bright Theme"))
            {
                AddBrightTheme();
            }
            if (GUILayout.Button("Add Warm Theme"))
            {
                AddWarmTheme();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Cool Theme"))
            {
                AddCoolTheme();
            }
            if (GUILayout.Button("Clear All Themes"))
            {
                ClearAllThemes();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);


        EditorGUILayout.Space(10);

        // Testing Controls
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Level 1 Theme"))
            {
                themeController.ApplyThemeForLevel(1);
            }
            if (GUILayout.Button("Test Level 5 Theme"))
            {
                themeController.ApplyThemeForLevel(5);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Level 10 Theme"))
            {
                themeController.ApplyThemeForLevel(10);
            }
            if (GUILayout.Button("Test Level 15 Theme"))
            {
                themeController.ApplyThemeForLevel(15);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Level 20 Theme"))
            {
                themeController.ApplyThemeForLevel(20);
            }
            if (GUILayout.Button("Reset to Default"))
            {
                themeController.ApplyTheme("Default");
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Testing controls available only during play mode", MessageType.Info);
        }

        // Force repaint to update the inspector
        if (GUI.changed)
        {
            EditorUtility.SetDirty(themeController);
        }
    }

    private void AddDefaultTheme()
    {
        Theme theme = new Theme("Default", Color.white, Color.black, Color.blue);
        themeController.AddTheme(theme);
    }

    private void AddDarkTheme()
    {
        Theme theme = new Theme("Dark", new Color(0.1f, 0.1f, 0.1f), new Color(0.9f, 0.9f, 0.9f), new Color(0.8f, 0.4f, 1f));
        themeController.AddTheme(theme);
    }

    private void AddBrightTheme()
    {
        Theme theme = new Theme("Bright", Color.white, new Color(0.1f, 0.1f, 0.1f), new Color(1f, 0.6f, 0.2f));
        themeController.AddTheme(theme);
    }

    private void AddWarmTheme()
    {
        Theme theme = new Theme("Warm", new Color(1f, 0.95f, 0.9f), new Color(0.2f, 0.15f, 0.1f), new Color(1f, 0.5f, 0.2f));
        themeController.AddTheme(theme);
    }

    private void AddCoolTheme()
    {
        Theme theme = new Theme("Cool", new Color(0.9f, 0.95f, 1f), new Color(0.1f, 0.15f, 0.2f), new Color(0.2f, 0.5f, 1f));
        themeController.AddTheme(theme);
    }

    private void ClearAllThemes()
    {
        if (EditorUtility.DisplayDialog("Clear All Themes", "Are you sure you want to clear all themes? This cannot be undone.", "Yes", "No"))
        {
            themeController.themes = new Theme[0];
            EditorUtility.SetDirty(themeController);
        }
    }
}
