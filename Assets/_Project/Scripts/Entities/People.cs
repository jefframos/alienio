using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class People : MonoBehaviour, ICollidable
{
    public float speed = 2f;

    public float wanderChangeInterval = 3f;

    public Vector3 fixedDirection = Vector3.forward;

    public float sensorDistance = 1f;

    public Vector3 sensorBoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    public UnityEvent OnWalk;
    public UnityEvent OnIdle;

    // Internal state for wandering.
    private Vector3 currentWanderDirection;
    private float wanderTimer = 0f;

    public event System.Action OnDisableMove;

    // Animator reference to control animation state.
    [SerializeField]
    private Animator animator;

    private bool movementEnabled = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentWanderDirection = Random.insideUnitSphere;
        currentWanderDirection.y = 0;
        currentWanderDirection.Normalize();
    }

    public Collider GetCollider()
    {
        return GetComponent<Collider>();
    }

    private bool IsSomethingAhead(List<ICollidable> collidables)
    {
        foreach (ICollidable c in collidables)
        {
            if (c == this)
                continue;
            Vector3 dirToOther = c.GetCollider().bounds.center - transform.position;
            // Consider an object “ahead” if it is nearly in front (dot > 0.8)
            // and within sensorDistance.
            if (
                Vector3.Dot(transform.forward, dirToOther.normalized) > 0.8f
                && dirToOther.magnitude < sensorDistance
            )
            {
                return true;
            }
        }
        return false;
    }

    public void Move(List<ICollidable> collidables, Collider[] areas)
    {
        if (!movementEnabled)
            return;

        // If a car or another person is ahead, stop.
        if (IsSomethingAhead(collidables))
        {
            SetIdle();
            return;
        }

        bool insideArea = false;
        Collider currentArea = null;
        if (areas != null)
        {
            foreach (Collider area in areas)
            {
                if (area != null && area.bounds.Contains(transform.position))
                {
                    insideArea = true;
                    currentArea = area;
                    break;
                }
            }
        }

        if (insideArea)
        {
            // Wander
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderChangeInterval)
            {
                currentWanderDirection = Random.insideUnitSphere;
                currentWanderDirection.y = 0;
                currentWanderDirection.Normalize();
                wanderTimer = 0f;
            }
            // Always face the direction of wander.
            transform.rotation = Quaternion.LookRotation(currentWanderDirection);

            Vector3 proposedPos =
                transform.position + currentWanderDirection * speed * Time.deltaTime;
            if (currentArea != null && currentArea.bounds.Contains(proposedPos))
            {
                SetWalking();
                transform.position = proposedPos;
            }
            else
            {
                SetIdle();
            }
        }
        else
        {
            // Outside any area: walk in the fixed direction.
            transform.rotation = Quaternion.LookRotation(fixedDirection.normalized);
            SetWalking();
            transform.position += fixedDirection.normalized * speed * Time.deltaTime;
        }
    }

    private void SetWalking()
    {
        if (!movementEnabled)
            return;

        OnWalk?.Invoke();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("isWalking", true);
        }
    }

    private void SetIdle()
    {
        if (!movementEnabled)
            return;

        OnIdle?.Invoke();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("isWalking", false);
        }
    }

    public void DisableMovement()
    {
        movementEnabled = false;
        OnDisableMove?.Invoke();

        if (animator != null)
        {
            // Snap to idle state at time 0.
            animator.Play("Idle", 0, 0);
            animator.speed = 0f;
        }
    }

    public void EnableMovement()
    {
        movementEnabled = true;
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }
}
