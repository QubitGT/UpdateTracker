using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public class ComputeCommandBuffer : BaseCommandBuffer, IComputeCommandBuffer, IBaseCommandBuffer
{
	internal ComputeCommandBuffer(CommandBuffer wrapped, RenderGraphPass executingPass, bool isAsync)
		: base(wrapped, executingPass, isAsync)
	{
	}

	public void SetInvertCulling(bool invertCulling)
	{
		m_WrappedCommandBuffer.SetInvertCulling(invertCulling);
	}

	public void SetComputeFloatParam(ComputeShader computeShader, int nameID, float val)
	{
		m_WrappedCommandBuffer.SetComputeFloatParam(computeShader, nameID, val);
	}

	public void SetComputeIntParam(ComputeShader computeShader, int nameID, int val)
	{
		m_WrappedCommandBuffer.SetComputeIntParam(computeShader, nameID, val);
	}

	public void SetComputeVectorParam(ComputeShader computeShader, int nameID, Vector4 val)
	{
		m_WrappedCommandBuffer.SetComputeVectorParam(computeShader, nameID, val);
	}

	public void SetComputeVectorArrayParam(ComputeShader computeShader, int nameID, Vector4[] values)
	{
		m_WrappedCommandBuffer.SetComputeVectorArrayParam(computeShader, nameID, values);
	}

	public void SetComputeMatrixParam(ComputeShader computeShader, int nameID, Matrix4x4 val)
	{
		m_WrappedCommandBuffer.SetComputeMatrixParam(computeShader, nameID, val);
	}

	public void SetComputeMatrixArrayParam(ComputeShader computeShader, int nameID, Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetComputeMatrixArrayParam(computeShader, nameID, values);
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

	public void SetBufferData(ComputeBuffer buffer, Array data)
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData<T>(ComputeBuffer buffer, List<T> data) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData<T>(ComputeBuffer buffer, NativeArray<T> data) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData(ComputeBuffer buffer, Array data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, managedBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferData<T>(ComputeBuffer buffer, List<T> data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, managedBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferData<T>(ComputeBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, nativeBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferCounterValue(ComputeBuffer buffer, uint counterValue)
	{
		m_WrappedCommandBuffer.SetBufferCounterValue(buffer, counterValue);
	}

	public void SetBufferData(GraphicsBuffer buffer, Array data)
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData<T>(GraphicsBuffer buffer, List<T> data) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data);
	}

	public void SetBufferData(GraphicsBuffer buffer, Array data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, managedBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferData<T>(GraphicsBuffer buffer, List<T> data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, managedBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct
	{
		m_WrappedCommandBuffer.SetBufferData(buffer, data, nativeBufferStartIndex, graphicsBufferStartIndex, count);
	}

	public void SetBufferCounterValue(GraphicsBuffer buffer, uint counterValue)
	{
		m_WrappedCommandBuffer.SetBufferCounterValue(buffer, counterValue);
	}

	public void SetupCameraProperties(Camera camera)
	{
		m_WrappedCommandBuffer.SetupCameraProperties(camera);
	}

	public void InvokeOnRenderObjectCallbacks()
	{
		m_WrappedCommandBuffer.InvokeOnRenderObjectCallbacks();
	}

	public void SetComputeFloatParam(ComputeShader computeShader, string name, float val)
	{
		m_WrappedCommandBuffer.SetComputeFloatParam(computeShader, name, val);
	}

	public void SetComputeIntParam(ComputeShader computeShader, string name, int val)
	{
		m_WrappedCommandBuffer.SetComputeIntParam(computeShader, name, val);
	}

	public void SetComputeVectorParam(ComputeShader computeShader, string name, Vector4 val)
	{
		m_WrappedCommandBuffer.SetComputeVectorParam(computeShader, name, val);
	}

	public void SetComputeVectorArrayParam(ComputeShader computeShader, string name, Vector4[] values)
	{
		m_WrappedCommandBuffer.SetComputeVectorArrayParam(computeShader, name, values);
	}

	public void SetComputeMatrixParam(ComputeShader computeShader, string name, Matrix4x4 val)
	{
		m_WrappedCommandBuffer.SetComputeMatrixParam(computeShader, name, val);
	}

	public void SetComputeMatrixArrayParam(ComputeShader computeShader, string name, Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetComputeMatrixArrayParam(computeShader, name, values);
	}

	public void SetComputeFloatParams(ComputeShader computeShader, string name, params float[] values)
	{
		m_WrappedCommandBuffer.SetComputeFloatParams(computeShader, name, values);
	}

	public void SetComputeFloatParams(ComputeShader computeShader, int nameID, params float[] values)
	{
		m_WrappedCommandBuffer.SetComputeFloatParams(computeShader, nameID, values);
	}

	public void SetComputeIntParams(ComputeShader computeShader, string name, params int[] values)
	{
		m_WrappedCommandBuffer.SetComputeIntParams(computeShader, name, values);
	}

	public void SetComputeIntParams(ComputeShader computeShader, int nameID, params int[] values)
	{
		m_WrappedCommandBuffer.SetComputeIntParams(computeShader, nameID, values);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, name, rt);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, nameID, rt);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt, int mipLevel)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, name, rt, mipLevel);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt, int mipLevel)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, nameID, rt, mipLevel);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt, int mipLevel, RenderTextureSubElement element)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, name, rt, mipLevel, element);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt, int mipLevel, RenderTextureSubElement element)
	{
		m_WrappedCommandBuffer.SetComputeTextureParam(computeShader, kernelIndex, nameID, rt, mipLevel, element);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, ComputeBuffer buffer)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, nameID, buffer);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, ComputeBuffer buffer)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, name, buffer);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, GraphicsBufferHandle bufferHandle)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, nameID, bufferHandle);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, GraphicsBufferHandle bufferHandle)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, name, bufferHandle);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, GraphicsBuffer buffer)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, nameID, buffer);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, GraphicsBuffer buffer)
	{
		m_WrappedCommandBuffer.SetComputeBufferParam(computeShader, kernelIndex, name, buffer);
	}

	public void SetComputeConstantBufferParam(ComputeShader computeShader, int nameID, ComputeBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetComputeConstantBufferParam(computeShader, nameID, buffer, offset, size);
	}

	public void SetComputeConstantBufferParam(ComputeShader computeShader, string name, ComputeBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetComputeConstantBufferParam(computeShader, name, buffer, offset, size);
	}

	public void SetComputeConstantBufferParam(ComputeShader computeShader, int nameID, GraphicsBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetComputeConstantBufferParam(computeShader, nameID, buffer, offset, size);
	}

	public void SetComputeConstantBufferParam(ComputeShader computeShader, string name, GraphicsBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetComputeConstantBufferParam(computeShader, name, buffer, offset, size);
	}

	public void DispatchCompute(ComputeShader computeShader, int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ)
	{
		m_WrappedCommandBuffer.DispatchCompute(computeShader, kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
	}

	public void DispatchCompute(ComputeShader computeShader, int kernelIndex, ComputeBuffer indirectBuffer, uint argsOffset)
	{
		m_WrappedCommandBuffer.DispatchCompute(computeShader, kernelIndex, indirectBuffer, argsOffset);
	}

	public void DispatchCompute(ComputeShader computeShader, int kernelIndex, GraphicsBuffer indirectBuffer, uint argsOffset)
	{
		m_WrappedCommandBuffer.DispatchCompute(computeShader, kernelIndex, indirectBuffer, argsOffset);
	}

	public void BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure)
	{
		m_WrappedCommandBuffer.BuildRayTracingAccelerationStructure(accelerationStructure);
	}

	public void BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure, Vector3 relativeOrigin)
	{
		m_WrappedCommandBuffer.BuildRayTracingAccelerationStructure(accelerationStructure, relativeOrigin);
	}

	public void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, string name, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		m_WrappedCommandBuffer.SetRayTracingAccelerationStructure(rayTracingShader, name, rayTracingAccelerationStructure);
	}

	public void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, int nameID, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		m_WrappedCommandBuffer.SetRayTracingAccelerationStructure(rayTracingShader, nameID, rayTracingAccelerationStructure);
	}

	public void SetRayTracingAccelerationStructure(ComputeShader computeShader, int kernelIndex, string name, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		m_WrappedCommandBuffer.SetRayTracingAccelerationStructure(computeShader, kernelIndex, name, rayTracingAccelerationStructure);
	}

	public void SetRayTracingAccelerationStructure(ComputeShader computeShader, int kernelIndex, int nameID, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		m_WrappedCommandBuffer.SetRayTracingAccelerationStructure(computeShader, kernelIndex, nameID, rayTracingAccelerationStructure);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, ComputeBuffer buffer)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, name, buffer);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, nameID, buffer);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBuffer buffer)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, name, buffer);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBuffer buffer)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, nameID, buffer);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBufferHandle bufferHandle)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, name, bufferHandle);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBufferHandle bufferHandle)
	{
		m_WrappedCommandBuffer.SetRayTracingBufferParam(rayTracingShader, nameID, bufferHandle);
	}

	public void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetRayTracingConstantBufferParam(rayTracingShader, nameID, buffer, offset, size);
	}

	public void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, string name, ComputeBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetRayTracingConstantBufferParam(rayTracingShader, name, buffer, offset, size);
	}

	public void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetRayTracingConstantBufferParam(rayTracingShader, nameID, buffer, offset, size);
	}

	public void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBuffer buffer, int offset, int size)
	{
		m_WrappedCommandBuffer.SetRayTracingConstantBufferParam(rayTracingShader, name, buffer, offset, size);
	}

	public void SetRayTracingTextureParam(RayTracingShader rayTracingShader, string name, TextureHandle rt)
	{
		m_WrappedCommandBuffer.SetRayTracingTextureParam(rayTracingShader, name, rt);
	}

	public void SetRayTracingTextureParam(RayTracingShader rayTracingShader, int nameID, TextureHandle rt)
	{
		m_WrappedCommandBuffer.SetRayTracingTextureParam(rayTracingShader, nameID, rt);
	}

	public void SetRayTracingFloatParam(RayTracingShader rayTracingShader, string name, float val)
	{
		m_WrappedCommandBuffer.SetRayTracingFloatParam(rayTracingShader, name, val);
	}

	public void SetRayTracingFloatParam(RayTracingShader rayTracingShader, int nameID, float val)
	{
		m_WrappedCommandBuffer.SetRayTracingFloatParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingFloatParams(RayTracingShader rayTracingShader, string name, params float[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingFloatParams(rayTracingShader, name, values);
	}

	public void SetRayTracingFloatParams(RayTracingShader rayTracingShader, int nameID, params float[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingFloatParams(rayTracingShader, nameID, values);
	}

	public void SetRayTracingIntParam(RayTracingShader rayTracingShader, string name, int val)
	{
		m_WrappedCommandBuffer.SetRayTracingIntParam(rayTracingShader, name, val);
	}

	public void SetRayTracingIntParam(RayTracingShader rayTracingShader, int nameID, int val)
	{
		m_WrappedCommandBuffer.SetRayTracingIntParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingIntParams(RayTracingShader rayTracingShader, string name, params int[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingIntParams(rayTracingShader, name, values);
	}

	public void SetRayTracingIntParams(RayTracingShader rayTracingShader, int nameID, params int[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingIntParams(rayTracingShader, nameID, values);
	}

	public void SetRayTracingVectorParam(RayTracingShader rayTracingShader, string name, Vector4 val)
	{
		m_WrappedCommandBuffer.SetRayTracingVectorParam(rayTracingShader, name, val);
	}

	public void SetRayTracingVectorParam(RayTracingShader rayTracingShader, int nameID, Vector4 val)
	{
		m_WrappedCommandBuffer.SetRayTracingVectorParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, string name, params Vector4[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingVectorArrayParam(rayTracingShader, name, values);
	}

	public void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, int nameID, params Vector4[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingVectorArrayParam(rayTracingShader, nameID, values);
	}

	public void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, string name, Matrix4x4 val)
	{
		m_WrappedCommandBuffer.SetRayTracingMatrixParam(rayTracingShader, name, val);
	}

	public void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, int nameID, Matrix4x4 val)
	{
		m_WrappedCommandBuffer.SetRayTracingMatrixParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, string name, params Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingMatrixArrayParam(rayTracingShader, name, values);
	}

	public void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, int nameID, params Matrix4x4[] values)
	{
		m_WrappedCommandBuffer.SetRayTracingMatrixArrayParam(rayTracingShader, nameID, values);
	}

	public void DispatchRays(RayTracingShader rayTracingShader, string rayGenName, uint width, uint height, uint depth, Camera camera)
	{
		m_WrappedCommandBuffer.DispatchRays(rayTracingShader, rayGenName, width, height, depth, camera);
	}

	public void DispatchRays(RayTracingShader rayTracingShader, string rayGenName, GraphicsBuffer argsBuffer, uint argsOffset, Camera camera)
	{
		m_WrappedCommandBuffer.DispatchRays(rayTracingShader, rayGenName, argsBuffer, argsOffset, camera);
	}

	public void CopyCounterValue(ComputeBuffer src, ComputeBuffer dst, uint dstOffsetBytes)
	{
		m_WrappedCommandBuffer.CopyCounterValue(src, dst, dstOffsetBytes);
	}

	public void CopyCounterValue(GraphicsBuffer src, ComputeBuffer dst, uint dstOffsetBytes)
	{
		m_WrappedCommandBuffer.CopyCounterValue(src, dst, dstOffsetBytes);
	}

	public void CopyCounterValue(ComputeBuffer src, GraphicsBuffer dst, uint dstOffsetBytes)
	{
		m_WrappedCommandBuffer.CopyCounterValue(src, dst, dstOffsetBytes);
	}

	public void CopyCounterValue(GraphicsBuffer src, GraphicsBuffer dst, uint dstOffsetBytes)
	{
		m_WrappedCommandBuffer.CopyCounterValue(src, dst, dstOffsetBytes);
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
