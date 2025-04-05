using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorSpeed : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Animator>().SetFloat("RandomSpeed", Random.Range(0.7f, 1.3f));
    }
}
