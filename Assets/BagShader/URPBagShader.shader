Shader "URPBagShader/BagShader"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]
        _Cull("Cull", Float) = 0
        _SplitX("SplitX",float) = 5
        _SplitY("SplitY",float) = 5
        _Shift("Shift",Range(0,1)) = 0.1
        _Frec("Frec",Range(1,20))=1
        _ColorGap("ColorGap",Range(-1,1)) = 1
        _Ratio("Ratio",Range(0,1))=0.1
        _Strength("Strength",Range(0,1))=0.5
        _Blur("Blur",Range(0,1))=0.5
    }

    SubShader
    {
        Cull [_Cull]
        Tags { "Queue" = "Transparent" }

        Pass {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define Bag_IS_URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Util.hlsl"

            
            
            struct appdata {
                float4 vertexOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            }; 

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };


            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertexOS.xyz);
                float4 worldPos=  ComputeScreenPos(o.pos);
                o.uv =worldPos.xy/worldPos.w;

                return o;
            }

            CBUFFER_START(UnityPerMaterial)
            float _SplitX;
			float _SplitY;
            half _Shift;
            half _Frec;
            float _ColorGap;
            float _Ratio;
            float _Strength;
            float _Blur;
            CBUFFER_END
            
            half4 frag (v2f i) : SV_Target
            {
                BagShaderData data;
                data.uv = i.uv;
                data.splitX = _SplitX;
                data.splitY = _SplitY;
                data.shift = _Shift;
                data.frec = _Frec;
                data.ratio = _Ratio;
                data.blur = _Blur;
                data.strength = _Strength;
                data.colorGap = _ColorGap;
                data.isImage = false;

                half4 col = CalBagShaderColor(data);
                return col;
            }

            ENDHLSL
        }
    }
}
