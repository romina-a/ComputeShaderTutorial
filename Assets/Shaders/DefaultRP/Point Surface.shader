Shader "Custom/Point Surface"
{
    Properties
    {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5 
    }
    SubShader
    {

        CGPROGRAM
		// pragma is a compiler directive
		#pragma surface ConfigureSurface Standard fullforwardshadows //create a surface shader with Standard lighting and full support for shadows
		#pragma target 4.5 //sets a minimum for the shaders target level and quality (?)

		struct Input{
			float3 worldPos;
		};

		float _Smoothness;

		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
			surface.Smoothness = _Smoothness;
			surface.Albedo.rgb = saturate(input.worldPos.xyz * 0.5 + 0.5);
		}
        ENDCG
    }
    FallBack "Diffuse"
}
