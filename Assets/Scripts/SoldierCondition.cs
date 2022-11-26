using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCondition : MonoBehaviour
{
    private Team team;
    private ObjectsInTrigger trigger;
    private SoldierVision vision;
    private SoldierMovement movement;

    [HideInInspector] public GameObject infoBar;

    private MeshRenderer meshRenderer;

    private Canvas canvas;

    private Rigidbody rb;

    [HideInInspector] public string ourNameTag;

    public float maxHealth = 100;
    private float lastHealth;
    private float deltaHealth;
    private float oneFifthOfHaelth;
    private float deltaTimer;
    private float deltaTime = 1;
    [HideInInspector] public float health;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trigger = GetComponentInChildren<ObjectsInTrigger>();
        trigger.numTeamsLayer = gameObject.layer;
        vision = GetComponentInChildren<SoldierVision>();
        movement = GetComponentInChildren<SoldierMovement>();
        canvas = GetComponentInChildren<Canvas>();
        meshRenderer = GetComponent<MeshRenderer>();
        team = GetComponentInParent<Team>();
    }

    void Start()
    {
        health = maxHealth;
        gameObject.tag = ourNameTag;
        meshRenderer.material = team.material;
        oneFifthOfHaelth = maxHealth / 5;
        lastHealth = maxHealth;
        deltaTimer = deltaTime;
    }

    void Update()
    {
        SetRetreatWhenManyHits();

        if (health <= 0)
            SoldierDead();

        else if (health < oneFifthOfHaelth && vision.findedNearestEnemy)
        {
            movement.advance = false;
            movement.SetRunning();
        }
    }

    private void SetRetreatWhenManyHits()
    {
        if (deltaTimer > 0) 
        { 
            deltaTimer -= Time.deltaTime;
        }
        else
        {
            deltaTimer = deltaTime;
            deltaHealth = lastHealth - health;

            if (deltaHealth > 0) 
            {
                lastHealth = health;

                if (deltaHealth > oneFifthOfHaelth)
                {
                    movement.advance = false;
                    movement.SetRunning();
                }
            }
        }
    }

    private void SoldierDead()
    {
        rb.freezeRotation = false;
        infoBar.SetActive(false);
        vision.gameObject.SetActive(false);
        trigger.gameObject.SetActive(false);
        movement.enabled = false;
        vision.enabled = false;
        rb.mass = 0;
        gameObject.tag = "Ded";
    }
}
