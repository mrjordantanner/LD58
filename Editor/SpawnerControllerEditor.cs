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


        EditorGUILayout.Space(5);

        // VFX Debugging
        EditorGUILayout.LabelField("VFX Debugging", EditorStyles.miniBoldLabel);

        // VFX object warning
        if (spawnerController.BallSpawnVFXObject == null)
        {
            EditorGUILayout.HelpBox("BallSpawnVFXObject is not assigned! Please assign a VFX prefab.", MessageType.Warning);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug VFX Spawning", GUILayout.Height(25)))
        {
            DebugVFXSpawning();
        }
        if (GUILayout.Button("Test Complete Round Flow", GUILayout.Height(25)))
        {
            TestCompleteRoundFlow();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test VFX Only", GUILayout.Height(25)))
        {
            TestVFXOnly();
        }
        if (GUILayout.Button("Test VFX Position", GUILayout.Height(25)))
        {
            TestVFXPosition();
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
                EditorGUILayout.LabelField("Round Timer:", GUILayout.Width(80));
                float timer = spawnerController.GetRoundTimer();
                EditorGUILayout.LabelField($"{timer:F1}s");
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

    private void DebugVFXSpawning()
    {
        Debug.Log("=== VFX SPAWNING DEBUG ===");
        
        // Check VFX object assignment
        if (spawnerController.BallSpawnVFXObject == null)
        {
            Debug.LogError("❌ BallSpawnVFXObject is NULL! Please assign a VFX prefab in the inspector.");
            return;
        }
        else
        {
            Debug.Log($"✅ BallSpawnVFXObject assigned: {spawnerController.BallSpawnVFXObject.name}");
        }
        
        // Check play area
        if (spawnerController.playAreaCollider == null)
        {
            Debug.LogError("❌ playAreaCollider is NULL! Please assign a BoxCollider2D in the inspector.");
            return;
        }
        else
        {
            Debug.Log($"✅ playAreaCollider assigned: {spawnerController.playAreaCollider.name}");
        }
        
        // Check ball prefab
        if (spawnerController.ballPrefab == null)
        {
            Debug.LogError("❌ ballPrefab is NULL! Please assign a ball prefab in the inspector.");
            return;
        }
        else
        {
            Debug.Log($"✅ ballPrefab assigned: {spawnerController.ballPrefab.name}");
        }
        
        // Check current parameters
        Debug.Log($"Current Parameters:");
        Debug.Log($"  - Anticipation: {spawnerController.anticipationMinDuration}s - {spawnerController.anticipationMaxDuration}s");
        Debug.Log($"  - VFX Lifespan: {spawnerController.ballSpawnVFXLifespan}s");
        Debug.Log($"  - Velocity: {spawnerController.minVelocity} - {spawnerController.maxVelocity}");
        Debug.Log($"  - Round Active: {spawnerController.isRoundActive}");
        
        // Test VFX instantiation
        Vector2 testPosition = GetRandomSpawnPosition();
        Debug.Log($"Testing VFX at position: {testPosition}");
        
        GameObject testVFX = Instantiate(spawnerController.BallSpawnVFXObject, testPosition, Quaternion.identity);
        testVFX.name = $"DebugVFX_{Time.time:F1}";
        
        Debug.Log($"✅ VFX instantiated successfully: {testVFX.name}");
        
        // Destroy after 2 seconds for testing
        Destroy(testVFX, 2f);
        
        Debug.Log("=== END VFX DEBUG ===");
    }

    private void TestCompleteRoundFlow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only test round flow during play mode");
            return;
        }

        Debug.Log("=== TESTING COMPLETE ROUND FLOW ===");
        
        if (spawnerController.isRoundActive)
        {
            Debug.LogWarning("Round is already active! Ending current round first.");
            spawnerController.EndRound();
        }
        
        spawnerController.StartCoroutine(DebugRoundFlow());
    }

    private System.Collections.IEnumerator DebugRoundFlow()
    {
        Debug.Log("🚀 Starting debug round flow...");
        
        // Step 1: Choose spawn point
        Vector2 spawnPosition = GetRandomSpawnPosition();
        Debug.Log($"📍 Spawn position selected: {spawnPosition}");
        
        // Step 2: Anticipation pause
        float anticipationDuration = Random.Range(spawnerController.anticipationMinDuration, spawnerController.anticipationMaxDuration);
        Debug.Log($"⏱️ Anticipation pause: {anticipationDuration:F1}s");
        yield return new WaitForSeconds(anticipationDuration);
        
        // Step 3: Spawn VFX
        if (spawnerController.BallSpawnVFXObject != null)
        {
            Debug.Log($"🎆 Spawning VFX at {spawnPosition}");
            GameObject vfx = Instantiate(spawnerController.BallSpawnVFXObject, spawnPosition, Quaternion.identity);
            vfx.name = $"DebugVFX_{Time.time:F1}";
            
            // Check if VFX has any components
           // var components = vfx.GetComponents<Component>();
            //Debug.Log($"VFX Components: {string.Join(", ", components.Select(c => c.GetType().Name))}");
            
            // Check if VFX has any children
            if (vfx.transform.childCount > 0)
            {
                Debug.Log($"VFX has {vfx.transform.childCount} children");
                for (int i = 0; i < vfx.transform.childCount; i++)
                {
                    var child = vfx.transform.GetChild(i);
                    Debug.Log($"  Child {i}: {child.name} (Active: {child.gameObject.activeInHierarchy})");
                }
            }
            
            // Destroy VFX after lifespan
            Destroy(vfx, spawnerController.ballSpawnVFXLifespan);
            Debug.Log($"✅ VFX spawned, will destroy in {spawnerController.ballSpawnVFXLifespan}s");
        }
        else
        {
            Debug.LogError("❌ BallSpawnVFXObject is null!");
        }
        
        // Step 4: Wait for VFX duration
        Debug.Log($"⏳ Waiting {spawnerController.ballSpawnVFXLifespan}s for VFX...");
        yield return new WaitForSeconds(spawnerController.ballSpawnVFXLifespan);
        
        // Step 5: Spawn ball
        Debug.Log($"⚽ Spawning ball at {spawnPosition}");
        GameObject ball = Instantiate(spawnerController.ballPrefab, spawnPosition, Quaternion.identity);
        ball.name = $"DebugBall_{spawnerController.totalBallsSpawned}";
        spawnerController.totalBallsSpawned++;
        
        spawnerController.ConfigureBall(ball);
        Debug.Log($"✅ Ball spawned and configured: {ball.name}");
        
        Debug.Log("🎉 Debug round flow completed!");
    }

    private void TestVFXOnly()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only test VFX during play mode");
            return;
        }

        if (spawnerController.BallSpawnVFXObject == null)
        {
            Debug.LogError("❌ BallSpawnVFXObject is not assigned!");
            return;
        }

        Vector2 testPosition = GetRandomSpawnPosition();
        Debug.Log($"🎆 Testing VFX only at position: {testPosition}");
        
        GameObject vfx = Instantiate(spawnerController.BallSpawnVFXObject, testPosition, Quaternion.identity);
        vfx.name = $"TestVFX_{Time.time:F1}";
        
        // Check VFX components and children
       // var components = vfx.GetComponents<Component>();
       // Debug.Log($"VFX Components: {string.Join(", ", components.Select(c => c.GetType().Name))}");
        
        if (vfx.transform.childCount > 0)
        {
            Debug.Log($"VFX has {vfx.transform.childCount} children");
            for (int i = 0; i < vfx.transform.childCount; i++)
            {
                var child = vfx.transform.GetChild(i);
                Debug.Log($"  Child {i}: {child.name} (Active: {child.gameObject.activeInHierarchy})");
            }
        }
        
        // Destroy after 3 seconds
        Destroy(vfx, 3f);
        Debug.Log("✅ VFX test completed - will destroy in 3 seconds");
    }

    private void TestVFXPosition()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("SpawnerControllerEditor: Can only test VFX position during play mode");
            return;
        }

        if (spawnerController.BallSpawnVFXObject == null)
        {
            Debug.LogError("❌ BallSpawnVFXObject is not assigned!");
            return;
        }

        // Test multiple positions
        for (int i = 0; i < 5; i++)
        {
            Vector2 testPosition = GetRandomSpawnPosition();
            Debug.Log($"🎆 Testing VFX position {i + 1}: {testPosition}");
            
            GameObject vfx = Instantiate(spawnerController.BallSpawnVFXObject, testPosition, Quaternion.identity);
            vfx.name = $"PositionTestVFX_{i + 1}_{Time.time:F1}";
            
            // Destroy after 2 seconds
            Destroy(vfx, 2f);
        }
        
        Debug.Log("✅ VFX position test completed - 5 VFX objects spawned at random positions");
    }

    private Vector2 GetRandomSpawnPosition()
    {
        if (spawnerController.playAreaCollider == null)
        {
            Debug.LogWarning("SpawnerControllerEditor: No play area collider, using origin");
            return Vector2.zero;
        }

        Bounds bounds = spawnerController.playAreaCollider.bounds;
        
        // Add some margin from the edges
        float margin = 0.5f;
        Vector2 min = new Vector2(bounds.min.x + margin, bounds.min.y + margin);
        Vector2 max = new Vector2(bounds.max.x - margin, bounds.max.y - margin);
        
        return new Vector2(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y)
        );
    }
}
