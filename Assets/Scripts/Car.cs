using System;
using UnityEngine;
using UnityEngine.UI;
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
    private float steeringWheelDegree = 0;
    private double deltax;
    private double deltaz;//2d中的y
    private Transform carTransform;
    public Text dL, dM, dR;
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
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistanceText(Vector3.Distance(CastRay(-45), carTransform.position),
                           Vector3.Distance(CastRay(0), carTransform.position),
                           Vector3.Distance(CastRay(45), carTransform.position));
        if (isCollideWall) Reset();
        Move();
    }
    void Move()
    {
        //cos(pi/2)有精確度問題
        float ydegree = 90 + carTransform.eulerAngles.y;//水平夾角
        deltax = (Math.Cos(ydegree * Math.PI / 180 + steeringWheelDegree * Math.PI / 180) + Math.Sin(steeringWheelDegree * Math.PI / 180) * Math.Sin(ydegree * Math.PI / 180)) * Time.deltaTime;
        deltaz = (Math.Sin(ydegree * Math.PI / 180 + steeringWheelDegree * Math.PI / 180) - Math.Sin(steeringWheelDegree * Math.PI / 180) * Math.Cos(ydegree * Math.PI / 180)) * Time.deltaTime;
        float deltaydegree = (float)(Math.Asin(2 * Math.Sin(steeringWheelDegree * Math.PI / 180) / 6) * Time.deltaTime);
        //move
        carTransform.Translate((float)deltax, 0, (float)deltaz);
        carTransform.Rotate(0, deltaydegree, 0);
    }
    void Reset()
    {
        isGoal = false;
        isCollideWall = false;
        transform.position = resetPosition;
        transform.rotation = resetDegree;
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
