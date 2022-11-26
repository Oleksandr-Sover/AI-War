using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsInTrigger : MonoBehaviour
{
    private Structures structures;

    [HideInInspector] public List<Collider> enemiesInTrigg = new List<Collider>();

    [HideInInspector] public List<Construction> constructionsInTrigg = new List<Construction>();

    private List<Collider> constructionsEnter = new List<Collider>();
    private List<Collider> constructionsExit = new List<Collider>();

    private SphereCollider triggerVision;

    private string ourNameTag;

    public float radiusVision = 10f;

    [HideInInspector] public int numTeamsLayer;

    private bool enter;
    private bool exit;

    void Awake()
    {
        triggerVision = GetComponent<SphereCollider>();
        structures = FindObjectOfType<Structures>();
    }

    void Start()
    {
        ourNameTag = GetComponentInParent<SoldierCondition>().ourNameTag;
        triggerVision.radius = radiusVision;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Structures"))
        {
            enter = true;
            constructionsEnter.Add(other);
        }
        else if (other.gameObject.layer == numTeamsLayer && !other.CompareTag(ourNameTag) && !other.CompareTag("Ded"))
            enemiesInTrigg.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Structures"))
        {
            exit = true;
            constructionsExit.Add(other);
        }
        else if (other.gameObject.layer == numTeamsLayer && !other.CompareTag(ourNameTag))
            enemiesInTrigg.Remove(other);
    }

    void LateUpdate()
    {
        if (enter)
        {
            foreach (var coll in constructionsEnter)
            {
                foreach (var construct in structures.constructions)
                {
                    if (coll == construct.ConstructCollider)
                        constructionsInTrigg.Add(construct);
                }
            }
            constructionsEnter.Clear();
            enter = false;
        }

        if (exit)
        {
            foreach (var coll in constructionsExit)
            {
                foreach (var construct in structures.constructions)
                {
                    if (coll == construct.ConstructCollider)
                        constructionsInTrigg.Remove(construct);
                }
            }
            constructionsExit.Clear();
            exit = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (constructionsInTrigg.Count > 0)
        {
            foreach (var construct in constructionsInTrigg)
            {
                Gizmos.DrawSphere(construct.ConstructCollider.transform.position, 0.4f);
            }
        }
    }
}
