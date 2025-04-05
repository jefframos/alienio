#if UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RevealSystem))]
public class RevealSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector.
        DrawDefaultInspector();

        // Get a reference to the RevealSystem.
        RevealSystem revealSystem = (RevealSystem)target;

        // Add a button to trigger the reveal.
        if (GUILayout.Button("Trigger Reveal"))
        {
            // Call RevealAsync with simple debug callbacks.
            revealSystem
                .RevealAsync(
                    (piece, width) =>
                    {
                        Debug.Log($"Vomited piece: {piece.name}, width: {width}");
                    },
                    () =>
                    {
                        Debug.Log("Reveal finished.");
                    }
                )
                .Forget();
        }
        // Add a button to trigger the reveal.
        if (GUILayout.Button("Trigger Reveal Tower"))
        {
            // Call RevealAsync with simple debug callbacks.
            revealSystem
                .RevealTowerAsync(
                    (piece, width) =>
                    {
                        Debug.Log($"Vomited piece: {piece.name}, width: {width}");
                    },
                    (piece, width) =>
                    {
                        Debug.Log("Reveal finished.");
                    }
                )
                .Forget();
        }
    }
}
#endif
