using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DifficultyManager))]
public class DifficultyManagerEditor : Editor
{
    private DifficultyManager difficultyManager;
    private int sliderLevel = 1;

    private void OnEnable()
    {
        difficultyManager = (DifficultyManager)target;
        sliderLevel = difficultyManager.currentLevel;
    }

    public override void OnInspectorGUI()
    {
        // Update slider level if it's out of sync
        if (sliderLevel != difficultyManager.currentLevel)
        {
            sliderLevel = difficultyManager.currentLevel;
        }

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Difficulty Testing", EditorStyles.boldLabel);

        // Current level controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Test Level:", GUILayout.Width(80));
        sliderLevel = EditorGUILayout.IntSlider(sliderLevel, 1, difficultyManager.maxLevel);
        if (sliderLevel != difficultyManager.currentLevel)
        {
            difficultyManager.UpdateDifficultyParameters(sliderLevel);
            EditorUtility.SetDirty(difficultyManager); // Mark as dirty to update inspector
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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test All Levels"))
        {
            TestAllLevels();
        }
        if (GUILayout.Button("Log Current Values"))
        {
            LogCurrentValues();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Simple Values"))
        {
            TestSimpleValues();
        }
        if (GUILayout.Button("Force Update Level 5"))
        {
            ForceUpdateLevel5();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug Level 1"))
        {
            DebugLevel1();
        }
        if (GUILayout.Button("Test Level 5 Again"))
        {
            TestLevel5Again();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Linear scaling info
        EditorGUILayout.LabelField("Difficulty Scaling", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox("Using linear interpolation from base values (Level 1) to max values (Level 20)", MessageType.Info);

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
        sliderLevel = level;
        if (Application.isPlaying)
        {
            difficultyManager.UpdateDifficultyParameters(level);
            difficultyManager.ApplyDifficultyToSpawnerController();
        }
        else
        {
            difficultyManager.UpdateDifficultyParameters(level);
        }
        EditorUtility.SetDirty(difficultyManager);
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

    private void TestAllLevels()
    {
        if (Application.isPlaying)
        {
            difficultyManager.TestDifficultyScaling();
        }
        else
        {
            Debug.LogWarning("DifficultyManagerEditor: Can only test all levels during play mode");
        }
    }

    private void LogCurrentValues()
    {
        Debug.Log($"=== CURRENT DIFFICULTY VALUES (Level {difficultyManager.currentLevel}) ===");
        Debug.Log($"Velocity: {difficultyManager.currentMinVelocity:F1} - {difficultyManager.currentMaxVelocity:F1}");
        Debug.Log($"Anticipation: {difficultyManager.currentAnticipationMin:F1}s - {difficultyManager.currentAnticipationMax:F1}s");
        
        Debug.Log($"Bounciness: {difficultyManager.currentMinBounciness:F1} - {difficultyManager.currentMaxBounciness:F1}");
        Debug.Log($"Gravity: {difficultyManager.currentMinGravityScale:F1} - {difficultyManager.currentMaxGravityScale:F1}");
        Debug.Log("================================================");
    }

    private void TestSimpleValues()
    {
        if (Application.isPlaying)
        {
            difficultyManager.TestSimpleValues();
        }
        else
        {
            Debug.LogWarning("DifficultyManagerEditor: Can only test simple values during play mode");
        }
    }

    private void ForceUpdateLevel5()
    {
        Debug.Log("=== FORCE UPDATE LEVEL 5 ===");
        Debug.Log($"Before: currentMinVelocity = {difficultyManager.currentMinVelocity}, currentMaxVelocity = {difficultyManager.currentMaxVelocity}");
        
        difficultyManager.UpdateDifficultyParameters(5);
        
        Debug.Log($"After: currentMinVelocity = {difficultyManager.currentMinVelocity}, currentMaxVelocity = {difficultyManager.currentMaxVelocity}");
        Debug.Log("=== END FORCE UPDATE ===");
        
        EditorUtility.SetDirty(difficultyManager);
    }

    private void DebugLevel1()
    {
        Debug.Log("=== DEBUG LEVEL 1 ===");
        Debug.Log($"Base Min: {difficultyManager.baseMinVelocity}, Base Max: {difficultyManager.baseMaxVelocity}");
        Debug.Log($"Max Min: {difficultyManager.maxMinVelocity}, Max Max: {difficultyManager.maxMaxVelocity}");
        
        difficultyManager.UpdateDifficultyParameters(1);
        
        Debug.Log($"After Level 1: currentMinVelocity = {difficultyManager.currentMinVelocity}, currentMaxVelocity = {difficultyManager.currentMaxVelocity}");
        Debug.Log("=== END DEBUG LEVEL 1 ===");
        
        EditorUtility.SetDirty(difficultyManager);
    }

    private void TestLevel5Again()
    {
        Debug.Log("=== TEST LEVEL 5 AFTER CURVE RESET ===");
        difficultyManager.UpdateDifficultyParameters(5);
        Debug.Log($"Level 5: currentMinVelocity = {difficultyManager.currentMinVelocity}, currentMaxVelocity = {difficultyManager.currentMaxVelocity}");
        Debug.Log("=== END TEST ===");
        
        EditorUtility.SetDirty(difficultyManager);
    }
}
