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
    public float roamWaitMin = 2f;
    public float roamWaitMax = 5f;

    [Header("Search")]
    public float searchRadius = 8f;
    public float searchDuration = 20f;

    [Header("Attack")]
    public float biteRange = 1.5f;
    public float windupDuration = 1f;
    public float biteDamage = 25f;
    public float attackCooldown = 3f;
    public float hurtboxDuration = 0.25f;
    public float hurtboxRadius = 0.5f;

    private NavMeshAgent agent;
    private Transform target;
    private Vector3 lastKnownPosition;
    private float searchTimer = 0f;
    private float roamTimer = 0f;
    private float windupTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isWindingUp = false;
    private Vector3 hurtboxPosition;
    private float hurtboxTimer = 0f;
    private bool hurtboxActive = false;

    private enum SpiderState { Roam, Chase, Search, Attack }
    private SpiderState state = SpiderState.Roam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = biteRange;
        roamTimer = Random.Range(roamWaitMin, roamWaitMax);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (hurtboxActive)
        {
            hurtboxTimer -= Time.deltaTime;
            checkHurtbox();
            if (hurtboxTimer <= 0f)
            {
                hurtboxActive = false;
            }
        }

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
            case SpiderState.Attack:
                handleAttack();
                break;
        }
    }

    void handleRoam()
    {
        agent.speed = roamSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer -= Time.deltaTime;
            if (roamTimer <= 0f)
            {
                setRandomDestination(transform.position, 200f);
                roamTimer = Random.Range(roamWaitMin, roamWaitMax);
            }
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

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= biteRange && cooldownTimer <= 0f)
        {
            agent.ResetPath();
            state = SpiderState.Attack;
            isWindingUp = true;
            windupTimer = windupDuration;
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

    void handleAttack()
    {
        agent.ResetPath();

        if (isWindingUp)
        {
            windupTimer -= Time.deltaTime;
            if (windupTimer <= 0f)
            {
                isWindingUp = false;
                spawnHurtbox();
                cooldownTimer = attackCooldown;
            }
        }
        else
        {
            if (target != null)
            {
                state = SpiderState.Chase;
            }
            else
            {
                enterSearch(transform.position);
            }
        }
    }

    void spawnHurtbox()
    {
        if (target == null) return;
        hurtboxPosition = target.position;
        hurtboxActive = true;
        hurtboxTimer = hurtboxDuration;
    }

    void checkHurtbox()
    {
        Collider[] hits = Physics.OverlapSphere(hurtboxPosition, hurtboxRadius, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            playerHP hp = hit.GetComponent<playerHP>();
            if (hp != null)
            {
                hp.takeDamage(biteDamage);
                hurtboxActive = false;
                return;
            }
        }
    }

    void handleSearch()
    {
        agent.speed = roamSpeed;
        searchTimer -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer -= Time.deltaTime;
            if (roamTimer <= 0f)
            {
                setRandomDestination(lastKnownPosition, searchRadius);
                roamTimer = Random.Range(roamWaitMin, roamWaitMax);
            }
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
        float minDistance = 15f;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = anchor + Random.insideUnitSphere * radius;
            randomDirection.y = anchor.y;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(anchor, hit.position) >= minDistance)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
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
        if (hurtboxActive)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hurtboxPosition, hurtboxRadius);
        }
        if (state == SpiderState.Search)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
        }
    }
}