using System.Collections.Generic;
using UnityEngine;

public abstract class ParentInteractionScript : MonoBehaviour
{
    public Material outlineMaterial;
    public Renderer[] renderers;
    private List<GameObject> clones = new List<GameObject>();

    /// <summary>
    /// Resalta el interactuable si interactuar es posible en ese momento
    /// </summary>
    public virtual void OutlineInteractable()
    {
        // Elimina outline antiguo, si existe
        DeleteOutline();

        // Obtiene todos los renderers hijos
        renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            // Clona el GameObject igual posición, rotación y padre
            GameObject clone = Instantiate(renderers[i].gameObject, renderers[i].transform.position, renderers[i].transform.rotation, renderers[i].transform.parent);
            clone.name = renderers[i].gameObject.name + "_Clone";

            // Cambia todos los materiales del clon al material de outline
            Renderer cloneRenderer = clone.GetComponent<Renderer>();
            if (cloneRenderer != null)
            {
                int matsCount = cloneRenderer.materials.Length;
                Material[] newMats = new Material[matsCount];
                for (int j = 0; j < matsCount; j++)
                {
                    newMats[j] = outlineMaterial;
                }
                cloneRenderer.materials = newMats;
            }

            clones.Add(clone);
        }
    }

    /// <summary>
    /// Elimina el outline
    /// </summary>
    public virtual void DeleteOutline()
    {
        // Limpia clones anteriores si hay
        foreach (var c in clones)
        {
            if (c != null) Destroy(c);
        }
        clones.Clear();
    }

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
