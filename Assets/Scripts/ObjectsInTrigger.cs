using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsInTrigger : MonoBehaviour
{
    public Structures structures;

    [HideInInspector] public List<Collider> enemiesInTrigg = new List<Collider>();
    [HideInInspector] public List<Collider> ourTeamInTrigg = new List<Collider>(); //temporary list
    private List<Collider> constructionsEnter = new List<Collider>();
    private List<Collider> constructionsExit = new List<Collider>();

    [HideInInspector] public List<Construction> constructionsInTrigg = new List<Construction>();

    private SphereCollider triggerVision;

    public string ourNameTag;

    public float radiusVision = 10f;

    private bool enter;
    private bool exit;

    void Awake()
    {
        triggerVision = GetComponent<SphereCollider>();
        structures = FindObjectOfType<Structures>();
    }

    void Start()
    {
        ourNameTag = transform.parent.tag;
        triggerVision.radius = radiusVision;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ourNameTag))
            ourTeamInTrigg.Add(other);
        else if (other.CompareTag("Structures"))
        {
            enter = true;
            constructionsEnter.Add(other);
        }
        else
            enemiesInTrigg.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(ourNameTag))
            ourTeamInTrigg.Remove(other);
        else if (other.CompareTag("Structures"))
        {
            exit = true;
            constructionsExit.Add(other);
        }
        else
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
