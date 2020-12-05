/*

MIT License

Copyright (c) 2020 GentleLeviathan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

//V.1.0.0.1128.U-PR1


Shader "Hidden/BadCompany/Bad Company Utility (Detail Subtract)"
{
		Properties
	{
		_MainTex("Diffuse", 2D) = "white" {}
		[HideInInspector]_CC("CC", 2D) = "red" {}
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		[HideInInspector]_MetallicTex("Metallic Texture", 2D) = "white" {}
		[HideInInspector]_RoughnessTex("Roughness Texture", 2D) = "white" {}
		[HideInInspector]_SpecularTex("Specular Texture", 2D) = "black" {}
		[HideInInspector]_ParallaxMap("Height Map", 2D) = "black" {}
		[HideInInspector]_ParallaxStrength("Height Strength", Range (0 , 0.1)) = 0.03
		[HideInInspector]_CubeReflection("Cubemapped Reflection", CUBE) = "" {}
		[HideInInspector]_IllumTex("Illumination Texture", 2D) = "black" {}
		[HideInInspector]_Color("Color", Color) = (1,0,0,0)
		[HideInInspector]_SecondaryColor("Secondary Color", Color) = (0,1,0,0)
		[HideInInspector]_TertiaryColor("Tertiary Color", Color) = (1,1,1,1)
		[HideInInspector]_IllumColor("Illumination Color", Color) = (0,0,0,0)
		[HideInInspector]_Specular("Specular Multiplier", Range( 0 , 1)) = 1
		[HideInInspector]_MetallicEffect("Metallic", Range(0 , 1)) = 1
		[HideInInspector]_Roughness("Roughness", Range(0 , 1)) = 0.3
		[HideInInspector]_Deepness("Color Deepness", Range(0,1)) = 0
		[HideInInspector]_ColorTexture("Color Texture", Int) = 0
		[HideInInspector]_isSimpleRoughness("Simple Roughness", Int) = 0
		[HideInInspector]_BungieRoughness("Bungie-era Alpha Roughness", Int) = 0
		[HideInInspector]_isH5("Halo 5 Armor", Int) = 0
		[HideInInspector]_isH4("Storm Armor? (H4)", Int) = 0
		[HideInInspector]_trueMetallic("True Metallic", Int) = 0
		[HideInInspector]_cubemappedReflection("Cubemapped Reflection?", Int) = 0
		[HideInInspector]_Occlusion("Occlusion Texture", 2D) = "white" {}
		[HideInInspector]_DetailMap("Detail Map", 2D) = "black" {}
		[HideInInspector]_DetailNormal("Detail Normal", 2D) = "bump" {}
		[HideInInspector]_DetailStrength("Detail Map Strength", Range(0, 2)) = 0.5
		[HideInInspector]_lightingBypass("bypassLighting", Int) = 0
	}

	SubShader
	{
		Pass
		{
			Name "META"
			Tags { "LightMode"="Meta" }
			Cull Off
			CGPROGRAM

			#pragma vertex vert_meta
            #pragma fragment frag_meta2
			#pragma shader_feature _EMISSION
			#include "UnityStandardMeta.cginc"
			#include "../CgInc/BadCoUtilities.cginc"


			sampler2D _CC;
			sampler2D _IllumTex;
			sampler2D _Occlusion;
			fixed4 _SecondaryColor;
			fixed4 _IllumColor;
			fixed _Deepness;
			fixed _isH5;
			fixed _isH4;
			fixed _ColorTexture;

			float4 frag_meta2 (v2f_meta i): SV_Target
			{
				FragmentCommonData data = UNITY_SETUP_BRDF_INPUT (i.uv);
				UnityMetaInput o;
				UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

				//tex
				half4 diffuse = tex2D(_MainTex, i.uv.xy);
				half4 cc = tex2D(_CC, i.uv.xy);
				half4 illumTex = tex2D(_IllumTex, i.uv.xy);
				half4 occlusionTex = tex2D(_Occlusion, i.uv.xy);

				//desaturation
				diffuse.xyz = lerp(diffuse.xyz, Desaturate(diffuse.xyz), gT(_ColorTexture, 0));

				//'deepness'
				diffuse = lerp(diffuse, diffuse * diffuse, _Deepness);

				//h5 cc correction because buttloads of compression artifacts
				half h5Check = gT(_isH5, 0);
				cc.r = (cc.r * lT(_isH5, 1)) + ((h5Check * 0.976) * gT(cc.r, 0.15));
				cc.g = (cc.g * lT(_isH5, 1)) + ((h5Check * 0.976) * lT(cc.r, 0.15) * gT(cc.g, 0.3));

				//storm texture replacements
				half stormCheck = gT(_isH4, 0);
				diffuse = lerp(diffuse, half4(diffuse.ggg, diffuse.a), stormCheck);
				cc.g = lerp(cc.g, clamp01((1.0 - diffuse.a) * 2), stormCheck * gT(diffuse.a, 0));
				cc.r = lerp(cc.r, pow(diffuse.a, 10), stormCheck);

				//masking
				half4 black = half4(0,0,0,0);
				half4 primary = lerp(black, diffuse * _Color, cc.r);
				half4 secondary = lerp(black, diffuse * _SecondaryColor, cc.g);
				half4 passthrough = lerp(diffuse, black, cc.r + cc.g);

				//finalColor
				half4 finalColor = primary + secondary + passthrough;
				finalColor *= occlusionTex;

				o.Albedo = finalColor;
				o.Emission = Emission(illumTex * _IllumColor);

				return UnityMetaFragment(o);
			}

			ENDCG
		}

		Pass
		{
			Name "ERROR"
			Tags { "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return half4(0, 0, 0, 1);
			}
			ENDCG
		}
		

		Tags { "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZWrite On
		ZTest LEqual
		CGPROGRAM

		#include "../CgInc/BadCoUtilities.cginc"
		#pragma target 4.0
		#pragma only_renderers d3d11 glcore gles
		#pragma surface surface SimpleBRDF addshadow fullforwardshadows
		#pragma shader_feature PARALLAX_ON

		uniform fixed _lightingBypass;

		half4 LightingSimpleBRDF (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half3 h = normalize (lightDir + viewDir);
			half diff = max (0, dot (s.Normal, lightDir));
			half4 NdotL = dot(s.Normal, lightDir);

			float specDist = GGXNDF(1 - s.Gloss, dot(s.Normal, h));
			float shadowDist = WalterEtAlGSF(NdotL, dot(s.Normal, viewDir), 1 - s.Gloss);
			half4 spec = s.Specular * specDist * shadowDist;
			spec *= NdotL;
			half3 hsvAlbedo = rgb2hsv(s.Albedo.rgb);
			half3 specMetalColor = hsv2rgb(half3(hsvAlbedo.r, hsvAlbedo.g, 1));
			spec = lerp(spec, spec * half4(specMetalColor,1), s.Alpha);
			spec = saturate(spec);

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
			c.rgb *= 1 - _lightingBypass;
			c.rgb += s.Albedo * _lightingBypass;
			c.a = 1;
			return c;
		}

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_MetallicTex;
			float2 uv_RoughnessTex;
			float2 uv_SpecularTex;
			float2 uv_IllumTex;
			float2 uv_Occlusion;
			float2 uv_DetailMap;
			half3 worldPos;
			half3 worldRefl; INTERNAL_DATA
			half3 worldNormal;
			half3 viewDir;
			half3 lightDir;
		};

		uniform sampler2D _MainTex;
		uniform sampler2D _BumpMap;
		uniform sampler2D _CC;
		uniform sampler2D _MetallicTex;
		uniform sampler2D _RoughnessTex;
		uniform sampler2D _SpecularTex;
		uniform sampler2D _ParallaxMap;
		UNITY_DECLARE_TEXCUBE(_CubeReflection);
		uniform sampler2D _IllumTex;
		uniform sampler2D _Occlusion;
		uniform sampler2D _DetailMap;
		uniform sampler2D _DetailNormal;

		uniform half4 _Color;
		uniform half4 _SecondaryColor;
		uniform half4 _TertiaryColor;
		uniform half4 _IllumColor;
		uniform half _Specular;
		uniform half _MetallicEffect;
		uniform half _Roughness;
		uniform half _Deepness;
		uniform half _DetailStrength;
		uniform half _ParallaxStrength;
		uniform fixed _isSimpleRoughness;
		uniform fixed _BungieRoughness;
		uniform fixed _isH5;
		uniform fixed _isH4;
		uniform fixed _trueMetallic;
		uniform fixed _ColorTexture;
		uniform fixed _cubemappedReflection;

		void surface( Input i, inout SurfaceOutput o )
		{
			//parallax
			#if PARALLAX_ON
				half4 parallaxTex = tex2D(_ParallaxMap, i.uv_MainTex.xy);
				half2 pOffset = ParallaxOffset(parallaxTex.r, _ParallaxStrength, i.viewDir);
				i.uv_MainTex.xy += pOffset.xy;
			#endif

			//tex
			half4 diffuse = tex2D(_MainTex, i.uv_MainTex.xy);
			half4 cc = tex2D(_CC, i.uv_MainTex.xy);
			half4 metallicTex = tex2D(_MetallicTex, i.uv_MetallicTex.xy);
			half4 roughTex = tex2D(_RoughnessTex, i.uv_RoughnessTex.xy);
			half4 specularTex = tex2D(_SpecularTex, i.uv_SpecularTex.xy);
			half4 illumTex = tex2D(_IllumTex, i.uv_IllumTex.xy);
			half4 occlusionTex = tex2D(_Occlusion, i.uv_Occlusion.xy);
			half4 detailTex = tex2D(_DetailMap, i.uv_DetailMap.xy) * _DetailStrength;
			float4 detailNormal = tex2D(_DetailNormal, i.uv_DetailMap.xy);
			float4 normal = tex2D(_BumpMap, i.uv_MainTex.xy);
			float3 finalNormal = UnpackNormal(float4(normal.r, 1 - normal.g, normal.b, normal.a));

			//desaturation
			diffuse.xyz = lerp(diffuse.xyz, Desaturate(diffuse.xyz), gT(_ColorTexture, 0));

			//color 'deepness'
			diffuse = lerp(diffuse, diffuse * diffuse, _Deepness);

			//h5 cc correction because buttloads of compression artifacts
			half h5Check = gT(_isH5, 0);
			cc.r = (cc.r * lT(_isH5, 1)) + ((h5Check * 0.976) * gT(cc.r, 0.15));
			cc.g = (cc.g * lT(_isH5, 1)) + ((h5Check * 0.976) * lT(cc.r, 0.15) * gT(cc.g, 0.3));

			//storm texture replacements
			half stormCheck = gT(_isH4, 0);
			diffuse = lerp(diffuse, half4(diffuse.ggg, diffuse.a), stormCheck);
			cc.g = lerp(cc.g, clamp01((1.0 - diffuse.a) * 2), stormCheck * gT(diffuse.a, 0));
			cc.r = lerp(cc.r, pow(diffuse.a, 10), stormCheck);

			//masking
			half4 black = half4(0,0,0,0);
			half4 primary = lerp(black, diffuse * _Color, cc.r);
			half4 secondary = lerp(black, diffuse * _SecondaryColor, cc.g);
			half4 tertiary = lerp(black, diffuse * _TertiaryColor, cc.b);
			half4 passthrough = lerp(diffuse, black, cc.r + cc.g + cc.b);

			//specular Masking
			half4 primarySpecular = lerp(black, specularTex * _Color, cc.r);
			half4 secondarySpecular = lerp(black, specularTex * _SecondaryColor, cc.g);
			half4 specularPassthrough = lerp(specularTex, black, cc.r + cc.g + cc.b);

			//roughness
			half calculatedRoughness = Desaturate(diffuse.xyz) * (0.99 - _Roughness);
			half textureRoughness = remap(0, 1, 1, 0, roughTex) * (0.99 - _Roughness);
			half diffuseA = remap(0, 1, 0.5, 1, diffuse.a);
			half bungieRoughness = clamp01(diffuseA * (0.99 - _Roughness));
			half h5Roughness = clamp01(cc.a * 1.5) * (0.99 - _Roughness) - 0.1;

			calculatedRoughness = lerp(calculatedRoughness, h5Roughness, h5Check);
			calculatedRoughness = lerp(calculatedRoughness, diffuse.r * (0.99 - _Roughness), stormCheck);
			calculatedRoughness = lerp(calculatedRoughness, bungieRoughness, gT(_BungieRoughness, 0));
			calculatedRoughness = lerp(calculatedRoughness, textureRoughness, gT(textureRoughness, 0));
			calculatedRoughness = lerp(calculatedRoughness, 0.99 - _Roughness, gT(_isSimpleRoughness, 0));

			//others
			half4 illumination = illumTex * _IllumColor;
			metallicTex *= _MetallicEffect;
			metallicTex = lerp(metallicTex, metallicTex * (1 - cc.b), h5Check);

			//finalColor
			half4 finalColor = primary + secondary + tertiary + passthrough;
			finalColor *= occlusionTex;
			detailTex *= occlusionTex;
			detailTex *= calculatedRoughness;
			finalColor -= detailTex;

			//specular calcs
			half4 finalSpecularColor = diffuse;
			finalSpecularColor *= occlusionTex;
			//finalSpecularColor -= detailTex;

			half customSpecular = (primarySpecular + secondarySpecular + specularPassthrough) * _Specular;
			half finalSpecular = _Specular * finalSpecularColor;
			half h5Spec = clamp01((cc.b + 0.01 * finalSpecular) / (cc.b * 4 + 0.01));
			finalSpecular = lerp(finalSpecular, h5Spec * _Specular, h5Check);
			finalSpecular = lerp(finalSpecular, diffuse.g * _Specular, stormCheck);
			finalSpecular = lerp(finalSpecular, finalColor * customSpecular, gT(customSpecular, 0));

			//reflection probe support
			half3 reflectDir = WorldReflectionVector(i, finalNormal);
			half3 boxProjectionDir = BoxProjectedCubemapDirection(reflectDir + 0.001, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
			half texCubeRoughness = calculatedRoughness * 1.7 - 0.7 * calculatedRoughness;

			//reflectionData
			Unity_GlossyEnvironmentData envData; envData.roughness = (1 - texCubeRoughness); envData.reflUVW = boxProjectionDir;

			//probe blending
			half3 skyColor = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
			half3 skyColor2 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
			skyColor = lerp(skyColor2, skyColor, unity_SpecCube0_BoxMin.w);

			//cubemapped reflection support
			half3 cubeColor = UNITY_SAMPLE_TEXCUBE_LOD(_CubeReflection, reflectDir, envData.roughness * UNITY_SPECCUBE_LOD_STEPS).rgb;
			skyColor = lerp(skyColor, cubeColor, gT(_cubemappedReflection, 0));
			skyColor *= occlusionTex.xyz;

			//metallic effect
			half displayMetalProperty = metallicTex * _trueMetallic;
			skyColor *= lerp(skyColor * Desaturate(finalColor.xyz), skyColor * finalColor.xyz, _MetallicEffect * displayMetalProperty);
			skyColor *= 1 - _lightingBypass;
			//skyColor *= 1 - (dot(finalNormal, i.viewDir) * calculatedRoughness);
			finalColor *= (0.99 - (calculatedRoughness * metallicTex));
			//NDotV
			finalColor *= 1 - (dot(finalNormal, i.viewDir) * displayMetalProperty * (1.0 - _lightingBypass));

			//out
			o.Albedo = finalColor;
			o.Normal = finalNormal + UnpackNormal(detailNormal);
			o.Specular = finalSpecular;
			o.Gloss = saturate(calculatedRoughness + 0.01);
			o.Emission = illumination + skyColor + (finalColor * _lightingBypass);
			o.Alpha = displayMetalProperty;
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "BadCompany.Shaders.Utility.BCUtilityEditor"
}