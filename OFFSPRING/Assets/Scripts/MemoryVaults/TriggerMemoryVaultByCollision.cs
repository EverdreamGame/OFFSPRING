using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerMemoryVaultByCollision : MonoBehaviour
{
    [Tooltip("Eventos adicionales que se ejecutan al entrar en el trigger")]
    public UnityEvent onTriggerEnter;

    public GameObject memoryVaultPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(memoryVaultPrefab);

            onTriggerEnter.Invoke();
        }
    }
}
