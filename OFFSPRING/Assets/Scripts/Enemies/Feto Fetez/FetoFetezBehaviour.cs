using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class FetoFetezBehaviour : MonoBehaviour
{
    public enum State { Wandering, Chasing, Attacking, Stunned, AggroDeathChase }

    public NavMeshAgent agent;
    public Rope rope;
    //public Animator animator;
    private Transform player;
    private RopeInteractScript ropePoint;

    [Space]
    public float wanderRadius = 10f;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public float chaseSpeed = 4f;
    public float wanderSpeed = 2f;
    public float aggroSpeed = 6f;
    public float stunDuration = 2f;
    public float aggroLifetime = 10f;
    private float aggroTimer = 0f;

    [Space]
    public State currentState = State.Wandering;

    [Space]
    public UnityEvent onEnemyDie;

    private void Start()
    {
        agent.speed = wanderSpeed;
        //animator.SetBool("isWalking", true); // Wandering anim

        player = Player.Instance.KinematicCharacterController.transform;
        ropePoint = rope.nodes[0].GetComponent<RopeInteractScript>();
    }

    private void Update()
    {
        if (rope.endAttachment == null && currentState != State.AggroDeathChase)
        {
            ChangeState(State.AggroDeathChase);
        }

        switch (currentState)
        {
            case State.Wandering:
                Wander();
                CheckTransitionToChase();
                break;

            case State.Chasing:
                Chase();
                CheckChaseTransitions();
                break;

            case State.Attacking:
                Attack();
                break;

            case State.Stunned:
                break;

            case State.AggroDeathChase:
                AggroChase();
                break;
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        //animator.SetBool("isWalking", false);
        //animator.SetBool("isChasing", false);
        //animator.SetBool("isAttacking", false);
        //animator.SetBool("isStunned", false);
        //animator.SetBool("isAggro", false);
        //animator.SetBool("isDead", false);

        switch (newState)
        {
            case State.Wandering:
                agent.speed = wanderSpeed;
                //animator.SetBool("isWalking", true);
                break;
            case State.Chasing:
                agent.speed = chaseSpeed;
                //animator.SetBool("isChasing", true);
                break;
            case State.Attacking:
                //animator.SetBool("isAttacking", true);
                break;
            case State.Stunned:
                //animator.SetBool("isStunned", true);
                break;
            case State.AggroDeathChase:
                agent.speed = aggroSpeed;
                aggroTimer = aggroLifetime; // Start the countdown
                //animator.SetBool("isAggro", true);
                break;
        }
    }

    void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewWanderDestination();
        }
        else if (!CanMoveTo(agent.destination))
        {
            SetNewWanderDestination();
        }
    }

    void SetNewWanderDestination()
    {
        Vector3 newPos = PickBiasedWanderDestination();
        Vector3 direction = (newPos - transform.position).normalized;

        if (ropePoint.CheckNextPlayerPositionAvailability(direction, agent.speed))
        {
            agent.SetDestination(newPos);
        }
    }


    Vector3 PickBiasedWanderDestination()
    {
        Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
        randomOffset.y = 0; // Keep movement on the same Y plane
        Vector3 baseTarget = transform.position + randomOffset;

        // If rope is stretched to its limit, bias toward endAttachment
        if (ropePoint != null && rope.endAttachment != null)
        {
            Vector3 toEnd = (rope.endAttachment.position - transform.position).normalized;
            float biasFactor = 0.7f; // 0 = ignore bias, 1 = full bias

            // Blend between a random direction and the direction toward endAttachment
            Vector3 biasedDir = Vector3.Lerp(randomOffset.normalized, toEnd, biasFactor);
            baseTarget = transform.position + biasedDir * wanderRadius;
        }

        // Make sure target is valid on navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(baseTarget, out hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback if sampling fails
        return transform.position;
    }

    void CheckTransitionToChase()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < chaseDistance)
        {
            ChangeState(State.Chasing);
        }
    }

    void Chase()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (ropePoint.CheckNextPlayerPositionAvailability(direction, agent.speed))
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Cannot chase anymore, stop and fallback
            ChangeState(State.Wandering);
        }
    }

    void CheckChaseTransitions()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > chaseDistance * 1.5f)
        {
            ChangeState(State.Wandering);
        }
        else if (distance <= attackDistance)
        {
            ChangeState(State.Attacking);
        }
    }

    void Attack()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            Debug.Log("Enemy attacks!");

            //TODO -> logica del ataque 

            agent.isStopped = true;
            ChangeState(State.Stunned);
            StartCoroutine(StunCoroutine());
        }
        else
        {
            ChangeState(State.Chasing);
        }
    }

    IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(stunDuration);
        agent.isStopped = false;
        ChangeState(State.Chasing);
    }

    void AggroChase()
    {
        if (player != null && agent.enabled == true)
        {
            agent.SetDestination(player.position);
        }

        aggroTimer -= Time.deltaTime;
        if (aggroTimer <= 0f)
        {
            Die();
        }
    }


    public void Die()
    {
        Debug.Log("FetoFetez died from aggressive chase!");
        agent.isStopped = true;
        agent.enabled = false;
        ChangeState(State.Stunned); // stays dead, plays animation
        //animator.SetBool("isDead", true);

        onEnemyDie?.Invoke();

        Destroy(gameObject, 3f);
    }


    bool CanMoveTo(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        return ropePoint.CheckNextPlayerPositionAvailability(direction, agent.speed);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    private void OnDrawGizmos()
    {
        // Chase range
        Gizmos.color = currentState == State.Chasing ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        // Attack range
        Gizmos.color = currentState == State.Attacking ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
