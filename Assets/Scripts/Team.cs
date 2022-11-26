using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team : MonoBehaviour
{
    private List<SoldierMovement> soldiersMovement = new List<SoldierMovement>();
    private List<SoldierVision> soldiersVision = new List<SoldierVision>();

    private List<VisibleEnemyPosition> visiblePositionOfEnemies = new List<VisibleEnemyPosition>();

    private TerrainCollider terrain;

    public Material material;

    private RaycastHit hit;

    private Ray ray;

    private Vector3 distance;
    private Vector3 position;
    private Vector3 nearestTarget;
    private Vector3 dirToPoint;

    public float advanceCoeff = 0.99f;
    private float sqrDistance;
    private float sqrLenth;
    private float sqrLenthMin;
    private float sqrDisToBroMin;
    private float sqrDisToBroMax;
    [SerializeField] private float disToBroMin = 2.5f;
    [SerializeField] private float disToBroMax = 20;
    [SerializeField] private float maxDisFromPosition = 3;
    [SerializeField] private float minTimeBackFromOur = 1f;
    [SerializeField] private float maxTimeBackFromOur = 3f;

    private int sqrDisToReachTarget;
    [SerializeField] private int disToReachTarget = 4;

    private bool disControlWorking = false;
    private bool targetFinded;
    private bool addToList;
    private bool transmisOfEnemyPosWorking = false;
    [SerializeField] private bool positionWithMouse;

    private class VisibleEnemyPosition
    {
        public Transform target;
        public Vector3 lastVisiblePosition;
        public bool positionChecked;

        public VisibleEnemyPosition(Transform target, Vector3 lastVisiblePosition, bool positionChecked)
        {
            this.target = target;
            this.lastVisiblePosition = lastVisiblePosition;
            this.positionChecked = positionChecked;
        }
    }

    void Awake()
    {
        foreach (var soldier in GetComponentsInChildren<SoldierMovement>())
            soldiersMovement.Add(soldier);

        foreach (var vision in GetComponentsInChildren<SoldierVision>())
            soldiersVision.Add(vision);

        foreach (var condition in GetComponentsInChildren<SoldierCondition>())
            condition.ourNameTag = gameObject.tag;

        terrain = FindObjectOfType<TerrainCollider>();
    }

    void Start()
    {
        sqrDisToBroMin = disToBroMin * disToBroMin;
        sqrDisToBroMax = disToBroMax * disToBroMax;
        sqrDisToReachTarget = disToReachTarget * disToReachTarget;
    }

    void Update()
    {
        if (positionWithMouse)
        {
            if (GetMousePosition())
            {
                foreach (var soldier in soldiersMovement)
                {
                    soldier.newGoal = true;
                    soldier.takePosition = true;
                    soldier.setRightOrLeft = true;
                    soldier.SetWalking();
                    soldier.position.x = position.x + Random.Range(-maxDisFromPosition, maxDisFromPosition);
                    soldier.position.z = position.z + Random.Range(-maxDisFromPosition, maxDisFromPosition);
                    soldier.position.y = soldier.transform.position.y;
                }
            }

            #if UNITY_EDITOR
                Debug.DrawRay(position, Vector3.up * 3, Color.white);
            #endif
        }
    }

    void LateUpdate()
    {
        if (!disControlWorking)
            StartCoroutine(DistanceControl());
        if (!transmisOfEnemyPosWorking)
            StartCoroutine(TransmissionOfEnemyPositions());
    }

    private IEnumerator TransmissionOfEnemyPositions()
    {
        transmisOfEnemyPosWorking = true;

        foreach (var vision in soldiersVision)
        {
            if (vision.enabled)
            {
                if (vision.findedNearestEnemy)
                {
                    addToList = true;

                    foreach (var enemy in visiblePositionOfEnemies)
                    {
                        if (vision.target == enemy.target)
                        {
                            addToList = false;
                            enemy.lastVisiblePosition = vision.targetLossPoint;
                            enemy.positionChecked = false;
                            break;
                        }
                    }
                    if (addToList)
                        visiblePositionOfEnemies.Add(new VisibleEnemyPosition(vision.target, vision.targetLossPoint, false));
                }
                else
                {
                    sqrLenthMin = 0;
                    targetFinded = false;

                    foreach (var enemy in visiblePositionOfEnemies)
                    {
                        if (!enemy.positionChecked)
                        {
                            sqrLenth = (enemy.lastVisiblePosition - vision.transform.position).sqrMagnitude;

                            if (sqrLenth < sqrDisToReachTarget)
                            {
                                enemy.positionChecked = true;
                                vision.enemyWasVisible = false;
                            }
                            else if (sqrLenthMin > sqrLenth || sqrLenthMin == 0)
                            {
                                sqrLenthMin = sqrLenth;
                                targetFinded = true;
                                nearestTarget = enemy.lastVisiblePosition;
                            }
                        }
                        else if (vision.targetLossPoint == enemy.lastVisiblePosition)
                            vision.enemyWasVisible = false;
                    }
                    if (targetFinded)
                    {
                        vision.targetLossPoint = nearestTarget;

                        if (!vision.enemyWasVisible)
                        {
                            vision.enemyWasVisible = true;
                            vision.setSearchValues = true;
                        }
                    }
                }
            }
            yield return null;
        }
        transmisOfEnemyPosWorking = false;
    }

    private IEnumerator DistanceControl()
    {
        disControlWorking = true;

        foreach (var soldier in soldiersMovement)
        {
            if (soldier.enabled)
            {
                foreach (var soldierCompair in soldiersMovement)
                {
                    if (soldier != soldierCompair && soldierCompair.enabled)
                    {
                        distance = soldierCompair.transform.position - soldier.transform.position;
                        sqrDistance = distance.sqrMagnitude;

                        if (sqrDistance < sqrDisToBroMin)
                        {
                            soldier.goAway = true;
                            soldier.backToTeam = false;
                            soldier.target = soldierCompair.transform;
                            soldier.backFromOurTimer = Random.Range(minTimeBackFromOur, maxTimeBackFromOur);
                            soldier.SetWalking();
                        }
                        else if (sqrDistance > sqrDisToBroMax)
                        {
                            soldier.goAway = false;
                            soldier.backToTeam = true;
                            soldier.target = soldierCompair.transform;
                            soldier.SetWalking();
                        }
                        else
                        {
                            if (soldier.backFromOurTimer < 0)
                                soldier.goAway = false;

                            soldier.backToTeam = false;
                        }
                    }
                }
            }
            yield return null;
        }
        disControlWorking = false;
    }

    private bool GetMousePosition()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (terrain.Raycast(ray, out hit, 1000))
                position = hit.point;

            return true;
        }
        else
            return false;
    }
}
