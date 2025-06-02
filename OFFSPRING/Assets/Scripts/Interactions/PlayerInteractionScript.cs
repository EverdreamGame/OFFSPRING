using UnityEngine;

public class PlayerInteractionScript : MonoBehaviour
{
    public LayerMask interactionLayerMask;
    public float interactionRadius;
    [Space]
    public ParentInteractionScript currentObjectInteraction;

    [Space]
    private bool canInteract;
    private Collider[] interactedCollider;
    public Collider primerColliderParaInteractuar;

    [Space]
    public Material outlineMaterial;

    private void Start()
    {
        canInteract = false;

        interactedCollider = new Collider[1];

        ParentInteractionScript.outlineMaterial = outlineMaterial;
    }

    private void Update()
    {
        // No queremos que interactue con más de una cosa a la vez
        if (currentObjectInteraction != null) return;
        
        int hits = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, interactedCollider, interactionLayerMask);

        if (hits > 0)
        {
            if (primerColliderParaInteractuar != null && interactedCollider[0] != primerColliderParaInteractuar)
            {
                primerColliderParaInteractuar.GetComponent<ParentInteractionScript>().DeleteOutline();
            }
            primerColliderParaInteractuar = interactedCollider[0];

            primerColliderParaInteractuar.GetComponent<ParentInteractionScript>().OutlineInteractable();
            //TODO -> UI/Sonido para indicar que puede interactuar
            canInteract = true;
        }
        else
        {
            if (primerColliderParaInteractuar != null)
                primerColliderParaInteractuar.GetComponent<ParentInteractionScript>().DeleteOutline();

            primerColliderParaInteractuar = null;
            canInteract = true;
        }
    }

    public void Interaction()
    {
        if (!canInteract) return;

        primerColliderParaInteractuar.GetComponent<ParentInteractionScript>().DeleteOutline();

        // Si ya estaba interactuando con algo, desinteractua
        if (currentObjectInteraction)
        {
            currentObjectInteraction.EndInteraction();
            currentObjectInteraction = null;
            return;
        }
        // Si no, interactua
        else if(!currentObjectInteraction && primerColliderParaInteractuar)
        {
            currentObjectInteraction = primerColliderParaInteractuar.GetComponent<ParentInteractionScript>();
            currentObjectInteraction.StartInteraction(transform);
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
