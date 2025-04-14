using System.Collections.Generic;
using UnityEngine;

public class LockScript : MonoBehaviour
{
    public List<LockScript> lockScript = new List<LockScript>();
    public DoorScript door;
    
    [Space]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Space]
    public bool isLockClosed = false; //Si está cerrado, la puerta está abierta

    private RopeInteractScript rope;

    //Si el player interactua con una cuerda atada, pasa la cuerda a este objeto y se cierra
    public void StartInteraction(RopeInteractScript rope)
    {
        //Si ya tiene un rope, lo desconecta y conecta el siguiente
        if (this.rope == null)
        {
            this.rope = rope;

            //Suscribirse al delegado
            this.rope.startRopeInteractionDelegate += EndInteraction;
        }
        else if(this.rope.transform != rope.transform)
        {
            EndInteraction();
            this.rope = rope;
            this.rope.startRopeInteractionDelegate += EndInteraction;
        }

        isLockClosed = true;
        CheckLockState();
        skinnedMeshRenderer.SetBlendShapeWeight(0, 0);
    }

    //Comprueba si el resto de cerraduras están cerrados, si lo están, abre la puerta
    public void CheckLockState()
    {
        foreach (var item in lockScript)
        {
            if (!item.isLockClosed)
                return;
        }

        door.OpenDoor();
    }

    //Se comprueba cada vez que el jugador interactua con el script, puede quitar la cuerda en cualquir momento
    public void EndInteraction()
    {
        //Desconecta la cuerda que tenga, si es que tiene
        rope.startRopeInteractionDelegate -= EndInteraction;
        rope.EndInteraction();

        rope = null;
        skinnedMeshRenderer.SetBlendShapeWeight(0, 100);

        door.CloseDoor();
        isLockClosed = false;
    }
}
