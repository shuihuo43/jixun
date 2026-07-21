Shader "Custom/BloodImprint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Depth", Range(0, 1)) = 0.4
        _Contrast ("Contrast", Range(0, 1)) = 0.3
        _EdgeGlow ("Edge Glow", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off
        GrabPass { "_BgTex" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BgTex;
            fixed _Threshold, _Contrast, _EdgeGlow;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 grabPos : TEXCOORD1; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 bg = tex2Dproj(_BgTex, i.grabPos);
                float bgGray = max(bg.r, max(bg.g, bg.b));

                // Overlay 叠加：暗处乘、亮处筛
                fixed3 low = 2.0 * bg.rgb * col.rgb;
                fixed3 high = 1.0 - 2.0 * (1.0 - bg.rgb) * (1.0 - col.rgb);
                fixed3 overlay = lerp(low, high, step(0.5, bg.rgb));

                // 深度参数控制叠加程度
                float depth = _Threshold * col.a;
                col.rgb = lerp(col.rgb, overlay, depth);

                // 整体提亮
                col.rgb = saturate(col.rgb + 0.2 * col.a);

                // 对比度
                col.rgb = (col.rgb - 0.5) * (1.0 + _Contrast) + 0.5;
                col.rgb = saturate(col.rgb);

                // 边缘柔亮
                float edge = 1.0 - abs(col.a * 2.0 - 1.0);
                col.rgb = saturate(col.rgb + edge * _EdgeGlow * 0.3);

                return col;
            }
            ENDCG
        }
    }
}
