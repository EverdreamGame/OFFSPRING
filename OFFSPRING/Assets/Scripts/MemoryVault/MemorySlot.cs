using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class MemorySlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] SO_Collectable memory;
    public SO_Collectable memoryUnknown;

    private TMP_Text memoryNameText;
    private TMP_Text memoryDescriptionText;

    private ShowMemoryDetails showMemoryDetails;

    private bool unlocked;

    private void Start()
    {
        memoryNameText = GameObject.Find("MemoryName").GetComponent<TMP_Text>();
        memoryDescriptionText = GameObject.Find("MemoryDescription").GetComponent<TMP_Text>();

        showMemoryDetails = GameObject.FindAnyObjectByType<ShowMemoryDetails>().GetComponent<ShowMemoryDetails>();
    }

    private void OnEnable()
    {
        if (memory == null) return;

        unlocked = MemoriesCollectedManager.collectables.Contains(memory);

        if (!unlocked)
        {
            gameObject.transform.GetChild(0).GetComponent<Image>().sprite = memoryUnknown.render_2D;
        }
        else
        {
            gameObject.transform.GetChild(0).GetComponent<Image>().sprite = memory.render_2D;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!unlocked)
        {
            showMemoryDetails.memorySelected = memoryUnknown;
        }
        else
        {
            showMemoryDetails.memorySelected = memory;
        }
        
        showMemoryDetails.SetMemoryDetails();
    }
}
