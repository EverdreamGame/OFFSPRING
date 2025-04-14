using UnityEngine;
using DG.Tweening;

public class BasicEnvironmentInteraction : ParentInteractionScript
{
    [Space]
    public Vector3 movingAxis;

    private KCharacterController player;
    private bool interactuando = false;
    private bool isColliding = false;
    private Vector3 lastCollisionNormal = Vector3.zero;

    private Vector3 initialOffset;

    private void Start()
    {
        player = Player.Instance.KinematicCharacterController;
    }

    public override void StartInteraction(Transform playerTrans)
    {
        interactuando = true;
        initialOffset = transform.position - player.transform.position;
    }

    public override void EndInteraction()
    {
        interactuando = false;
    }

    private void FixedUpdate()
    {
        if (!interactuando) return;

        // Create a plane normal (X-axis in this case, since we want YZ movement)
        Vector3 planeNormal = Vector3.right;

        // Project the offset onto the YZ plane
        Vector3 offset = player.transform.position + initialOffset - transform.position;
        Vector3 desiredOffset = Vector3.ProjectOnPlane(offset, planeNormal);

        // Apply movement
        Vector3 newPos = transform.position + desiredOffset;

        // You can use MovePosition for physics-based movement, or DOTween for smoothed movement
        transform.position = newPos;
    }

    public override bool CheckNextPlayerPositionAvailability(Vector3 direction, float speed)
    {
        Vector3 directionNormalized = direction.normalized;

        // Don't allow movement along X (i.e., if direction is mostly horizontal)
        float xComponent = Mathf.Abs(Vector3.Dot(directionNormalized, Vector3.right));
        if (xComponent > 0.2f)  // Adjust threshold as needed
            return false;

        if (isColliding)
        {
            float pushingIntoCollision = Vector3.Dot(directionNormalized, lastCollisionNormal);
            if (pushingIntoCollision > 0.3f)
                return false;
        }

        return true;
    }
    public override float GetMaximumSpeedToReachTheExtendedPosition(Vector3 direction, float speed)
    {
        Vector3 directionNormalized = direction.normalized;

        // If mostly moving along X, don't allow it
        float xComponent = Mathf.Abs(Vector3.Dot(directionNormalized, Vector3.right));
        if (xComponent > 0.2f)
            return 0f;

        if (isColliding)
        {
            float pushingIntoCollision = Vector3.Dot(directionNormalized, lastCollisionNormal);
            if (pushingIntoCollision > 0.3f)
                return 0f;
        }

        return speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            lastCollisionNormal = collision.contacts[0].normal;
            isColliding = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            lastCollisionNormal = collision.contacts[0].normal;
            isColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        lastCollisionNormal = Vector3.zero;
    }
}
