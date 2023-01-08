Shader "Unlit/ParticleShader"
{
    Properties {
        _Color ("Main Color", Color) = (1.000000,1.000000,1.000000,1.000000)      
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color);
            UNITY_INSTANCING_BUFFER_END(Props)
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct MeshProperties
            {
                float4x4 mat;
            };

            StructuredBuffer<MeshProperties> _Properties;
            
            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;
                float4 pos = mul(_Properties[instanceID].mat, v.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                return UNITY_ACCESS_INSTANCED_PROP(float4, _Color);
            }
            ENDCG
        }
    }
}
