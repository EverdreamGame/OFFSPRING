using UnityEngine;

public class PlayerInteractionScript : MonoBehaviour
{
    public LayerMask interactionLayerMask;
    public float interactionRadius;
    [Space]
    public ParentInteractionScript currentObjectInteraction;
    private Player player;

    [Space]
    private bool canInteract;
    private Collider[] interactedCollider;
    public Collider primerColliderParaInteractuar;

    private void Start()
    {
        player = Player.Instance;
        canInteract = false;

        interactedCollider = new Collider[1];
    }

    private void Update()
    {
        // No queremos que interactue con más de una cosa a la vez
        if (currentObjectInteraction != null) return;
        
        int hits = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, interactedCollider, interactionLayerMask);

        if (hits > 0)
        {
            primerColliderParaInteractuar = interactedCollider[0];

            //TODO -> UI/Sonido para indicar que puede interactuar
            canInteract = true;
        }
        else
        {
            primerColliderParaInteractuar = null;
            canInteract = true;
        }
    }

    public void Interaction()
    {
        if (!canInteract) return;

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
