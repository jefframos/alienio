using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerLevelManager))]
public class PlayerLevelManagerEditor : Editor
{
    // Field to store the level to simulate (0-indexed)
    private int simulationLevel = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerLevelManager levelManager = (PlayerLevelManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Simulation", EditorStyles.boldLabel);

        // Input field for simulation level
        simulationLevel = EditorGUILayout.IntField("Simulation Level (0-indexed)", simulationLevel);

        if (GUILayout.Button("Simulate and Apply Level"))
        {
            levelManager.SimulateAndApplyLevel(simulationLevel);
        }
        if (GUILayout.Button("Apply Start Data"))
        {
            levelManager.ApplyStartData();
        }
    }
}
