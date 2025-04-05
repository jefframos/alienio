using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlePoolItem : MonoBehaviour
{
    [HideInInspector]
    public string poolId;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void PlayAndReturn()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
        StartCoroutine(WaitAndReturn());
    }

    private IEnumerator WaitAndReturn()
    {
        while (ps.IsAlive(true))
        {
            yield return null;
        }
        ParticleManager.Instance.ReturnToPool(poolId, gameObject);
    }
}
