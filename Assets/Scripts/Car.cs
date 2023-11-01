using UnityEngine;

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
    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.positionCount = 6;
        Reset();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CastRay(+45);
        CastRay(0);
        CastRay(-45);
        if (isCollideWall) Reset();
    }
    void Move()
    {
        float degree = transform.rotation.eulerAngles.y;
        degree = degree + 45f;
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
}
