using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointsDrawer : MonoBehaviour
{
    public List<Vector3> wayPoints;
    private LineRenderer wayPointRenderer;
    void Awake()
    {
        wayPoints = new List<Vector3>();
        wayPointRenderer = gameObject.AddComponent<LineRenderer>();
        wayPointRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }
    void Update()
    {
        DrawPoints();
    }
    public void ClearWayPoint()
    {
        wayPoints.Clear();
    }
    public void AddPoint(Vector3 vector3)
    {
        wayPoints.Add(vector3);
        wayPointRenderer.positionCount += 1;
    }
    void DrawPoints()
    {
        wayPointRenderer.positionCount = wayPoints.Count;
        for (int i = 0; i < wayPointRenderer.positionCount; i++)
        {
            wayPointRenderer.SetPosition(i, wayPoints[i]);
        }
    }
}
