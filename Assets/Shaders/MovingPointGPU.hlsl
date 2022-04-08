#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3> _Positions;
#endif

float _Step;
float4x4 _ContainerTransformMatrix;
float3 _ContainerPosition;

void ConfigureProcedural() {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Positions[unity_InstanceID]; //gets its position calculated by the compute shader, then sets the object to world based on the position.

		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0) + float4(_ContainerPosition, 0);
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0) * abs(float4(_ContainerTransformMatrix._m00_m11_m22_m33)) + float4(_ContainerTransformMatrix._m03_m13_m23, 0);
		unity_ObjectToWorld._m00_m11_m22_m33 = _Step * abs(float4(_ContainerTransformMatrix._m00_m11_m22_m33));
		//_ContainerTransform._m00_m11_m22 = 1.0;
		//unity_ObjectToWorld._m03_m13_m23 = unity_ObjectToWorld._m03_m13_m23 + _ContainerPosition;
	#endif
}
