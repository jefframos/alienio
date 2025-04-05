using UnityEngine;

public class ImageRotator : MonoBehaviour
{
    public float rotationSpeed = 90f;

    private RectTransform rectTransform;

    [SerializeField]
    private RectTransform target;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        rectTransform.localPosition = target.localPosition;
        rectTransform.localScale = target.localScale;
    }
}
