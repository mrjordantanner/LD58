using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DifficultyManager))]
public class DifficultyManagerEditor : Editor
{
    private DifficultyManager difficultyManager;

    private void OnEnable()
    {
        difficultyManager = (DifficultyManager)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Difficulty Testing", EditorStyles.boldLabel);

        // Current level controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Test Level:", GUILayout.Width(80));
        int testLevel = EditorGUILayout.IntSlider(difficultyManager.currentLevel, 1, difficultyManager.maxLevel);
        if (testLevel != difficultyManager.currentLevel)
        {
            difficultyManager.UpdateDifficultyParameters(testLevel);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Quick level buttons
        EditorGUILayout.LabelField("Quick Level Tests", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Level 1"))
        {
            TestLevel(1);
        }
        if (GUILayout.Button("Level 5"))
        {
            TestLevel(5);
        }
        if (GUILayout.Button("Level 10"))
        {
            TestLevel(10);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Level 15"))
        {
            TestLevel(15);
        }
        if (GUILayout.Button("Level 20"))
        {
            TestLevel(20);
        }
        if (GUILayout.Button("Max Level"))
        {
            TestLevel(difficultyManager.maxLevel);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Difficulty visualization
        EditorGUILayout.LabelField("Current Difficulty Parameters", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Level: {difficultyManager.currentLevel}");
        EditorGUILayout.LabelField($"Velocity: {difficultyManager.currentMinVelocity:F1} - {difficultyManager.currentMaxVelocity:F1}");
        EditorGUILayout.LabelField($"Anticipation: {difficultyManager.currentAnticipationMin:F1}s - {difficultyManager.currentAnticipationMax:F1}s");
        EditorGUILayout.LabelField($"Round Duration: {difficultyManager.currentRoundDuration:F1}s");
        EditorGUILayout.LabelField($"Bounciness: {difficultyManager.currentMinBounciness:F1} - {difficultyManager.currentMaxBounciness:F1}");
        EditorGUILayout.LabelField($"Gravity: {difficultyManager.currentMinGravityScale:F1} - {difficultyManager.currentMaxGravityScale:F1}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Management buttons
        EditorGUILayout.LabelField("Management", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply to SpawnerController"))
        {
            ApplyToSpawnerController();
        }
        if (GUILayout.Button("Reset to Level 1"))
        {
            ResetDifficulty();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Curve visualization
        EditorGUILayout.LabelField("Difficulty Curves", EditorStyles.miniBoldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Curve visualization available in play mode", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Enter play mode to see curve visualization", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // Progression integration
        EditorGUILayout.LabelField("Progression Integration", EditorStyles.miniBoldLabel);
        
        if (Progression.Instance != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Progression Level:", GUILayout.Width(150));
            EditorGUILayout.LabelField(Progression.Instance.currentLevel.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sync with Progression"))
            {
                SyncWithProgression();
            }
            if (GUILayout.Button("Update Progression Level"))
            {
                UpdateProgressionLevel();
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("No Progression instance found", MessageType.Warning);
        }
    }

    private void TestLevel(int level)
    {
        if (Application.isPlaying)
        {
            difficultyManager.UpdateDifficultyParameters(level);
            difficultyManager.ApplyDifficultyToSpawnerController();
        }
        else
        {
            difficultyManager.UpdateDifficultyParameters(level);
        }
    }

    private void ApplyToSpawnerController()
    {
        if (Application.isPlaying)
        {
            difficultyManager.ApplyDifficultyToSpawnerController();
        }
        else
        {
            Debug.LogWarning("DifficultyManagerEditor: Can only apply to SpawnerController during play mode");
        }
    }

    private void ResetDifficulty()
    {
        if (Application.isPlaying)
        {
            difficultyManager.ResetDifficulty();
        }
        else
        {
            difficultyManager.UpdateDifficultyParameters(1);
        }
    }

    private void SyncWithProgression()
    {
        if (Application.isPlaying && Progression.Instance != null)
        {
            int progressionLevel = Progression.Instance.currentLevel;
            difficultyManager.UpdateDifficultyParameters(progressionLevel);
            difficultyManager.ApplyDifficultyToSpawnerController();
            Debug.Log($"DifficultyManagerEditor: Synced with Progression level {progressionLevel}");
        }
        else
        {
            Debug.LogWarning("DifficultyManagerEditor: Can only sync during play mode with active Progression");
        }
    }

    private void UpdateProgressionLevel()
    {
        if (Application.isPlaying && Progression.Instance != null)
        {
            Progression.Instance.InitializeLevel(difficultyManager.currentLevel);
            Debug.Log($"DifficultyManagerEditor: Updated Progression to level {difficultyManager.currentLevel}");
        }
        else
        {
            Debug.LogWarning("DifficultyManagerEditor: Can only update Progression during play mode");
        }
    }
}
