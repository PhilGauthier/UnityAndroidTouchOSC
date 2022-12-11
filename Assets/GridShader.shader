Shader "Unlit/GridShader"
{
    Properties
    {
        _MainColor("Color", Color) = (1,1,1,1)
        _GridColor("GridColor", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Division("Division", Float) = 7.
        _TouchCount("TouchCount", Integer) = 0
        _Amplitude("Amplitude", Float) = .1
        _Hue("Hue", Float) = 0
        _Saturation("Saturation", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST, _MainTex_TexelSize;
            fixed4 _MainColor;
            fixed4 _GridColor;
            float _Division;
            int _TouchCount;
            float _Amplitude;
            float _Hue;
            float _Saturation;

            float3 HSVToRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _MainColor;//tex2D(_MainTex, i.uv);
                float2 R = _MainTex_TexelSize.zw;
                float2 uv = i.uv-.5;
                float2 aspect = float2(1./16.,1./9.);
                uv /= aspect*16.;
                float zoom = _Division;
                float margin = .01;
                float2 guv = abs(frac(uv* zoom));
                if(guv.x< margin || guv.y < margin)
                    col += _GridColor;
                col += clamp(smoothstep(.01,.0,length(uv)),0,1);
                float step = 1./10.;
                int j=0;
                float3 frontCol = HSVToRGB(float3(_Hue, _Saturation, 1.));
                for(float i=0.;i<1. && j < _TouchCount;i+= step){
                    float2 p = tex2D(_MainTex, float2(i,0.)).xy-.5;
                    p /= aspect * 16.;
                    col.rgb += smoothstep(_Amplitude,.5* _Amplitude,length(uv-p))* frontCol;
                    j++;
                }                
                return col;
            }
            ENDCG
        }
    }
}
