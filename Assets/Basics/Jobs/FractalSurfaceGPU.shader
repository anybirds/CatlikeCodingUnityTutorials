Shader "Graph/PointSurfaceGPU" {
	Properties {
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
	}

	SubShader {
		CGPROGRAM

		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		#pragma target 4.5
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation

		struct Input {
			float3 worldPos;
		};

		#include "FractalGPU.hlsl"

		void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
			float4 color = GetFractalColor();
			surface.Albedo = color.rgb;
			surface.Smoothness = color.a;
		}

		ENDCG
	}

	Fallback "Diffuse"
}