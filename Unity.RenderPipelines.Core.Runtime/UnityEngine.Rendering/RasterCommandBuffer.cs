using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public class RasterCommandBuffer : BaseCommandBuffer, IRasterCommandBuffer, IBaseCommandBuffer
{
	internal RasterCommandBuffer(CommandBuffer wrapped, RenderGraphPass executingPass, bool isAsync)
		: base(wrapped, executingPass, isAsync)
	{
	}

	public void SetInvertCulling(bool invertCulling)
	{
		m_WrappedCommandBuffer.SetInvertCulling(invertCulling);
	}

	public void SetViewport(Rect pixelRect)
	{
		m_WrappedCommandBuffer.SetViewport(pixelRect);
	}

	public void EnableScissorRect(Rect scissor)
	{
		m_WrappedCommandBuffer.EnableScissorRect(scissor);
	}

	public void DisableScissorRect()
	{
		m_WrappedCommandBuffer.DisableScissorRect();
	}

	public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor)
	{
		m_WrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor);
	}

	public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth)
	{
		m_WrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor, depth);
	}

	public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth, uint stencil)
	{
		m_WrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor, depth, stencil);
	}

	public void ClearRenderTarget(RTClearFlags clearFlags, Color backgroundColor, float depth, uint stencil)
	{
		m_WrappedCommandBuffer.ClearRenderTarget(clearFlags, backgroundColor, depth, stencil);
	}

	public void ClearRenderTarget(RTClearFlags clearFlags, Color[] backgroundColors, float depth, uint stencil)
	{
		m_WrappedCommandBuffer.ClearRenderTarget(clearFlags, backgroundColors, depth, stencil);
	}

	public void SetGlobalFloat(int nameID, float value)
	{
		m_WrappedCommandBuffer.SetGlobalFloat(nameID, value);
	}

	public void SetGlobalInt(int nameID, int value)
	{
		m_WrappedCommandBuffer.SetGlobalInt(nameID, value);
	}

	public void SetGlobalInteger(int nameID, int value)
	{
		m_WrappedCommandBuffer.SetGlobalInteger(nameID, value);
	}

	public void SetGlobalVector(int nameID, Vector4 value)
	{
		m_WrappedCommandBuffer.SetGlobalVector(nameID, value);
	}

	public void SetGlobalColor(int nameID, Color value)
	{
		m_WrappedCommandBuffer.SetGlobalColor(nameID, value);
	}

	public void SetGlobalMatrix(int nameID, Matrix4x4 value)
	{
		m_WrappedCommandBuffer.SetGlobalMatrix(nameID, value);
	}

	public void EnableShaderKeyword(string keyword)
	{
		m_WrappedCommandBuffer.EnableShaderKeyword(keyword);
	}

	public void EnableKeyword(in GlobalKeyword keyword)
	{
		m_WrappedCommandBuffer.EnableKeyword(in keyword);
	}

	public void EnableKeyword(Material material, in LocalKeyword keyword)
	{
		m_WrappedCommandBuffer.EnableKeyword(material, in keyword);
	}

	public void EnableKeyword(ComputeShader computeShader, in LocalKeyword keyword)
	{
		m_WrappedCommandBuffer.EnableKeyword(computeShader, in keyword);
	}

	public void DisableShaderKeyword(string keyword)
	{
		m_WrappedCommandBuffer.DisableShaderKeyword(keyword);
	}

	public void DisableKeyword(in GlobalKeyword keyword)
	{
		m_WrappedCommandBuffer.DisableKeyword(in keyword);
	}

	public void DisableKeyword(Material material, in LocalKeyword keyword)
	{
		m_WrappedCommandBuffer.DisableKeyword(material, in keyword);
	}

	public void DisableKeyword(ComputeShader computeShader, in LocalKeyword keyword)
	{
		m_WrappedCommandBuffer.DisableKeyword(computeShader, in keyword);
	}

	public void SetKeyword(in GlobalKeyword keyword, bool value)
	{
		m_WrappedCommandBuffer.SetKeyword(in keyword, value);
	}

	public void SetKeyword(Material material, in LocalKeyword keyword, bool value)
	{
		m_WrappedCommandBuffer.SetKeyword(material, in keyword, value);
	}

	public void SetKeyword(ComputeShader computeShader, in LocalKeyword keyword, bool value)
	{
		m_WrappedCommandBuffer.SetKeyword(computeShader, in keyword, value);
	}

	public void SetViewProjectionMatrices(Matrix4x4 view, Matrix4x4 proj)
	{
		m_WrappedCommandBuffer.SetViewProjectionMatrices(view, proj);
	}

	public void SetGlobalDepthBias(float bias, float slopeBias)
	{
		m_WrappedCommandBuffer.SetGlobalDepthBias(bias, slopeBias);
	}

	public void SetGlobalFloatArray(int nameID, float[] values)
	{
		m_WrappedCommandBuffer.SetGlobalFloatArray(nameID, values);
	}

	public void SetGlobalVectorArray(int nameID, Vector4[] values)
	{
		m_WrappedCommandBuffer.SetGlobalVectorArray(nameID, values);
	}

	public void SetGlobalMatrixArray(int nameID, Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetGlobalMatrixArray(nameID, values);
	}

	public void SetLateLatchProjectionMatrices(Matrix4x4[] projectionMat)
	{
		m_WrappedCommandBuffer.SetLateLatchProjectionMatrices(projectionMat);
	}

	public void MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType matrixPropertyType, int shaderPropertyID)
	{
		m_WrappedCommandBuffer.MarkLateLatchMatrixShaderPropertyID(matrixPropertyType, shaderPropertyID);
	}

	public void UnmarkLateLatchMatrix(CameraLateLatchMatrixType matrixPropertyType)
	{
		m_WrappedCommandBuffer.UnmarkLateLatchMatrix(matrixPropertyType);
	}

	public void BeginSample(string name)
	{
		m_WrappedCommandBuffer.BeginSample(name);
	}

	public void EndSample(string name)
	{
		m_WrappedCommandBuffer.EndSample(name);
	}

	public void BeginSample(CustomSampler sampler)
	{
		m_WrappedCommandBuffer.BeginSample(sampler);
	}

	public void EndSample(CustomSampler sampler)
	{
		m_WrappedCommandBuffer.EndSample(sampler);
	}

	public void BeginSample(ProfilerMarker marker)
	{
	}

	public void EndSample(ProfilerMarker marker)
	{
	}

	public void IncrementUpdateCount(RenderTargetIdentifier dest)
	{
		m_WrappedCommandBuffer.IncrementUpdateCount(dest);
	}

	public void SetInstanceMultiplier(uint multiplier)
	{
		m_WrappedCommandBuffer.SetInstanceMultiplier(multiplier);
	}

	public void SetFoveatedRenderingMode(FoveatedRenderingMode foveatedRenderingMode)
	{
		m_WrappedCommandBuffer.SetFoveatedRenderingMode(foveatedRenderingMode);
	}

	public void SetWireframe(bool enable)
	{
		m_WrappedCommandBuffer.SetWireframe(enable);
	}

	public void ConfigureFoveatedRendering(IntPtr platformData)
	{
		m_WrappedCommandBuffer.ConfigureFoveatedRendering(platformData);
	}

	public void SetupCameraProperties(Camera camera)
	{
		m_WrappedCommandBuffer.SetupCameraProperties(camera);
	}

	public void InvokeOnRenderObjectCallbacks()
	{
		m_WrappedCommandBuffer.InvokeOnRenderObjectCallbacks();
	}

	public void SetShadingRateFragmentSize(ShadingRateFragmentSize shadingRateFragmentSize)
	{
		m_WrappedCommandBuffer.SetShadingRateFragmentSize(shadingRateFragmentSize);
	}

	public void SetShadingRateCombiner(ShadingRateCombinerStage stage, ShadingRateCombiner combiner)
	{
		m_WrappedCommandBuffer.SetShadingRateCombiner(stage, combiner);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass)
	{
		m_WrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex)
	{
		m_WrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material)
	{
		m_WrappedCommandBuffer.DrawMesh(mesh, matrix, material);
	}

	public void DrawMultipleMeshes(Matrix4x4[] matrices, Mesh[] meshes, int[] subsetIndices, int count, Material material, int shaderPass, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMultipleMeshes(matrices, meshes, subsetIndices, count, material, shaderPass, properties);
	}

	public void DrawRenderer(Renderer renderer, Material material, int submeshIndex, int shaderPass)
	{
		m_WrappedCommandBuffer.DrawRenderer(renderer, material, submeshIndex, shaderPass);
	}

	public void DrawRenderer(Renderer renderer, Material material, int submeshIndex)
	{
		m_WrappedCommandBuffer.DrawRenderer(renderer, material, submeshIndex);
	}

	public void DrawRenderer(Renderer renderer, Material material)
	{
		m_WrappedCommandBuffer.DrawRenderer(renderer, material);
	}

	public void DrawRendererList(RendererList rendererList)
	{
		m_WrappedCommandBuffer.DrawRendererList(rendererList);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount, properties);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount)
	{
		m_WrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount)
	{
		m_WrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount, properties);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount)
	{
		m_WrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount)
	{
		m_WrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs);
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count, properties);
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count)
	{
		m_WrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count);
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices)
	{
		m_WrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices);
	}

	public void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedProcedural(mesh, submeshIndex, material, shaderPass, count, properties);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, properties);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, properties);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs)
	{
		m_WrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs);
	}

	public void DrawOcclusionMesh(RectInt normalizedCamViewport)
	{
		m_WrappedCommandBuffer.DrawOcclusionMesh(normalizedCamViewport);
	}

	public void SetGlobalFloat(string name, float value)
	{
		m_WrappedCommandBuffer.SetGlobalFloat(name, value);
	}

	public void SetGlobalInt(string name, int value)
	{
		m_WrappedCommandBuffer.SetGlobalInt(name, value);
	}

	public void SetGlobalInteger(string name, int value)
	{
		m_WrappedCommandBuffer.SetGlobalInteger(name, value);
	}

	public void SetGlobalVector(string name, Vector4 value)
	{
		m_WrappedCommandBuffer.SetGlobalVector(name, value);
	}

	public void SetGlobalColor(string name, Color value)
	{
		m_WrappedCommandBuffer.SetGlobalColor(name, value);
	}

	public void SetGlobalMatrix(string name, Matrix4x4 value)
	{
		m_WrappedCommandBuffer.SetGlobalMatrix(name, value);
	}

	public void SetGlobalFloatArray(string propertyName, List<float> values)
	{
		m_WrappedCommandBuffer.SetGlobalFloatArray(propertyName, values);
	}

	public void SetGlobalFloatArray(int nameID, List<float> values)
	{
		m_WrappedCommandBuffer.SetGlobalFloatArray(nameID, values);
	}

	public void SetGlobalFloatArray(string propertyName, float[] values)
	{
		m_WrappedCommandBuffer.SetGlobalFloatArray(propertyName, values);
	}

	public void SetGlobalVectorArray(string propertyName, List<Vector4> values)
	{
		m_WrappedCommandBuffer.SetGlobalVectorArray(propertyName, values);
	}

	public void SetGlobalVectorArray(int nameID, List<Vector4> values)
	{
		m_WrappedCommandBuffer.SetGlobalVectorArray(nameID, values);
	}

	public void SetGlobalVectorArray(string propertyName, Vector4[] values)
	{
		m_WrappedCommandBuffer.SetGlobalVectorArray(propertyName, values);
	}

	public void SetGlobalMatrixArray(string propertyName, List<Matrix4x4> values)
	{
		m_WrappedCommandBuffer.SetGlobalMatrixArray(propertyName, values);
	}

	public void SetGlobalMatrixArray(int nameID, List<Matrix4x4> values)
	{
		m_WrappedCommandBuffer.SetGlobalMatrixArray(nameID, values);
	}

	public void SetGlobalMatrixArray(string propertyName, Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetGlobalMatrixArray(propertyName, values);
	}

	public void SetGlobalTexture(string name, TextureHandle value)
	{
		m_WrappedCommandBuffer.SetGlobalTexture(name, value);
	}

	public void SetGlobalTexture(int nameID, TextureHandle value)
	{
		m_WrappedCommandBuffer.SetGlobalTexture(nameID, value);
	}

	public void SetGlobalTexture(string name, TextureHandle value, RenderTextureSubElement element)
	{
		m_WrappedCommandBuffer.SetGlobalTexture(name, value, element);
	}

	public void SetGlobalTexture(int nameID, TextureHandle value, RenderTextureSubElement element)
	{
		m_WrappedCommandBuffer.SetGlobalTexture(nameID, value, element);
	}

	public void SetGlobalBuffer(string name, ComputeBuffer value)
	{
		m_WrappedCommandBuffer.SetGlobalBuffer(name, value);
	}

	public void SetGlobalBuffer(int nameID, ComputeBuffer value)
	{
		m_WrappedCommandBuffer.SetGlobalBuffer(nameID, value);
	}

	public void SetGlobalBuffer(string name, GraphicsBuffer value)
	{
		m_WrappedCommandBuffer.SetGlobalBuffer(name, value);
	}

	public void SetGlobalBuffer(int nameID, GraphicsBuffer value)
	{
		m_WrappedCommandBuffer.SetGlobalBuffer(nameID, value);
	}

	public void SetGlobalConstantBuffer(ComputeBuffer buffer, int nameID, int offset, int size)
	{
		m_WrappedCommandBuffer.SetGlobalConstantBuffer(buffer, nameID, offset, size);
	}

	public void SetGlobalConstantBuffer(ComputeBuffer buffer, string name, int offset, int size)
	{
		m_WrappedCommandBuffer.SetGlobalConstantBuffer(buffer, name, offset, size);
	}

	public void SetGlobalConstantBuffer(GraphicsBuffer buffer, int nameID, int offset, int size)
	{
		m_WrappedCommandBuffer.SetGlobalConstantBuffer(buffer, nameID, offset, size);
	}

	public void SetGlobalConstantBuffer(GraphicsBuffer buffer, string name, int offset, int size)
	{
		m_WrappedCommandBuffer.SetGlobalConstantBuffer(buffer, name, offset, size);
	}

	public void SetShadowSamplingMode(RenderTargetIdentifier shadowmap, ShadowSamplingMode mode)
	{
		m_WrappedCommandBuffer.SetShadowSamplingMode(shadowmap, mode);
	}

	public void SetSinglePassStereo(SinglePassStereoMode mode)
	{
		m_WrappedCommandBuffer.SetSinglePassStereo(mode);
	}

	public void IssuePluginEvent(IntPtr callback, int eventID)
	{
		m_WrappedCommandBuffer.IssuePluginEvent(callback, eventID);
	}

	public void IssuePluginEventAndData(IntPtr callback, int eventID, IntPtr data)
	{
		m_WrappedCommandBuffer.IssuePluginEventAndData(callback, eventID, data);
	}

	public void IssuePluginCustomBlit(IntPtr callback, uint command, RenderTargetIdentifier source, RenderTargetIdentifier dest, uint commandParam, uint commandFlags)
	{
		m_WrappedCommandBuffer.IssuePluginCustomBlit(callback, command, source, dest, commandParam, commandFlags);
	}

	public void IssuePluginCustomTextureUpdateV2(IntPtr callback, Texture targetTexture, uint userData)
	{
		m_WrappedCommandBuffer.IssuePluginCustomTextureUpdateV2(callback, targetTexture, userData);
	}

	void IBaseCommandBuffer.EnableKeyword(in GlobalKeyword keyword)
	{
		EnableKeyword(in keyword);
	}

	void IBaseCommandBuffer.EnableKeyword(Material material, in LocalKeyword keyword)
	{
		EnableKeyword(material, in keyword);
	}

	void IBaseCommandBuffer.EnableKeyword(ComputeShader computeShader, in LocalKeyword keyword)
	{
		EnableKeyword(computeShader, in keyword);
	}

	void IBaseCommandBuffer.DisableKeyword(in GlobalKeyword keyword)
	{
		DisableKeyword(in keyword);
	}

	void IBaseCommandBuffer.DisableKeyword(Material material, in LocalKeyword keyword)
	{
		DisableKeyword(material, in keyword);
	}

	void IBaseCommandBuffer.DisableKeyword(ComputeShader computeShader, in LocalKeyword keyword)
	{
		DisableKeyword(computeShader, in keyword);
	}

	void IBaseCommandBuffer.SetKeyword(in GlobalKeyword keyword, bool value)
	{
		SetKeyword(in keyword, value);
	}

	void IBaseCommandBuffer.SetKeyword(Material material, in LocalKeyword keyword, bool value)
	{
		SetKeyword(material, in keyword, value);
	}

	void IBaseCommandBuffer.SetKeyword(ComputeShader computeShader, in LocalKeyword keyword, bool value)
	{
		SetKeyword(computeShader, in keyword, value);
	}
}
