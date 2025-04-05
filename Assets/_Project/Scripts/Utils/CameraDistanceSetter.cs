using Cinemachine;
using UnityEngine;

public class CameraDistanceSetter : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private float lerpSpeed = 2f;

    private CinemachineTransposer transposer;
    private Vector3 targetOffset;
    private Vector3 initialDirection;
    private float startDistance;
    private float currentDistance;

    private void Awake()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
            return;
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            Debug.LogError(
                "No CinemachineTransposer found on the Virtual Camera. Make sure your Virtual Camera has a Body component set to Transposer."
            );
            return;
        }

        // Calculate the initial distance and normalized direction from the current follow offset.
        startDistance = transposer.m_FollowOffset.magnitude;
        currentDistance = startDistance;
        initialDirection = transposer.m_FollowOffset.normalized;
        targetOffset = transposer.m_FollowOffset;
    }

    public void IncrementDistance(float deltaDistance)
    {
        currentDistance += deltaDistance;
        // Calculate the new target offset based on the initial direction.
        targetOffset = initialDirection * currentDistance;
    }

    public void SetDistance(float distance)
    {
        currentDistance = startDistance + distance;
        // Calculate the new target offset based on the initial direction.
        targetOffset = initialDirection * currentDistance;
    }

    public void SnapDistance(float distance)
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        // Calculate the new target offset based on the initial direction.
        transposer.m_FollowOffset = initialDirection * distance;
        targetOffset = transposer.m_FollowOffset;
    }

    private void Update()
    {
        if (transposer != null)
        {
            // Smoothly lerp the follow offset to the target offset.
            transposer.m_FollowOffset = Vector3.Lerp(
                transposer.m_FollowOffset,
                targetOffset,
                Time.deltaTime * lerpSpeed
            );
        }
    }
}
