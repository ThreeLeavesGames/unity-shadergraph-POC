using UnityEngine;


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(BoidsWorldManagerV1))]
public class BoidsWorldManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoidsWorldManagerV1 script = (BoidsWorldManagerV1)target;

        if(GUILayout.Button("Increase boids"))
        {
            script.IncreaseBoidsByRandomPoints(0,100,5);
        }
        if(GUILayout.Button("Decrease boids"))
        {
            script.DecreaseBoidsByRandomPoints(0,100,5);
        }
    }
}

[UnityEditor.CustomEditor(typeof(BoidsWorldManagerV2))]
public class BoidsWorldManagerV2Editor : UnityEditor.Editor
{
    private int selectedPondIndex = 0;
    private int selectedRankIndex = 0;
    private int boidCountToModify = 10;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoidsWorldManagerV2 script = (BoidsWorldManagerV2)target;
        
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Rank-Based Boid Controls", UnityEditor.EditorStyles.boldLabel);
        
        // Pond selection
        selectedPondIndex = UnityEditor.EditorGUILayout.IntField("Pond Index", selectedPondIndex);
        selectedPondIndex = Mathf.Max(0, selectedPondIndex);
        
        // Rank selection
        selectedRankIndex = UnityEditor.EditorGUILayout.IntField("Rank Index", selectedRankIndex);
        selectedRankIndex = Mathf.Max(0, selectedRankIndex);
        
        // Count to modify
        boidCountToModify = UnityEditor.EditorGUILayout.IntField("Count to Modify", boidCountToModify);
        boidCountToModify = Mathf.Max(1, boidCountToModify);
        
        UnityEditor.EditorGUILayout.Space();
        
        // Display current counts if available
        if (Application.isPlaying && script.boidsMs.Count > selectedPondIndex)
        {
            int[] currentCounts = script.GetRankCounts(selectedPondIndex);
            if (currentCounts.Length > 0)
            {
                UnityEditor.EditorGUILayout.LabelField($"Current Counts: [{string.Join(", ", currentCounts)}]");
                UnityEditor.EditorGUILayout.LabelField($"Total Boids: {script.GetTotalBoidCount(selectedPondIndex)}");
                UnityEditor.EditorGUILayout.Space();
            }
        }
        
        // Rank modification buttons
        UnityEditor.EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button($"Increase Rank {selectedRankIndex}"))
        {
            if (Application.isPlaying)
            {
                script.IncreaseBoidsByRank(selectedPondIndex, selectedRankIndex, boidCountToModify);
            }
        }
        if(GUILayout.Button($"Decrease Rank {selectedRankIndex}"))
        {
            if (Application.isPlaying)
            {
                script.DecreaseBoidsByRank(selectedPondIndex, selectedRankIndex, boidCountToModify);
            }
        }
        UnityEditor.EditorGUILayout.EndHorizontal();
        
        UnityEditor.EditorGUILayout.Space();
        
        // Quick preset buttons
        UnityEditor.EditorGUILayout.LabelField("Quick Presets", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Balanced\n[80, 15, 5]"))
        {
            if (Application.isPlaying)
            {
                script.SetRankCounts(selectedPondIndex, new int[]{80, 15, 5});
            }
        }
        if(GUILayout.Button("Many Low\n[150, 10, 2]"))
        {
            if (Application.isPlaying)
            {
                script.SetRankCounts(selectedPondIndex, new int[]{150, 10, 2});
            }
        }
        if(GUILayout.Button("Pyramid\n[100, 50, 25]"))
        {
            if (Application.isPlaying)
            {
                script.SetRankCounts(selectedPondIndex, new int[]{100, 50, 25});
            }
        }
        UnityEditor.EditorGUILayout.EndHorizontal();
        
        // Boundary debugging section
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Boundary Debug Controls", UnityEditor.EditorStyles.boldLabel);
        
        if (Application.isPlaying && script.boidsMs.Count > selectedPondIndex)
        {
            var boidObject = script.boidsMs[selectedPondIndex];
            if (boidObject.boidGameObject != null)
            {
                var boidsManager = boidObject.boidGameObject.GetComponent<BoidsManagerV11>();
                if (boidsManager != null)
                {
                    // Boundary force settings
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    boidsManager.boundaryTurnForce = UnityEditor.EditorGUILayout.FloatField("Boundary Turn Force", boidsManager.boundaryTurnForce);
                    boidsManager.antiBoundaryForce = UnityEditor.EditorGUILayout.FloatField("Anti-Boundary Force", boidsManager.antiBoundaryForce);
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    
                    // Debug toggles
                    UnityEditor.EditorGUI.BeginChangeCheck();
                    
                    boidsManager.debugBoundaryForces = UnityEditor.EditorGUILayout.Toggle("Debug Boundary Forces", boidsManager.debugBoundaryForces);
                    
                    // Natural movement controls
                    UnityEditor.EditorGUILayout.Space();
                    UnityEditor.EditorGUILayout.LabelField("Natural Movement", UnityEditor.EditorStyles.boldLabel);
                    boidsManager.rotationSmoothness = UnityEditor.EditorGUILayout.FloatField("Rotation Smoothness", boidsManager.rotationSmoothness);
                    boidsManager.maxVelocityChange = UnityEditor.EditorGUILayout.FloatField("Max Velocity Change", boidsManager.maxVelocityChange);
                    
                    // V7 Boundary system info
                    UnityEditor.EditorGUILayout.Space();
                    if (boidsManager.polygonPoints != null)
                        UnityEditor.EditorGUILayout.LabelField($"🟡 Outer Boundary (AVOID): {boidsManager.polygonPoints.Length}");
                    if (boidsManager.antiPolygonPoints != null)
                        UnityEditor.EditorGUILayout.LabelField($"🔴 Inner Obstacles (AVOID): {boidsManager.antiPolygonPoints.Length}");
                    
                    UnityEditor.EditorGUILayout.Space();
                    UnityEditor.EditorGUILayout.HelpBox("✅ V7 Working Boundary System:\n🟡 Yellow = Outer boundary (repel outward)\n🔴 Red = Inner obstacles (repel away)\nBoth use inverse-square force falloff\nBoids stay between outer and inner boundaries", UnityEditor.MessageType.Info);
                }
            }
        }
        
        if (!Application.isPlaying)
        {
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.HelpBox("Rank controls and boundary debugging only work in Play Mode", UnityEditor.MessageType.Info);
        }
    }
}
#endif
