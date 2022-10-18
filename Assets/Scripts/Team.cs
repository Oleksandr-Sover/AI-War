using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public List<SoldierMovement> soldiersMovement = new List<SoldierMovement>();

    public float sqrDisToBroMin = 6;
    public float sqrDisToBroMax = 255;
    private float sqrDistance;

    private Vector3 distance;

    private bool disControlWorking = false;

    void Awake()
    {
        foreach (var soldier in GetComponentsInChildren<SoldierMovement>())
        {
            soldiersMovement.Add(soldier);
        }
    }

    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (!disControlWorking)
            StartCoroutine(DistanceControl());   
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
                    }
                    else if (sqrDistance > sqrDisToBroMax)
                    {
                        soldier.goAway = false;
                        soldier.backToTeam = true;
                        soldier.target = soldierCompair.transform;
                    }
                    else
                    {
                        soldier.goAway = false;
                        soldier.backToTeam = false;
                    }
                }
                yield return null;
            }
        }
        disControlWorking = false;
    }
}
