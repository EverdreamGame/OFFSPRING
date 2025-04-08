using UnityEngine;

public abstract class ParentInteractionScript : MonoBehaviour
{
    /// <summary>
    /// Comportamiento del interactuable al interactuar
    /// </summary>
    public virtual void StartInteraction(Transform playerTrans)
    { }

    /// <summary>
    /// Comportamiento del interactuable al dejar de interactuar
    /// </summary>
    public virtual void EndInteraction()
    { }
    
    /// <summary>
    /// Funcion para comprobar si el jugador puede avanzar hacia una direccion concreta con una velocidad concreta
    /// </summary>
    public virtual bool CheckNextPlayerPositionAvailability(Vector3 direction, float speed)
    {
        return true;
    }

    /// <summary>
    /// Dada la direccion del player, devuelve la máxima velocidad posible para llegar al limite de extension del interactuable, por defecto será la velocidad dada
    /// </summary>
    public virtual float GetMaximumSpeedToReachTheExtendedPosition(Vector3 direction, float speed)
    {
        return speed;
    }
}
