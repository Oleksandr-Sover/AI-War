using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoldierInfoBars : MonoBehaviour
{
    [SerializeField] private List<GameObject> infoBars = new List<GameObject>();

    [SerializeField] private GameObject soldier;

    [SerializeField] private GameObject infoBarPrefab;

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        foreach (var condition in FindObjectsOfType<SoldierCondition>())
        {
            infoBars.Add(Instantiate(infoBarPrefab, canvas.transform));
            infoBars.Last().GetComponent<SoldierInfoBar>().condition = condition;
            infoBars.Last().GetComponent<SoldierInfoBar>().soldier = condition.gameObject;
            infoBars.Last().GetComponent<SoldierInfoBar>().movement = condition.gameObject.GetComponent<SoldierMovement>();
            infoBars.Last().GetComponent<SoldierInfoBar>().weapon = condition.gameObject.GetComponentInChildren<Weapon>();
            condition.infoBar = infoBars.Last();
        }
    }
}
