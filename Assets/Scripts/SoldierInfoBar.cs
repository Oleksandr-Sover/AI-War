using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoldierInfoBar : MonoBehaviour
{
    [SerializeField] private Slider roundsInMagazine;
    [SerializeField] private Slider healthy;
    [SerializeField] private Slider runEnergy;

    [HideInInspector] public SoldierMovement movement;
    [HideInInspector] public SoldierCondition condition;
    [HideInInspector] public Weapon weapon;

    [SerializeField] public GameObject soldier;

    private Vector3 barUpToPos;

    private float health;

    private int roundsInMagLate;

    void Start()
    {
        runEnergy.maxValue = movement.runEnergyTime;
        runEnergy.value = movement.runEnergyTime;
        roundsInMagazine.maxValue = weapon.maxRoundsInMagazin;
        roundsInMagazine.value = weapon.maxRoundsInMagazin;
        healthy.maxValue = condition.maxHealth;
        healthy.value = condition.maxHealth;
        barUpToPos = Vector3.up * 3;
    }

    void Update()
    {
        if (movement.runEnergyTimer < movement.runEnergyTime)
            runEnergy.value = movement.runEnergyTimer;
        if (roundsInMagLate != weapon.roundsInMagazin)
        {
            roundsInMagazine.value = weapon.roundsInMagazin;
            roundsInMagLate = weapon.roundsInMagazin;
        }
        if (health != condition.health)
        {
            healthy.value = condition.health;
            health = condition.health;
        }

        transform.position = Camera.main.WorldToScreenPoint(soldier.transform.position + barUpToPos);
    }
}
