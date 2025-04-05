using System;
using System.Collections.Generic;
using UnityEngine;

public class HoleController : MonoBehaviour
{
    public float speed = 10f;
    public float growthFactor = 0.1f;
    public float smoothTime = 0.1f;

    public Action<SwalllowableEntity> OnSwallow;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 velocitySmooth = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;

    [SerializeField]
    private SwallowDetector swallowDetector;

    [SerializeField]
    private HoleView holeView;

    [SerializeField]
    private ScaleLerpComponent scaleLerpComponent;

    [SerializeField]
    private PinchControllerLocal pinchControllerLocal;

    [SerializeField]
    private SphereCollider sphereCollider;

    public float Radius => holeView.transform.lossyScale.x * 0.5f;

    private Rigidbody rb;

    public float footstepInterval = 0.5f;
    private float footstepTimer;

    public float movementThreshold = 0.1f;
    public string[] footstepSounds;

    void OnEnable()
    {
        swallowDetector.OnObjectSwallowed += ConfirmSwallow;
    }

    void OnDisable()
    {
        swallowDetector.OnObjectSwallowed -= ConfirmSwallow;
    }

    private void ConfirmSwallow(GameObject swallowedObject)
    {
        if (swallowedObject.TryGetComponent<SwalllowableEntity>(out var sw))
        {
            OnSwallow?.Invoke(sw);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Initialize footstep timer.
        footstepTimer = footstepInterval;
    }

    public void IncreaseHole(float increment)
    {
        scaleLerpComponent.IncreaseScale(increment);
    }

    public void SetHoleSize(float newSize)
    {
        scaleLerpComponent.SetHoleSize(newSize);
    }

    private void Update()
    {
        sphereCollider.radius = Radius;

        currentVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref velocitySmooth,
            smoothTime
        );

        rb.velocity = currentVelocity;
        pinchControllerLocal.SetSpeed(currentVelocity.magnitude);

        // Only trigger footsteps if moving above a threshold.
        if (currentVelocity.magnitude > movementThreshold)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootsteps();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset the timer if not moving.
            footstepTimer = footstepInterval;
        }
    }

    public void Move(Vector2 input)
    {
        targetVelocity = new Vector3(input.x, 0, input.y) * speed;
        pinchControllerLocal.SetDirection(targetVelocity);
    }

    public void StopMoving()
    {
        targetVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;
        rb.velocity = targetVelocity;
    }

    private void PlayFootsteps()
    {
        SoundManager.Instance.PlaySound(
            footstepSounds,
            0.2f,
            0.3f + UnityEngine.Random.value * 0.2f
        );
    }
}
