using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwalllowableEntity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public SwallowableData data;

    // Determines if the object can be swallowed by comparing its size to the hole's scale.
    public bool CanBeSwallowed(Vector3 holeScale)
    {
        // Here we compare the magnitudes. You can adjust the logic as needed.
        float objectSize = transform.localScale.magnitude;
        float holeSize = holeScale.magnitude;
        return objectSize <= holeSize * data.swallowThreshold;
    }
}
