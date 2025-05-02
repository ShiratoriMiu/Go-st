Shader "Hidden/CenterGrayOnly"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Radius("Radius", Float) = 0.3
        _Smoothness("Smoothness", Float) = 0.2
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            Pass
            {
                ZTest Always Cull Off ZWrite Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                sampler2D _MainTex;
                float _Radius;
                float _Smoothness;

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(float4 vertex : POSITION, float2 uv : TEXCOORD0)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(vertex);
                    o.uv = uv;
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float4 col = tex2D(_MainTex, i.uv);
                    float gray = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(i.uv, center);

                    // 中心がグレー、周辺はカラー
                    float t = smoothstep(_Radius, _Radius + _Smoothness, dist);
                    col.rgb = lerp(gray.xxx, col.rgb, t);

                    return col;
                }
                ENDCG
            }
        }
}
