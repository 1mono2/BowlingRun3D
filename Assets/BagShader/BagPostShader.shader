Shader "Hidden/BagPostShader"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define Bag_IS_URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Util.hlsl"

            // TEXTURE2D(_MainTex);
            // SAMPLER(sampler_MainTex);
            sampler2D _MainTex;
            float _SplitX;
			float _SplitY;
            half _Shift;
            half _Frec;
            float _ColorGap;
            float _Ratio;
            float _Strength;
            float _Blur;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {                
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                // col.rgb *= half4(1,0,0,1);

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
                data.isImage = true;
                data.mainTex = _MainTex;

                half4 col = CalBagShaderColor(data);


                return col;
            }
            ENDHLSL
        }
    }
}