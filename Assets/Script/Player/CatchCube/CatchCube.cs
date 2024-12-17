using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Analytics;

public class CatchObject : MonoBehaviour
{
    public static CatchObject instance;
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float rotationSpeed = 5f;        // 物体旋转跟随速度
    [SerializeField] private float holdForce = 20f;           // 保持物体位置的力
    [SerializeField] private float maxGrabDistance = 100f;    // 最大抓取距离
    [SerializeField] private float minHoldDistance = 2f;      // 最小持握距离
    [SerializeField] private float maxHoldDistance = 5f;      // 最大持握距离
    [SerializeField] private float defaultHoldDistance = 3f;   // 默认持握距离
    [SerializeField] private float distanceChangeSpeed = 2f;   // 距离调整速度

    //these are caught object's Rigid and Self,but i want to figure out how to union them,because it look like a mass
    private Rigidbody caughtObjectRigidbody;
    public CubeCaught catchObject;

    //this will be less affect on game,but add it would be rigor
    private Quaternion targetRotation;
    private Vector3 originalScale;

    //用来查看玩家的状态是否正在抓住某个事务
    public bool isHolding { get;private set; }
    private float currentHoldDistance;//当前抓住物体的距离


    private MyClock myClock; //控制释放的时间
    private void Awake()
    {
        myClock = GetComponent<MyClock>();
        if (instance == null)
            instance = this;

        currentHoldDistance = defaultHoldDistance;
    }

    private void Update()
    {

    }
    void FixedUpdate()
    {
        //Fixupdate好像会导致这个Getinput出问题,所以我自己写了一个MyClock类来控制释放时间,然后就是这个GetKeyDown后面要用新的InputSystem代替
        if (Input.GetKeyDown(KeyCode.F) && myClock.state == MyClock.State.Idle)
        {
            //每隔0.5秒才能按下一次
            myClock.StartClock(0.5f);
            if (!isHolding)
            {
                CastRayToCatchObject();
            }
            else
            {
                ReleaseObject();
            }
        }

        if (isHolding && caughtObjectRigidbody != null)
        {
            // 使用鼠标滚轮调整距离
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            currentHoldDistance = Mathf.Clamp(
                currentHoldDistance - scrollWheel * distanceChangeSpeed,
                minHoldDistance,
                maxHoldDistance
            );

            UpdateHeldObject();
        }
    }

    private void CastRayToCatchObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrabDistance, objectLayer))
        {
            GameObject caughtObject = hit.transform.gameObject;
            catchObject = caughtObject.GetComponent<CubeCaught>();
            caughtObjectRigidbody = caughtObject.GetComponent<Rigidbody>();
            catchObject.getCaught = true;

            if (caughtObjectRigidbody != null)
            {
                isHolding = true;
                currentHoldDistance = Mathf.Max(defaultHoldDistance, Vector3.Distance(transform.position, hit.point));

                // 保存原始缩放
                originalScale = caughtObject.transform.localScale;

                // 设置刚体属性
                caughtObjectRigidbody.useGravity = false;
                caughtObjectRigidbody.drag = 10;
                caughtObjectRigidbody.angularDrag = 5;
                caughtObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                // 初始化目标旋转为当前旋转
                targetRotation = caughtObject.transform.rotation;
            }
        }
    }

    //实时根据相机的位置更新,抓住方块的位置,然后就是根据holdForce判断是否抓力大小来设置方块的速度
    private void UpdateHeldObject()
    {
        // 计算目标位置（在相机前方指定距离处）
        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * currentHoldDistance;

        // 计算当前位置到目标位置的方向和距离
        Vector3 directionToTarget = targetPosition - caughtObjectRigidbody.position;
        float distanceToTarget = directionToTarget.magnitude;

        // 使用力将物体拉向目标位置
        caughtObjectRigidbody.velocity = directionToTarget.normalized * holdForce * distanceToTarget;



        // 限制速度
        if (caughtObjectRigidbody.velocity.magnitude > holdForce)
        {
            caughtObjectRigidbody.velocity = caughtObjectRigidbody.velocity.normalized * holdForce;
        }

        // 更新物体的旋转，使其跟随相机
        targetRotation = holdPoint.rotation;
        caughtObjectRigidbody.MoveRotation(Quaternion.Lerp(
            caughtObjectRigidbody.rotation,
            targetRotation,
            Time.fixedDeltaTime * rotationSpeed
        ));

        // 保持物体的原始缩放
        caughtObjectRigidbody.transform.localScale = originalScale;
    }

    //丢弃物体
    private void ReleaseObject()
    {
        catchObject.getCaught = false;
        if (caughtObjectRigidbody != null)
        {
            // 恢复物理属性
            caughtObjectRigidbody.useGravity = true;
            caughtObjectRigidbody.drag = 0;
            caughtObjectRigidbody.angularDrag = 0.05f;

            // 给物体一个向前的力
            caughtObjectRigidbody.AddForce(Camera.main.transform.forward * 2, ForceMode.VelocityChange);

            caughtObjectRigidbody = null;
            catchObject = null;//将脚本的引用舍弃
        }

        isHolding = false;
        currentHoldDistance = defaultHoldDistance; // 重置距离到默认值
    }


    public void switchOnGrab()
    {
        catchObject.TransPosition();
    }
}