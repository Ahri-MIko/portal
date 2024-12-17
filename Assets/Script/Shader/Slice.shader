Shader "Custom/Slice"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        sliceNormal("normal", Vector) = (0,0,0,0)
        sliceCentre ("centre", Vector) = (0,0,0,0)
        sliceOffsetDst("offset", Float) = 0
    }
    SubShader
    {
        //定义渲染队列,和光影效果,指定渲染类型
        Tags { "Queue" = "Geometry" "IgnoreProjector" = "True"  "RenderType"="Geometry" }
        //渲染级别,200是一个低质量的渲染级别
        LOD 200

        CGPROGRAM
        
        #pragma surface surf Standard addshadow
        
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // 切片的法线,由传送门确定
        float3 sliceNormal;
        // 切片的中心
        float3 sliceCentre;
        // 偏移量
        float sliceOffsetDst;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //经过调整后的中心
            float3 adjustedCentre = sliceCentre + sliceNormal * sliceOffsetDst;
            //计算片元到切片的距离
            float3 offsetToSliceCentre = adjustedCentre - IN.worldPos;

            //用这个判断现在是否在切片面的显示侧还是不显示侧面
            clip (dot(offsetToSliceCentre, sliceNormal));
            
            
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "VertexLit"
}
