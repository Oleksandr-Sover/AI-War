using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoBar : MonoBehaviour
{
    [SerializeField] private Slider roundsInMagazine;
    [SerializeField] private Slider healthy;
    [SerializeField] private Slider runEnergy;

    private SoldierMovement soldier;
    private SoldierCondition condition;
    [SerializeField] private Weapon weapon;

    private float health;

    private int roundsInMagLate;

    void Awake()
    {
        soldier = GetComponentInParent<SoldierMovement>();
        condition = GetComponentInParent<SoldierCondition>();
    }

    void Start()
    {
        runEnergy.maxValue = soldier.runEnergyTime;
        runEnergy.value = soldier.runEnergyTime;
        roundsInMagazine.maxValue = weapon.maxRoundsInMagazin;
        roundsInMagazine.value = weapon.maxRoundsInMagazin;
        healthy.maxValue = condition.maxHealth;
        healthy.value = condition.maxHealth;
    }

    void Update()
    {
        if (soldier.runEnergyTimer < soldier.runEnergyTime)
            runEnergy.value = soldier.runEnergyTimer;
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
    }
}
