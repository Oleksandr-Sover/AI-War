using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsOfMoving : MonoBehaviour
{
    public List<Point> points = new List<Point>();
    private List<Point> failPoints = new List<Point>();

    [SerializeField] private LayerMask layerMask;

    private RaycastHit hit;

    private Vector3 distance;
    private Vector3 compareDis;
    private Vector3 direction;
    private Vector3 compareDir;

    private float dot;
    private float sqrDistance;
    private float compareSqrDis;



    void Awake()
    {
        foreach (var point in GetComponentsInChildren<Collider>())
        {
            points.Add(new Point(point));
        }
    }

    void Start()
    {
        foreach (var point in points)
        {
            foreach (var comparePoint in points)
            {
                if (point != comparePoint)
                {
                    distance = comparePoint.thisPoint.transform.position - point.thisPoint.transform.position;

                    if (Physics.Raycast(point.thisPoint.transform.position, distance, out hit, Mathf.Infinity, layerMask))
                    {
                        if (hit.collider == comparePoint.thisPoint)
                        {
                            point.neighborPoints.Add(comparePoint);
                        }
                    }
                }
            }
        }

        foreach (var point in points)
        {
            failPoints.Clear();

            foreach (var neighPoint in point.neighborPoints)
            {
                distance = neighPoint.thisPoint.transform.position - point.thisPoint.transform.position;
                sqrDistance = distance.sqrMagnitude;
                direction = distance.normalized;

                foreach (var compareNeighPoint in point.neighborPoints)
                {
                    if (neighPoint != compareNeighPoint)
                    {
                        compareDis = compareNeighPoint.thisPoint.transform.position - point.thisPoint.transform.position;
                        compareSqrDis = compareDis.sqrMagnitude;
                        compareDir = compareDis.normalized;

                        dot = Vector3.Dot(direction, compareDir);

                        if (dot > 0.5)
                        {
                            if (compareSqrDis > sqrDistance) 
                                failPoints.Add(compareNeighPoint);
                        }
                    }
                }
            }
            foreach (var failPoint in failPoints)
            {
                if (point.neighborPoints.Contains(failPoint))
                    point.neighborPoints.Remove(failPoint);
            }
        }
        foreach (var point in points)
        {
            foreach (var neighPoint in point.neighborPoints)
            {
                direction = (neighPoint.thisPoint.transform.position - point.thisPoint.transform.position).normalized;
                point.directionsToPoint.Add(direction);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var point in points)
        {
            foreach (var neighbPoint in point.neighborPoints)
            {
                Gizmos.DrawRay(point.thisPoint.transform.position, (neighbPoint.thisPoint.transform.position - point.thisPoint.transform.position).normalized * 2);
            }
        }
        //UnityEditor.Handles.Label(point.thisPoint.transform.position, point.dots.Count.ToString());
    }
}

public class Point
{
    public Collider thisPoint;
    public List<Point> neighborPoints;
    public List<Vector3> directionsToPoint;
    public float sqrDistance;

    public Point(Collider thisPoint)
    {
        this.thisPoint = thisPoint;
        neighborPoints = new List<Point>();
        directionsToPoint = new List<Vector3>();
    }
}