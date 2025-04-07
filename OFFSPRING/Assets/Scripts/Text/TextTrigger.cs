using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TextTrigger : MonoBehaviour
{
    [Tooltip("Evento que se ejecuta al entrar en el trigger")]
    public UnityEvent onTriggerEnter;

    [SerializeField] private SO_TextData textData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            transform.parent.GetComponentInChildren<TMP_Text>().text = textData.text;

            //onTriggerEnter.Invoke();
        }
    }
}
