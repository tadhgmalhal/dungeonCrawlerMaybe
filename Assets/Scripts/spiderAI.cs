using UnityEngine;
using UnityEngine.AI;

public class spiderAI : MonoBehaviour
{
    [Header("Detection")]
    public float forwardRange = 10f;
    public float peripheralRange = 3.33f;
    public float forwardAngle = 120f;
    public float loseRange = 15f;

    [Header("Movement")]
    public float chaseSpeed = 4f;
    public float roamSpeed = 2f;
    public float roamRadius = 10f;
    public float roamWaitMin = 2f;
    public float roamWaitMax = 5f;

    [Header("Search")]
    public float searchRadius = 8f;
    public float searchDuration = 20f;

    private NavMeshAgent agent;
    private Transform target;
    private Vector3 lastKnownPosition;
    private float searchTimer = 0f;
    private float roamTimer = 0f;

    private enum SpiderState { Roam, Chase, Search }
    private SpiderState state = SpiderState.Roam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        roamTimer = Random.Range(roamWaitMin, roamWaitMax);
    }

    void Update()
    {
        switch (state)
        {
            case SpiderState.Roam:
                handleRoam();
                break;
            case SpiderState.Chase:
                handleChase();
                break;
            case SpiderState.Search:
                handleSearch();
                break;
        }
    }

    void handleRoam()
    {
        agent.speed = roamSpeed;
        roamTimer -= Time.deltaTime;

        if (roamTimer <= 0f)
        {
            setRandomDestination(transform.position, roamRadius * 5f);
            roamTimer = Random.Range(roamWaitMin, roamWaitMax);
        }

        Transform player = getPlayerInRange();
        if (player != null)
        {
            target = player;
            state = SpiderState.Chase;
        }
    }

    void handleChase()
    {
        agent.speed = chaseSpeed;

        if (target == null)
        {
            enterSearch(transform.position);
            return;
        }

        agent.SetDestination(target.position);
        lastKnownPosition = target.position;

        Transform stillVisible = getPlayerInRangeChasing();
        if (stillVisible == null)
        {
            enterSearch(lastKnownPosition);
            return;
        }
    }

    void handleSearch()
    {
        agent.speed = roamSpeed;
        searchTimer -= Time.deltaTime;

        if (roamTimer <= 0f)
        {
            setRandomDestination(lastKnownPosition, searchRadius);
            roamTimer = Random.Range(roamWaitMin, roamWaitMax);
        }

        Transform player = getPlayerInRange();
        if (player != null)
        {
            target = player;
            state = SpiderState.Chase;
            return;
        }

        if (searchTimer <= 0f)
        {
            state = SpiderState.Roam;
            roamTimer = Random.Range(roamWaitMin, roamWaitMax);
        }
    }

    void enterSearch(Vector3 anchor)
    {
        lastKnownPosition = anchor;
        searchTimer = searchDuration;
        roamTimer = 0f;
        state = SpiderState.Search;
    }

    void setRandomDestination(Vector3 anchor, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += anchor;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    Transform getPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, forwardRange, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            Transform player = hit.transform;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            float dist = Vector3.Distance(transform.position, player.position);

            if (angle <= forwardAngle / 2f && dist <= forwardRange)
            {
                if (hasLineOfSight(player, forwardRange))
                {
                    return player;
                }
            }
            else if (dist <= peripheralRange)
            {
                if (hasLineOfSight(player, peripheralRange))
                {
                    return player;
                }
            }
        }
        return null;
    }

    Transform getPlayerInRangeChasing()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, loseRange, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            if (hasLineOfSight(hit.transform, loseRange))
            {
                return hit.transform;
            }
        }
        return null;
    }

    bool hasLineOfSight(Transform target, float range)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = (target.position + Vector3.up * 0.5f) - origin;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, forwardRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, peripheralRange);
        if (state == SpiderState.Search)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
        }
    }
}