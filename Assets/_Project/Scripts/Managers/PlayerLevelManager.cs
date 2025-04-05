using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLevelManager : MonoBehaviour
{
    public GameLevelSetup gameLevelSetup;

    [SerializeField]
    private HoleController holeController;

    [SerializeField]
    private CameraDistanceSetter cameraDistanceSetter;

    public int currentLevel = 0;

    public int currentSwallowCount = 0;

    // Property to return the required swallows for the current level.
    public int RequiredSwallows
    {
        get
        {
            if (gameLevelSetup == null || gameLevelSetup.levels.Length == 0)
                return 0;
            LevelData levelData = gameLevelSetup.levels[
                Mathf.Clamp(currentLevel, 0, gameLevelSetup.levels.Length - 1)
            ];
            return levelData.swallowRequirement;
        }
    }

    // Property to return the number of swallows still needed to level up.
    public int RelativeSwallowCount => Mathf.Max(RequiredSwallows - currentSwallowCount, 0);

    public Action<int, float> OnLevelProgressUpdated;

    public void AddSwallow()
    {
        if (gameLevelSetup == null || gameLevelSetup.levels.Length == 0)
            return;

        // Ensure currentLevel does not exceed available level data.
        currentLevel = Mathf.Clamp(currentLevel, 0, gameLevelSetup.levels.Length - 1);

        currentSwallowCount++;

        LevelData levelData = gameLevelSetup.levels[currentLevel];

        // Check if we've met or exceeded the requirement.
        if (currentSwallowCount >= levelData.swallowRequirement)
        {
            LevelUp();
        }

        OnLevelProgressUpdated?.Invoke(currentLevel, GetNormalizedProgress());
    }

    public float GetNormalizedProgress()
    {
        if (gameLevelSetup == null || gameLevelSetup.levels.Length == 0)
            return 0f;

        LevelData levelData = gameLevelSetup.levels[
            Mathf.Clamp(currentLevel, 0, gameLevelSetup.levels.Length - 1)
        ];
        return Mathf.Clamp01(currentSwallowCount / (float)levelData.swallowRequirement);
    }

    private void LevelUp()
    {
        currentSwallowCount = 0;
        currentLevel++;

        SoundManager.Instance.PlayUnique("levelUp", 0.1f);

        SimulateAndApplyLevel(currentLevel);

        if (currentLevel >= gameLevelSetup.levels.Length)
        {
            currentLevel = gameLevelSetup.levels.Length - 1;
        }
    }

    public void ApplyStartData()
    {
        if (cameraDistanceSetter != null)
        {
            cameraDistanceSetter.SnapDistance(gameLevelSetup.startCameraDistance);
        }
        if (holeController != null)
        {
            holeController.SetHoleSize(gameLevelSetup.startScale);
        }
    }

    public void SimulateAndApplyLevel(int targetLevel)
    {
        if (
            gameLevelSetup == null
            || gameLevelSetup.levels == null
            || gameLevelSetup.levels.Length <= targetLevel
        )
        {
            Debug.LogWarning("Invalid target level for simulation.");
            return;
        }

        int cumulativeSwallows = 0;
        float cumulativeCameraDistance = 0f;
        float cumulativeHoleIncrement = 0f;

        for (int i = 0; i <= targetLevel; i++)
        {
            LevelData levelData = gameLevelSetup.levels[i];
            cumulativeSwallows += levelData.swallowRequirement;
            cumulativeCameraDistance += levelData.cameraDistanceIncrement;
            cumulativeHoleIncrement += levelData.holeIncrement;
        }

        Debug.Log(
            $"Simulation for Level {targetLevel + 1}:\n"
                + $"Total Swallows Required: {cumulativeSwallows}\n"
                + $"Cumulative Camera Distance: {cumulativeCameraDistance}\n"
                + $"Cumulative Hole Increment: {cumulativeHoleIncrement}"
        );

        if (cameraDistanceSetter != null)
        {
            cameraDistanceSetter.SetDistance(cumulativeCameraDistance);
        }
        if (holeController != null)
        {
            holeController.SetHoleSize(cumulativeHoleIncrement);
        }
    }
}
