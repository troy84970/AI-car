using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject goalPrefab;
    private List<Vector3> boarderPoints = new List<Vector3>();
    void Start()
    {
        TextAsset textAsset = Resources.Load("軌道座標點") as TextAsset;
        string[] lines = textAsset.text.Split("\n");
        //i= 0,1,2 非軌道
        //goal left top pos
        string[] goalCoordinates = lines[1].Split(',');
        Vector3 goalLeftTopPos = new Vector3(float.Parse(goalCoordinates[0]), 1, float.Parse(goalCoordinates[1]));
        //goal right bottom pos
        goalCoordinates = lines[2].Split(',');
        Vector3 goalRightBottomPos = new Vector3(float.Parse(goalCoordinates[0]), 1, float.Parse(goalCoordinates[1]));
        //create goal line
        CreateGoalLine(goalLeftTopPos, goalRightBottomPos);
        //Wall border Pos
        for (int i = 3; i < lines.Length; i++)
        {
            string[] coordinates = lines[i].Split(',');
            float x = float.Parse(coordinates[0]);
            float z = float.Parse(coordinates[1]);
            boarderPoints.Add(new Vector3(x, 0, z));
        }
        for (int i = 0; i < boarderPoints.Count; i++)
        {
            Vector3 start = boarderPoints[i];
            Vector3 end = boarderPoints[(i + 1) % boarderPoints.Count];
            CreateWallSegment(start, end);
        }
    }

    void CreateWallSegment(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 midPoint = (startPoint + endPoint) / 2;
        float distance = Vector3.Distance(startPoint, endPoint);
        GameObject wallSegment;
        if (startPoint.x == endPoint.x)
            wallSegment = Instantiate(wallPrefab, midPoint, Quaternion.Euler(0, 90, 0));
        else
            wallSegment = Instantiate(wallPrefab, midPoint, Quaternion.Euler(0, 0, 0));
        wallSegment.transform.localScale = new Vector3(distance / 2 * 1.005f, 1, 1f);//prefab的x大小是兩單位
        wallSegment.transform.parent = transform;
    }
    void CreateGoalLine(Vector3 topLeft, Vector3 bottomRight)
    {
        Vector3 midPoint = new Vector3((topLeft.x + bottomRight.x) / 2, 1, bottomRight.z);
        float distance = Vector3.Distance(midPoint, bottomRight) * 2;
        GameObject goal;
        goal = Instantiate(goalPrefab, midPoint, Quaternion.Euler(0, 0, 0));
        goal.transform.localScale = new Vector3(distance, 2, 0.02f);
        goal.transform.parent = transform;
    }
}





