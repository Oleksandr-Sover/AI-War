using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    public SoldierVision soldierVision;
    public ObjectsInTrigger objectsInTrigger;

    private Rigidbody rb;

    public Transform target;

    [HideInInspector] public Vector3 movement = Vector3.zero;
    [HideInInspector] public Vector3 crashedPoint;
    private Vector3 retreatDirection;
    private Vector3 directionToTarget;
    private Vector3 movementAround;

    [HideInInspector] public float movingTimer;
    public float advanceCoeff = 0.5f;
    public float walkVelocity = 2.8f;
    public float runEnergyTime = 8;
    private float sqrMaxVelocity;
    private float sqrRunVelocity;
    private float sqrWalkVelocity;
    private float forceVelocity;
    private float runForceVelocity;
    private float walkForceVelocity;
    private float brakeForceVelocity;
    private float runEnergyTimer;
    private float inputX;
    private float inputZ;
    private float randState;
    private float dot;
    public float backTimer;

    public int minNumOfFreeMov = 1;
    public int maxNumOfFreeMov = 6;
    private int numberOfFreeMovement;

    public bool setBack;
    public bool setSkip;
    public bool goAway;
    public bool backToTeam;
    public bool crashed;
    public bool moveUseKeyboard;
    public bool seeWhereGoing;
    public bool setBrakingValues = true;
    public bool setMovingToShelter = true;
    public bool setRightOrLeft = true;
    private bool setMoving = true;
    private bool setMovingToEnemy = true;
    private bool setMovingToTarget = true;
    private bool setMovingAround = true;
    private bool aroundToRight;
    [SerializeField] private bool rightMovement;
    [SerializeField] private bool advance;
    [SerializeField] private bool running;
    [SerializeField] private bool standing;
    [SerializeField] private bool forceRb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();        
    }

    void Start()
    {
        runEnergyTimer = runEnergyTime;
        walkForceVelocity = walkVelocity * 2.5f;
        runForceVelocity = walkForceVelocity * 1.2f;
        brakeForceVelocity = walkForceVelocity * 1.8f;
        sqrWalkVelocity = walkVelocity * walkVelocity;
        sqrRunVelocity = sqrWalkVelocity * 2 * 2;
        sqrMaxVelocity = sqrWalkVelocity;
        numberOfFreeMovement = Random.Range(minNumOfFreeMov, maxNumOfFreeMov);
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
        }

        else if (goAway)
            SkipOur(target.position);

        else if (soldierVision.findedNearestEnemy)
            AdvanceRetreatRegardingEnemy();

        else if (soldierVision.enemyWasVisible)
            AdvanceRetreatInSearch(soldierVision.targetLossPoint);

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
                SetRunning();
            }
        }

        else
            FreeMovement();

        RunningEnergy();

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, movement * forceVelocity, Color.magenta);
            Debug.DrawRay(transform.position, rb.velocity, Color.cyan);
        #endif
    }

    void FixedUpdate()
    {
        if (forceRb)
            rb.AddForce(movement * forceVelocity, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //when crash into a shelter that have not seen, move away from the shelter
        if (collision.collider.CompareTag("Structures"))
        {
            crashedPoint = collision.contacts[0].point;
            movement = crashedPoint - transform.position;
            movement = -movement.normalized;
            SetWalking();
            movingTimer = 1f;
            crashed = true;
            setRightOrLeft = true;
            soldierVision.aimedAtTarget = false;
        }   
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
            dot = SetRightOrLeftDot(target);

            if (dot > 0)
                movement = RightParallelToShelter();
            else
                movement = LeftParallelToShelter();
        }
        else
            Retreat(target);

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, target - transform.position, Color.green);
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

    private void AdvanceRetreatRegardingEnemy()
    {
        if (setMovingToEnemy)
        {
            setMovingToEnemy = false;
            setMovingAround = true;
            SetWalking();
            advance = AdvanceOrRetreat(advanceCoeff);
        }
        else if (advance)
        {
            if (soldierVision.sqrLengthNearEnemy < 6)
            {
                backTimer = 1;
                movement = (transform.position - soldierVision.target.position).normalized;
            }
            else if (backTimer > 0)
            {
                backTimer -= Time.deltaTime;

                if (soldierVision.nextToShelter)
                {
                    dot = SetRightOrLeftDot(soldierVision.target.position);

                    if (dot > 0)
                        movement = RightParallelToShelter();
                    else
                        movement = LeftParallelToShelter();
                    Debug.Log(dot);
                }
                else
                    movement = MovingAround(soldierVision.target.position);
                
            }
            else if (soldierVision.nextToShelter)
            {
                AdvanceNearShelter(soldierVision.target.position);
                Debug.Log("advance nea shelter");
            }
            else
                Advance(soldierVision.target.position);
        }
        else //retreat
        {
            if (soldierVision.nextToShelter)
                RetreatNearShelter(soldierVision.target.position);
            else
                //moving away from the enemy
                Retreat(soldierVision.target.position);
        }
    }

    private void AdvanceRetreatInSearch(Vector3 target)
    {
        if (setMovingToTarget)
        {
            setMovingToTarget = false;
            setMovingAround = true;
            SetWalking();
            advance = AdvanceOrRetreat(advanceCoeff);
        }
        else if (advance)
        {
            if (soldierVision.nextToShelter)
                MoveToPositionNearShelter(target);
            else if ((target - transform.position).sqrMagnitude < 6)
                movement = MovingAround(target);
            else
                Advance(target);
        }
        else //retreat
        {
            if (soldierVision.nextToShelter)
                RetreatNearShelter(target);
            else
                //moving away from the target
                Retreat(target);
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

            movingTimer = Random.Range(0.2f, 2f);

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
            dot = SetRightOrLeftDot(targetPosition);

            if (dot < 0)
                rightMovement = true;
            else
                rightMovement = false;
        }
        else if (rightMovement)
            movement = RightParallelToShelter();
        else
            movement = LeftParallelToShelter();
    }

    private void AdvanceNearShelter(Vector3 targetPosition)
    {
        directionToTarget = (targetPosition - transform.position).normalized;
        dot = Vector3.Dot(soldierVision.directionToNearestShelter.normalized, directionToTarget);

        if (dot > 0)
        {
            retreatDirection = RightParallelToShelter();
            dot = Vector3.Dot(retreatDirection, directionToTarget);

            if (dot > 0)
                movement = retreatDirection;
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
            dot = SetRightOrLeftDot(targetPosition);

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

    private float SetRightOrLeftDot(Vector3 targetPosition)
    {
        directionToTarget = (transform.position - targetPosition).normalized;
        retreatDirection = RightParallelToShelter();
        return Vector3.Dot(directionToTarget, retreatDirection);
    }

    private bool AdvanceOrRetreat(float advanceRate)
    {
        randState = Random.value;

        if (randState < advanceRate)
            return true;
        else
            return false;
    }

    private Vector3 RightParallelToShelter()
    {
        return (Quaternion.Euler(0, 90, 0) * soldierVision.directionToNearestShelter).normalized;
    }

    private Vector3 LeftParallelToShelter()
    {
        return (Quaternion.Euler(0, -90, 0) * soldierVision.directionToNearestShelter).normalized;
    }

    private void MovementNearShelter()
    {
        if (setMoving)
        {
            setMoving = false;
            seeWhereGoing = true;
            movingTimer = Random.Range(1f, 5f);
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
            seeWhereGoing = true;
            movingTimer = Random.Range(1f, 5f);
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
                runEnergyTimer += Time.deltaTime / 4;
        }
    }

    private void LeaveShelterOrStay(Vector3 position)
    {
        randState = Random.value;

        if (randState > 0.15) //stay in shelter
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
            Debug.Log("Num " + numberOfFreeMovement);
        }
    }

    private void MovementStateNearShelter()
    {
        randState = Random.value;

        if (randState > 0.8) //stand still
            SetStanding();
        else //move
            SetWalking();
    }

    private void MovementState()
    {
        randState = Random.value;

        if (randState < 0.8) //0.8
            SetWalking();
        else if (randState > 0.95) //0.95
            SetRunning();
        else
            SetStanding();
    }

    private void SetStanding()
    {
        standing = true;
        running = false;
    }

    private void SetWalking()
    {
        standing = false;
        running = false;
        forceVelocity = walkForceVelocity;
        sqrMaxVelocity = sqrWalkVelocity;
    }

    private void SetRunning()
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
