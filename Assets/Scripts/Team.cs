using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [HideInInspector] public List<SoldierMovement> soldiersMovement = new List<SoldierMovement>();
    private List<SoldierVision> soldiersVision = new List<SoldierVision>();

    private List<VisibleEnemyPosition> visiblePositionOfEnemies = new List<VisibleEnemyPosition>();

    private TerrainCollider terrain;

    private RaycastHit hit;

    private Ray ray;

    private Vector3 distance;
    private Vector3 position;
    private Vector3 nearestTarget;

    public float sqrDisToBroMin = 6;
    public float sqrDisToBroMax = 255;
    public float maxDisFromPosition = 3;
    private float sqrDistance;
    private float sqrLenth;
    private float sqrLenthMin;

    public bool positionWithMouse;
    private bool disControlWorking = false;
    private bool newPosition;
    private bool targetFinded;
    private bool addToList;
    private bool transmisOfEnemyPosWorking = false;

    void Awake()
    {
        foreach (var soldier in GetComponentsInChildren<SoldierMovement>())
            soldiersMovement.Add(soldier);

        foreach (var vision in GetComponentsInChildren<SoldierVision>())
            soldiersVision.Add(vision);

        terrain = FindObjectOfType<TerrainCollider>();
    }

    void Update()
    {
        if (positionWithMouse)
        {
            GetMousePosition();

            if (newPosition)
            {
                newPosition = false;

                foreach (var soldier in soldiersMovement)
                {
                    soldier.takePosition = true;
                    soldier.setRightOrLeft = true;
                    soldier.SetWalking();
                    soldier.position.x = position.x + Random.Range(-maxDisFromPosition, maxDisFromPosition);
                    soldier.position.z = position.z + Random.Range(-maxDisFromPosition, maxDisFromPosition);
                    soldier.position.y = soldier.transform.position.y;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!disControlWorking)
            StartCoroutine(DistanceControl());
        if (!transmisOfEnemyPosWorking)
            StartCoroutine(TransmissionOfEnemyPositions());
    }

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

    private IEnumerator TransmissionOfEnemyPositions()
    {
        transmisOfEnemyPosWorking = true;

        foreach (var vision in soldiersVision)
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

                        if (sqrLenth < 16)
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
                    targetFinded = false;
                    vision.targetLossPoint = nearestTarget;

                    if (!vision.enemyWasVisible)
                    {
                        vision.enemyWasVisible = true;
                        vision.setSearchValues = true;
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
            foreach (var soldierCompair in soldiersMovement)
            {
                if (soldier != soldierCompair)
                {
                    distance = soldierCompair.transform.position - soldier.transform.position;
                    sqrDistance = distance.sqrMagnitude;

                    if (sqrDistance < sqrDisToBroMin)
                    {
                        soldier.goAway = true;
                        soldier.backToTeam = false;
                        soldier.target = soldierCompair.transform;
                        soldier.backFromOurTimer = Random.Range(1f, 3f);
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
                yield return null;
            }
        }
        disControlWorking = false;
    }

    private void GetMousePosition()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (terrain.Raycast(ray, out hit, 1000))
                position = hit.point;

            newPosition = true;
        }
        #if UNITY_EDITOR
            Debug.DrawRay(position, Vector3.up * 3, Color.white);
        #endif
    }
}
