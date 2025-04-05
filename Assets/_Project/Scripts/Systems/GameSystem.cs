using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField]
    private PlayerLevelManager playerLevelManager;

    [SerializeField]
    private HoleController holeController;

    [SerializeField]
    private RevealSystem revealSystem;

    [SerializeField]
    private HoleMover holeMover;

    [SerializeField]
    private CinemachineManager cinemachineManager;

    [SerializeField]
    private CameraDistanceSetter cameraDistanceSetter;

    [SerializeField]
    private UISystem uiSystem;

    [SerializeField]
    private Stomach stomach;

    [Header("Gameplay Settings")]
    [SerializeField]
    private float maxHight = 1000f;

    [SerializeField]
    private float gameTimerDuration = 60f;

    // A running total of the reveal tower height.
    private float revealTowerHeight = 0f;

    // Current timer value
    private float currentTimer = 0f;

    public string spawnerTag = "Spawner";

    void Awake()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag(spawnerTag);
        if (spawners.Length > 0)
        {
            // Pick a random spawner.
            int randomIndex = UnityEngine.Random.Range(0, spawners.Length);
            Transform chosenSpawner = spawners[randomIndex].transform;
            holeController.transform.position = chosenSpawner.position;
        }
    }

    void Start()
    {
        uiSystem.HideAll();
        _ = StartAsync();
        cinemachineManager.SnapCamera(true);

        SoundManager.Instance.PlayUniqueAmbientSong("music1", 0.1f);
        SoundManager.Instance.PlayUniqueAmbientSong("city", 0.1f);
    }

    async Task StartAsync()
    {
        uiSystem.tutorialDismissed = true;
        uiSystem.ShowTutorialPanel();

        await TransitionManager.Instance.FadeOutOnStartAsync();

        uiSystem.tutorialDismissed = false;
        uiSystem.OnGameStart += StartGame;
        uiSystem.ShowJoystick();
        //await UniTask.Delay(500); // Wait for 1 second before showing the UI

        UpdateUI();
    }

    private void Reload()
    {
        _ = ReloadAsync();
    }

    //redirects to the main menu
    private async Task ReloadAsync()
    {
        await TransitionManager.Instance.TransitionToSceneAsync("MainMenuScene");
    }

    private void OnEnable()
    {
        holeController.OnSwallow += OnSwallowHandler;
        uiSystem.OnPlayAgain += Reload;
    }

    private void OnDisable()
    {
        holeController.OnSwallow -= OnSwallowHandler;
        uiSystem.OnPlayAgain -= Reload;
        uiSystem.OnGameStart -= StartGame;
    }

    public void StartGame()
    {
        uiSystem.tutorialDismissed = true;
        SoundManager.Instance.PlayUnique("start", 0.1f, 1f);
        cinemachineManager.SnapCamera(true);

        uiSystem.OnGameStart -= StartGame;

        holeMover.gameObject.SetActive(true);

        uiSystem.ShowTimer();

        uiSystem.HideTutorialPanel();

        // Hide gameplay HUD and remove joystick.
        uiSystem.ShowGameplayHUD();
        // uiSystem.ShowJoystick();

        // Show the vertical counter meter.
        uiSystem.HideCounterMeter();

        // Start the game timer.
        StartGameTimer().Forget();
    }

    private async UniTaskVoid StartGameTimer()
    {
        currentTimer = 0f;
        while (gameTimerDuration <= 0f || currentTimer < gameTimerDuration)
        {
            uiSystem.UpdateTimer(gameTimerDuration - currentTimer);
            await UniTask.Yield();
            currentTimer += Time.deltaTime;
        }
        EndGame();
    }

    public void EndGame()
    {
        holeMover.gameObject.SetActive(false);
        // Hide gameplay HUD and remove joystick.
        uiSystem.HideGameplayHUD();
        uiSystem.HideJoystick();
        uiSystem.HideTimer();

        // Show the vertical counter meter.
        uiSystem.ShowCounterMeter();

        holeController.StopMoving();

        SoundManager.Instance.StopAmbientSong("city");
        // Start the reveal tower sequence.
        _ = StartRevealTowerAsync();
    }

    private void OnSwallowHandler(SwalllowableEntity swallowable)
    {
        // Add the swallowed entity to the stomach.
        if (stomach != null)
        {
            Handheld.Vibrate();
            SoundManager.Instance.PlayUnique(
                new string[] { "squash_1", "squash_2" },
                0.2f + UnityEngine.Random.value * 0.1f,
                0.5f + UnityEngine.Random.value * 0.3f
            );

            if (UnityEngine.Random.value > 0.5)
            {
                SoundManager.Instance.PlayUnique(
                    new string[] { "monster_1", "monster_2" },
                    0.2f + UnityEngine.Random.value * 0.1f,
                    0.5f + UnityEngine.Random.value * 0.3f
                );
            }
            if (UnityEngine.Random.value > 0.5)
            {
                SoundManager.Instance.PlayUnique(
                    "burp",
                    0.1f + UnityEngine.Random.value * 0.1f,
                    0.5f + UnityEngine.Random.value * 0.3f
                );
            }

            ParticleManager
                .Instance.PlayParticle("swallow", Vector3.zero, holeController.transform)
                .transform.localScale = holeController.transform.localScale;

            stomach.AddSwallow(swallowable);
        }

        // Update the player's swallow count.
        playerLevelManager.AddSwallow();
        UpdateUI();
    }

    public async Task StartRevealTowerAsync()
    {
        uiSystem.UpdateCounterMeter(0f, maxHight);
        revealTowerHeight = 0f;
        cinemachineManager.SetToReveal();

        await revealSystem.RevealTowerAsync(OnPieceVomit, OnRevealFinished);
        await UniTask.Delay(1000);

        var percentage = Mathf.Clamp01(revealTowerHeight / maxHight);
        var time = 6f * percentage;
        revealSystem.TransitionToRevealTower(time);

        await uiSystem.UpdateCounterMeterAsync(0, percentage, time, maxHight);

        uiSystem.ShowEndgame();
        Debug.Log("Reveal tower process started.");
    }

    private void OnRevealFinished(GameObject @object, float totalHeight)
    {
        Debug.Log($"Reveal finished: {@object.name}, total height: {revealTowerHeight}");
    }

    //not really being used, but could be useful in the future
    private void OnPieceVomit(GameObject vomitedPiece, float pieceHeight)
    {
        revealTowerHeight += pieceHeight;
        //uiSystem.UpdateCounterMeter(revealTowerHeight / maxHight, maxHight);
        Debug.Log(
            $"Piece vomited: {vomitedPiece.name}, height: {pieceHeight}, total height: {revealTowerHeight}"
        );
    }

    private void UpdateUI()
    {
        uiSystem.OnLevelUpdated(
            playerLevelManager.currentLevel,
            playerLevelManager.currentSwallowCount,
            playerLevelManager.RequiredSwallows
        );
    }
}
