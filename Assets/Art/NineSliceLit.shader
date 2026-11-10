Shader "Custom/NineSliceLit"
{
    Properties
    {
        _MainTex ("Texture (Wrap Mode = Clamp)", 2D) = "white" {}
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0,2)) = 1
        _Color ("Color", Color) = (1,1,1,1)
        _BorderUV ("Border width in texture (U, V)", Vector) = (0.1, 0.05, 0, 0)
        _BorderWorld ("Border width in world units", Float) = 0.25
        _TileWorld ("Middle tile size in world units", Float) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.3
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "DisableBatching" = "True"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float _BumpScale;
                float4 _Color;
                float4 _BorderUV;
                float _BorderWorld;
                float _TileWorld;
                float _Glossiness;
                float _Metallic;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float3 normalOS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            float SliceAxis(float p, float size, float borderUV, out float uvSlope)
            {
                p = saturate(p);
                borderUV = clamp(borderUV, 0.0, 0.5);
                float borderWorld = clamp(_BorderWorld, 0.0, size * 0.5);
                float middleUV = 1.0 - 2.0 * borderUV;

                if (borderWorld > 1e-5 && p * size < borderWorld)
                {
                    uvSlope = borderUV / borderWorld;
                    return (p * size / borderWorld) * borderUV;
                }

                if (borderWorld > 1e-5 && p * size > size - borderWorld)
                {
                    uvSlope = borderUV / borderWorld;
                    return 1.0 - ((size - p * size) / borderWorld) * borderUV;
                }

                float middleSize = max(size - 2.0 * borderWorld, 1e-5);
                float tileCount = max(1.0, round(middleSize / max(_TileWorld, 1e-5)));
                uvSlope = tileCount * middleUV / middleSize;
                float tilePosition = (p * size - borderWorld) / middleSize * tileCount;
                return borderUV + frac(tilePosition) * middleUV;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22));

                float3 faceNormal = abs(input.normalOS);
                float2 facePosition;
                float2 faceSize;
                float2 faceBorderUV;
                float3 tangentOS;
                float3 bitangentOS;

                if (faceNormal.x > faceNormal.y && faceNormal.x > faceNormal.z)
                {
                    facePosition = input.positionOS.zy;
                    faceSize = objectScale.zy;
                    faceBorderUV = _BorderUV.xy;
                    tangentOS = float3(0, 0, 1);
                    bitangentOS = float3(0, 1, 0);
                }
                else if (faceNormal.y > faceNormal.z)
                {
                    facePosition = input.positionOS.xz;
                    faceSize = objectScale.xz;
                    faceBorderUV = _BorderUV.xy;
                    tangentOS = float3(1, 0, 0);
                    bitangentOS = float3(0, 0, 1);
                }
                else
                {
                    facePosition = input.positionOS.xy;
                    faceSize = objectScale.xy;
                    faceBorderUV = _BorderUV.xy;
                    tangentOS = float3(1, 0, 0);
                    bitangentOS = float3(0, 1, 0);
                }

                facePosition += 0.5;

                float uvSlopeX;
                float uvSlopeY;
                float2 uv = float2(
                    SliceAxis(facePosition.x, faceSize.x, faceBorderUV.x, uvSlopeX),
                    SliceAxis(facePosition.y, faceSize.y, faceBorderUV.y, uvSlopeY));
                float2 uvSlope = float2(uvSlopeX, uvSlopeY);
                half4 textureColor = SAMPLE_TEXTURE2D_GRAD(
                    _MainTex,
                    sampler_MainTex,
                    uv,
                    uvSlope * ddx(facePosition),
                    uvSlope * ddy(facePosition));
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D_GRAD(
                        _BumpMap,
                        sampler_BumpMap,
                        uv,
                        uvSlope * ddx(facePosition),
                        uvSlope * ddy(facePosition)),
                    _BumpScale);
                half4 baseColor = textureColor * _Color;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                half3 geometricNormalWS = NormalizeNormalPerPixel(input.normalWS);
                float3 tangentWS = TransformObjectToWorldDir(tangentOS);
                tangentWS = normalize(tangentWS - geometricNormalWS * dot(tangentWS, geometricNormalWS));
                float3 bitangentWS = TransformObjectToWorldDir(bitangentOS);
                bitangentWS = normalize(cross(geometricNormalWS, tangentWS)
                    * sign(dot(cross(geometricNormalWS, tangentWS), bitangentWS)));
                inputData.normalWS = NormalizeNormalPerPixel(
                    tangentWS * normalTS.x + bitangentWS * normalTS.y + geometricNormalWS * normalTS.z);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Glossiness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = baseColor.a;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode" = "DepthNormalsOnly" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct DepthNormalsAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DepthNormalsVaryings DepthNormalsVert(DepthNormalsAttributes input)
            {
                DepthNormalsVaryings output = (DepthNormalsVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                return output;
            }

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BorderUV;
                float _BorderWorld;
                float _TileWorld;
                float _BumpScale;
            CBUFFER_END

            float DepthNormalsSliceAxis(float p, float size, float borderUV, out float uvSlope)
            {
                p = saturate(p);
                borderUV = clamp(borderUV, 0.0, 0.5);
                float borderWorld = clamp(_BorderWorld, 0.0, size * 0.5);
                float middleUV = 1.0 - 2.0 * borderUV;

                if (borderWorld > 1e-5 && p * size < borderWorld)
                {
                    uvSlope = borderUV / borderWorld;
                    return (p * size / borderWorld) * borderUV;
                }

                if (borderWorld > 1e-5 && p * size > size - borderWorld)
                {
                    uvSlope = borderUV / borderWorld;
                    return 1.0 - ((size - p * size) / borderWorld) * borderUV;
                }

                float middleSize = max(size - 2.0 * borderWorld, 1e-5);
                float tileCount = max(1.0, round(middleSize / max(_TileWorld, 1e-5)));
                uvSlope = tileCount * middleUV / middleSize;
                float tilePosition = (p * size - borderWorld) / middleSize * tileCount;
                return borderUV + frac(tilePosition) * middleUV;
            }

            void DepthNormalsFrag(
                DepthNormalsVaryings input,
                out half4 normalWS : SV_Target0
                #ifdef _WRITE_RENDERING_LAYERS
                    , out uint renderingLayers : SV_Target1
                #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22));

                float3 faceNormal = abs(input.normalOS);
                float2 facePosition;
                float2 faceSize;
                float3 tangentOS;
                float3 bitangentOS;

                if (faceNormal.x > faceNormal.y && faceNormal.x > faceNormal.z)
                {
                    facePosition = input.positionOS.zy;
                    faceSize = objectScale.zy;
                    tangentOS = float3(0, 0, 1);
                    bitangentOS = float3(0, 1, 0);
                }
                else if (faceNormal.y > faceNormal.z)
                {
                    facePosition = input.positionOS.xz;
                    faceSize = objectScale.xz;
                    tangentOS = float3(1, 0, 0);
                    bitangentOS = float3(0, 0, 1);
                }
                else
                {
                    facePosition = input.positionOS.xy;
                    faceSize = objectScale.xy;
                    tangentOS = float3(1, 0, 0);
                    bitangentOS = float3(0, 1, 0);
                }

                facePosition += 0.5;

                float uvSlopeX;
                float uvSlopeY;
                float2 uv = float2(
                    DepthNormalsSliceAxis(facePosition.x, faceSize.x, _BorderUV.x, uvSlopeX),
                    DepthNormalsSliceAxis(facePosition.y, faceSize.y, _BorderUV.y, uvSlopeY));
                float2 uvSlope = float2(uvSlopeX, uvSlopeY);
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D_GRAD(
                        _BumpMap,
                        sampler_BumpMap,
                        uv,
                        uvSlope * ddx(facePosition),
                        uvSlope * ddy(facePosition)),
                    _BumpScale);

                half3 geometricNormalWS = NormalizeNormalPerPixel(
                    TransformObjectToWorldNormal(input.normalOS));
                float3 tangentWS = TransformObjectToWorldDir(tangentOS);
                tangentWS = normalize(tangentWS - geometricNormalWS * dot(tangentWS, geometricNormalWS));
                float3 bitangentWS = TransformObjectToWorldDir(bitangentOS);
                bitangentWS = normalize(cross(geometricNormalWS, tangentWS)
                    * sign(dot(cross(geometricNormalWS, tangentWS), bitangentWS)));
                half3 normal = NormalizeNormalPerPixel(
                    tangentWS * normalTS.x + bitangentWS * normalTS.y + geometricNormalWS * normalTS.z);

                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 octNormal = PackNormalOctQuadEncode(normal);
                    float2 remappedOctNormal = saturate(octNormal * 0.5 + 0.5);
                    normalWS = half4(PackFloat2To888(remappedOctNormal), 0.0);
                #else
                    normalWS = half4(normal, 0.0);
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    renderingLayers = EncodeMeshRenderingLayer();
                #endif
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
