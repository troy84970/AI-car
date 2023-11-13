using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra;
public class Car : MonoBehaviour
{
    //unity前方0度與pdf中的說明有異，需將輸出or期望輸出扣除90度才是Unity角度
    Vector3 resetPosition = new Vector3(0, 0.25f, 0);
    Quaternion resetDegree = Quaternion.Euler(0, 0, 0);
    private LineRenderer lineRenderer;
    bool isCollideWall = false;
    bool isGoal = false;
    public LayerMask wallLayer;
    public int rayIndex = 0;
    public float steeringWheelDegree = 0;
    private double deltax;
    private double deltaz;//2d中的y
    public float speed = 1;
    private Transform carTransform;
    public Text dL, dM, dR;
    public PointsDrawer pointsDrawer;
    private MLP mlp;
    private float frontD, leftD, rightD;
    public Text trainingText;
    public Dropdown dataSelectDropdown;
    private int dropdownOption;
    public DataWriter dataWriter;
    void Awake()
    {

        carTransform = transform;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.positionCount = 6;
        deltax = 0;
        deltaz = 0;
        Reset();
        //mlp = new MLP(3);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isGoal && GameController.isStart && !GameController.isTraining)
        {
            frontD = Vector3.Distance(CastRay(0), carTransform.position);
            rightD = Vector3.Distance(CastRay(45), carTransform.position);
            leftD = Vector3.Distance(CastRay(-45), carTransform.position);
            UpdateDistanceText(leftD, frontD, rightD);
            if (isCollideWall) Reset();
            UpdateSteeringWheelDegree();
            Move();
        }

    }
    public void Train()
    {
        GameController.isTraining = true;
        trainingText.text = "訓練中....";
        dropdownOption = dataSelectDropdown.value;
        if (dropdownOption == 0)
        {
            dropdownOption = 0;
            mlp = new MLP(3);
        }
        else if (dropdownOption == 1)
        {
            dropdownOption = 1;
            mlp = new MLP(5);
        }
        GameController.isTraining = false;
        trainingText.text = "訓練完成!";
    }
    void UpdateSteeringWheelDegree()
    {
        if (dropdownOption == 0)
        {
            Vector<double> v = Vector<double>.Build.Dense(3);
            v[0] = frontD;
            v[1] = rightD;
            v[2] = leftD;
            steeringWheelDegree = (float)(-0.5 * (float)mlp.Predict(v));
            dataWriter.Write4d(v, -steeringWheelDegree);
        }
        else if (dropdownOption == 1)
        {
            Vector<double> v = Vector<double>.Build.Dense(5);
            v[0] = carTransform.position.x;
            v[1] = carTransform.position.z;
            v[2] = frontD;
            v[4] = leftD;
            v[3] = rightD;
            steeringWheelDegree = (float)(-0.5 * (float)mlp.Predict(v));
            dataWriter.Write4d(v, -steeringWheelDegree);
        }
    }
    void Move()
    {
        //cos(pi/2)有精確度問題
        float ydegree = 90 - carTransform.eulerAngles.y;//水平夾角
        deltax = (Math.Cos(ydegree * Math.PI / 180 + steeringWheelDegree * Math.PI / 180) + Math.Sin(steeringWheelDegree * Math.PI / 180) * Math.Sin(ydegree * Math.PI / 180)) * Time.deltaTime * speed;
        deltaz = (Math.Sin(ydegree * Math.PI / 180 + steeringWheelDegree * Math.PI / 180) - Math.Sin(steeringWheelDegree * Math.PI / 180) * Math.Cos(ydegree * Math.PI / 180)) * Time.deltaTime * speed;
        float deltaydegree = (float)Math.Asin(2 * Math.Sin(steeringWheelDegree * Math.PI / 180) / 6);
        //move
        carTransform.Translate((float)deltax, 0, (float)deltaz, Space.World);
        carTransform.Rotate(0, -deltaydegree, 0, Space.World);
        pointsDrawer.AddPoint(new Vector3(carTransform.position.x, 0.1f, carTransform.position.z));
    }


    void Reset()
    {
        isGoal = false;
        isCollideWall = false;
        transform.position = resetPosition;
        transform.rotation = resetDegree;
        pointsDrawer.ClearWayPoint();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
            isCollideWall = true;
        else if (other.CompareTag("Goal"))
            isGoal = true;
    }
    Vector3 CastRay(float angleDegrees)
    {
        Vector3 rayDirection = Quaternion.Euler(0, angleDegrees, 0) * transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, wallLayer))
        {
            lineRenderer.SetPosition(rayIndex++, transform.position);
            rayIndex %= 6;
            lineRenderer.SetPosition(rayIndex++, hit.point);
            rayIndex %= 6;
        }
        return hit.point;
    }
    void UpdateDistanceText(float l, float m, float r)
    {
        dL.text = "左側距離:" + l;
        dM.text = "中間距離:" + m;
        dR.text = "右側距離:" + r;
    }
}
