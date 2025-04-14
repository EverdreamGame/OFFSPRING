using UnityEngine;

public class RopeInteractScript : ParentInteractionScript
{
    RopeNode currentRopeNode;
    RopeNode previousRopeNode;

    Rope ropeReference;
    bool isStartRope;

    public delegate void RopeInteractionDelegate();
    public RopeInteractionDelegate startRopeInteractionDelegate;

    private void Start()
    {
        //Cambiar la layer del objeto
        gameObject.layer = LayerMask.NameToLayer("Interactions");

        // Asignar las referencias necesarias 
        ropeReference = GetComponentInParent<Rope>();
        currentRopeNode = GetComponent<RopeNode>();

        isStartRope = ropeReference.nodes[0] == currentRopeNode;
        previousRopeNode = isStartRope ? ropeReference.nodes[1] : ropeReference.nodes[ropeReference.nodes.Count - 2];

        // Añadir el componente de collider para ser detectado
        SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
        sphere.radius = .2f;
        sphere.isTrigger = true;
    }

    public override void StartInteraction(Transform playerTrans)
    {
        //TODO -> si el enemigo está vivo, todavía no es interactuable
        startRopeInteractionDelegate?.Invoke();

        if (isStartRope)
            ropeReference.startAttachment = playerTrans;
        else
            ropeReference.endAttachment = playerTrans;
    }
    [Space]
    public float distanceToCheckNearLock = 1.5f;
    public override void EndInteraction()
    {
        if (isStartRope)
        {
            //Comprueba si hay un lock cerca
            if (IsNearLock(distanceToCheckNearLock))
            {
                LockScript myLock = GetNearLock(distanceToCheckNearLock);

                if (myLock.transform != ropeReference.startAttachment)
                {
                    myLock.StartInteraction(this);
                    ropeReference.startAttachment = myLock.transform;
                    return;
                }
            }
            //Si no hay, attatch es null
            ropeReference.startAttachment = null;
        }
        else
        {
            //Comprueba si hay un lock cerca
            if (IsNearLock(distanceToCheckNearLock))
            {
                LockScript myLock = GetNearLock(distanceToCheckNearLock);

                if (myLock.transform != ropeReference.endAttachment)
                {
                    myLock.StartInteraction(this);
                    ropeReference.endAttachment = myLock.transform;
                    return;
                }
            }
            //Si no hay, attatch es null
            ropeReference.endAttachment = null;
        }
    }

    public override bool CheckNextPlayerPositionAvailability(Vector3 direction, float speed)
    {
        // Si ya está en el máximo posible
        if (ropeReference.IsRopeMaxExtent())
        {
            //Check de la dirección por si hace backtracking, es ese caso, sí podrá moverse
            if (!IsNextPositionInsideConeBounds(currentRopeNode.Position + direction * speed))
            {
                return false;
            }
        }
        // Si no está al máximo, comprueba si en el próximo step lo estará
        //else
        //{
        //    float restRopeLength = ropeReference.ropeLength - ropeReference.CalculateActualRopeLength();
        //    if (speed > restRopeLength) return false;
        //}

        return true;
    }

    public override float GetMaximumSpeedToReachTheExtendedPosition(Vector3 direction, float speed)
    {
        // Si ya está en el máximo posible
        if (ropeReference.IsRopeMaxExtent())
        {
            //Check de la dirección por si hace backtracking, es ese caso, sí podrá moverse
            if (IsNextPositionInsideConeBounds(currentRopeNode.Position + direction * speed))
            {
                return speed;
            }
            return 0f;
        }
        // Si no está al máximo, comprueba si en el próximo step lo estará
        //else
        //{
        //    float restRopeLength = ropeReference.ropeLength - ropeReference.CalculateActualRopeLength();
        //    if (speed > restRopeLength) return restRopeLength;
        //}

        return speed;
    }

    public float coneAngleInDegrees = 165f;
    public bool IsNextPositionInsideConeBounds(Vector3 position)
    {
        Vector3 currentRopeDirection = currentRopeNode.Position - previousRopeNode.Position;

        if (currentRopeDirection == Vector3.zero)
            return false;

        Vector3 coneDirection = -currentRopeDirection.normalized;
        Vector3 toPosition = (position - currentRopeNode.Position).normalized;

        float angleToPoint = Vector3.Angle(coneDirection, toPosition);
        return angleToPoint <= (coneAngleInDegrees * 0.5f);
    }

    public bool IsNearLock(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<LockScript>() != null)
                return true;
        }

        return false;
    }

    public LockScript GetNearLock(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<LockScript>() != null)
                return hit.GetComponent<LockScript>();
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Vector3 currentRopeDirection = currentRopeNode.Position - previousRopeNode.Position;

        if (currentRopeDirection == Vector3.zero)
            return;

        Vector3 coneDirection = -currentRopeDirection.normalized;

        // Centro del cono
        Gizmos.color = Color.white;
        Gizmos.DrawRay(currentRopeNode.Position, coneDirection * 5);

        // Bordes del cono
        Quaternion leftRotation = Quaternion.AngleAxis(-coneAngleInDegrees * 0.5f, Vector3.up);
        Quaternion rightRotation = Quaternion.AngleAxis(coneAngleInDegrees * 0.5f, Vector3.up);
        Vector3 leftDirection = leftRotation * coneDirection;
        Vector3 rightDirection = rightRotation * coneDirection;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(currentRopeNode.Position, leftDirection * 5);
        Gizmos.DrawRay(currentRopeNode.Position, rightDirection * 5);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToCheckNearLock);
    }
}
