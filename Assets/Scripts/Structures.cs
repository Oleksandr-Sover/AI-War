using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structures : MonoBehaviour
{
    [HideInInspector] public List<Construction> constructions = new List<Construction>();

    [SerializeField] private Collider[] colliders;
    private MeshFilter meshFilter;
    private Mesh mesh;

    private float sqrMinDistaceToCorner;
    private float sqrDistaceToCorner;
    [SerializeField] private float minDistaceToCorner = 0.5f;

    private bool setCorner;

    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        sqrMinDistaceToCorner = minDistaceToCorner * minDistaceToCorner;

        List<Construction> tempConstructions = new List<Construction>();

        foreach (var coll in colliders)
        {
            //create a temporary list of structures, which includes all the corners of each element of the structure
            tempConstructions.Add(new Construction(coll, CreateCornerPointsForCollider(coll)));
        }

        foreach (var construction in tempConstructions)
        {
            //create a new structural element in the final list of structures, with an empty list of structural corners
            constructions.Add(new Construction(construction.ConstructCollider, new List<Vector3>()));

            foreach (var corner in construction.Corners)
            {
                setCorner = true;

                foreach (var compairConstruction in tempConstructions)
                {
                    //ignore the current construction, because it does not participate in comparison with itself
                    if (compairConstruction.ConstructCollider != construction.ConstructCollider)
                    {
                        foreach (var compairCorner in compairConstruction.Corners)
                        {
                            //calculate the square of the length between the compared angles
                            sqrDistaceToCorner = (corner - compairCorner).sqrMagnitude;
                            //if the distance between the given corner and the compared corner is less than the minimum,
                            //then this corner is not a corner of the structure group
                            if (sqrDistaceToCorner < sqrMinDistaceToCorner)
                                setCorner = false;
                        }
                    }
                }
                if (setCorner)
                    //add an angle to the list, which is an angle of a group of structures
                    constructions.Last().Corners.Add(new Vector3(corner.x, corner.y, corner.z));
            }
        }
        //call garbage collection
        GC.Collect();
    }

    private List<Vector3> CreateCornerPointsForCollider(Collider coll)
    {
        meshFilter = coll.GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        List<Vector3> colliderCorners = new List<Vector3>();

        //for imported objects from 3D visualization programs, where X, Y changes to X, Z
        if (coll.transform.rotation == Quaternion.Euler(-90, 0, 0))
        {
            //get the first corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.max.x, mesh.bounds.max.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count - 1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]);
            //get the second corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count - 1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]);
            //get the third corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.max.x, mesh.bounds.min.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count - 1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]);
            //get the fourth corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.min.x, mesh.bounds.max.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count - 1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]);
        }
        else
        {
            //get the first corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.max.x, mesh.bounds.min.y, mesh.bounds.max.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count -1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count -1]);
            //get the second corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count -1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]); 
            //get the third corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.max.x, mesh.bounds.min.y, mesh.bounds.min.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count -1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]); 
            //get the fourth corner of the collider section
            colliderCorners.Add(new Vector3(mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.max.z));
            //convert to world coordinates
            colliderCorners[colliderCorners.Count - 1] = coll.transform.TransformPoint(colliderCorners[colliderCorners.Count - 1]);
        }

        return colliderCorners;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (constructions.Count > 0)
        {
            foreach (var construction in constructions)
            {
                foreach (var corners in construction.Corners)
                {
                    //draw marks on all corners of the group of structures
                    Gizmos.DrawSphere(corners, 0.15f);
                }
            }
        }
    }
}

public class Construction
{
    public Collider ConstructCollider;
    public List<Vector3> Corners;

    public Construction(Collider collider, List<Vector3> corners)
    {
        ConstructCollider = collider;
        Corners = corners;
    }
}

