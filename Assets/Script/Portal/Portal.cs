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

    [Header("是否固定后背的collider")]
    [SerializeField] private bool FixedWall = false;
    [SerializeField] private Collider Wall;

    // Private variables
    RenderTexture viewTexture;
    Camera portalCam;
    Camera playerCam;
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    //这里可能只有一个盒子,因为主角一次只能抓住一个盒子
    List<CubeCaught> trackedCatchObject;
    MeshFilter screenMeshFilter;

    //分别是玩家,发射的子弹
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


    //更新通过传送门的物体
    void LateUpdate()
    {
        HandleTravellers();
        HandleCatchObject();
    }

    private void HandleCatchObject()
    {
        for (int i = 0; i < trackedCatchObject.Count; i++)
        {
            CubeCaught catchedobject = trackedCatchObject[i];//对每一个的引用
            Transform catchedobjectT = catchedobject.transform;//保存原始的位置
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * catchedobjectT.localToWorldMatrix;
            Vector3 offsetFromPortal = catchedobjectT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(catchedobject.previousOffsetFromPortal, transform.forward));
            // 只有在当在已经穿过传送门的时候,而且是并没有被主角抓住
            if (portalSide != portalSideOld && !catchedobject.getCaught)
            {
                Debug.Log("盒子已经被传送了");
                catchedobject.throughPortal = true;
                catchedobject.TransPosition();
                catchedobject.TransVelocity(transform, linkedPortal.transform);
                //因为是被传送走的
                colliderOnable(catchedobject.transform.GetComponent<Collider>());
                trackedCatchObject.Remove(catchedobject);
                
            }
            else if (portalSide != portalSideOld && catchedobject.getCaught)
            {
                Debug.Log("盒子没有被传送走,还被抓在手上");
                //玩家抓住了方块,方块穿过了传送门,但是自身没有穿过传送门,这个时候还是继续保持
                catchedobject.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                catchedobject.throughPortal = true;
            }
            else
            {
                catchedobject.throughPortal = false;
                //还没穿过传送门
                catchedobject.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                //更新传送门相应的位置
                catchedobject.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    void HandleTravellers()
    {

        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];//当前遍历到的穿越者
            Transform travellerT = traveller.transform;
            if (traveller as Player)
            {
                Player player = (Player)traveller;
                travellerT = player.Point;
            }
            //利用矩阵计算效率高,并且简洁,但是有一个缺点就是如果两个物体的本地坐标不一样的话就不能用这个来计算
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            //从门中心指向传送物
            Vector3 offsetFromPortal = travellerT.position - transform.position;
            //记录实时和门的朝向
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            //记录开始和门的位置朝向
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            if (portalSide != portalSideOld)
            {
                Debug.Log("trans");
                // 先计算目标变换
                var targetPosition = linkedPortal.transform.TransformPoint(transform.InverseTransformPoint(travellerT.position));
                var targetRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * travellerT.rotation;

                FixCatchObject(traveller);
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;

                Collider tempCollider = traveller.transform.GetComponent<Collider>();
                colliderOnable(tempCollider);

                // 使用直接计算的目标变换而不是矩阵m的结果
                traveller.Teleport(transform, linkedPortal.transform, targetPosition, targetRotation);
                traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);

                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            }
            else
            {

                //就正常记录
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                //UpdateSliceParams (traveller);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }



    // 注意在所有的传送门相机渲染之前调用,使得更加合理
    public void PrePortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
    }

    // 手动调用Render
    public void Render()
    {

        //自定义相机小工具,用来判断是否看得到
        if (!CameraUtility.VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            return;
        }

        //为screen创建TargetTexture
        CreateViewTexture();

        //实现门中门的效果
        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;

        //创建门中门层数,也就是相对的位置和旋转
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];


        int startIndex = 0;
        //确保玩家和渲染出来的效果一致
        portalCam.projectionMatrix = playerCam.projectionMatrix;

        //核心循环,每一个相机的位置
        for (int i = 0; i < recursionLimit; i++)
        {
            if (i > 0)
            {
                //每次都要检查是否能够看到另一个传送门,因为有的时候会因为旋转的角度将东西转没了
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

        // 首先隐藏传送门,使得相机能正确拍摄内容
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt("displayMask", 0);

        //这里就是逐帧渲染了
        for (int i = startIndex; i < recursionLimit; i++)
        {
            portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
            SetNearClipPlane();
            HandleClipping();
            portalCam.Render();

            //开启显示
            if (i == startIndex)
            {
                linkedPortal.screen.material.SetInt("displayMask", 1);
            }
        }

        // 开启投影
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }


    //调节传送的东西和克隆体的显示问题
    void HandleClipping()
    {
        const float hideDst = -1000;
        const float showDst = 1000;
        //
        float screenThickness = linkedPortal.ProtectScreenFromClipping(portalCam.transform.position);

        //显示问题
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
            //控制clone物体的切片
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

    // 渲染出slice切片后的效果,并且设置好传送门的屏幕位置
    public void PostPortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
        //screen的厚度
        ProtectScreenFromClipping(playerCam.transform.position);
    }
    //创建目标渲染材质
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

    // 防止切片不够厚导致的出现切片的问题
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {

        //利用POV的角度的一半的Tan乘以屏幕到nearClipPlane的距离来得到屏幕的一半的高度
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //利用POV的一半的高度乘上一个摄像机的宽高度比,得到一半的宽度
        float halfWidth = halfHeight * playerCam.aspect;
        //计算到屏幕左上角的距离
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;

        //计算视觉点和门的方向是否一致,并且适当调整屏幕的位置
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness * 25.0f);
        //screenT.localPosition = Vector3.forward * screenThickness * 0.1f * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }


    //设置目标的切片
    void UpdateSliceParams(PortalTraveller traveller)
    {
        // 得到裁切面的方向
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // 得到裁切面的中心点的坐标
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // 这下面的微调都是对于一些显示上的调整
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

        // 将调整的参数应用到着色器上面
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

    //总的来说就是适应裁切平面来得到完美的图像
    void SetNearClipPlane()
    {
        Transform clipPlane = transform; //获得传送门的中心点
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));//门的forward和相机到门的方向的夹角

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);//转化成相机坐标,并且会自动将w分量除进去
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
            //开启一个物体的另一个分身,这里的分身除了一层皮没有任何的物理碰撞的作用
            OnTravellerEnterPortal(traveller);
            //暂时开启传送门后面的墙和物体的碰撞
            colliderDisable(other);

        }

        var catchobject = other.GetComponent<CubeCaught>();
        if (catchobject != null)
        {
            //和上面是一样的
            OnCatchObjectEnterPortal(catchobject);
            colliderDisable(other);
        }

    }

    private void OnCatchObjectEnterPortal(CubeCaught other)
    {
        if (!trackedCatchObject.Contains(other))
        {
            //只需要将其写入列表即可
            other.EnterPortalThreshold();//穿过传送门

            //计算进入的时候与传送门的距离,用来判断是否在传送门的一侧
            other.previousOffsetFromPortal = other.transform.position - transform.position;
            trackedCatchObject.Add(other);

        }
    }


    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            //得到当前的坐标到门的坐标的差距 

            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    //这个的作用是当有传送者传送出的时候就将它的分身替换掉
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
     ** 一些工具函数
     */

    //检测是否在传送门前面的一侧,或者是在门的背后,一个是
    int SideOfPortal(Vector3 pos)
    {
        return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    //判断是否在基于这一个传送门的同一侧
    bool SameSideOfPortal(Vector3 posA, Vector3 posB)
    {
        return SideOfPortal(posA) == SideOfPortal(posB);
    }

    //获得传送门的坐标属性
    Vector3 portalCamPos
    {
        get
        {
            return portalCam.transform.position;
        }
    }

    //不用手动也能拖动也能将两个传送门连接起来
    void OnValidate()
    {
        if (linkedPortal != null)
        {
            linkedPortal.linkedPortal = this;
        }
    }

    //设置这个门对应的墙体
    public void SetBackCollider(Collider other)
    {
        backCollider = other;
    }





    //这个函数是当检测到的是玩家,并且手上抓住了东西,就交换抓住物体和克隆体的位置
    private void FixCatchObject(PortalTraveller traveller)
    {
        if (traveller.gameObject.transform.GetComponent<CatchObject>())
        {
            //判断是否是玩家
            if (CatchObject.instance.isHolding)
            {
                Collider tempCollider = CatchObject.instance.catchObject.transform.GetComponent<Collider>();
                colliderOnable(tempCollider);
                CatchObject.instance.switchOnGrab();
                trackedCatchObject.Remove(CatchObject.instance.catchObject);
            }

        }
    }

    //仅仅保证三种东西能进入传送门,Player,Cube,Bullet
    void colliderDisable(Collider other)
    {
        Debug.Log(other.gameObject.name);
        Debug.Log("进入传送门,并且禁用与墙的碰撞");
        if (other.CompareTag(playerTag) || other.CompareTag(bulletTag) || other.CompareTag(boxTag))
        {
            if (FixedWall)
                Physics.IgnoreCollision(other, Wall, true);
            else
                Physics.IgnoreCollision(other, backCollider, true);
        }
    }

    //重新恢碰撞送效果
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