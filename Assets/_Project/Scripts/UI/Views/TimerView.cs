using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerLabel;

    public void UpdateTimer(float timeLeft)
    {
        // Update the timer label with the remaining time formatted as minutes:seconds
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        timerLabel.text = string.Format("{0:D1}:{1:D2}", minutes, seconds);
    }
}
