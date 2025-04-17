using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerTextByCollision : MonoBehaviour
{
    //[Tooltip("Eventos adicionales que se ejecutan al entrar en el trigger")]
    //public UnityEvent onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            transform.parent.GetComponentInChildren<TextAnimations>().ShowTextLetterByLetter();

            //onTriggerEnter.Invoke();
        }
    }
}
