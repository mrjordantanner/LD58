using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnerController))]
public class SpawnerControllerEditor : Editor
{
    private SpawnerController spawnerController;

    private void OnEnable()
    {
        spawnerController = (SpawnerController)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Physics Ball Testing", EditorStyles.boldLabel);

        // Ball prefab warning
        if (spawnerController.ballPrefab == null)
        {
            EditorGUILayout.HelpBox("Ball Prefab is not assigned! Please assign a prefab with BallMovement component.", MessageType.Warning);
        }

        // Play area collider warning
        if (spawnerController.playAreaCollider == null)
        {
            EditorGUILayout.HelpBox("Play Area Collider is not assigned! Balls will spawn at origin.", MessageType.Warning);
        }

        EditorGUILayout.Space(5);

        // Spawning controls
        EditorGUILayout.LabelField("Spawning Controls", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Ball", GUILayout.Height(30)))
        {
            SpawnBall();
        }
        if (GUILayout.Button("Spawn 5 Balls", GUILayout.Height(30)))
        {
            SpawnMultipleBalls(5);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn 10 Balls", GUILayout.Height(30)))
        {
            SpawnMultipleBalls(10);
        }
        if (GUILayout.Button("Spawn 20 Balls", GUILayout.Height(30)))
        {
            SpawnMultipleBalls(20);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Physics testing
        EditorGUILayout.LabelField("Physics Testing", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("High Velocity Test", GUILayout.Height(25)))
        {
            SpawnHighVelocityBall();
        }
        if (GUILayout.Button("Low Velocity Test", GUILayout.Height(25)))
        {
            SpawnLowVelocityBall();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("High Bounce Test", GUILayout.Height(25)))
        {
            SpawnHighBounceBall();
        }
        if (GUILayout.Button("Heavy Ball Test", GUILayout.Height(25)))
        {
            SpawnHeavyBall();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Low Gravity Test", GUILayout.Height(25)))
        {
            SpawnLowGravityBall();
        }
        if (GUILayout.Button("High Gravity Test", GUILayout.Height(25)))
        {
            SpawnHighGravityBall();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Management buttons
        EditorGUILayout.LabelField("Management", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Balls", GUILayout.Height(25)))
        {
            ClearAllBalls();
        }
        if (GUILayout.Button("Reset Spawn System", GUILayout.Height(25)))
        {
            ResetSpawnSystem();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Statistics
        EditorGUILayout.LabelField("Statistics", EditorStyles.miniBoldLabel);

        if (Application.isPlaying)
        {
            string stats = spawnerController.GetSpawnStatistics();
            EditorGUILayout.TextArea(stats, GUILayout.Height(60));
        }
        else
        {
            EditorGUILayout.HelpBox("Statistics available only during play mode", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // Global controls
        EditorGUILayout.LabelField("Global Controls", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable Spawning"))
        {
            SetGlobalSpawnEnabled(true);
        }
        if (GUILayout.Button("Disable Spawning"))
        {
            SetGlobalSpawnEnabled(false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Random Spawning"))
        {
            StartRandomSpawning();
        }
        if (GUILayout.Button("Stop Random Spawning"))
        {
            StopRandomSpawning();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Round controls
        EditorGUILayout.LabelField("Round Controls", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Round"))
        {
            StartRound();
        }
        if (GUILayout.Button("End Round"))
        {
            EndRound();
        }
        EditorGUILayout.EndHorizontal();

        // Round status
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Round Active:", GUILayout.Width(80));
            EditorGUILayout.LabelField(spawnerController.isRoundActive ? "Yes" : "No");
            EditorGUILayout.EndHorizontal();

            if (spawnerController.isRoundActive)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Round Progress:", GUILayout.Width(80));
                float progress = spawnerController.GetRoundProgress();
                EditorGUILayout.LabelField($"{progress:P1}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time Remaining:", GUILayout.Width(80));
                float remaining = spawnerController.GetRemainingRoundTime();
                EditorGUILayout.LabelField($"{remaining:F1}s");
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(5);

        // Quick physics presets
        EditorGUILayout.LabelField("Physics Presets", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bouncy Balls"))
        {
            SetBouncyPreset();
        }
        if (GUILayout.Button("Heavy Balls"))
        {
            SetHeavyPreset();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Fast Balls"))
        {
            SetFastPreset();
        }
        if (GUILayout.Button("Slow Balls"))
        {
            SetSlowPreset();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SpawnBall()
    {
        if (Application.isPlaying)
        {
            spawnerController.SpawnBall();
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnMultipleBalls(int count)
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < count; i++)
            {
                spawnerController.SpawnBall();
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnHighVelocityBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with high velocity
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 15f, 1f, 1.2f, 1f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnLowVelocityBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with low velocity
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 1f, 1f, 0.8f, 1f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnHighBounceBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with high bounciness
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 8f, 1f, 2f, 1f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnHeavyBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with heavy mass
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 6f, 5f, 0.9f, 1f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void ClearAllBalls()
    {
        if (Application.isPlaying)
        {
            spawnerController.ClearAllBalls();
        }
        else
        {
            // Clear in edit mode
            BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
            foreach (BallMovement ball in allBalls)
            {
                if (ball.name.StartsWith("Ball_"))
                {
                    DestroyImmediate(ball.gameObject);
                }
            }
            Debug.Log("SpawnerControllerEditor: Cleared all balls (edit mode)");
        }
    }

    private void ResetSpawnSystem()
    {
        if (Application.isPlaying)
        {
            spawnerController.ResetSpawnSystem();
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only reset spawn system during play mode");
        }
    }

    private void SetGlobalSpawnEnabled(bool enabled)
    {
        if (Application.isPlaying)
        {
            spawnerController.SetGlobalSpawnEnabled(enabled);
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only control spawn during play mode");
        }
    }

    private void StartRandomSpawning()
    {
        if (Application.isPlaying)
        {
            spawnerController.SetGlobalSpawnEnabled(true);
            spawnerController.useRandomInterval = true;
            Debug.Log("SpawnerControllerEditor: Started random spawning");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only start spawning during play mode");
        }
    }

    private void StopRandomSpawning()
    {
        if (Application.isPlaying)
        {
            spawnerController.SetGlobalSpawnEnabled(false);
            Debug.Log("SpawnerControllerEditor: Stopped random spawning");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only stop spawning during play mode");
        }
    }

    private void SetBouncyPreset()
    {
        if (Application.isPlaying)
        {
            spawnerController.minBounciness = 1.5f;
            spawnerController.maxBounciness = 2f;
            Debug.Log("SpawnerControllerEditor: Set bouncy ball preset");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only modify settings during play mode");
        }
    }

    private void SetHeavyPreset()
    {
        if (Application.isPlaying)
        {
            spawnerController.minMass = 3f;
            spawnerController.maxMass = 5f;
            Debug.Log("SpawnerControllerEditor: Set heavy ball preset");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only modify settings during play mode");
        }
    }

    private void SetFastPreset()
    {
        if (Application.isPlaying)
        {
            spawnerController.minVelocity = 8f;
            spawnerController.maxVelocity = 15f;
            Debug.Log("SpawnerControllerEditor: Set fast ball preset");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only modify settings during play mode");
        }
    }

    private void SetSlowPreset()
    {
        if (Application.isPlaying)
        {
            spawnerController.minVelocity = 1f;
            spawnerController.maxVelocity = 3f;
            Debug.Log("SpawnerControllerEditor: Set slow ball preset");
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only modify settings during play mode");
        }
    }

    private void SpawnLowGravityBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with low gravity
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 8f, 1f, 1f, 0.2f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void SpawnHighGravityBall()
    {
        if (Application.isPlaying)
        {
            GameObject ball = spawnerController.SpawnBall();
            if (ball != null)
            {
                BallMovement ballMovement = ball.GetComponent<BallMovement>();
                if (ballMovement != null)
                {
                    // Override with high gravity
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    ballMovement.Initialize(ball.transform.position, direction, 8f, 1f, 1f, 2.5f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only spawn balls during play mode");
        }
    }

    private void StartRound()
    {
        if (Application.isPlaying)
        {
            spawnerController.StartRound();
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only start rounds during play mode");
        }
    }

    private void EndRound()
    {
        if (Application.isPlaying)
        {
            spawnerController.EndRound();
        }
        else
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only end rounds during play mode");
        }
    }
}
