using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoldierVision : MonoBehaviour
{
    public ObjectsInTrigger objectsInTrigger;
    public SoldierMovement soldierMovement;
    public Structures structures;

    [HideInInspector] public List<Construction> visibleConstructions = new List<Construction>();
    private List<Construction> tempVisibleConstruction = new List<Construction>();
    private List<Construction> tempConstructionsInTrigger = new List<Construction>();
    private List<Construction> removeConstructions = new List<Construction>();

    [HideInInspector] public List<Vector3> visibleCorners = new List<Vector3>();
    private List<Vector3> tempVisibleCorners = new List<Vector3>();

    [HideInInspector] public Transform target;

    private RaycastHit hit;

    [HideInInspector] public Vector3 oppositePoint;
    [HideInInspector] public Vector3 directionToNearestShelter;
    [HideInInspector] public Vector3 nearestCorner;
    [HideInInspector] public Vector3 nearestPoint;
    [HideInInspector] public Vector3 targetLossPoint;
    [HideInInspector] public Vector3 searchDirection;
    private Vector3 tempNearestCorner;
    private Vector3 directionTarget;
    private Vector3 directionTargetHorizont;
    private Vector3 cornerPoint;
    private Vector3 preCornerPoint;
    private Vector3 point;
    private Vector3 direction;
    private Vector3 angularVelocity;
    private Vector3 angularVelocityRight = new Vector3(0, 35f, 0);
    private Vector3 angularVelocityLeft = new Vector3(0, -35f, 0);
    
    private Vector3 lookAroundPoint;
    private Vector3 movementPoint;

    private Vector3 cornerDirection;
    private Vector3 maxDotCorner;
    private Vector3 dirToCorner;
    
    private Quaternion quaRotation;

    [SerializeField] private LayerMask layerStruct;

    [HideInInspector] public float sqrLengthNearEnemy = 0;
    public float angleFOV = 80;
    public float speedAimRotation = 8;
    public float maxLengthToShelter = 2;
    private float sqrLengthToShelter = 0;
    private float enemySearchTimer;
    private float minLengthToSelter;
    private float maxSqrLengthToShelter;
    private float minSqrLengthToSelter;
    private float sqrLengthToShelterTest;
    private float sqrLengthTest = 0;
    private float speedRotation;
    private float rotationTimer;
    private float dot;
    private float maxDot;
    private float halfAngleFOV;
    private float angleTarget;
    private float randValue;
    private float searchDistance;
    private float sqrDistanceToCorner;
    private float sqrDistanceToHit;
    private float sqrMinDistanceToCorner;

    private int cornerIndex;
    private int numberViewsShelters;
    private int numberFreeInspection;
    public int minNumViewsShelters = 1;
    public int maxNumViewsShelters = 4;
    public int minNumFreeInspection = 2;
    public int maxNumFreeInspection = 4;

    public bool aimedAtEnemy = false;
    public bool nextToShelter;
    public bool tooNearToShelter;
    public bool findedNearestEnemy;
    public bool aimedAtTarget = false;
    public bool enemyWasVisible;
    private bool setSearchValues = true;
    private bool setRandomAngleOfShelter = true;
    private bool setLookAroundValues = true;
    private bool setListsConstructAndCorner = true;

    void Start()
    {
        halfAngleFOV = angleFOV / 2;
        speedRotation = speedAimRotation / 4;
        numberViewsShelters = Random.Range(minNumViewsShelters, maxNumViewsShelters);
        numberFreeInspection = Random.Range(minNumFreeInspection, maxNumFreeInspection);
        minLengthToSelter = maxLengthToShelter / 2;
        maxSqrLengthToShelter = maxLengthToShelter * maxLengthToShelter;
        minSqrLengthToSelter = minLengthToSelter * minLengthToSelter;
    }

    void Update()
    {
        if (objectsInTrigger.enemiesInTrigg.Count > 0)
            //find the nearest enemy in the field of view and visibility
            FindNearestEnemy(objectsInTrigger.enemiesInTrigg);
        else if (findedNearestEnemy)
            findedNearestEnemy = false;

        if (visibleConstructions.Count > 0)
        {
           //if the soldier is approaching the shelter, we pass the opposite point of the shelter
           //for inspection
           IsItNearConstruction(visibleConstructions, maxSqrLengthToShelter, minSqrLengthToSelter);
        }
        else
        {
            nextToShelter = false;
            tooNearToShelter = false;
        }

        if (soldierMovement.crashed)
        {
            if (!aimedAtTarget)
                //look at the crash site
                aimedAtTarget = AimRotation(soldierMovement.crashedPoint, speedRotation, 0.93f);
            else
            {
                movementPoint = soldierMovement.movement + transform.position;
                //look in the direction of movement
                AimRotation(movementPoint, speedRotation, 0.93f);
            }
        }

        else if (findedNearestEnemy)
        {
            enemyWasVisible = true;
            setSearchValues = true;
            //aiming at the nearest enemy
            aimedAtEnemy = AimRotation(target.position, speedAimRotation, 0.997f);
        }

        else if (enemyWasVisible)
            //trace the position where the enemy disappeared  
            enemyWasVisible = SeeWhereEnemyMightAppear(target.position, 100f, 200f);

        else if (soldierMovement.seeWhereGoing && soldierMovement.movement != Vector3.zero)
        {
            movementPoint = soldierMovement.movement + transform.position;
            //look in the direction of movement
            if (AimRotation(movementPoint, speedRotation, 0.95f))
                soldierMovement.seeWhereGoing = false;
        }

        else if (nextToShelter)
            //look around being near the shelter
            LookAraundShelter(oppositePoint);

        else if (visibleCorners.Count > 3)
            //inspect all visible corner of shelters one by one, randomly
            InspectRandomCorner(1.5f, 2.5f);

        else if (visibleCorners.Count > 0)
        //
        {
            if (numberViewsShelters > 0)
                InspectRandomCornerOfStructures(2f, 4f);
            else
                FreeInspectBetweenInspection();
        }

        else
            //search inspection with random rotation
            FreeInspection();

        #if UNITY_EDITOR
            //draw borders of the review of the soldier
            Debug.DrawRay(transform.position, BorderFOV(halfAngleFOV), Color.blue);
            Debug.DrawRay(transform.position, BorderFOV(-halfAngleFOV), Color.blue);
            //
            Debug.DrawRay(transform.position, directionToNearestShelter, Color.yellow);
        #endif
    }

    void LateUpdate()
    {
        if (setListsConstructAndCorner)
        {
            nearestCorner = tempNearestCorner;
            visibleCorners.Clear();
            visibleCorners.AddRange(tempVisibleCorners);
            visibleConstructions.Clear();
            visibleConstructions.AddRange(tempVisibleConstruction);
            tempConstructionsInTrigger.Clear();
            tempConstructionsInTrigger.AddRange(objectsInTrigger.constructionsInTrigg);
            StartCoroutine(CreateVisibleConstructionsAndCorners(tempConstructionsInTrigger, tempVisibleConstruction, tempVisibleCorners));
        }
    }

    private Vector3 BorderFOV(float halfAngle)
    {
        return Quaternion.Euler(0, halfAngle, 0) * transform.forward * objectsInTrigger.radiusVision;
    }

    private void FreeInspectBetweenInspection()
    {
        if (rotationTimer < 0)
        {
            rotationTimer = Random.Range(2.5f, 5f);
            angularVelocity = RandomDirectionOfRotation();
            numberFreeInspection--;

            if (numberFreeInspection < 0)
                numberViewsShelters = Random.Range(minNumViewsShelters, maxNumViewsShelters);
        }
        else
            SearchRotation(angularVelocity);
    }

    private void FreeInspection()
    {
        if (rotationTimer < 0)
        {
            rotationTimer = Random.Range(1f, 5f);
            angularVelocity = RandomDirectionOfRotation();
        }
        else
            SearchRotation(angularVelocity);
    }

    private Quaternion TurnRightOrLeftRandomly(float angle)
    {
        randValue = Random.value;

        if (randValue > 0.5)
            return Quaternion.Euler(0, angle, 0);
        else
            return Quaternion.Euler(0, -angle, 0);
    }

    public Vector3 TurnVectorRightOrLeft(Vector3 posAxesRotat, Vector3 position, float angle)
    {
        position -= posAxesRotat; //get a direction vector
        return TurnRightOrLeftRandomly(angle) * position; //rotate this vector
    }

    private void LookAraundShelter(Vector3 point)
    {
        if (setLookAroundValues)
        {
            setLookAroundValues = false;
            lookAroundPoint = TurnVectorRightOrLeft(transform.position, point, 70);
            lookAroundPoint += transform.position; //get the point position
        }
        else
            setLookAroundValues = AimRotation(lookAroundPoint, speedRotation, 0.99f);
    }

    private bool SeeWhereEnemyMightAppear(Vector3 targetLoc, float minTimer, float maxTimer)
    {
        if (setSearchValues)
        {
            aimedAtEnemy = false;
            aimedAtTarget = false;
            setSearchValues = false;
            targetLossPoint = new Vector3(targetLoc.x, targetLoc.y, targetLoc.z);
            enemySearchTimer = Random.Range(minTimer, maxTimer);
            return true;
        }
        else if (enemySearchTimer > 0)
        {
            enemySearchTimer -= Time.deltaTime;
            searchDirection = targetLossPoint - transform.position;
            searchDistance = searchDirection.magnitude;

            if (Physics.Raycast(transform.position, searchDirection, out hit, searchDistance, layerStruct))
            {
                if (visibleCorners.Count > 0)
                {
                    maxDot = 0;

                    foreach (var corner in visibleCorners)
                    {
                        cornerDirection = corner - transform.position;

                        dot = Vector3.Dot(searchDirection.normalized, cornerDirection.normalized);

                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            maxDotCorner = corner;
                        }
                    }
                    RotateToTarget(maxDotCorner, speedRotation);

                    #if UNITY_EDITOR
                        Debug.DrawRay(transform.position, maxDotCorner - transform.position, Color.yellow);
                    #endif
                }
            }
            else
                RotateToTarget(targetLossPoint, speedRotation);

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, searchDirection, Color.white);
            #endif
            return true;
        }
        else
        {
            setSearchValues = true;
            return false;
        } 
    }

    private void IsItNearConstruction(List<Construction> visibleConstruct, float sqrDistanceMax, float sqrDistanceMin)
    {
        nextToShelter = false;
        tooNearToShelter = false;
        sqrLengthToShelter = 0;

        foreach (var construction in visibleConstruct)
        {
            point = construction.ConstructCollider.ClosestPoint(transform.position);
            direction = point - transform.position;
            sqrLengthToShelterTest = direction.sqrMagnitude;

            if (sqrLengthToShelterTest < sqrLengthToShelter || sqrLengthToShelter == 0)
            {
                sqrLengthToShelter = sqrLengthToShelterTest;
                directionToNearestShelter = direction;
                nearestPoint = point;
            }
        }

        if (sqrLengthToShelter < sqrDistanceMin)
        {
            tooNearToShelter = true;
            oppositePoint = GetOppositePoint(directionToNearestShelter);
        }
        else if (sqrLengthToShelter < sqrDistanceMax)
        {
            nextToShelter = true;
            soldierMovement.setBrakingValues = true;
            oppositePoint = GetOppositePoint(directionToNearestShelter);
        }
        else
            soldierMovement.setRightOrLeft = true;
    }

    private Vector3 GetOppositePoint(Vector3 direction)
    {
        direction *= -1;
        return direction + transform.position;
    }

    private IEnumerator CreateVisibleConstructionsAndCorners(List<Construction> constructInTrigg, List<Construction> tempVisibleConstruct, List<Vector3> tempVisibleCorners)
    {
        setListsConstructAndCorner = false;

        removeConstructions.Clear();

        foreach (var construction in tempVisibleConstruct)
        {
            if (!constructInTrigg.Contains(construction))
                removeConstructions.Add(construction);
        }
        foreach (var construction in removeConstructions)
            tempVisibleConstruct.Remove(construction);
        yield return null;

        foreach (var construction in constructInTrigg)
        {
            if (!tempVisibleConstruct.Contains(construction))
            {
                point = construction.ConstructCollider.ClosestPoint(transform.position);
                direction = point - transform.position;

                if (IsVisibleInFOV(direction)
                || RayHitTest(construction.ConstructCollider.transform, BorderFOV(halfAngleFOV))
                || RayHitTest(construction.ConstructCollider.transform, BorderFOV(-halfAngleFOV)))
                    tempVisibleConstruct.Add(construction);
            }
        }
        yield return null;

        tempVisibleCorners.Clear();
        sqrMinDistanceToCorner = 0;

        foreach (var construction in tempVisibleConstruct)
        {
            foreach (var corner in construction.Corners)
            {
                dirToCorner = corner - transform.position;

                if (Physics.Raycast(transform.position, dirToCorner, out hit, Mathf.Infinity, layerStruct))
                {
                    sqrDistanceToCorner = dirToCorner.sqrMagnitude;
                    sqrDistanceToHit = (hit.point - transform.position).sqrMagnitude;
                    sqrDistanceToHit += 0.2f; //accuracy is reduced

                    if (sqrDistanceToHit > sqrDistanceToCorner)
                    {
                        tempVisibleCorners.Add(corner);

                        if (sqrMinDistanceToCorner > sqrDistanceToCorner || sqrMinDistanceToCorner == 0)
                        {
                            sqrMinDistanceToCorner = sqrDistanceToCorner;
                            tempNearestCorner = corner;
                        }
                    }
                }
                yield return null;
            }
        }
        setListsConstructAndCorner = true;
    }

    private void InspectRandomCornerOfStructures(float minTimer, float maxTimer)
    {
        if (setRandomAngleOfShelter)
        {
            setRandomAngleOfShelter = false;
            rotationTimer = Random.Range(minTimer, maxTimer);
            ChooseCornerOfStructures(visibleCorners);

            if (numberFreeInspection < 0)
                numberFreeInspection = Random.Range(minNumFreeInspection, maxNumFreeInspection);
        }
        else if (rotationTimer > 0)
        {
            RotateToTarget(cornerPoint, speedRotation);
            rotationTimer -= Time.deltaTime;

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, cornerPoint - transform.position, Color.yellow);
        #endif
        }
        else
        {
            setRandomAngleOfShelter = true;
            if (numberViewsShelters > 0)
                numberViewsShelters--;
        }
    }

    private void InspectRandomCorner(float minTimer, float maxTimer)
    {
        if (setRandomAngleOfShelter)
        {
            setRandomAngleOfShelter = false;
            rotationTimer = Random.Range(minTimer, maxTimer);
            ChooseCornerOfStructures(visibleCorners);
        }
        else if (rotationTimer > 0)
        {
            RotateToTarget(cornerPoint, speedRotation);
            rotationTimer -= Time.deltaTime;

            #if UNITY_EDITOR
                Debug.DrawRay(transform.position, cornerPoint - transform.position, Color.yellow);
            #endif
        }
        else
            setRandomAngleOfShelter = true;
    }

    private void ChooseCornerOfStructures(List<Vector3> visibleCorners)
    {
        cornerIndex = Random.Range(0, visibleCorners.Count - 1);
        cornerPoint = visibleCorners[cornerIndex];
        cornerPoint.y = transform.position.y;

        if (preCornerPoint == cornerPoint && visibleCorners.Count > 1)
        {
            if (cornerIndex != visibleCorners.Count - 1)
            {
                cornerPoint = visibleCorners[cornerIndex + 1];
                cornerPoint.y = transform.position.y;
            }
            else
            {
                cornerPoint = visibleCorners[cornerIndex - 1];
                cornerPoint.y = transform.position.y;
            }
        }
        preCornerPoint = cornerPoint;
    }

    private Vector3 RandomDirectionOfRotation()
    {
        randValue = Random.value;

        if (randValue > 0.5)
            return angularVelocityLeft;
        else
            return angularVelocityRight;
    }

    private void SearchRotation(Vector3 anglVelocity)
    {
        transform.Rotate(anglVelocity * Time.deltaTime);
        rotationTimer -= Time.deltaTime;
    }

    private void FindNearestEnemy(List<Collider> collidders)
    {
        sqrLengthNearEnemy = 0;
        findedNearestEnemy = false;
        
        foreach (var coll in collidders)
        {
            directionTarget = coll.transform.position - transform.position;

            if (IsVisibleInFOV(directionTarget) && RayHitTest(coll.transform, directionTarget))
            {
                sqrLengthTest = directionTarget.sqrMagnitude;

                #if UNITY_EDITOR
                    Debug.DrawRay(transform.position, directionTarget, Color.red);
                #endif

                if (sqrLengthTest < sqrLengthNearEnemy || sqrLengthNearEnemy == 0)
                {
                    sqrLengthNearEnemy = sqrLengthTest;
                    target = coll.transform;
                    findedNearestEnemy = true;
                }
            }
        }
    }

    private bool AimRotation(Vector3 position, float speedRot, float dotPrecision)
    {
        RotateToTarget(position, speedRot);
        dot = Mathf.Abs(Quaternion.Dot(transform.rotation, quaRotation));

        if (dot > dotPrecision)
            return true;
        else
            return false;
    }

    private void RotateToTarget(Vector3 position, float speedRot)
    {
        directionTarget = position - transform.position;
        quaRotation = Quaternion.LookRotation(directionTarget);
        transform.rotation = Quaternion.Lerp(transform.rotation, quaRotation, speedRot * Time.deltaTime);

        //rotation lock on the X and Z axis
        directionTargetHorizont = transform.eulerAngles;
        directionTargetHorizont.x = 0;
        directionTargetHorizont.z = 0;
        transform.eulerAngles = directionTargetHorizont;
    }

    private bool IsVisibleInFOV(Vector3 dirTarget)
    {
        angleTarget = Vector3.Angle(dirTarget, transform.forward);

        if (angleTarget < halfAngleFOV)
            return true;
        else
            return false;
    }
    
    private bool RayHitTest(Transform tr, Vector3 dirTarget)
    {
        if (Physics.Raycast(transform.position, dirTarget, out hit) && tr == hit.transform)
            return true;
        else
            return false;
    }

    private void OnDrawGizmos()
    {
        if (objectsInTrigger.ourTeamInTrigg.Count > 0)
        {
            Gizmos.color = Color.green;

            foreach (var coll in objectsInTrigger.ourTeamInTrigg)
            {
                Vector3 dir = coll.transform.position - transform.position;
                if (IsVisibleInFOV(dir) && RayHitTest(coll.transform, dir))
                    Gizmos.DrawRay(transform.position, dir);
            }
        }

        if (visibleConstructions.Count > 0)
        {
            Gizmos.color = Color.green;

            foreach (var construction in visibleConstructions)
            {
                Gizmos.DrawWireSphere(construction.ConstructCollider.transform.position, 0.5f);
            }
        }

        if (visibleCorners.Count > 0)
        {
            Gizmos.color = Color.red;

            foreach (var corner in visibleCorners)
            {
                Gizmos.DrawRay(corner, Vector3.up * 4);
            }
        }
    }
}
