using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerTextByCollision : MonoBehaviour
{
    [Tooltip("Evento que se ejecuta al entrar en el trigger")]
    public UnityEvent onTriggerEnter;
    
    [SerializeField] private SO_TextData textData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger Text");

            transform.parent.GetComponentInChildren<TextAnimations>().ShowTextLetterByLetter(textData.content);

            //onTriggerEnter.Invoke();
        }
    }
}
