using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class InteractuableHook : ParentInteractionScript
{
    public float maxDistanceAvailable;
    public float minDistance = 1f;
    [Space]
    public Transform parent;
    public List<ProceduralCylinder> cylinder = new List<ProceduralCylinder>();
    [Space]
    public Vector3 movingAxis;

    private KCharacterController player;

    private bool interactuando = false;

    public float returnSpeed = 2f; // Speed at which object returns to start
    private Vector3 startPosition;

    private Vector3 normalizedMovingAxis;
    private Tween moveTween;
    private float lastDistance = -1f;

    private void Start()
    {
        player = Player.Instance.KinematicCharacterController;
        startPosition = new Vector3(0f, 0f, 1f);
        normalizedMovingAxis = movingAxis.normalized;
    }

    public override void StartInteraction(Transform playerTrans)
    {
        interactuando = true;
        transform.DOKill();
    }

    public override void EndInteraction()
    {
        interactuando = false;
        
        float distance = Vector3.Distance(transform.position, startPosition);
        float duration = distance / returnSpeed;

        transform.DOLocalMove(startPosition, duration);
    }

    //float distanceBetweenStartToCurrent;
    public float offsetFromPlayer = 0.5f; // Set this from Inspector

    public void FixedUpdate()
    {
        float newDistance;

        if (interactuando)
        {
            Vector3 offset = player.transform.position - parent.position;
            float projection = Vector3.Dot(offset, normalizedMovingAxis);
            float clampedDistance = Mathf.Clamp(projection, minDistance, maxDistanceAvailable);
            Vector3 targetPosition = parent.position + normalizedMovingAxis * clampedDistance + normalizedMovingAxis * offsetFromPlayer;

            // Only move if not already moving there
            if (moveTween == null || !moveTween.IsActive() || moveTween.IsComplete())
            {
                moveTween = transform.DOMove(targetPosition, 0.1f).SetEase(Ease.Linear);
            }

            newDistance = clampedDistance;
        }
        else
        {
            // Use actual object position for mesh calculation
            Vector3 offsetFromParent = transform.position - parent.position;
            newDistance = Vector3.Dot(offsetFromParent, normalizedMovingAxis);
        }

        // Only update mesh if distance changed significantly
        if (Mathf.Abs(newDistance - lastDistance) > 0.01f)
        {
            foreach (var item in cylinder)
            {
                item.UpdateMeshIfNeeded(newDistance, maxDistanceAvailable);
            }
            lastDistance = newDistance;
        }
    }

    public override bool CheckNextPlayerPositionAvailability(Vector3 direction, float speed)
    {
        Vector3 directionNormalized = direction.normalized;

        // Must be aligned with movement axis
        float axisAlignment = Vector3.Dot(directionNormalized, movingAxis);
        if (Mathf.Abs(axisAlignment) < 0.8f) // (1 = perfect alignment, 0 = perpendicular)
            return false;

        // Must be roughly pointing toward the hook
        Vector3 toHook = (transform.position - player.transform.position).normalized;
        float facingHook = Vector3.Dot(directionNormalized, toHook);
        if (facingHook > 0.3f) // (1 = perfect alignment, 0 = perpendicular)
            return true;

        // Predict future player position
        Vector3 futurePlayerPosition = player.transform.position + directionNormalized * speed;
        Vector3 futureOffset = futurePlayerPosition - parent.position;
        Vector3 projectedOffset = Vector3.Dot(futureOffset, movingAxis) * movingAxis;

        float futureDistance = projectedOffset.magnitude;

        foreach (var item in cylinder)
        {
            if (item.hasReachedObstacle)
                return false;
        }

        return futureDistance >= minDistance && futureDistance <= maxDistanceAvailable;
    }

    public override float GetMaximumSpeedToReachTheExtendedPosition(Vector3 direction, float speed)
    {
        Vector3 currentOffset = player.transform.position - parent.position;
        float currentDistance = Vector3.Dot(currentOffset, movingAxis);

        float alignment = Vector3.Dot(direction.normalized, movingAxis);

        if (alignment <= 0f)
            return 0f;

        float remainingDistance = maxDistanceAvailable - Mathf.Abs(currentDistance);
        float maxAllowedSpeed = remainingDistance / alignment;

        return Mathf.Min(speed, maxAllowedSpeed);
    }
}
