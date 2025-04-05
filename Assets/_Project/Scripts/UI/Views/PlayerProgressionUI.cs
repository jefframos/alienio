using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgressUI : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 2f, 0);

    [SerializeField]
    private Image progressBar;

    [SerializeField]
    private Image background;

    [SerializeField]
    private TextMeshProUGUI levelLabel;

    [SerializeField]
    private TextMeshProUGUI progressLabel;

    public Transform markerSpawnPoint;

    public GameObject markerPrefab;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void UpdateLevel(int currentLevel, int currentSwallowCount, int requiredSwallows)
    {
        levelLabel.text = "LVL " + currentLevel;
        progressLabel.text = currentSwallowCount + "/" + requiredSwallows;

        // Calculate normalized progress (ensure requiredSwallows is not zero).
        float normalizedProgress =
            requiredSwallows > 0 ? (float)currentSwallowCount / requiredSwallows : 0;
        progressBar.fillAmount = normalizedProgress;

        // Animate marker from the spawn point into the progress bar fill edge.
        AnimateMarker(normalizedProgress).Forget();
    }

    private async UniTask AnimateMarker(float progress)
    {
        if (markerPrefab == null || markerSpawnPoint == null || progressBar == null || !mainCamera)
            return;

        // Instantiate marker and set its start position (converted from world space).
        GameObject markerInstance = Instantiate(markerPrefab, transform);
        RectTransform markerRect = markerInstance.GetComponent<RectTransform>();
        markerRect.position = mainCamera.WorldToScreenPoint(markerSpawnPoint.position);

        // Calculate the target position on the progress bar.
        RectTransform fillRect = progressBar.rectTransform;
        Vector3 fillScreenPos = fillRect.position;
        float barHeight = fillRect.rect.height;
        Vector3 targetScreenPos = new Vector3(
            fillScreenPos.x,
            fillScreenPos.y - barHeight * (1 - progress),
            fillScreenPos.z
        );

        float duration = 0.5f;

        // Define a path for a curved motion.
        // Start point is the marker's current position.
        Vector3 startPos = markerRect.position;
        // End point is targetScreenPos.
        Vector3 endPos = targetScreenPos;

        // Generate a random midpoint for a bezier-like curve.
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        // Add random offset in X and Y to vary the curve.
        midPoint += new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0f);

        // Build the path array.
        Vector3[] path = new Vector3[] { startPos, midPoint, endPos };

        // Animate using DOTween DOPath with a CatmullRom path type (which gives smooth curves).
        await markerRect
            .DOPath(path, duration, PathType.CatmullRom, PathMode.TopDown2D)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();

        SoundManager.Instance.PlaySound("coin", 0.6f, UnityEngine.Random.Range(0.8f, 1.2f));
        // Ensure marker is at the final position.
        markerRect.position = targetScreenPos;
        Destroy(markerInstance);
    }

    public void ResetBar()
    {
        progressBar.fillAmount = 0f;
        progressLabel.text = "0/0";
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (target != null && mainCamera != null)
        {
            Vector3 worldPosition = target.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            transform.position = Vector3.Lerp(transform.position, screenPosition, 0.05f);
        }
    }
}
