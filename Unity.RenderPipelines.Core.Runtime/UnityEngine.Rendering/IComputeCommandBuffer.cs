using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public interface IComputeCommandBuffer : IBaseCommandBuffer
{
	void SetComputeFloatParam(ComputeShader computeShader, int nameID, float val);

	void SetComputeIntParam(ComputeShader computeShader, int nameID, int val);

	void SetComputeVectorParam(ComputeShader computeShader, int nameID, Vector4 val);

	void SetComputeVectorArrayParam(ComputeShader computeShader, int nameID, Vector4[] values);

	void SetComputeMatrixParam(ComputeShader computeShader, int nameID, Matrix4x4 val);

	void SetComputeMatrixArrayParam(ComputeShader computeShader, int nameID, Matrix4x4[] values);

	void SetBufferData(ComputeBuffer buffer, Array data);

	void SetBufferData<T>(ComputeBuffer buffer, List<T> data) where T : struct;

	void SetBufferData<T>(ComputeBuffer buffer, NativeArray<T> data) where T : struct;

	void SetBufferData(ComputeBuffer buffer, Array data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count);

	void SetBufferData<T>(ComputeBuffer buffer, List<T> data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct;

	void SetBufferData<T>(ComputeBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct;

	void SetBufferCounterValue(ComputeBuffer buffer, uint counterValue);

	void SetBufferData(GraphicsBuffer buffer, Array data);

	void SetBufferData<T>(GraphicsBuffer buffer, List<T> data) where T : struct;

	void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data) where T : struct;

	void SetBufferData(GraphicsBuffer buffer, Array data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count);

	void SetBufferData<T>(GraphicsBuffer buffer, List<T> data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct;

	void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : struct;

	void SetBufferCounterValue(GraphicsBuffer buffer, uint counterValue);

	void SetComputeFloatParam(ComputeShader computeShader, string name, float val);

	void SetComputeIntParam(ComputeShader computeShader, string name, int val);

	void SetComputeVectorParam(ComputeShader computeShader, string name, Vector4 val);

	void SetComputeVectorArrayParam(ComputeShader computeShader, string name, Vector4[] values);

	void SetComputeMatrixParam(ComputeShader computeShader, string name, Matrix4x4 val);

	void SetComputeMatrixArrayParam(ComputeShader computeShader, string name, Matrix4x4[] values);

	void SetComputeFloatParams(ComputeShader computeShader, string name, params float[] values);

	void SetComputeFloatParams(ComputeShader computeShader, int nameID, params float[] values);

	void SetComputeIntParams(ComputeShader computeShader, string name, params int[] values);

	void SetComputeIntParams(ComputeShader computeShader, int nameID, params int[] values);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt, int mipLevel);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt, int mipLevel);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, TextureHandle rt, int mipLevel, RenderTextureSubElement element);

	void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle rt, int mipLevel, RenderTextureSubElement element);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, ComputeBuffer buffer);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, ComputeBuffer buffer);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, GraphicsBufferHandle bufferHandle);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, GraphicsBufferHandle bufferHandle);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, int nameID, GraphicsBuffer buffer);

	void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, GraphicsBuffer buffer);

	void SetComputeConstantBufferParam(ComputeShader computeShader, int nameID, ComputeBuffer buffer, int offset, int size);

	void SetComputeConstantBufferParam(ComputeShader computeShader, string name, ComputeBuffer buffer, int offset, int size);

	void SetComputeConstantBufferParam(ComputeShader computeShader, int nameID, GraphicsBuffer buffer, int offset, int size);

	void SetComputeConstantBufferParam(ComputeShader computeShader, string name, GraphicsBuffer buffer, int offset, int size);

	void DispatchCompute(ComputeShader computeShader, int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ);

	void DispatchCompute(ComputeShader computeShader, int kernelIndex, ComputeBuffer indirectBuffer, uint argsOffset);

	void DispatchCompute(ComputeShader computeShader, int kernelIndex, GraphicsBuffer indirectBuffer, uint argsOffset);

	void BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure);

	void BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure, Vector3 relativeOrigin);

	void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, string name, RayTracingAccelerationStructure rayTracingAccelerationStructure);

	void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, int nameID, RayTracingAccelerationStructure rayTracingAccelerationStructure);

	void SetRayTracingAccelerationStructure(ComputeShader computeShader, int kernelIndex, string name, RayTracingAccelerationStructure rayTracingAccelerationStructure);

	void SetRayTracingAccelerationStructure(ComputeShader computeShader, int kernelIndex, int nameID, RayTracingAccelerationStructure rayTracingAccelerationStructure);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, ComputeBuffer buffer);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBuffer buffer);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBuffer buffer);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBufferHandle bufferHandle);

	void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBufferHandle bufferHandle);

	void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer, int offset, int size);

	void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, string name, ComputeBuffer buffer, int offset, int size);

	void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, int nameID, GraphicsBuffer buffer, int offset, int size);

	void SetRayTracingConstantBufferParam(RayTracingShader rayTracingShader, string name, GraphicsBuffer buffer, int offset, int size);

	void SetRayTracingTextureParam(RayTracingShader rayTracingShader, string name, TextureHandle rt);

	void SetRayTracingTextureParam(RayTracingShader rayTracingShader, int nameID, TextureHandle rt);

	void SetRayTracingFloatParam(RayTracingShader rayTracingShader, string name, float val);

	void SetRayTracingFloatParam(RayTracingShader rayTracingShader, int nameID, float val);

	void SetRayTracingFloatParams(RayTracingShader rayTracingShader, string name, params float[] values);

	void SetRayTracingFloatParams(RayTracingShader rayTracingShader, int nameID, params float[] values);

	void SetRayTracingIntParam(RayTracingShader rayTracingShader, string name, int val);

	void SetRayTracingIntParam(RayTracingShader rayTracingShader, int nameID, int val);

	void SetRayTracingIntParams(RayTracingShader rayTracingShader, string name, params int[] values);

	void SetRayTracingIntParams(RayTracingShader rayTracingShader, int nameID, params int[] values);

	void SetRayTracingVectorParam(RayTracingShader rayTracingShader, string name, Vector4 val);

	void SetRayTracingVectorParam(RayTracingShader rayTracingShader, int nameID, Vector4 val);

	void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, string name, params Vector4[] values);

	void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, int nameID, params Vector4[] values);

	void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, string name, Matrix4x4 val);

	void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, int nameID, Matrix4x4 val);

	void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, string name, params Matrix4x4[] values);

	void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, int nameID, params Matrix4x4[] values);

	void DispatchRays(RayTracingShader rayTracingShader, string rayGenName, uint width, uint height, uint depth, Camera camera);

	void DispatchRays(RayTracingShader rayTracingShader, string rayGenName, GraphicsBuffer argsBuffer, uint argsOffset, Camera camera);

	void CopyCounterValue(ComputeBuffer src, ComputeBuffer dst, uint dstOffsetBytes);

	void CopyCounterValue(GraphicsBuffer src, ComputeBuffer dst, uint dstOffsetBytes);

	void CopyCounterValue(ComputeBuffer src, GraphicsBuffer dst, uint dstOffsetBytes);

	void CopyCounterValue(GraphicsBuffer src, GraphicsBuffer dst, uint dstOffsetBytes);
}
