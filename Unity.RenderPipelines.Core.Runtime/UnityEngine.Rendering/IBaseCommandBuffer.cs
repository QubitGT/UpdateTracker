using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public interface IBaseCommandBuffer
{
	void SetInvertCulling(bool invertCulling);

	void SetViewport(Rect pixelRect);

	void EnableScissorRect(Rect scissor);

	void DisableScissorRect();

	void SetGlobalFloat(int nameID, float value);

	void SetGlobalInt(int nameID, int value);

	void SetGlobalInteger(int nameID, int value);

	void SetGlobalVector(int nameID, Vector4 value);

	void SetGlobalColor(int nameID, Color value);

	void SetGlobalMatrix(int nameID, Matrix4x4 value);

	void EnableShaderKeyword(string keyword);

	void EnableKeyword(in GlobalKeyword keyword);

	void EnableKeyword(Material material, in LocalKeyword keyword);

	void EnableKeyword(ComputeShader computeShader, in LocalKeyword keyword);

	void DisableShaderKeyword(string keyword);

	void DisableKeyword(in GlobalKeyword keyword);

	void DisableKeyword(Material material, in LocalKeyword keyword);

	void DisableKeyword(ComputeShader computeShader, in LocalKeyword keyword);

	void SetKeyword(in GlobalKeyword keyword, bool value);

	void SetKeyword(Material material, in LocalKeyword keyword, bool value);

	void SetKeyword(ComputeShader computeShader, in LocalKeyword keyword, bool value);

	void SetViewProjectionMatrices(Matrix4x4 view, Matrix4x4 proj);

	void SetGlobalDepthBias(float bias, float slopeBias);

	void SetGlobalFloatArray(int nameID, float[] values);

	void SetGlobalVectorArray(int nameID, Vector4[] values);

	void SetGlobalMatrixArray(int nameID, Matrix4x4[] values);

	void SetLateLatchProjectionMatrices(Matrix4x4[] projectionMat);

	void MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType matrixPropertyType, int shaderPropertyID);

	void UnmarkLateLatchMatrix(CameraLateLatchMatrixType matrixPropertyType);

	void BeginSample(string name);

	void EndSample(string name);

	void BeginSample(CustomSampler sampler);

	void EndSample(CustomSampler sampler);

	void BeginSample(ProfilerMarker marker);

	void EndSample(ProfilerMarker marker);

	void IncrementUpdateCount(RenderTargetIdentifier dest);

	void SetupCameraProperties(Camera camera);

	void InvokeOnRenderObjectCallbacks();

	void SetGlobalFloat(string name, float value);

	void SetGlobalInt(string name, int value);

	void SetGlobalInteger(string name, int value);

	void SetGlobalVector(string name, Vector4 value);

	void SetGlobalColor(string name, Color value);

	void SetGlobalMatrix(string name, Matrix4x4 value);

	void SetGlobalFloatArray(string propertyName, List<float> values);

	void SetGlobalFloatArray(int nameID, List<float> values);

	void SetGlobalFloatArray(string propertyName, float[] values);

	void SetGlobalVectorArray(string propertyName, List<Vector4> values);

	void SetGlobalVectorArray(int nameID, List<Vector4> values);

	void SetGlobalVectorArray(string propertyName, Vector4[] values);

	void SetGlobalMatrixArray(string propertyName, List<Matrix4x4> values);

	void SetGlobalMatrixArray(int nameID, List<Matrix4x4> values);

	void SetGlobalMatrixArray(string propertyName, Matrix4x4[] values);

	void SetGlobalTexture(string name, TextureHandle value);

	void SetGlobalTexture(int nameID, TextureHandle value);

	void SetGlobalTexture(string name, TextureHandle value, RenderTextureSubElement element);

	void SetGlobalTexture(int nameID, TextureHandle value, RenderTextureSubElement element);

	void SetGlobalBuffer(string name, ComputeBuffer value);

	void SetGlobalBuffer(int nameID, ComputeBuffer value);

	void SetGlobalBuffer(string name, GraphicsBuffer value);

	void SetGlobalBuffer(int nameID, GraphicsBuffer value);

	void SetGlobalConstantBuffer(ComputeBuffer buffer, int nameID, int offset, int size);

	void SetGlobalConstantBuffer(ComputeBuffer buffer, string name, int offset, int size);

	void SetGlobalConstantBuffer(GraphicsBuffer buffer, int nameID, int offset, int size);

	void SetGlobalConstantBuffer(GraphicsBuffer buffer, string name, int offset, int size);

	void SetShadowSamplingMode(RenderTargetIdentifier shadowmap, ShadowSamplingMode mode);

	void SetSinglePassStereo(SinglePassStereoMode mode);

	void IssuePluginEvent(IntPtr callback, int eventID);

	void IssuePluginEventAndData(IntPtr callback, int eventID, IntPtr data);

	void IssuePluginCustomBlit(IntPtr callback, uint command, RenderTargetIdentifier source, RenderTargetIdentifier dest, uint commandParam, uint commandFlags);

	void IssuePluginCustomTextureUpdateV2(IntPtr callback, Texture targetTexture, uint userData);
}
