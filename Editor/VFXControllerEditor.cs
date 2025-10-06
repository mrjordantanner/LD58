using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VFX))]
public class VFXControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Add some space
        EditorGUILayout.Space();
        
        // Add a horizontal line
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Add test section header
        EditorGUILayout.LabelField("Test Effects", EditorStyles.boldLabel);
        
        // Floating Text Test
        EditorGUILayout.LabelField("Floating Text", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("Test Floating Text Above Player", GUILayout.Height(25)))
        {
            TestFloatingText();
        }
        
        EditorGUILayout.Space(5);
        
        // Screen Flash Tests
        EditorGUILayout.LabelField("Screen Flash", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Success Flash", GUILayout.Height(25)))
        {
            TestSuccessFlash();
        }
        if (GUILayout.Button("Test Alert Flash", GUILayout.Height(25)))
        {
            TestAlertFlash();
        }
        EditorGUILayout.EndHorizontal();
        
        // Add help box
        EditorGUILayout.HelpBox("These tests only work in Play Mode. Success flash is quick, Alert flash holds for 1 second.", MessageType.Info);
    }
    
    private void TestFloatingText()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Test Floating Text", 
                "This test only works in Play Mode. Please start the game first.", 
                "OK");
            return;
        }
        
        // Find the player character
        PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
        if (playerCharacter == null)
        {
            EditorUtility.DisplayDialog("Test Floating Text", 
                "PlayerCharacter not found in the scene. Make sure the player is spawned.", 
                "OK");
            return;
        }
        
        // Get the VFX instance
        VFX vfxInstance = (VFX)target;
        if (vfxInstance == null)
        {
            EditorUtility.DisplayDialog("Test Floating Text", 
                "VFX instance not found.", 
                "OK");
            return;
        }
        
        // Calculate position above player
        Vector3 playerPosition = playerCharacter.transform.position;
        Vector3 floatingTextPosition = playerPosition + Vector3.up * 2f; // 2 units above player
        
        // Create test floating text
        vfxInstance.CreateFloatingText(
            "TEST FLOATING TEXT!", 
            floatingTextPosition, 
            Color.yellow, 
            2f, // duration
            1.5f // scale
        );
        
        Debug.Log("VFX Editor: Created test floating text above player at position: " + floatingTextPosition);
    }
    
    private void TestSuccessFlash()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Test Success Flash", 
                "This test only works in Play Mode. Please start the game first.", 
                "OK");
            return;
        }
        
        // Get the VFX instance
        VFX vfxInstance = (VFX)target;
        if (vfxInstance == null)
        {
            EditorUtility.DisplayDialog("Test Success Flash", 
                "VFX instance not found.", 
                "OK");
            return;
        }
        
        // Trigger success flash
        vfxInstance.FlashSuccessColor();
        Debug.Log("VFX Editor: Triggered test success flash");
    }
    
    private void TestAlertFlash()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Test Alert Flash", 
                "This test only works in Play Mode. Please start the game first.", 
                "OK");
            return;
        }
        
        // Get the VFX instance
        VFX vfxInstance = (VFX)target;
        if (vfxInstance == null)
        {
            EditorUtility.DisplayDialog("Test Alert Flash", 
                "VFX instance not found.", 
                "OK");
            return;
        }
        
        // Trigger alert flash
        vfxInstance.FlashAlertColor();
        Debug.Log("VFX Editor: Triggered test alert flash");
    }
}
