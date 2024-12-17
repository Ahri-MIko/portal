using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Main Settings")]
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 10;

    [Header("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    public Collider backCollider;

    [Header("�Ƿ�̶��󱳵�collider")]
    [SerializeField] private bool FixedWall = false;
    [SerializeField] private Collider Wall;

    // Private variables
    RenderTexture viewTexture;
    Camera portalCam;
    Camera playerCam;
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    //�������ֻ��һ������,��Ϊ����һ��ֻ��ץסһ������
    List<CubeCaught> trackedCatchObject;
    MeshFilter screenMeshFilter;

    //�ֱ������,������ӵ�
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string bulletTag = "Bullet";
    [SerializeField] private string boxTag = "BOX";

    void Awake()
    {
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;
        trackedTravellers = new List<PortalTraveller>();
        trackedCatchObject = new List<CubeCaught>();
        screenMeshFilter = screen.GetComponent<MeshFilter>();
        screen.material.SetInt("displayMask", 1);
    }


    //����ͨ�������ŵ�����
    void LateUpdate()
    {
        HandleTravellers();
        HandleCatchObject();
    }

    private void HandleCatchObject()
    {
        for (int i = 0; i < trackedCatchObject.Count; i++)
        {
            CubeCaught catchedobject = trackedCatchObject[i];//��ÿһ��������
            Transform catchedobjectT = catchedobject.transform;//����ԭʼ��λ��
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * catchedobjectT.localToWorldMatrix;
            Vector3 offsetFromPortal = catchedobjectT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(catchedobject.previousOffsetFromPortal, transform.forward));
            // ֻ���ڵ����Ѿ����������ŵ�ʱ��,�����ǲ�û�б�����ץס
            if (portalSide != portalSideOld && !catchedobject.getCaught)
            {
                Debug.Log("�����Ѿ���������");
                catchedobject.throughPortal = true;
                catchedobject.TransPosition();
                catchedobject.TransVelocity(transform, linkedPortal.transform);
                //��Ϊ�Ǳ������ߵ�
                colliderOnable(catchedobject.transform.GetComponent<Collider>());
                trackedCatchObject.Remove(catchedobject);
                
            }
            else if (portalSide != portalSideOld && catchedobject.getCaught)
            {
                Debug.Log("����û�б�������,����ץ������");
                //���ץס�˷���,���鴩���˴�����,��������û�д���������,���ʱ���Ǽ�������
                catchedobject.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                catchedobject.throughPortal = true;
            }
            else
            {
                catchedobject.throughPortal = false;
                //��û����������
                catchedobject.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                //���´�������Ӧ��λ��
                catchedobject.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    void HandleTravellers()
    {

        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];//��ǰ�������Ĵ�Խ��
            Transform travellerT = traveller.transform;
            if (traveller as Player)
            {
                Player player = (Player)traveller;
                travellerT = player.Point;
            }
            //���þ������Ч�ʸ�,���Ҽ��,������һ��ȱ����������������ı������겻һ���Ļ��Ͳ��������������
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            //��������ָ������
            Vector3 offsetFromPortal = travellerT.position - transform.position;
            //��¼ʵʱ���ŵĳ���
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            //��¼��ʼ���ŵ�λ�ó���
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            if (portalSide != portalSideOld)
            {
                Debug.Log("trans");
                // �ȼ���Ŀ��任
                var targetPosition = linkedPortal.transform.TransformPoint(transform.InverseTransformPoint(travellerT.position));
                var targetRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * travellerT.rotation;

                FixCatchObject(traveller);
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;

                Collider tempCollider = traveller.transform.GetComponent<Collider>();
                colliderOnable(tempCollider);

                // ʹ��ֱ�Ӽ����Ŀ��任�����Ǿ���m�Ľ��
                traveller.Teleport(transform, linkedPortal.transform, targetPosition, targetRotation);
                traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);

                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            }
            else
            {

                //��������¼
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                //UpdateSliceParams (traveller);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }



    // ע�������еĴ����������Ⱦ֮ǰ����,ʹ�ø��Ӻ���
    public void PrePortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
    }

    // �ֶ�����Render
    public void Render()
    {

        //�Զ������С����,�����ж��Ƿ񿴵õ�
        if (!CameraUtility.VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            return;
        }

        //Ϊscreen����TargetTexture
        CreateViewTexture();

        //ʵ�������ŵ�Ч��
        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;

        //���������Ų���,Ҳ������Ե�λ�ú���ת
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];


        int startIndex = 0;
        //ȷ����Һ���Ⱦ������Ч��һ��
        portalCam.projectionMatrix = playerCam.projectionMatrix;

        //����ѭ��,ÿһ�������λ��
        for (int i = 0; i < recursionLimit; i++)
        {
            if (i > 0)
            {
                //ÿ�ζ�Ҫ����Ƿ��ܹ�������һ��������,��Ϊ�е�ʱ�����Ϊ��ת�ĽǶȽ�����תû��
                if (!CameraUtility.BoundsOverlap(screenMeshFilter, linkedPortal.screenMeshFilter, portalCam))
                {
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // �������ش�����,ʹ���������ȷ��������
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt("displayMask", 0);

        //���������֡��Ⱦ��
        for (int i = startIndex; i < recursionLimit; i++)
        {
            portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
            SetNearClipPlane();
            HandleClipping();
            portalCam.Render();

            //������ʾ
            if (i == startIndex)
            {
                linkedPortal.screen.material.SetInt("displayMask", 1);
            }
        }

        // ����ͶӰ
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }


    //���ڴ��͵Ķ����Ϳ�¡�����ʾ����
    void HandleClipping()
    {
        const float hideDst = -1000;
        const float showDst = 1000;
        //
        float screenThickness = linkedPortal.ProtectScreenFromClipping(portalCam.transform.position);

        //��ʾ����
        foreach (var traveller in trackedTravellers)
        {
            if (SameSideOfPortal(traveller.transform.position, portalCamPos))
            {

                traveller.SetSliceOffsetDst(hideDst, false);
            }
            else
            {

                traveller.SetSliceOffsetDst(showDst, false);
            }


            int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
            bool camSameSideAsClone = linkedPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone)
            {
                traveller.SetSliceOffsetDst(screenThickness, true);
            }
            else
            {
                traveller.SetSliceOffsetDst(-screenThickness, true);
            }
        }

        var offsetFromPortalToCam = portalCamPos - transform.position;
        foreach (var linkedTraveller in linkedPortal.trackedTravellers)
        {
            var travellerPos = linkedTraveller.graphicsObject.transform.position;
            var clonePos = linkedTraveller.graphicsClone.transform.position;
            //����clone�������Ƭ
            bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos);
            if (cloneOnSameSideAsCam)
            {

                linkedTraveller.SetSliceOffsetDst(hideDst, true);
            }
            else
            {

                linkedTraveller.SetSliceOffsetDst(showDst, true);
            }

            bool camSameSideAsTraveller = linkedPortal.SameSideOfPortal(linkedTraveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller)
            {
                linkedTraveller.SetSliceOffsetDst(screenThickness, false);
            }
            else
            {
                linkedTraveller.SetSliceOffsetDst(-screenThickness, false);
            }
        }
    }

    // ��Ⱦ��slice��Ƭ���Ч��,�������úô����ŵ���Ļλ��
    public void PostPortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
        //screen�ĺ��
        ProtectScreenFromClipping(playerCam.transform.position);
    }
    //����Ŀ����Ⱦ����
    void CreateViewTexture()
    {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null)
            {
                viewTexture.Release();
            }
            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);

            portalCam.targetTexture = viewTexture;

            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    // ��ֹ��Ƭ�������µĳ�����Ƭ������
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {

        //����POV�ĽǶȵ�һ���Tan������Ļ��nearClipPlane�ľ������õ���Ļ��һ��ĸ߶�
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //����POV��һ��ĸ߶ȳ���һ��������Ŀ�߶ȱ�,�õ�һ��Ŀ��
        float halfWidth = halfHeight * playerCam.aspect;
        //���㵽��Ļ���Ͻǵľ���
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;

        //�����Ӿ�����ŵķ����Ƿ�һ��,�����ʵ�������Ļ��λ��
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness * 25.0f);
        //screenT.localPosition = Vector3.forward * screenThickness * 0.1f * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }


    //����Ŀ�����Ƭ
    void UpdateSliceParams(PortalTraveller traveller)
    {
        // �õ�������ķ���
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // �õ�����������ĵ������
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // �������΢�����Ƕ���һЩ��ʾ�ϵĵ���
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(playerCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller)
        {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal(playerCam.transform.position);
        if (!playerSameSideAsCloneAppearing)
        {
            cloneSliceOffsetDst = -screenThickness;
        }

        // �������Ĳ���Ӧ�õ���ɫ������
        for (int i = 0; i < traveller.originalMaterials.Length; i++)
        {
            traveller.originalMaterials[i].SetVector("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveller.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);

        }

    }

    //�ܵ���˵������Ӧ����ƽ�����õ�������ͼ��
    void SetNearClipPlane()
    {
        Transform clipPlane = transform; //��ô����ŵ����ĵ�
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));//�ŵ�forward��������ŵķ���ļн�

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);//ת�����������,���һ��Զ���w��������ȥ
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }



    void OnTriggerEnter(Collider other)
    {

        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && screen.enabled)
        {
            //����һ���������һ������,����ķ������һ��Ƥû���κε�������ײ������
            OnTravellerEnterPortal(traveller);
            //��ʱ���������ź����ǽ���������ײ
            colliderDisable(other);

        }

        var catchobject = other.GetComponent<CubeCaught>();
        if (catchobject != null)
        {
            //��������һ����
            OnCatchObjectEnterPortal(catchobject);
            colliderDisable(other);
        }

    }

    private void OnCatchObjectEnterPortal(CubeCaught other)
    {
        if (!trackedCatchObject.Contains(other))
        {
            //ֻ��Ҫ����д���б���
            other.EnterPortalThreshold();//����������

            //��������ʱ���봫���ŵľ���,�����ж��Ƿ��ڴ����ŵ�һ��
            other.previousOffsetFromPortal = other.transform.position - transform.position;
            trackedCatchObject.Add(other);

        }
    }


    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            //�õ���ǰ�����굽�ŵ�����Ĳ�� 

            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    //����������ǵ��д����ߴ��ͳ���ʱ��ͽ����ķ����滻��
    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller) && screen.enabled)
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
            colliderOnable(other);

        }

        var catchobject = other.GetComponent<CubeCaught>();
        if(catchobject && trackedCatchObject.Contains(catchobject) && screen.enabled)
        {

            colliderOnable(other);
        }

    }

    /*
     ** һЩ���ߺ���
     */

    //����Ƿ��ڴ�����ǰ���һ��,���������ŵı���,һ����
    int SideOfPortal(Vector3 pos)
    {
        return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    //�ж��Ƿ��ڻ�����һ�������ŵ�ͬһ��
    bool SameSideOfPortal(Vector3 posA, Vector3 posB)
    {
        return SideOfPortal(posA) == SideOfPortal(posB);
    }

    //��ô����ŵ���������
    Vector3 portalCamPos
    {
        get
        {
            return portalCam.transform.position;
        }
    }

    //�����ֶ�Ҳ���϶�Ҳ�ܽ�������������������
    void OnValidate()
    {
        if (linkedPortal != null)
        {
            linkedPortal.linkedPortal = this;
        }
    }

    //��������Ŷ�Ӧ��ǽ��
    public void SetBackCollider(Collider other)
    {
        backCollider = other;
    }





    //��������ǵ���⵽�������,��������ץס�˶���,�ͽ���ץס����Ϳ�¡���λ��
    private void FixCatchObject(PortalTraveller traveller)
    {
        if (traveller.gameObject.transform.GetComponent<CatchObject>())
        {
            //�ж��Ƿ������
            if (CatchObject.instance.isHolding)
            {
                Collider tempCollider = CatchObject.instance.catchObject.transform.GetComponent<Collider>();
                colliderOnable(tempCollider);
                CatchObject.instance.switchOnGrab();
                trackedCatchObject.Remove(CatchObject.instance.catchObject);
            }

        }
    }

    //������֤���ֶ����ܽ��봫����,Player,Cube,Bullet
    void colliderDisable(Collider other)
    {
        Debug.Log(other.gameObject.name);
        Debug.Log("���봫����,���ҽ�����ǽ����ײ");
        if (other.CompareTag(playerTag) || other.CompareTag(bulletTag) || other.CompareTag(boxTag))
        {
            if (FixedWall)
                Physics.IgnoreCollision(other, Wall, true);
            else
                Physics.IgnoreCollision(other, backCollider, true);
        }
    }

    //���»���ײ��Ч��
    void colliderOnable(Collider other)
    {
        if (other.CompareTag(playerTag) || other.CompareTag(bulletTag)|| other.CompareTag(boxTag))
        {
            if (FixedWall)
            {
               
                Physics.IgnoreCollision(other, Wall, false);
            }
            else
                Physics.IgnoreCollision(other, backCollider, false);
        }
    }

}