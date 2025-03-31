using System.Collections.Generic;
using UnityEngine;

public class FinalRope3D : MonoBehaviour
{
    [Header("Rope Settings")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float nodeDistance = 0.25f;
    [SerializeField] private int totalNodes = 20; // Reduced for better performance
    [SerializeField] private float ropeWidth = 0.1f;

    [Header("Physics Settings")]
    [SerializeField] private Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] [Range(0.95f, 0.999f)] private float damping = 0.998f; // Higher damping
    [SerializeField] [Range(0.1f, 1f)] private float stiffness = 0.7f; // Increased stiffness
    [SerializeField] private float sleepThreshold = 0.001f; // New parameter

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.22f;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private float friction = 0.2f;
    [SerializeField] private int solverIterations = 3; // Reduced iterations

    private LineRenderer lineRenderer;
    private List<RopeNode> nodes = new List<RopeNode>();
    private Vector3 mouseLockPosition;
    private Collider[] overlapResults = new Collider[5];
    private List<Collider> currentCollisions = new List<Collider>();
    private List<float> nodeSpeeds = new List<float>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        InitializeRope();
    }

    void Update()
    {
        if (Camera.main != null)
        {
            mouseLockPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        }
        DrawRope();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Calculate node velocities first
        CalculateVelocities();

        // Apply forces only to active nodes
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodeSpeeds[i] > sleepThreshold)
            {
                VerletIntegration(nodes[i], dt);
            }
        }

        // Solve constraints
        for (int iter = 0; iter < solverIterations; iter++)
        {
            ApplyDistanceConstraints();
            HandleCollisions();
            ApplyPositionSmoothing(); // New smoothing pass
        }

        // Lock first node to mouse
        if (mouseLockPosition != Vector3.zero)
        {
            nodes[0].transform.position = mouseLockPosition;
            nodes[0].previousPosition = mouseLockPosition;
        }
    }

    private void InitializeRope()
    {
        Vector3 startPos = transform.position;

        for (int i = 0; i < totalNodes; i++)
        {
            RopeNode node = Instantiate(nodePrefab, transform).GetComponent<RopeNode>();
            node.transform.position = startPos;
            node.previousPosition = startPos;
            nodes.Add(node);
            nodeSpeeds.Add(0f);
            startPos.y -= nodeDistance;
        }

        lineRenderer.positionCount = totalNodes;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
    }

    private void CalculateVelocities()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodeSpeeds[i] = (nodes[i].transform.position - nodes[i].previousPosition).sqrMagnitude;
        }
    }

    private void VerletIntegration(RopeNode node, float dt)
    {
        Vector3 temp = node.transform.position;
        Vector3 velocity = (temp - node.previousPosition) * damping;

        node.transform.position += velocity + gravity * (dt * dt);
        node.previousPosition = temp;
    }

    private void ApplyDistanceConstraints()
    {
        // Forward pass
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            ApplyNodeConstraint(i, i + 1);
        }

        // Backward pass for stability
        for (int i = nodes.Count - 2; i >= 0; i--)
        {
            ApplyNodeConstraint(i, i + 1);
        }
    }

    private void ApplyNodeConstraint(int indexA, int indexB)
    {
        RopeNode nodeA = nodes[indexA];
        RopeNode nodeB = nodes[indexB];

        Vector3 delta = nodeB.transform.position - nodeA.transform.position;
        float distance = delta.magnitude;
        float correction = (distance - nodeDistance) * stiffness;

        if (distance > 0.0001f)
        {
            Vector3 correctionVector = (delta / distance) * correction;

            if (indexA != 0 || mouseLockPosition == Vector3.zero)
            {
                nodeA.transform.position += correctionVector * 0.5f;
            }
            nodeB.transform.position -= correctionVector * 0.5f;
        }
    }

    private void HandleCollisions()
    {
        currentCollisions.Clear();

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodeSpeeds[i] < sleepThreshold) continue;

            RopeNode node = nodes[i];
            Vector3 position = node.transform.position;

            // Node collision
            int hits = Physics.OverlapSphereNonAlloc(
                position,
                collisionRadius + skinWidth,
                overlapResults,
                collisionMask);

            if (hits > 0)
            {
                Vector3 pushDirection = Vector3.zero;
                int validHits = 0;

                for (int j = 0; j < hits; j++)
                {
                    if (overlapResults[j].transform != node.transform)
                    {
                        currentCollisions.Add(overlapResults[j]);
                        Vector3 closestPoint = overlapResults[j].ClosestPoint(position);
                        Vector3 normal = (position - closestPoint).normalized;
                        float distance = Vector3.Distance(position, closestPoint);

                        if (distance < collisionRadius + skinWidth)
                        {
                            pushDirection += normal * (collisionRadius + skinWidth - distance);
                            validHits++;
                        }
                    }
                }

                if (validHits > 0)
                {
                    node.transform.position += pushDirection / validHits;
                    node.previousPosition = node.transform.position;
                }
            }
        }
    }

    private void ApplyPositionSmoothing()
    {
        // Average positions with neighbors to reduce jitter
        for (int i = 1; i < nodes.Count - 1; i++)
        {
            Vector3 prevPos = nodes[i - 1].transform.position;
            Vector3 nextPos = nodes[i + 1].transform.position;
            Vector3 smoothedPos = (prevPos + nextPos) * 0.5f;

            nodes[i].transform.position = Vector3.Lerp(
                nodes[i].transform.position,
                smoothedPos,
                0.3f); // Smoothing factor
        }
    }

    private void DrawRope()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            lineRenderer.SetPosition(i, nodes[i].transform.position);
        }
    }
}