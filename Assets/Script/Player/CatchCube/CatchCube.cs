using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Analytics;

public class CatchObject : MonoBehaviour
{
    public static CatchObject instance;
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float rotationSpeed = 5f;        // ������ת�����ٶ�
    [SerializeField] private float holdForce = 20f;           // ��������λ�õ���
    [SerializeField] private float maxGrabDistance = 100f;    // ���ץȡ����
    [SerializeField] private float minHoldDistance = 2f;      // ��С���վ���
    [SerializeField] private float maxHoldDistance = 5f;      // �����վ���
    [SerializeField] private float defaultHoldDistance = 3f;   // Ĭ�ϳ��վ���
    [SerializeField] private float distanceChangeSpeed = 2f;   // ��������ٶ�

    //these are caught object's Rigid and Self,but i want to figure out how to union them,because it look like a mass
    private Rigidbody caughtObjectRigidbody;
    public CubeCaught catchObject;

    //this will be less affect on game,but add it would be rigor
    private Quaternion targetRotation;
    private Vector3 originalScale;

    //�����鿴��ҵ�״̬�Ƿ�����ץסĳ������
    public bool isHolding { get;private set; }
    private float currentHoldDistance;//��ǰץס����ľ���


    private MyClock myClock; //�����ͷŵ�ʱ��
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
        //Fixupdate����ᵼ�����Getinput������,�������Լ�д��һ��MyClock���������ͷ�ʱ��,Ȼ��������GetKeyDown����Ҫ���µ�InputSystem����
        if (Input.GetKeyDown(KeyCode.F) && myClock.state == MyClock.State.Idle)
        {
            //ÿ��0.5����ܰ���һ��
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
            // ʹ�������ֵ�������
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

                // ����ԭʼ����
                originalScale = caughtObject.transform.localScale;

                // ���ø�������
                caughtObjectRigidbody.useGravity = false;
                caughtObjectRigidbody.drag = 10;
                caughtObjectRigidbody.angularDrag = 5;
                caughtObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                // ��ʼ��Ŀ����תΪ��ǰ��ת
                targetRotation = caughtObject.transform.rotation;
            }
        }
    }

    //ʵʱ���������λ�ø���,ץס�����λ��,Ȼ����Ǹ���holdForce�ж��Ƿ�ץ����С�����÷�����ٶ�
    private void UpdateHeldObject()
    {
        // ����Ŀ��λ�ã������ǰ��ָ�����봦��
        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * currentHoldDistance;

        // ���㵱ǰλ�õ�Ŀ��λ�õķ���;���
        Vector3 directionToTarget = targetPosition - caughtObjectRigidbody.position;
        float distanceToTarget = directionToTarget.magnitude;

        // ʹ��������������Ŀ��λ��
        caughtObjectRigidbody.velocity = directionToTarget.normalized * holdForce * distanceToTarget;



        // �����ٶ�
        if (caughtObjectRigidbody.velocity.magnitude > holdForce)
        {
            caughtObjectRigidbody.velocity = caughtObjectRigidbody.velocity.normalized * holdForce;
        }

        // �����������ת��ʹ��������
        targetRotation = holdPoint.rotation;
        caughtObjectRigidbody.MoveRotation(Quaternion.Lerp(
            caughtObjectRigidbody.rotation,
            targetRotation,
            Time.fixedDeltaTime * rotationSpeed
        ));

        // ���������ԭʼ����
        caughtObjectRigidbody.transform.localScale = originalScale;
    }

    //��������
    private void ReleaseObject()
    {
        catchObject.getCaught = false;
        if (caughtObjectRigidbody != null)
        {
            // �ָ���������
            caughtObjectRigidbody.useGravity = true;
            caughtObjectRigidbody.drag = 0;
            caughtObjectRigidbody.angularDrag = 0.05f;

            // ������һ����ǰ����
            caughtObjectRigidbody.AddForce(Camera.main.transform.forward * 2, ForceMode.VelocityChange);

            caughtObjectRigidbody = null;
            catchObject = null;//���ű�����������
        }

        isHolding = false;
        currentHoldDistance = defaultHoldDistance; // ���þ��뵽Ĭ��ֵ
    }


    public void switchOnGrab()
    {
        catchObject.TransPosition();
    }
}