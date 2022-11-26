using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierVision : MonoBehaviour
{
    public ObjectsInTrigger objectsInTrigger;
    public SoldierMovement soldierMovement;

    private List<Construction> tempVisibleConstruction = new List<Construction>();
    private List<Construction> tempConstructionsInTrigger = new List<Construction>();
    private List<Construction> removeConstructions = new List<Construction>();
    [HideInInspector] public List<Construction> visibleConstructions = new List<Construction>();

    private List<Vector3> visibleCorners = new List<Vector3>();
    private List<Vector3> tempVisibleCorners = new List<Vector3>();

    [HideInInspector] public Transform target;

    private RaycastHit hit;

    private Vector3 nearestCorner;
    private Vector3 tempNearestCorner;
    private Vector3 directionTarget;
    private Vector3 directionTargetHorizont;
    private Vector3 cornerPoint;
    private Vector3 preCornerPoint;
    private Vector3 point;
    private Vector3 direction;
    private Vector3 angularVelocity;
    private Vector3 angularVelocityRight;
    private Vector3 angularVelocityLeft;
    private Vector3 lookAroundPoint;
    private Vector3 movementPoint;
    private Vector3 cornerDirection;
    private Vector3 maxDotCorner;
    private Vector3 dirToCorner;
    private Vector3 searchDirection;
    [HideInInspector] public Vector3 oppositePoint;
    [HideInInspector] public Vector3 directionToNearestShelter;
    [HideInInspector] public Vector3 directionToNearestCorner;
    [HideInInspector] public Vector3 targetLossPoint;

    private Quaternion quaRotation;

    [SerializeField] private LayerMask layerStruct;
    [SerializeField] private LayerMask layerHit;

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
    private float lookWhereGoTimer;
    [SerializeField] private float angleFOV = 80;
    [SerializeField] private float speedAimRotation = 8;
    [SerializeField] private float maxLengthToShelter = 2;
    [SerializeField] private float minTimeLookWhereGo = 2;
    [SerializeField] private float maxTimeLookWhereGo = 8;
    [SerializeField] private float minTimeSearchEnemy = 10;
    [SerializeField] private float maxTimeSearchEnemy = 20;
    [SerializeField] private float minTimeInspAllCorner = 1.5f;
    [SerializeField] private float maxTimeInspAllCorner = 2.5f;
    [SerializeField] private float minTimeInspSomeCorner = 2;
    [SerializeField] private float maxTimeInspSomeCorner = 4;
    [SerializeField] private float minTimeFreeInspectSome = 2.5f;
    [SerializeField] private float maxTimeFreeInspectSome = 5;
    [SerializeField] private float minTimeFreeInspection = 1;
    [SerializeField] private float maxTimeFreeInspection = 5;
    [HideInInspector] public float sqrLengthNearEnemy = 0;

    private int cornerIndex;
    private int numberViewsShelters;
    private int numberFreeInspection;
    private int sqrEnemyHearingDis;
    [SerializeField] private int enemyHearingDis = 3;
    [SerializeField] private int valueOfAngularVelocity = 35;
    [SerializeField] private int minNumViewsShelters = 1;
    [SerializeField] private int maxNumViewsShelters = 4;
    [SerializeField] private int minNumFreeInspection = 2;
    [SerializeField] private int maxNumFreeInspection = 4;

    public bool aimedAtEnemy = false;
    public bool findedNearestEnemy;
    public bool enemyWasVisible;
    private bool setRandomAngleOfShelter = true;
    private bool setLookAroundValues = true;
    private bool setListsConstructAndCorner = true;
    [HideInInspector] public bool nextToShelter;
    [HideInInspector] public bool tooNearToShelter;
    [HideInInspector] public bool aimedAtTarget = false;
    [HideInInspector] public bool setSearchValues = true;

    void Start()
    {
        halfAngleFOV = angleFOV / 2;
        speedRotation = speedAimRotation / 4;
        numberViewsShelters = Random.Range(minNumViewsShelters, maxNumViewsShelters);
        numberFreeInspection = Random.Range(minNumFreeInspection, maxNumFreeInspection);
        minLengthToSelter = maxLengthToShelter / 2;
        maxSqrLengthToShelter = maxLengthToShelter * maxLengthToShelter;
        minSqrLengthToSelter = minLengthToSelter * minLengthToSelter;
        lookWhereGoTimer = Random.Range(minTimeLookWhereGo, maxTimeLookWhereGo);
        angularVelocityRight = new Vector3(0, valueOfAngularVelocity, 0);
        angularVelocityLeft = new Vector3(0, -valueOfAngularVelocity, 0);
        sqrEnemyHearingDis = enemyHearingDis * enemyHearingDis;
    }

    void Update()
    {
        if (objectsInTrigger.enemiesInTrigg.Count > 0)
            //find the nearest enemy in the field of view and visibility
            FindNearestEnemy(objectsInTrigger.enemiesInTrigg);
        else if (findedNearestEnemy)
        {
            aimedAtEnemy = false;
            findedNearestEnemy = false;
        }

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

        if (lookWhereGoTimer > 0)
            lookWhereGoTimer -= Time.deltaTime;

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
            targetLossPoint = target.position;
            aimedAtEnemy = AimRotation(target.position, speedAimRotation, 0.997f); //aiming at the nearest enemy
        }

        else if (lookWhereGoTimer < 0 && soldierMovement.movement != Vector3.zero)
            //periodically checks where are going
            SeeWhereGoing(minTimeLookWhereGo, maxTimeLookWhereGo);

        else if (enemyWasVisible)
        {
            aimedAtEnemy = false;
            //trace the position where the enemy disappeared  
            enemyWasVisible = SeeWhereEnemyMightAppear(targetLossPoint, minTimeSearchEnemy, maxTimeSearchEnemy);
        }

        else if (nextToShelter)
            //look around being near the shelter
            LookAraundShelter(oppositePoint);

        else if (visibleCorners.Count > 3)
            //inspect all visible corner of shelters one by one, randomly
            InspectRandomAllCorner(minTimeInspAllCorner, maxTimeInspAllCorner);

        else if (visibleCorners.Count > 0)
        {
            if (numberViewsShelters > 0)
                //inspect the corners randomly, a given number of times
                InspectRandomSomeCorner(minTimeInspSomeCorner, maxTimeInspSomeCorner);
            else
                //search inspection with random rotation, a given number of times
                FreeInspectBetweenInspection(minTimeFreeInspectSome, maxTimeFreeInspectSome);
        }

        else
            //search inspection with random rotation
            FreeInspection(minTimeFreeInspection, maxTimeFreeInspection);

        #if UNITY_EDITOR
            //draw borders of the review of the soldier
            //Debug.DrawRay(transform.position, BorderFOV(halfAngleFOV), Color.blue);
            //Debug.DrawRay(transform.position, BorderFOV(-halfAngleFOV), Color.blue);
            //draw the position of the enemy where it was visible
            Debug.DrawRay(targetLossPoint, Vector3.up * 5, Color.red);
        #endif
    }

    void LateUpdate()
    {
        if (setListsConstructAndCorner)
        {
            nearestCorner = tempNearestCorner;
            directionToNearestCorner = (nearestCorner - transform.position).normalized;
            visibleCorners.Clear();
            visibleCorners.AddRange(tempVisibleCorners);
            visibleConstructions.Clear();
            visibleConstructions.AddRange(tempVisibleConstruction);
            tempConstructionsInTrigger.Clear();
            tempConstructionsInTrigger.AddRange(objectsInTrigger.constructionsInTrigg);
            StartCoroutine(CreateVisibleConstructionsAndCorners(tempConstructionsInTrigger, tempVisibleConstruction, tempVisibleCorners));
        }
    }

    private void SeeWhereGoing(float minTime, float maxTime)
    {
        movementPoint = soldierMovement.movement + transform.position;

        if (AimRotation(movementPoint, speedRotation, 0.997f))
            lookWhereGoTimer = Random.Range(minTime, maxTime);

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, Vector3.up * 6, Color.magenta);
        #endif
    }

    private Vector3 BorderFOV(float halfAngle)
    {
        return Quaternion.Euler(0, halfAngle, 0) * transform.forward * objectsInTrigger.radiusVision;
    }

    private void FreeInspectBetweenInspection(float minTime, float maxTime)
    {
        if (rotationTimer < 0)
        {
            rotationTimer = Random.Range(minTime, maxTime);
            angularVelocity = RandomDirectionOfRotation();
            numberFreeInspection--;

            if (numberFreeInspection < 0)
                numberViewsShelters = Random.Range(minNumViewsShelters, maxNumViewsShelters);
        }
        else
            SearchRotation(angularVelocity);
    }

    private void FreeInspection(float minTime, float maxTime)
    {
        if (rotationTimer < 0)
        {
            rotationTimer = Random.Range(minTime, maxTime);
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
            enemySearchTimer = Random.Range(minTimer, maxTimer);
            soldierMovement.SetWalking();
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
                || RayHitTest(construction.ConstructCollider.transform, BorderFOV(halfAngleFOV), layerStruct)
                || RayHitTest(construction.ConstructCollider.transform, BorderFOV(-halfAngleFOV), layerStruct))
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
                    sqrDistanceToHit += 0.5f; //accuracy is reduced
                    
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

    private void InspectRandomSomeCorner(float minTimer, float maxTimer)
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

    private void InspectRandomAllCorner(float minTimer, float maxTimer)
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
        aimedAtEnemy = false;
        
        foreach (var coll in collidders)
        {
            if (!coll.CompareTag("Ded"))
            {
                directionTarget = coll.transform.position - transform.position;

                if (RayHitTest(coll.transform, directionTarget, layerHit))
                {
                    sqrLengthTest = directionTarget.sqrMagnitude;

                    if (sqrLengthTest < sqrEnemyHearingDis)
                        SetEnemyOptions(coll);
                    else if (IsVisibleInFOV(directionTarget))
                    {
                        if (sqrLengthTest < sqrLengthNearEnemy || sqrLengthNearEnemy == 0)
                            SetEnemyOptions(coll);
                    }
                }
            }
        }  
    }

    private void SetEnemyOptions(Collider coll)
    {
        findedNearestEnemy = true;
        sqrLengthNearEnemy = sqrLengthTest;
        target = coll.transform;
        targetLossPoint = target.position;

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, directionTarget, Color.red);
        #endif
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
    
    private bool RayHitTest(Transform tr, Vector3 dirTarget, LayerMask mask)
    {
        if (Physics.Raycast(transform.position, dirTarget, out hit, Mathf.Infinity, mask) && tr == hit.transform)
            return true;
        else
            return false;
    }

    private void OnDrawGizmos()
    {
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
                Gizmos.DrawRay(corner, Vector3.up * 6);
            }
        }
    }
}
