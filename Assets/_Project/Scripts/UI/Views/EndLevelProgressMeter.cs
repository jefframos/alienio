using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelProgressMeter : MonoBehaviour
{
    [SerializeField]
    private Image fillBar;

    [SerializeField]
    private TextMeshProUGUI progressText;

    [SerializeField]
    private RectTransform marker;

    [SerializeField]
    private TextMeshProUGUI markerText;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    void Awake()
    {
        finalScoreText.transform.localScale = Vector3.zero;
    }

    public void UpdateProgress(float progress, float maxMeters)
    {
        finalScoreText.transform.localScale = Vector3.zero;
        // Clamp progress between 0 and 1.
        progress = Mathf.Clamp01(progress);

        // Update the fill amount.
        if (fillBar != null)
        {
            fillBar.fillAmount = progress;
        }

        // Update the overall progress text (e.g., as a percentage).
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }

        // Update the marker position along the fill bar.
        if (marker != null && fillBar != null)
        {
            RectTransform fillRect = fillBar.rectTransform;
            float barHeight = fillRect.rect.height;
            // Calculate the vertical offset accounting for the pivot.
            float offset = barHeight * progress - (barHeight * fillRect.pivot.y);
            Vector2 newMarkerPos = marker.anchoredPosition;
            newMarkerPos.y = offset;
            marker.anchoredPosition = newMarkerPos;
        }

        // Update the marker's text.
        if (markerText != null)
        {
            marker.gameObject.SetActive(true);
            markerText.text = $"{Mathf.RoundToInt(progress * maxMeters)} m";
        }
        if (finalScoreText != null)
        {
            finalScoreText.text = markerText.text;
        }
    }

    public async UniTask TweenProgressAsync(float from, float to, float duration, float maxMeters)
    {
        finalScoreText.transform.localScale = Vector3.zero;
        float elapsed = 0f;
        float beepTimer = 0f; // Accumulates time for playing beep sound

        while (elapsed < duration)
        {
            // Lerp the progress value between "from" and "to".
            float progress = Mathf.Lerp(from, to, elapsed / duration);
            UpdateProgress(progress, maxMeters);

            // Accumulate time for beep
            float delta = Time.deltaTime;
            elapsed += delta;
            beepTimer += delta;

            // Every 250 milliseconds, play the beep uniquely.
            if (beepTimer >= 0.25f)
            {
                SoundManager.Instance.PlaySound(
                    "coin",
                    0.2f + UnityEngine.Random.value * 0.3f,
                    0.9f + UnityEngine.Random.value * 0.2f
                );
                beepTimer = 0f;
            }

            // Yield until next frame.
            await UniTask.Yield();
        }

        // After the tween, play whoosh and applause sounds.
        SoundManager.Instance.PlayUnique(
            "whoosh",
            0.6f + UnityEngine.Random.value * 0.1f,
            0.5f + UnityEngine.Random.value * 0.3f
        );
        SoundManager.Instance.PlayUnique("applause", 0.6f);

        // Ensure final value is set.
        UpdateProgress(to, maxMeters);
        marker.gameObject.SetActive(false);
        finalScoreText
            .transform.DOScale(Vector3.one, 0.65f)
            .SetEase(Ease.OutBack)
            .From(Vector3.zero)
            .SetDelay(0.6f);
    }
}
