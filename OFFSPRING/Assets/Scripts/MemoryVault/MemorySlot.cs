using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MemorySlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] SO_Collectable memory;

    private TMP_Text memoryNameText;
    private TMP_Text memoryDescriptionText;

    private void Start()
    {
        memoryNameText = GameObject.Find("memoryName").GetComponent<TMP_Text>();
        memoryNameText = GameObject.Find("memoryDescription").GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (memory == null) return;

        gameObject.GetComponentInChildren<Image>().sprite = memory.render_2D;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        memoryNameText.text = memory.memoryName;
        memoryDescriptionText.text = memory.description;
    }

}
