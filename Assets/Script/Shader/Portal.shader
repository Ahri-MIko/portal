Shader "Custom/Portal"
{
    Properties
    {
    _MainTex ("Main Texture", 2D) = "white" {} // 添加这行来在Inspector中显示纹理设置
        _InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv :TEXCOORD;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _InactiveColour;
            int displayMask; //置一显示渲染图片,因为在门中门和看不见传送门的时候都要关闭传送门
            

            v2f vert (appdata v)
            {
                v2f o;
                //得到裁切面的坐标
                o.vertex = UnityObjectToClipPos(v.vertex);
                //得到屏幕的坐标
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                //由于%w是opengl自己动手做的,所以我们这里要自己除一个w保持正确显示
                float2 uv = i.screenPos.xy/i.screenPos.w;
                fixed4 portalCol = tex2D(_MainTex, uv);
                return portalCol * displayMask + _InactiveColour * (1-displayMask);
            }
            ENDCG
        }
    }
    Fallback "Standard" // for shadows
}
