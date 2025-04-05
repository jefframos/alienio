using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RevealSystem : MonoBehaviour
{
    public GameObject cityGameObject;

    public GameObject groundRevealPlane;

    public HoleController holeController;

    public Stomach stomach;

    public GameObject monster;

    public float vomitInterval = 0.5f;

    public float vomitForce = 50f;

    public async UniTask RevealAsync(Action<GameObject, float> onPieceVomit, Action onFinished)
    {
        // Hide the city and show the ground reveal plane.
        if (cityGameObject != null)
            cityGameObject.SetActive(false);
        // if (groundRevealPlane != null)
        //     groundRevealPlane.SetActive(true);

        // Shrink the hole to its first stage.
        if (holeController != null)
        {
            // Assume the HoleController has an async method to shrink the hole.
            holeController.SetHoleSize(1);
        }

        // Access the stomach and call its vomit function.
        if (stomach != null)
        {
            // Vomit the swallowed entities piece by piece.
            await stomach.VomitPiecesAsync(vomitForce, vomitInterval, onPieceVomit, onFinished);
        }
    }

    public async UniTask RevealTowerAsync(
        Action<GameObject, float> onPieceVomit,
        Action<GameObject, float> onFinished
    )
    {
        // Hide the city and show the ground reveal plane.
        if (cityGameObject != null)
            cityGameObject.SetActive(false);
        if (groundRevealPlane != null)
            groundRevealPlane.SetActive(true);

        // Shrink the hole to its first stage.
        if (holeController != null)
        {
            // Assume the HoleController has an async method to shrink the hole.
            holeController.SetHoleSize(1);
        }

        // Access the stomach and call its vomit function.
        if (stomach != null)
        {
            // Vomit the swallowed entities piece by piece.
            await stomach.VomitPieceTowerAsync(vomitInterval, onPieceVomit, onFinished);
        }
    }

    internal void TransitionToRevealTower(float timer)
    {
        stomach.TransitionCameraToRevealTower(timer);
    }
}
