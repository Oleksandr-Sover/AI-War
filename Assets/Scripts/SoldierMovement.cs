using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    public SoldierVision soldierVision;
    public ObjectsInTrigger objectsInTrigger;
    private Team team;

    private PointsOfMoving pointsOfMoving;

    private List<Point> movingPoints;

    private Point nearestMovePoint;
    private Point previousPoint;
    private Point nextPoint;

    private Rigidbody rb;

    [SerializeField] private LayerMask layerStruct;
    [SerializeField] private LayerMask hitPointLayer;

    [HideInInspector] public Transform target;

    private RaycastHit hit;

    private Vector3 direction;
    private Vector3 directionToPoint;
    private Vector3 directionToGoal;
    private Vector3 directionToTarget;
    private Vector3 movementAround;
    private Vector3 pathToTarget;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 movement = Vector3.zero;
    [HideInInspector] public Vector3 crashedPoint;

    public float runEnergyTime = 8;
    private float movingTimer;
    private float backFromEnemyTimer;
    private float sqrMaxVelocity;
    private float sqrRunVelocity;
    private float sqrWalkVelocity;
    private float forceVelocity;
    private float runForceVelocity;
    private float walkForceVelocity;
    private float brakeForceVelocity;
    private float inputX;
    private float inputZ;
    private float randState;
    private float dot;
    private float goalDot;
    private float maxGoalDot;
    private float raycastTimer;
    private float sqrLenthToTarget;
    [SerializeField] private float walkVelocity = 2.6f;
    [SerializeField] private float stayInShelterCoeff = 0.85f;
    [SerializeField] private float standNearShelter = 0.2f;
    [SerializeField] private float minTimeFreeMovement = 1;
    [SerializeField] private float maxTimeFreeMovement = 6;
    [SerializeField] private float minTimeNearShelter = 2;
    [SerializeField] private float maxTimeNearShelter = 5;
    [SerializeField] private float minTimeMovingAround = 0.5f;
    [SerializeField] private float maxTimeMovingAround = 2;
    [SerializeField] private float minTimeBackFromEnemy = 1;
    [SerializeField] private float maxTimeBackFromEnemy = 2;
    [HideInInspector] public float backFromOurTimer;
    [HideInInspector] public float runEnergyTimer;

    private int raycastTime = 3;
    private int numberOfFreeMovement;
    private int sqrDisToReachTarget;
    private int sqrMaxDisToEnemy;
    [SerializeField] private int maxDisToEnemy = 4;
    [SerializeField] private int disToReachTarget = 4;
    [SerializeField] private int minNumOfFreeMov = 1;
    [SerializeField] private int maxNumOfFreeMov = 6;

    public bool advance;
    private bool setMoving = true;
    private bool setMovingToEnemy = true;
    private bool setMovingToTarget = true;
    private bool setMovingAround = true;
    private bool aroundToRight;
    private bool rightMovement;
    private bool forceRb;
    [SerializeField] private bool moveUseKeyboard;
    [HideInInspector] public bool running;
    [HideInInspector] public bool standing;
    [HideInInspector] public bool takePosition;
    [HideInInspector] public bool goAway;
    [HideInInspector] public bool backToTeam;
    [HideInInspector] public bool crashed;
    [HideInInspector] public bool setBrakingValues = true;
    [HideInInspector] public bool setMovingToShelter = true;
    [HideInInspector] public bool setRightOrLeft = true;
    [HideInInspector] public bool newGoal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        team = GetComponentInParent<Team>();
        pointsOfMoving = FindObjectOfType<PointsOfMoving>();
    }

    void Start()
    {
        runEnergyTimer = runEnergyTime;
        walkForceVelocity = walkVelocity * 2.5f;
        runForceVelocity = walkForceVelocity * 1.2f;
        brakeForceVelocity = walkForceVelocity * 2f;
        sqrWalkVelocity = walkVelocity * walkVelocity;
        sqrRunVelocity = sqrWalkVelocity * 2 * 2;
        sqrMaxVelocity = sqrWalkVelocity;
        numberOfFreeMovement = Random.Range(minNumOfFreeMov, maxNumOfFreeMov);
        raycastTimer = raycastTime;
        sqrDisToReachTarget = disToReachTarget * disToReachTarget;
        sqrMaxDisToEnemy = maxDisToEnemy * maxDisToEnemy;
    }

    void Update()
    {
        if (rb.velocity.sqrMagnitude < sqrMaxVelocity && !standing)
            forceRb = true;
        else
            forceRb = false;

        if (moveUseKeyboard)
            MovingUsingKeyboard();

        else if (crashed)
            OursCrashed();

        else if (soldierVision.nextToShelter && running)
        {
            //set values to stop in front of cover and not crash into it
            //value "running" is assigned false
            SetBrakes();
            setMoving = true;
            setMovingToEnemy = true;
            setRightOrLeft = true;
        }

        else if (soldierVision.tooNearToShelter)
        {
            if (setBrakingValues)
            {
                SetBrakes();
                setMoving = true;
                setMovingToEnemy = true;
            }
            movement = (soldierVision.oppositePoint - transform.position).normalized;
        }

        else if (goAway)
            SkipOur(target.position);

        else if (soldierVision.findedNearestEnemy)
            AdvanceRetreatRegardingEnemy();

        else if (soldierVision.enemyWasVisible)
            AdvanceRetreatInSearch(soldierVision.targetLossPoint);

        else if (takePosition)
        {
            MoveToPosition(BuildingRoute(position));
            takePosition = !GoalAchieved(position);
        }

        else if (backToTeam)
            MoveToPosition(target.position);

        else if (soldierVision.nextToShelter)
            MovementNearShelter();

        else if (soldierVision.visibleConstructions.Count > 0 && numberOfFreeMovement == 0)
        {
            if (setMovingToShelter)
            {
                setMovingToShelter = false;
                movement = soldierVision.directionToNearestShelter.normalized;
            }
        }

        else
            FreeMovement();

        RunningEnergy();

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, movement * forceVelocity, Color.magenta);
            Debug.DrawRay(transform.position, rb.velocity, Color.cyan);
            if (previousPoint != null)
            { 
                Debug.DrawRay(previousPoint.thisPoint.transform.position, Vector3.up * 10, Color.white);
            }
        #endif
    }

    void FixedUpdate()
    {
        if (forceRb)
        {
            rb.AddForce(CorrectYMovment(movement) * forceVelocity, ForceMode.Force);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //when crash into a shelter that have not seen, move away from the shelter
        if (collision.collider.CompareTag("Structures"))
        {
            SetWalking();
            crashedPoint = collision.contacts[0].point;
            movement = crashedPoint - transform.position;
            movement = -movement.normalized;
            movingTimer = 1;
            crashed = true;
            setRightOrLeft = true;
            soldierVision.aimedAtTarget = false;
        }
    }

    private Vector3 CorrectYMovment(Vector3 movement)
    {
        movement = movement + transform.position;
        movement.y = transform.position.y;
        return movement - transform.position;
    }

    private Vector3 BuildingRoute(Vector3 Goal)
    {
        if (newGoal)
        {
            newGoal = false;
            FindNearestMovingPoint();
            return Goal;
        }
        else if (GoalAchieved(nearestMovePoint.thisPoint.transform.position))
            return ChooseNextPoint(Goal);
        else
            return nearestMovePoint.thisPoint.transform.position;
    }

    private void FindNearestMovingPoint()
    {
        foreach (var point in pointsOfMoving.points)
            point.sqrDistance = (point.thisPoint.transform.position - transform.position).sqrMagnitude;

        //sort the list of points, from smallest to largest, by distance from the soldier
        movingPoints = pointsOfMoving.points.OrderBy(p => p.sqrDistance).ToList();

        foreach (var point in movingPoints) //set nearest visible moving point
        {
            directionToPoint = point.thisPoint.transform.position - transform.position;

            if (Physics.Raycast(transform.position, directionToPoint, out hit, Mathf.Infinity, hitPointLayer))
                if (hit.collider == point.thisPoint)
                {
                    nearestMovePoint = point;
                    break;
                }
        }
    }

    private Vector3 ChooseNextPoint(Vector3 goal)
    {
        maxGoalDot = 0;

        if (nearestMovePoint.neighborPoints.Count > 2)
        {
            for (int i = 0; i < nearestMovePoint.neighborPoints.Count; i++)
            {
                if (nearestMovePoint.neighborPoints[i] != previousPoint)
                {
                    directionToGoal = (goal - nearestMovePoint.thisPoint.transform.position).normalized;
                    goalDot = Vector3.Dot(directionToGoal, nearestMovePoint.directionsToPoint[i]);

                    if (goalDot > maxGoalDot || maxGoalDot == 0)
                    {
                        maxGoalDot = goalDot;
                        nextPoint = nearestMovePoint.neighborPoints[i];
                    }
                }
            }
            previousPoint = nearestMovePoint;
            nearestMovePoint = nextPoint;
        }
        else
        {
            foreach (var neighPoint in nearestMovePoint.neighborPoints)
            {
                if (neighPoint != previousPoint)
                {
                    previousPoint = nearestMovePoint;
                    nearestMovePoint = neighPoint;
                    break;
                }
            }
        }
        return nearestMovePoint.thisPoint.transform.position;
    }

    private void OursCrashed()
    {
        if (movingTimer > 0)
            movingTimer -= Time.deltaTime;
        else
        {
            crashed = false;
            setMoving = true;
            setMovingToEnemy = true;
        }
    }

    private void SkipOur(Vector3 target)
    {
        if (soldierVision.nextToShelter)
        {
            dot = RightOrLeftDot(target);

            if (dot > 0)
            {
                movement = RightParallelToShelter();
                rightMovement = true;
            }
            else 
            { 
                movement = LeftParallelToShelter();
                rightMovement = false;
            }
        }
        else
            Retreat(target);

        if (backFromOurTimer > 0)
            backFromOurTimer -= Time.deltaTime;

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, target - transform.position, Color.gray);
        #endif
    }

    private void MoveToPosition(Vector3 target)
    {
        if (soldierVision.nextToShelter)
            MoveToPositionNearShelter(target);
        else
            Advance(target);

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, target - transform.position, Color.green);
        #endif
    }

    private bool GoalAchieved(Vector3 target)
    {
        sqrLenthToTarget = (target - transform.position).sqrMagnitude;

        if (sqrLenthToTarget < sqrDisToReachTarget)
            return true;
        else
            return false;
    }

    private void AdvanceRetreatRegardingEnemy()
    {
        if (setMovingToEnemy)
        {
            setMovingToEnemy = false;
            setMovingAround = true;
            SetWalking();
            advance = AdvanceOrRetreat();
        }
        else if (advance)
        {
            if (soldierVision.sqrLengthNearEnemy < sqrMaxDisToEnemy)
            {
                backFromEnemyTimer = Random.Range(minTimeBackFromEnemy, maxTimeBackFromEnemy);
                movement = (transform.position - soldierVision.target.position).normalized;
            }
            else if (backFromEnemyTimer > 0)
            {
                backFromEnemyTimer -= Time.deltaTime;

                if (soldierVision.nextToShelter)
                {
                    dot = RightOrLeftDot(soldierVision.target.position);

                    if (dot > 0)
                        movement = RightParallelToShelter();
                    else
                        movement = LeftParallelToShelter();
                }
                else
                    movement = MovingAround(soldierVision.target.position);

                #if UNITY_EDITOR
                    Debug.DrawRay(transform.position, transform.position - soldierVision.target.position, Color.white);
                #endif
            }
            else if (soldierVision.nextToShelter)
                AdvanceNearShelter(soldierVision.target.position);
            else
                Advance(soldierVision.target.position);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, Vector3.up * 4, Color.red);
            #endif
        }
        else //retreat
        {
            if (soldierVision.nextToShelter)
                RetreatNearShelter(soldierVision.target.position);
            else
                //moving away from the enemy
                Retreat(soldierVision.target.position);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, Vector3.up * 4, Color.green);
            #endif
        }
    }

    private void AdvanceRetreatInSearch(Vector3 target)
    {
        if (setMovingToTarget)
        {
            setMovingToTarget = false;
            setMovingAround = true;
            SetWalking();
            advance = AdvanceOrRetreat();
        }
        else if (advance)
        {
            MoveToPosition(target);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, Vector3.up * 4, Color.red);
            #endif
        }
        else //retreat
        {
            if (soldierVision.nextToShelter)
                RetreatNearShelter(target);
            else
                //moving away from the target
                Retreat(target);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, Vector3.up * 4, Color.green);
            #endif
        }
    }

    private void Advance(Vector3 target)
    {
        movementAround = MovingAround(target);
        movement = (target - transform.position).normalized;
        movement = (movement + movementAround).normalized;
    }

    private void Retreat(Vector3 target)
    {
        movementAround = MovingAround(target);
        movement = (transform.position - target).normalized;
        movement = (movement + movementAround).normalized;
    }

    private Vector3 MovingAround(Vector3 target)
    {
        if (setMovingAround)
        {
            setMovingAround = false;
            randState = Random.value;

            if (randState > 0.5)
                aroundToRight = true;
            else
                aroundToRight = false;

            movingTimer = Random.Range(minTimeMovingAround, maxTimeMovingAround);

            return Vector3.zero;
        }
        else if (movingTimer > 0)
        {
            movingTimer -= Time.deltaTime;

            if (aroundToRight)
                return Vector3.Cross(transform.position - target, Vector3.up).normalized;
            else
                return Vector3.Cross(target - transform.position, Vector3.up).normalized;
        }
        else
        {
            setMovingAround = true;
            return Vector3.zero;
        }
    }

    private void MoveToPositionNearShelter(Vector3 targetPosition)
    {
        if (setRightOrLeft)
        {
            setRightOrLeft = false;
            dot = RightOrLeftDot(targetPosition);

            if (dot < 0)
                rightMovement = true;
            else
                rightMovement = false;
        }
        else if (rightMovement)
            LeaveShelterOrMoveNearShelter(RightParallelToShelter(), RightParallelToShelter(72), targetPosition);
        else
            LeaveShelterOrMoveNearShelter(LeftParallelToShelter(), LeftParallelToShelter(-72), targetPosition);
    }

    private void LeaveShelterOrMoveNearShelter(Vector3 parallelDirection, Vector3 turnDirection, Vector3 targetPosition)
    {
        if (LeaveShelter(targetPosition))
            AdvanceNearShelter(targetPosition);
        else
        {
            dot = Vector3.Dot(soldierVision.directionToNearestShelter, soldierVision.directionToNearestCorner);

            if (dot > 0.8)
                movement = turnDirection; //turn the corner
            else
                movement = parallelDirection;
        }
    }

    private bool LeaveShelter(Vector3 targetPosition)
    {
        if (raycastTimer > 0)
        {
            raycastTimer -= Time.deltaTime;
            return false;
        }
        else
        {
            pathToTarget = targetPosition - transform.position;

            if (Physics.Raycast(transform.position, pathToTarget, Mathf.Infinity, layerStruct))
            {
                raycastTimer = raycastTime;
                return false;                
            }
            else
                return true;
        }
    }

    private void AdvanceNearShelter(Vector3 targetPosition)
    {
        directionToTarget = (targetPosition - transform.position).normalized;
        dot = Vector3.Dot(soldierVision.directionToNearestShelter.normalized, directionToTarget);
       
        if (dot > 0)
        {
            direction = RightParallelToShelter();
            dot = Vector3.Dot(direction, directionToTarget);

            if (dot > 0)
                movement = direction;
            else
                movement = LeftParallelToShelter();
        }
        else
            movement = directionToTarget;
    }

    private void RetreatNearShelter(Vector3 targetPosition)
    {
        if (setRightOrLeft)
        {
            setRightOrLeft = false;
            dot = RightOrLeftDot(targetPosition);

            if (dot > 0)
                rightMovement = true;
            else
                rightMovement = false;
        }
        else if (rightMovement)
            movement = RightParallelToShelter();
        else
            movement = LeftParallelToShelter();
    }

    private float RightOrLeftDot(Vector3 targetPosition)
    {
        directionToTarget = (transform.position - targetPosition).normalized;
        direction = RightParallelToShelter();
        return Vector3.Dot(directionToTarget, direction);
    }

    public bool AdvanceOrRetreat()
    {
        randState = Random.value;

        if (randState < team.advanceCoeff)
            return true;
        else
            return false;
    }

    private Vector3 RightParallelToShelter(int angle = 90)
    {
        return (Quaternion.Euler(0, angle, 0) * soldierVision.directionToNearestShelter).normalized;
    }

    private Vector3 LeftParallelToShelter(int angle = -90)
    {
        return (Quaternion.Euler(0, angle, 0) * soldierVision.directionToNearestShelter).normalized;
    }

    private void MovementNearShelter()
    {
        if (setMoving)
        {
            setMoving = false;
            movingTimer = Random.Range(minTimeNearShelter, maxTimeNearShelter);
            LeaveShelterOrStay(soldierVision.oppositePoint);            
        }
        else if (movingTimer > 0)
            movingTimer -= Time.deltaTime;
        else
            setMoving = true;
    }

    private void FreeMovement()
    {
        if (setMoving)
        {
            setMoving = false;
            movingTimer = Random.Range(minTimeFreeMovement, maxTimeFreeMovement);
            inputX = Random.Range(-1f, 1f);
            inputZ = Random.Range(-1f, 1f);
            movement = new Vector3(inputX, 0, inputZ);
            movement = movement.normalized;
            MovementState();

            if (numberOfFreeMovement > 0)
                numberOfFreeMovement--;
        }
        else if (movingTimer > 0)
            movingTimer -= Time.deltaTime;
        else
            setMoving = true;
    }

    private void RunningEnergy()
    {
        if (running)
        {
            if (runEnergyTimer > 0)
                runEnergyTimer -= Time.deltaTime;
            else
                SetWalking();
        }
        else if (runEnergyTimer < runEnergyTime)
        {
            if (standing)
                runEnergyTimer += Time.deltaTime;
            else
                runEnergyTimer += Time.deltaTime / 6;
        }
    }

    private void LeaveShelterOrStay(Vector3 position)
    {
        randState = Random.value;

        if (randState < stayInShelterCoeff) //stay in shelter
        {
            movement = soldierVision.TurnVectorRightOrLeft(transform.position, position, 90).normalized;
            MovementStateNearShelter();
        }
        else //leave shelter
        {
            SetWalking();
            movement = (position - transform.position).normalized;
            numberOfFreeMovement = Random.Range(minNumOfFreeMov, maxNumOfFreeMov);
            setMovingToShelter = true;
            movingTimer = 2;
        }
    }

    private void MovementStateNearShelter()
    {
        randState = Random.value;

        if (randState < standNearShelter) //stand still
            SetStanding();
        else //move
            SetWalking();
    }

    private void MovementState()
    {
        randState = Random.value;

        if (randState < 0.75) //0.8
            SetWalking();
        else if (randState > 0.9) //0.95
            SetRunning();
        else
            SetStanding();
    }

    private void SetStanding()
    {
        standing = true;
        running = false;
    }

    public void SetWalking()
    {
        standing = false;
        running = false;
        forceVelocity = walkForceVelocity;
        sqrMaxVelocity = sqrWalkVelocity;
    }

    public void SetRunning()
    {
        standing = false;
        running = true;
        forceVelocity = runForceVelocity;
        sqrMaxVelocity = sqrRunVelocity;
    }

    private void SetBrakes()
    {
        standing = false;
        running = false;
        setBrakingValues = false;
        forceVelocity = brakeForceVelocity;
        movement = -rb.velocity.normalized;
    }

    private void MovingUsingKeyboard()
    {
        forceVelocity = walkForceVelocity;
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
        movement = new Vector3 (inputX, 0, inputZ);
        movement = movement.normalized;
    }
}
