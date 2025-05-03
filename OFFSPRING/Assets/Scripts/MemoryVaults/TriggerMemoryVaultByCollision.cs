using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerMemoryVaultByCollision : MonoBehaviour
{
    [Header("*Arrastrar prefab de CollectableParticleSystem como hijo*")]
    [Space]

    [Tooltip("Eventos adicionales que se ejecutan al recoger el objeto")]
    public UnityEvent onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GetComponentInChildren<ParticleSystem>().Stop();
            GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            GetComponentInChildren<ParticleSystem>().loop = false;
            transform.GetChild(0).GetComponentInChildren<ParticleSystem>().Play();

            Destroy(gameObject, 1f);

            onTriggerEnter.Invoke();
        }
    }
}
