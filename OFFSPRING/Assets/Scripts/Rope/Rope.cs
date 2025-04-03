using System.Collections.Generic;
using UnityEngine;

public class Rope3D : MonoBehaviour
{
    [Header("Rope Settings")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float nodeDistance = 0.25f;
    [SerializeField] private int totalNodes = 20;
    [SerializeField] private float ropeWidth = 0.1f;

    [Header("Attachment Points")]
    [SerializeField] private Transform startAttachment;
    [SerializeField] private Transform endAttachment;

    [Header("Physics Settings")]
    [SerializeField] private Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] [Range(0.95f, 0.999f)] private float damping = 0.998f;
    [SerializeField] private float sleepThreshold = 0.001f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.22f;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private int solverIterations = 3;

    private LineRenderer lineRenderer;
    private List<RopeNode> nodes = new List<RopeNode>();
    private List<float> nodeSpeeds = new List<float>();
    private float ropeLength;

    public float CurrentLength => CalculateActualRopeLength(); // Now calculates real path length
    public float RestLength => (totalNodes - 1) * nodeDistance;
    public Vector3 StartPoint => startAttachment != null ? startAttachment.position : nodes[0].Position;
    public Vector3 EndPoint => endAttachment != null ? endAttachment.position : nodes[nodes.Count - 1].Position;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        InitializeRope();
        ropeLength = RestLength;
    }

    void Update() => DrawRope();

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Calculate actual rope length along all segments
        float currentActualLength = CalculateActualRopeLength();

        // Enforce fixed length between attachments
        if (startAttachment != null && endAttachment != null && currentActualLength > ropeLength)
        {
            // Calculate the direction from each attachment to the next node
            Vector3 startToNext = nodes[1].Position - startAttachment.position;
            Vector3 endToPrev = nodes[nodes.Count - 2].Position - endAttachment.position;

            // Only apply correction if we have valid directions (avoid division by zero)
            if (startToNext.sqrMagnitude > 0.001f && endToPrev.sqrMagnitude > 0.001f)
            {
                Vector3 startDirection = startToNext.normalized;
                Vector3 endDirection = endToPrev.normalized;

                float distanceToCorrect = currentActualLength - ropeLength;

                // Calculate correction amounts (distribute between both ends)
                float startCorrection = distanceToCorrect * 0.5f;
                float endCorrection = distanceToCorrect * 0.5f;

                // Calculate new positions
                Vector3 newStartPos = startAttachment.position + startDirection * startCorrection;
                Vector3 newEndPos = endAttachment.position + endDirection * endCorrection;

                // Verify the new positions don't overshoot the nodes
                if (Vector3.Dot(newStartPos - nodes[1].Position, startDirection) > 0)
                    newStartPos = nodes[1].Position - startDirection * 0.01f;

                if (Vector3.Dot(newEndPos - nodes[nodes.Count - 2].Position, endDirection) > 0)
                    newEndPos = nodes[nodes.Count - 2].Position - endDirection * 0.01f;

                // Move attachments if they're not kinematic
                Rigidbody startRb = startAttachment.GetComponent<Rigidbody>();
                if (startRb == null || !startRb.isKinematic)
                {
                    startAttachment.position = newStartPos;
                }

                Rigidbody endRb = endAttachment.GetComponent<Rigidbody>();
                if (endRb == null || !endRb.isKinematic)
                {
                    endAttachment.position = newEndPos;
                }
            }
        }

        CalculateVelocities();

        // Apply physics to free nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            bool isStartNode = i == 0 && startAttachment != null;
            bool isEndNode = i == nodes.Count - 1 && endAttachment != null;

            if (!isStartNode && !isEndNode && nodeSpeeds[i] > sleepThreshold)
            {
                VerletIntegration(nodes[i], dt);
            }
        }

        // Solve constraints
        for (int iter = 0; iter < solverIterations; iter++)
        {
            ApplyDistanceConstraints();
            HandleCollisions();
        }

        // Update attachment points
        if (startAttachment != null)
        {
            nodes[0].Position = startAttachment.position;
            nodes[0].previousPosition = startAttachment.position;
        }

        if (endAttachment != null)
        {
            nodes[nodes.Count - 1].Position = endAttachment.position;
            nodes[nodes.Count - 1].previousPosition = endAttachment.position;
        }
    }

    private void InitializeRope()
    {
        // Clear existing nodes
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        nodes.Clear();
        nodeSpeeds.Clear();

        // Create new nodes
        Vector3 spawnPos = transform.position;
        for (int i = 0; i < totalNodes; i++)
        {
            RopeNode node = Instantiate(nodePrefab, transform).GetComponent<RopeNode>();
            node.Position = spawnPos;
            node.previousPosition = spawnPos;
            nodes.Add(node);
            nodeSpeeds.Add(0f);
            spawnPos.y -= nodeDistance;
        }

        lineRenderer.positionCount = totalNodes;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
    }
    
    private float CalculateActualRopeLength()
    {
        float length = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            length += Vector3.Distance(nodes[i].Position, nodes[i + 1].Position);
        }
        return length;
    }

    private void CalculateVelocities()
    {
        for (int i = 0; i < nodes.Count; i++)
            nodeSpeeds[i] = (nodes[i].Position - nodes[i].previousPosition).sqrMagnitude;
    }

    private void VerletIntegration(RopeNode node, float dt)
    {
        Vector3 temp = node.Position;
        Vector3 velocity = (temp - node.previousPosition) * damping;
        node.Position += velocity + gravity * (dt * dt);
        node.previousPosition = temp;
    }

    private void ApplyDistanceConstraints()
    {
        // Forward pass
        for (int i = 0; i < nodes.Count - 1; i++)
            ApplyNodeConstraint(i, i + 1);

        // Backward pass for stability
        for (int i = nodes.Count - 2; i >= 0; i--)
            ApplyNodeConstraint(i, i + 1);
    }

    private void ApplyNodeConstraint(int indexA, int indexB)
    {
        RopeNode nodeA = nodes[indexA];
        RopeNode nodeB = nodes[indexB];

        Vector3 delta = nodeB.Position - nodeA.Position;
        float distance = delta.magnitude;
        float correction = (distance - nodeDistance) * 0.5f;

        if (distance > 0.0001f)
        {
            Vector3 correctionVector = (delta / distance) * correction;

            bool canMoveA = indexA != 0 || startAttachment == null;
            bool canMoveB = indexB != nodes.Count - 1 || endAttachment == null;

            if (canMoveA) nodeA.Position += correctionVector;
            if (canMoveB) nodeB.Position -= correctionVector;
        }
    }

    private void HandleCollisions()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            // Skip attached nodes
            bool isStartNode = i == 0 && startAttachment != null;
            bool isEndNode = i == nodes.Count - 1 && endAttachment != null;
            if (isStartNode || isEndNode || nodeSpeeds[i] < sleepThreshold)
                continue;

            RopeNode node = nodes[i];
            Collider[] hits = new Collider[5];
            int numHits = Physics.OverlapSphereNonAlloc(
                node.Position,
                collisionRadius + skinWidth,
                hits,
                collisionMask
            );

            if (numHits > 0)
            {
                Vector3 pushDirection = Vector3.zero;
                int validHits = 0;

                for (int j = 0; j < numHits; j++)
                {
                    if (hits[j].transform != node.transform)
                    {
                        Vector3 closestPoint = hits[j].ClosestPoint(node.Position);
                        Vector3 normal = (node.Position - closestPoint).normalized;
                        float distance = Vector3.Distance(node.Position, closestPoint);

                        if (distance < collisionRadius + skinWidth)
                        {
                            pushDirection += normal * (collisionRadius + skinWidth - distance);
                            validHits++;
                        }
                    }
                }

                if (validHits > 0)
                {
                    node.Position += pushDirection / validHits;
                    node.previousPosition = node.Position;
                }
            }
        }
    }

    private void DrawRope()
    {
        for (int i = 0; i < nodes.Count; i++)
            lineRenderer.SetPosition(i, nodes[i].Position);
    }

    public void SetAttachments(Transform start, Transform end)
    {
        startAttachment = start;
        endAttachment = end;
    }
}