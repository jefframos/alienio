using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Tooltip("Number of swallows required to complete this level.")]
    public int swallowRequirement;

    [Tooltip("How much to increment the camera distance when leveling up.")]
    public float cameraDistanceIncrement;

    [Tooltip("How much to increment the hole size when leveling up.")]
    public float holeIncrement;
}

[CreateAssetMenu(menuName = "Game/Game Level Setup")]
public class GameLevelSetup : ScriptableObject
{
    [Tooltip("Array of level configurations.")]
    public float startScale = 1;
    public float startCameraDistance = 10;
    public LevelData[] levels;
}
