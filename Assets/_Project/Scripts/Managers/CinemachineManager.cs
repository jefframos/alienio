using System.Threading.Tasks;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CinemachineManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera gameplayCamera;

    [SerializeField]
    private CinemachineVirtualCamera revealCamera;

    [SerializeField]
    private float defaultBlendDuration = 1f;
    private Transform RevealTarget
    {
        get
        {
            if (revealCamera.LookAt != null)
                return revealCamera.LookAt;
            return revealCamera.Follow;
        }
    }

    public void SetToGameplay()
    {
        if (gameplayCamera != null && revealCamera != null)
        {
            gameplayCamera.Priority = 20;
            revealCamera.Priority = 10;
        }
    }

    public void SetToReveal()
    {
        if (gameplayCamera != null && revealCamera != null)
        {
            gameplayCamera.Priority = 10;
            revealCamera.Priority = 20;
        }
    }

    public async UniTask StartRevealTween(Vector3 fromPoint, Vector3 toPoint, float duration)
    {
        Transform target = RevealTarget;
        if (target == null)
        {
            Debug.LogWarning("Reveal camera does not have a valid LookAt or Follow target.");
            return;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Lerp the target's position between fromPoint and toPoint.
            Vector3 newPos = Vector3.Lerp(fromPoint, toPoint, elapsed / duration);
            target.position = newPos;
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        target.position = toPoint;
    }

    public UniTask TransitionCamera(Vector3 fromPoint, Vector3 toPoint)
    {
        return StartRevealTween(fromPoint, toPoint, defaultBlendDuration);
    }

    public void SnapCamera(bool toGameplay)
    {
        if (gameplayCamera == null || revealCamera == null)
            return;

        if (toGameplay)
        {
            gameplayCamera.Priority = 100;
            revealCamera.Priority = 0;
        }
        else
        {
            revealCamera.Priority = 100;
            gameplayCamera.Priority = 0;
        }
    }
}
