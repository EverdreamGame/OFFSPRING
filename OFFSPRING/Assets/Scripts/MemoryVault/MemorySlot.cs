using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class MemorySlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] SO_Collectable memory;

    private TMP_Text memoryNameText;
    private TMP_Text memoryDescriptionText;

    private ShowMemoryDetails showMemoryDetails;

    private void Start()
    {
        memoryNameText = GameObject.Find("MemoryName").GetComponent<TMP_Text>();
        memoryNameText = GameObject.Find("MemoryDescription").GetComponent<TMP_Text>();

        showMemoryDetails = GameObject.FindAnyObjectByType<ShowMemoryDetails>().GetComponent<ShowMemoryDetails>();
    }

    private void OnEnable()
    {
        if (memory == null) return;

        //gameObject.GetComponentInChildren<Image>(true).sprite = memory.render_2D; // No funcionaba pillaba el mismo gameobject >:(
        gameObject.transform.GetChild(0).GetComponent<Image>().sprite = memory.render_2D;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showMemoryDetails.memorySelected = memory;
        showMemoryDetails.SetMemoryDetails();
    }
}
