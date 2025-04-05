using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISystem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private PlayerProgressUI playerProgressUI;

    [SerializeField]
    private Canvas joystickUI;

    [SerializeField]
    private EndLevelProgressMeter endLevelProgressMeter;

    [SerializeField]
    private RectTransform endGameContainer;

    [SerializeField]
    private TimerView timerView;

    // Reference for the Play Again button and its action (from previous implementation)
    [SerializeField]
    private Button playAgainButton;
    public Action OnPlayAgain;

    [SerializeField]
    private CanvasGroup tutorialPanel;

    // This Action is invoked when the tutorial panel is tapped and faded out.
    public Action OnGameStart;

    public bool tutorialDismissed;

    private void Awake()
    {
        // Ensure Play Again button is wired.
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() => OnPlayAgain?.Invoke());
        }
    }

    private void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if the tutorial panel is active.
        if (tutorialPanel != null && tutorialPanel.gameObject.activeSelf && !tutorialDismissed)
        {
            //tutorialDismissed = true;
            FadeOutTutorialPanel();
        }
    }

    private void FadeOutTutorialPanel()
    {
        // Fade out over 0.5 seconds.
        tutorialPanel
            .DOFade(0f, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                tutorialPanel.gameObject.SetActive(false);
                OnGameStart?.Invoke();
            });
    }

    public void UpdateTimer(float timerLeft)
    {
        timerView.UpdateTimer(timerLeft);
    }

    public void OnLevelUpdated(int currentLevel, int currentSwallowCount, int requiredSwallows)
    {
        playerProgressUI.UpdateLevel(currentLevel + 1, currentSwallowCount, requiredSwallows);
    }

    internal void HideCounterMeter()
    {
        endLevelProgressMeter.gameObject.SetActive(false);
        endGameContainer.gameObject.SetActive(false);
    }

    internal void HideGameplayHUD()
    {
        playerProgressUI.gameObject.SetActive(false);
    }

    internal void HideJoystick()
    {
        joystickUI.gameObject.SetActive(false);
    }

    internal void ShowCounterMeter()
    {
        endLevelProgressMeter.gameObject.SetActive(true);
    }

    internal void ShowEndgame()
    {
        endGameContainer.gameObject.SetActive(true);
        RectTransform rect = endGameContainer;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -Screen.height);
        rect.localScale = Vector3.zero; // * 0.3f;
        rect.DOAnchorPosY(0, 1.0f).SetEase(Ease.OutBack);
        rect.DOScale(Vector3.one, 1.0f).SetEase(Ease.OutElastic).SetDelay(1f);
    }

    internal void ShowTimer()
    {
        timerView.gameObject.SetActive(true);
    }

    internal void HideTimer()
    {
        timerView.gameObject.SetActive(false);
    }

    internal void ShowGameplayHUD()
    {
        playerProgressUI.gameObject.SetActive(true);
    }

    internal void ShowJoystick()
    {
        joystickUI.gameObject.SetActive(true);
    }

    internal void UpdateCounterMeter(float progress, float maxMeters)
    {
        endLevelProgressMeter.UpdateProgress(progress, maxMeters);
    }

    public async Task UpdateCounterMeterAsync(float from, float to, float duration, float maxMeters)
    {
        await endLevelProgressMeter.TweenProgressAsync(from, to, duration, maxMeters);
    }

    internal void HideAll()
    {
        HideGameplayHUD();
        HideJoystick();
        HideCounterMeter();
        HideTimer();
    }

    public void ShowTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 1f;
            tutorialPanel.gameObject.SetActive(true);
        }
    }

    public void HideTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0f;
            tutorialPanel.gameObject.SetActive(false);
        }
    }
}
