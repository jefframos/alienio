#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSystem))]
public class GameSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameSystem gameSystem = (GameSystem)target;
        if (GUILayout.Button("End Game"))
        {
            gameSystem.EndGame();
            Debug.Log("EndGame called: Reveal Tower started.");
        }
    }
}
#endif
