using System;
using UnityEngine;

public class SwallowDetector : MonoBehaviour
{
    public Action<GameObject> OnObjectSwallowed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Swallowable"))
        {
            OnObjectSwallowed?.Invoke(other.gameObject);
        }
    }
}
