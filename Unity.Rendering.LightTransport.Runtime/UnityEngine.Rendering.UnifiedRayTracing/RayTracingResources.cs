namespace UnityEngine.Rendering.UnifiedRayTracing;

internal class RayTracingResources
{
	public ComputeShader geometryPoolKernels;

	public ComputeShader copyBuffer;

	public Shader hardwareRayTracingMaterial;

	public ComputeShader copyPositions;

	public ComputeShader bitHistogram;

	public ComputeShader blockReducePart;

	public ComputeShader blockScan;

	public ComputeShader buildHlbvh;

	public ComputeShader restructureBvh;

	public ComputeShader scatter;
}
