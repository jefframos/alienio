using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Stomach : MonoBehaviour
{
    [SerializeField]
    private LayerMask vomitLayer;

    [SerializeField]
    private LayerMask holeLayer;

    [SerializeField]
    private Transform tipTransform; // The tip of the stomach where pieces are vomited from.

    [SerializeField]
    private Transform cameraTarget;

    [SerializeField]
    private Transform contentContainer;

    private float totalHeight = 0f;
    private float targetY = 0f;

    // List of swallowed entities.
    public List<SwalllowableEntity> swallowedEntities = new List<SwalllowableEntity>();

    // Dictionary to store each entity's measured height.
    private Dictionary<SwalllowableEntity, float> entityHeights =
        new Dictionary<SwalllowableEntity, float>();

    // List of pieces that have been vomited (the tower).
    public List<GameObject> revealedPieces = new List<GameObject>();

    // Helper: Convert a LayerMask to its layer number.

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layerValue = mask.value;
        int layerNumber = 0;
        while (layerValue > 1)
        {
            layerValue = layerValue >> 1;
            layerNumber++;
        }
        return layerNumber;
    }

    public async UniTask VomitPiecesAsync(
        float vomitForceMagnitude,
        float interval,
        Action<GameObject, float> onPieceVomit,
        Action onFinished
    )
    {
        float totalWidth = 0f;
        // Copy the list so that we can clear the stomach.
        List<SwalllowableEntity> piecesToVomit = new List<SwalllowableEntity>(swallowedEntities);
        swallowedEntities.Clear();

        foreach (var piece in piecesToVomit)
        {
            piece.gameObject.SetActive(true);
            // Detach from the stomach.
            piece.transform.SetParent(null);

            // Enable physics by re-enabling simulation and making it non-kinematic.
            if (piece.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Set layer to vomit layer.
                rb.gameObject.layer = LayerMaskToLayer(vomitLayer);
                rb.isKinematic = false;

                // Calculate a random force direction.
                // Y is fixed to 1; X and Z are random between -1 and 1.
                Vector3 randomDir = new Vector3(
                    UnityEngine.Random.Range(-0.2f, 0.2f),
                    1f,
                    UnityEngine.Random.Range(-0.2f, 0.2f)
                ).normalized;
                rb.AddForce(randomDir * vomitForceMagnitude, ForceMode.Impulse);

                // After 0.5 seconds, reset the layer to default (layer 0) asynchronously.
                ResetLayerAfterDelay(rb.gameObject, 0.5f).Forget();
            }

            // Calculate the width of the piece using its collider.
            float pieceWidth = 0f;
            Collider col = piece.GetComponent<Collider>();
            if (col != null)
            {
                pieceWidth = col.bounds.size.x; // Using X as width.
            }
            totalWidth += pieceWidth;

            // (Optional) Adjust scaling or other properties here if needed.

            // Trigger callback for this vomited piece.
            onPieceVomit?.Invoke(piece.gameObject, pieceWidth);

            // Wait for a defined interval before vomiting the next piece.
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }

        // Invoke the finished callback.
        onFinished?.Invoke();
    }

    void Update()
    {
        contentContainer.transform.localPosition = Vector3.Lerp(
            contentContainer.transform.localPosition,
            new Vector3(0, targetY, 0),
            0.2f
        );

        //transform.localPosition = new Vector3(0, targetY, 0);
    }

    public async UniTask VomitPieceTowerAsync(
        float interval,
        Action<GameObject, float> onPieceVomit,
        Action<GameObject, float> onFinished
    )
    {
        // Copy and clear the swallowed list.
        List<SwalllowableEntity> piecesToVomit = new(swallowedEntities);
        swallowedEntities.Clear();

        contentContainer.transform.localPosition = new Vector3(0, totalHeight, 0); // Reset stomach position.

        // For tower stacking, we assume the pieces are inserted from bottom to top.
        // For this example, we simply add them in the order they come.

        SoundManager.Instance.PlayUnique(
            "burp",
            0.2f + UnityEngine.Random.value * 0.1f,
            0.5f + UnityEngine.Random.value * 0.3f
        );

        await UniTask.Delay(1000);

        SoundManager.Instance.PlayUnique(
            "fart",
            0.3f + UnityEngine.Random.value * 0.1f,
            0.5f + UnityEngine.Random.value * 0.3f
        );
        float segment = totalHeight / piecesToVomit.Count;
        float previousTargetY = 0f;
        int count = 0;

        foreach (var piece in piecesToVomit)
        {
            // Activate and detach the piece.
            piece.gameObject.SetActive(true);

            // Set its layer initially.
            if (piece.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.gameObject.layer = LayerMaskToLayer(vomitLayer);
                rb.isKinematic = true; // keep kinematic for precise placement.
            }
            Collider col = piece.GetComponent<Collider>();
            if (col != null)
            {
                targetY = totalHeight / piecesToVomit.Count * count - totalHeight;
                count++;

                float heightDelta = targetY - previousTargetY;
                previousTargetY = targetY;

                // Invoke callback for this piece.
                onPieceVomit?.Invoke(piece.gameObject, Math.Abs(heightDelta));

                // Wait for the defined interval before processing the next piece.
                SoundManager.Instance.PlayUnique(
                    "bubbles",
                    0.6f + UnityEngine.Random.value * 0.1f,
                    0.5f + count / piecesToVomit.Count
                );
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
            }
            onFinished?.Invoke(tipTransform.gameObject, targetY);
            targetY = 0f;
        }
        SoundManager.Instance.PlayUnique(
            "punchline",
            0.3f + UnityEngine.Random.value * 0.1f,
            0.5f + UnityEngine.Random.value * 0.3f
        );
    }

    private async UniTaskVoid ResetLayerAfterDelay(GameObject obj, float delaySeconds)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
        if (obj != null)
        {
            obj.layer = LayerMaskToLayer(holeLayer); // Default layer.
        }
    }

    public void AddSwallow(SwalllowableEntity entity)
    {
        // Disable movement if the entity has an ICollidable component.
        ICollidable collidable = entity.GetComponent<ICollidable>();
        collidable?.DisableMovement();

        // Disable physics on the entity.
        if (entity.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.gameObject.layer = LayerMaskToLayer(vomitLayer);
        }

        // Change the tag to match the stomach.
        entity.tag = gameObject.tag;

        // Parent the entity to the stomach and reset its rotation.
        entity.transform.SetParent(contentContainer.transform);
        entity.transform.rotation = Quaternion.identity;

        // Temporarily position it at (0,0,0).
        entity.transform.localPosition = Vector3.zero;

        // Add the entity.
        swallowedEntities.Insert(0, entity);

        // Recalculate and update the stacking positions.
        float entitySize = CalculateEntitySize(entity);
        totalHeight += entitySize;

        entity.gameObject.SetActive(false);

        tipTransform.localPosition = new Vector3(0f, totalHeight, 0f);
    }

    private float CalculateEntitySize(SwalllowableEntity entity)
    {
        float effectiveHeight = 0f;
        float bottomOffset = 0f;
        Collider col = entity.GetComponent<Collider>();
        if (col != null)
        {
            effectiveHeight = col.bounds.size.y;
            if (col is BoxCollider box)
            {
                bottomOffset = box.center.y - box.size.y / 2f / entity.transform.localScale.y;
            }
            else if (col is CapsuleCollider capsule)
            {
                bottomOffset =
                    capsule.center.y - capsule.height / 2f / entity.transform.localScale.y;
            }
            else
            {
                // Fallback: assume the pivot is at the center.
                bottomOffset = -effectiveHeight / 2f;
            }
        }
        float newY = totalHeight - bottomOffset;
        entity.transform.localPosition = new Vector3(0f, newY, 0f);

        float heightAdded = newY + effectiveHeight - totalHeight;
        return heightAdded;
    }

    internal void TransitionCameraToRevealTower(float timer)
    {
        cameraTarget.DOLocalMoveY(tipTransform.localPosition.y, timer, false).SetEase(Ease.OutQuad);
    }
}
