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

//V.312.U-V-PR


//-----------------helpers and utilities-------------------------------
	float Desaturate(half3 inTex){
		return dot(inTex.rgb, half3(0.2126, 0.7152, 0.0722));
	}

	float invLerp(float from, float to, float value){
		return (value - from) / (to - from);
	}

	float remap(float targetFrom, float targetTo, float value){
		float rel = invLerp(0, 1, value);
		return lerp(targetFrom, targetTo, rel);
	}

	float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value){
	  float rel = invLerp(origFrom, origTo, value);
	  return lerp(targetFrom, targetTo, rel);
	}

	float clamp01(float _in){
		if(_in > 1){ _in = 1; }
		if(_in < 0){ _in = 0; }
		return _in;
	}

	half eq(half a, half b){
		return 1.0 - abs(sign(a - b));
	}

	half gT(half a, half b){
		return max(sign(a - b), 0.0);
	}

	half lT(half a, half b){
		return max(sign(b - a), 0.0);
	}

	//Borrowed from https://catlikecoding.com/, thanks!
	float3 BoxProjection (float3 direction, float3 position, float3 cubemapPosition, float3 boxMin, float3 boxMax){
		float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / (direction + 0.0001);
		float scalar = min(min(factors.x, factors.y), factors.z);
		return boxMin > 0 ? direction * scalar + (position - cubemapPosition) : direction;
		//return direction * scalar + (position - cubemapPosition);
	}

	//Borrowed from Sam Hocevar, thanks!
	float3 rgb2hsv(float3 c)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;
		return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	float3 hsv2rgb(float3 c)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
		return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	}



//----------------LIGHTING------------------------------------------
//BadCompanyUtilityBRDF------------------------

	float GGXNDF(float roughness, float NdotH){
		float roughnessSqr = roughness*roughness;
		float NdotHSqr = NdotH*NdotH;
		float TanNdotHSqr = (1-NdotHSqr)/NdotHSqr;
		return (1.0/3.1415926535) * ((roughness/(NdotHSqr * (roughnessSqr + TanNdotHSqr))) * (roughness/(NdotHSqr * (roughnessSqr + TanNdotHSqr))));
	}

	//smoother than the GGX modified-WalterEtAl, so we're gonna stick with this one!
	float WalterEtAlGSF (float NdotL, float NdotV, float alpha){
		float alphaSqr = alpha*alpha;
		float NdotLSqr = NdotL*NdotL;
		float NdotVSqr = NdotV*NdotV;

		float SmithL = 2/(1 + sqrt(1 + alphaSqr * (1-NdotLSqr)/(NdotLSqr)));
		float SmithV = 2/(1 + sqrt(1 + alphaSqr * (1-NdotVSqr)/(NdotVSqr)));


		float Gs =  (SmithL * SmithV);
		return Gs;
	}


	half ivGmIeU(half3 vRmD, half3 rLWM)
	{
		half3 vRmDr = vRmD;
		half3 rLWMr = rLWM;
		half3 vR = reflect(-vRmDr, rLWMr);
		half wuiN = dot(vRmDr, vR);
		half oiNV = 0.5 * dot(rLWMr, vRmDr);
		return wuiN + oiNV;
		//half ivG = abs(ivGmIeU(i.viewDir.xyz, reflectDir.xyz) * _IVG);
	}

	half2 parallaxUVs(half3 tangentViewDir, half strength, half height, half2 uv){
		tangentViewDir = normalize(tangentViewDir);
		return uv.xy += tangentViewDir.xy * (height * strength);
	}




	//OLD
	/*half4 LightingSimpleBRDF_GI (SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
	{
		half3 bakedLightmap = half3(0,0,0);
		#if defined(LIGHTMAP_ON)
			half4 bakedLightmapTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);
			bakedLightmap = DecodeLightmap(bakedLightmapTex);

			#ifdef DIRLIGHTMAP_COMBINED
				fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
				bakedLightmap += DecodeDirectionalLightmap(bakedLightmap, bakedDirTex, s.Normal);
				bakedLightmap += gi.indirect.specular;
			#else
				#if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
					bakedLightmap = SubtractMainLightWithRealtimeAttenuationFromLightmap(bakedLightmap, data.atten, bakedLightmapTex, s.Normal);
				#endif
			#endif
		#endif
		return half4(bakedLightmap, 0);
	}*/
