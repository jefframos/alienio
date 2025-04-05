using UnityEngine;

public class FloatingUIElement : MonoBehaviour
{
    [Header("Vertical Float Settings")]
    [Tooltip("Vertical movement amplitude in pixels.")]
    [SerializeField]
    private float floatAmplitude = 10f;

    [Tooltip("Speed of vertical float motion.")]
    [SerializeField]
    private float floatSpeed = 1f;

    [Header("Rotation Settings")]
    [Tooltip("Maximum rotation in degrees.")]
    [SerializeField]
    private float rotationAmplitude = 5f;

    [Tooltip("Speed of rotation motion.")]
    [SerializeField]
    private float rotationSpeed = 1f;

    // Cache the starting local position and rotation.
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Start()
    {
        // Save the initial transform values.
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        // Calculate the vertical offset using a sine wave.
        float verticalOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = initialLocalPosition + new Vector3(0, verticalOffset, 0);

        // Calculate a slight rotation offset.
        float zRotation = Mathf.Sin(Time.time * rotationSpeed) * rotationAmplitude;
        transform.localRotation = initialLocalRotation * Quaternion.Euler(0, 0, zRotation);
    }
}
