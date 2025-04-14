using UnityEngine;
using DG.Tweening;

public class VaginaKiller : MonoBehaviour
{
    public ProceduralCylinder cylinder;
    public LayerMask vaginaLayerMask;

    private void Start()
    {
        cylinder.changeOfDimensionsDelegate += ChangeOfHeight;
    }

    public void ChangeOfHeight(float height)
    {
        Vector3 direction = transform.position - transform.parent.position;
        transform.DOMove(transform.parent.position + (direction.normalized * height), .1f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (LayerMask.Equals(other.gameObject.layer, gameObject.layer))
        {
            Vector3 position = (transform.position + other.transform.position) / 2f;
            Collider[] collider = Physics.OverlapSphere(position, 1f, vaginaLayerMask);
            if (collider.Length > 0)
                Destroy(collider[0].gameObject);
        }

        cylinder.hasReachedObstacle = true;
    }

    public void OnTriggerExit(Collider other)
    {
        cylinder.hasReachedObstacle = false;
    }
}
