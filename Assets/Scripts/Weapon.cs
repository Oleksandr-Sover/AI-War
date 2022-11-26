using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private SoldierVision soldierVision;
    private SoldierMovement soldierMovement;

    [SerializeField] private GameObject hitPrefab;
    [SerializeField] private GameObject hitEnemyPrefab;

    private AudioSource audioSource;

    [SerializeField] private ParticleSystem muzzleFlash;

    private List<GameObject> hitEffects = new List<GameObject>();
    private List<GameObject> hitEnemyEffects = new List<GameObject>();

    private RaycastHit hit;

    private Vector3 shotDirection = Vector3.forward;

    private float weaponDeflectAlongX;
    private float weaponDeflectAlongZ;
    private float shotTime;
    private float shotTimer;
    private float ammoReloadTimer;
    [SerializeField] private float shotsPerSec = 3;
    [SerializeField] private float weaponDeflectRun = 15;
    [SerializeField] private float weaponDeflectWalk = 8;
    [SerializeField] private float weaponDeflectStand = 4;
    [SerializeField] private float ammoReloadTime = 2;
    [SerializeField] private float shotDamage = 8;

    private bool instanceEffect;

    public int maxRoundsInMagazin = 20;
    private int numTeamsLayer;
    [HideInInspector] public int roundsInMagazin;

    void Awake()
    {
        soldierVision = GetComponentInParent<SoldierVision>();
        soldierMovement = GetComponentInParent<SoldierMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        numTeamsLayer = gameObject.layer;
        shotTime = 1 / shotsPerSec;
        roundsInMagazin = maxRoundsInMagazin;
        ammoReloadTimer = ammoReloadTime;
    }

    void Update()
    {
        if (soldierVision.aimedAtEnemy)
        {
            if (roundsInMagazin <= 0)
            {
                AmmoReload();
            }
            else if (shotTimer < 0)
            {
                Shot();
                shotTimer = shotTime;
            }
            else
                shotTimer -= Time.deltaTime;
        }
        else if (roundsInMagazin < 2)
            AmmoReload();
    }

    private void AmmoReload()
    {
        if (ammoReloadTimer > 0)
        {
            soldierMovement.advance = false;
            ammoReloadTimer -= Time.deltaTime;
        }
        else
        {
            soldierMovement.advance = soldierMovement.AdvanceOrRetreat();
            roundsInMagazin = maxRoundsInMagazin;
            ammoReloadTimer = ammoReloadTime;
        }
    }

    private void Shot()
    {
        roundsInMagazin--;

        shotDirection = soldierVision.transform.rotation * Vector3.forward;
        SetDeflection();
        shotDirection = Quaternion.Euler(weaponDeflectAlongX, 0, weaponDeflectAlongZ) * shotDirection;
        transform.rotation = Quaternion.LookRotation(shotDirection);

        if (muzzleFlash.isStopped)
            muzzleFlash.Play();

        audioSource.Play();

        if (Physics.Raycast(transform.position, shotDirection, out hit))
        {
            if (hit.collider.gameObject.layer == numTeamsLayer)
            {
                HitControl(hitEnemyEffects, hitEnemyPrefab);
                hit.collider.gameObject.GetComponent<SoldierCondition>().health -= shotDamage;
            }
            else
                HitControl(hitEffects, hitPrefab);
        }
    }

    private void SetDeflection()
    {
        if (soldierMovement.running)
        {
            weaponDeflectAlongX = Random.Range(-weaponDeflectRun, weaponDeflectRun);
            weaponDeflectAlongZ = Random.Range(-weaponDeflectRun, weaponDeflectRun);
        }
        else if (soldierMovement.standing)
        {
            weaponDeflectAlongX = Random.Range(-weaponDeflectStand, weaponDeflectStand);
            weaponDeflectAlongZ = Random.Range(-weaponDeflectStand, weaponDeflectStand);
        }
        else
        {
            weaponDeflectAlongX = Random.Range(-weaponDeflectWalk, weaponDeflectWalk);
            weaponDeflectAlongZ = Random.Range(-weaponDeflectWalk, weaponDeflectWalk);
        }
    }

    private void HitControl(List<GameObject> hitsEffect, GameObject hitPrefab)
    {
        instanceEffect = true;

        foreach (var hitEffect in hitsEffect)
        {
            if (!hitEffect.GetComponent<ParticleSystem>().isPlaying)
            {
                hitEffect.transform.position = hit.point;
                hitEffect.transform.rotation = transform.rotation * Quaternion.Euler(0, 135, 0);
                hitEffect.GetComponent<ParticleSystem>().Play();
                instanceEffect = false;
                break;
            }
        }
        if (instanceEffect)
        {
            hitsEffect.Add(Instantiate(hitPrefab, hit.point, transform.rotation * Quaternion.Euler(0, 135, 0)));
            hitsEffect.Last().GetComponent<ParticleSystem>().Play();
        }

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, hit.point - transform.position, Color.red);
        #endif
    }
}
