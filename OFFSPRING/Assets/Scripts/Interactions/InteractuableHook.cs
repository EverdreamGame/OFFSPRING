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

    private void Start()
    {
        player = Player.Instance.KinematicCharacterController;
    }

    public override void StartInteraction(Transform playerTrans)
    {
        interactuando = true;
    }

    public override void EndInteraction()
    {
        interactuando = false;
    }

    float distanceBetweenStartToCurrent;
    public float offsetFromPlayer = 0.5f; // Set this from Inspector

    public void FixedUpdate()
    {
        if (!interactuando) return;

        // Get vector from parent to player
        Vector3 offset = player.transform.position - parent.position;

        // Project offset onto movingAxis to constrain movement
        Vector3 constrainedOffset = Vector3.Dot(offset, movingAxis) * movingAxis;

        // Clamp the distance to be within min and max bounds
        float clampedDistance = Mathf.Clamp(constrainedOffset.magnitude, minDistance, maxDistanceAvailable);
        Vector3 clampedOffset = movingAxis.normalized * clampedDistance;

        // Final position with offset from player
        transform.DOMove(parent.position + clampedOffset + (clampedOffset.normalized * offsetFromPlayer), .1f);

        distanceBetweenStartToCurrent = clampedOffset.magnitude;

        foreach (var item in cylinder)
        {
            item.UpdateMeshIfNeeded(distanceBetweenStartToCurrent, maxDistanceAvailable);
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
