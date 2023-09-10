#if defined(Bag_IS_URP)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#endif

struct BagShaderData 
{
    half2 uv;
    float splitX;
    float splitY;
    half shift;
    half frec;
    float ratio;
    float blur;
    float strength;
    float colorGap;
    bool isImage;
    sampler2D mainTex;
}; 

static const float division = 768;
static const float blackinterval = 6;

float rand(float2 co) 
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

half4 SceneColor(float2 uv,bool isImage,sampler2D mainTex){
    if(isImage){
        return tex2D(mainTex, uv);
    }
    #if defined(Bag_IS_URP)
    return half4(SampleSceneColor(uv),1);
    #else
    return half4(1,1,1,1);
    #endif
}

half4 CalBagShaderColor(BagShaderData data){

    data.blur*=0.01;
    data.strength *= 0.002;
    data.colorGap*=0.1;

    int divisionindex = data.uv.y * division;

    int noiseindex = divisionindex / blackinterval;

    float3 timenoise = float3(0, int(_Time.x*data.frec * 61), int(_Time.x*data.frec * 83));
    float noiserate = rand(timenoise.xy) < 0.05&&data.strength!=0 ? 10 : 1;

    float xnoise = rand((float3(noiseindex, 0, 0) + timenoise).xy);
    xnoise = xnoise * xnoise - 0.5; 
    xnoise *= noiserate;
    xnoise *= 0.002* (_SinTime.w*(data.strength==0?1:data.frec)/ 2 + 1.1); 
    if(data.strength!=0){
        xnoise = xnoise + ((uint(_Time.x * 2000) % int(division / blackinterval) - noiseindex) < 5 ? 0.005 : 0); 
    }
    
    data.uv += float2(xnoise, 0);

    //ちょっとぼかす
    half4 col1 = SceneColor(data.uv,data.isImage,data.mainTex);
    half4 col2 = SceneColor(data.uv + float2(data.blur,0),data.isImage,data.mainTex);
    half4 col3 = SceneColor(data.uv + float2(-data.blur,0),data.isImage,data.mainTex);
    half4 col4 = SceneColor(data.uv + float2(0, data.blur),data.isImage,data.mainTex);
    half4 col5 = SceneColor(data.uv + float2(0,-data.blur),data.isImage,data.mainTex);
    half4 col = (col1 * 4 + col2 + col3 + col4 + col5) / 8;

    float4 shiftColor = float4(1,1,1,1);

    //分割
    float2 uv = float2(floor(data.uv.x/(1/data.splitX))+1,floor(data.uv.y/(1/data.splitY))+1);

    half time = _Time.x*0.00001*data.frec;

    //ランダムでずらす
    if(rand(uv*sin(time))>(1-data.ratio)){
        data.uv.y+=data.shift;
        half3 uvVec = half3(data.uv.xy-0.5,1);
        half3x3 scaleMatrix = half3x3(0.95, 0, 0,
                                    0,0.95,0,
                                    0,0,1);
        uvVec = mul(scaleMatrix,uvVec);
        data.uv.x = uvVec.x+0.5;

        col = SceneColor(data.uv,data.isImage,data.mainTex);

        if(abs(data.colorGap)>0.01){
            float r = SceneColor(data.uv + data.colorGap,data.isImage,data.mainTex).x;
            float b = SceneColor(data.uv - data.colorGap,data.isImage,data.mainTex).y;
        
            shiftColor = half4(r, col.g, b, col.a);
        }
        col *= shiftColor;
    }
                

    return col;
}