Shader "Custom/Point Surface GPU"
{
    Properties
    {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5 
    }

    SubShader
    {

        CGPROGRAM

		// pragma is a compiler directive
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow //create a surface shader with Standard lighting and full support for shadows
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma target 4.5 //sets a minimum for the shaders target level and quality (?) was 3 made it 4.5 to use compute shader

		#include "..\PointGPU.hlsl"

		struct Input{
			float3 worldPos;
		};

		float _Smoothness;

		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) 
		{
			surface.Smoothness = _Smoothness;
			surface.Albedo.rgb = saturate(input.worldPos.xyz * 0.5 + 0.5);
		}
        ENDCG
    }
    FallBack "Diffuse"
}
