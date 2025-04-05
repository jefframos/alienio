using UnityEngine;

public class ScaleLerpComponent : MonoBehaviour
{
    [SerializeField]
    private float lerpSpeed = 2f;

    private Vector3 targetScale;
    private bool isLerping = false;

    private void Awake()
    {
        targetScale = transform.localScale;
    }

    public void IncreaseScale(float increment)
    {
        targetScale = new Vector3(
            transform.localScale.x + increment,
            transform.localScale.y + increment,
            transform.localScale.z + increment
        );
        isLerping = true;
    }

    public void SetHoleSize(float newSize)
    {
        targetScale = new Vector3(newSize, newSize, newSize);
        isLerping = true;
    }

    private void Update()
    {
        if (isLerping)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * lerpSpeed
            );
            if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            {
                transform.localScale = targetScale;
                isLerping = false;
            }
        }
    }
}
