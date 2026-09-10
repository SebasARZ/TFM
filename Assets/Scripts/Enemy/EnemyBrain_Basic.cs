using UnityEngine;

public class EnemyBrain_Basic : MonoBehaviour
{
    private enum EnemyState { Idle, Patrol, Chase, Shoot }

    public Transform target;

    private EnemyReference enemyRef;

    [Header("Ranges")]
    public float shootRange = 6f;
    public float chaseRange = 10f;
    public float rotationSpeed = 5f;

    [Header("Speeds")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Patrol")]
    public float waypointTolerance = 0.5f;
    public float waypointWaitTime = 1.5f;

    private EnemyState currentState = EnemyState.Idle;
    private float pathUpdateTimer;
    private float waypointWaitTimer;
    private bool waitingAtWaypoint;

    private void Awake()
    {
        enemyRef = GetComponent<EnemyReference>();
    }

    private void Update()
    {
        EnemyState newState = DetermineState();
        if (newState != currentState)
        {
            EnterState(newState);
            currentState = newState;
        }

        switch (currentState)
        {
            case EnemyState.Shoot:
                LookAtTarget();
                break;
            case EnemyState.Chase:
                UpdatePath();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Idle:
                break;
        }

        enemyRef.anim.SetBool("Shooting", currentState == EnemyState.Shoot);
        enemyRef.anim.SetFloat("Speed", enemyRef.agnt.desiredVelocity.sqrMagnitude);
    }

    private EnemyState DetermineState()
    {
        if (target == null)
            return HasWaypoints() ? EnemyState.Patrol : EnemyState.Idle;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= shootRange) return EnemyState.Shoot;
        if (distanceToTarget <= chaseRange) return EnemyState.Chase;

        return HasWaypoints() ? EnemyState.Patrol : EnemyState.Idle;
    }

    private void EnterState(EnemyState newState)
    {
        waitingAtWaypoint = false;

        if (newState == EnemyState.Shoot || newState == EnemyState.Patrol || newState == EnemyState.Idle)
        {
            if (enemyRef.agnt.hasPath)
            {
                enemyRef.agnt.ResetPath();
            }
        }
        switch (newState)
        {
            case EnemyState.Patrol:
                enemyRef.agnt.speed = patrolSpeed;
                break;
            case EnemyState.Chase:
                enemyRef.agnt.speed = chaseSpeed;
                break;
        }
    }

    private bool HasWaypoints()
    {
        return enemyRef.wayPoints != null && enemyRef.wayPoints.childCount > 0;
    }

    private void UpdatePath()
    {
        if (Time.time >= pathUpdateTimer)
        {
            enemyRef.agnt.SetDestination(target.position);
            pathUpdateTimer = Time.time + enemyRef.pathUpdateRate;
        }
    }

    private void Patrol()
    {
        if (!HasWaypoints()) return;

        if (waitingAtWaypoint)
        {
            if (Time.time >= waypointWaitTimer)
            {
                waitingAtWaypoint = false;
                GoToNextWaypoint();
            }
            return;
        }

        if (!enemyRef.agnt.hasPath && !enemyRef.agnt.pathPending)
        {
            GoToNextWaypoint();
            return;
        }

        bool arrived = !enemyRef.agnt.pathPending
            && enemyRef.agnt.remainingDistance <= Mathf.Max(enemyRef.agnt.stoppingDistance, waypointTolerance)
            && (!enemyRef.agnt.hasPath || enemyRef.agnt.velocity.sqrMagnitude < 0.01f);

        if (arrived)
        {
            waitingAtWaypoint = true;
            waypointWaitTimer = Time.time + waypointWaitTime;
        }
    }

    private void GoToNextWaypoint()
    {
        enemyRef.currentWayPointIndex = (enemyRef.currentWayPointIndex + 1) % enemyRef.wayPoints.childCount;
        Transform nextPoint = enemyRef.wayPoints.GetChild(enemyRef.currentWayPointIndex);
        enemyRef.agnt.SetDestination(nextPoint.position);
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0f;
        if (lookPos.sqrMagnitude < 0.0001f) return;

        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }
}
