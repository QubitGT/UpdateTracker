using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal class LightBatch
{
	private static readonly ProfilingSampler profilingDrawBatched = new ProfilingSampler("Light2D Batcher");

	private static readonly int k_BufferOffset = Shader.PropertyToID("_BatchBufferOffset");

	private static int sBatchIndexCounter = 0;

	private int[] subsets = new int[LightBuffer.kMax];

	private Mesh[] lightMeshes = new Mesh[LightBuffer.kMax];

	private Matrix4x4[] matrices = new Matrix4x4[LightBuffer.kMax];

	private LightBuffer[] lightBuffer = new LightBuffer[LightBuffer.kCount];

	private Light2D cachedLight;

	private Material cachedMaterial;

	private int hashCode;

	private int lightCount;

	private int maxIndex;

	private int batchCount;

	private int activeCount;

	private static int batchLightMod => LightBuffer.kLightMod;

	private static float batchRunningIndex => (float)(sBatchIndexCounter++ % LightBuffer.kLightMod) / (float)LightBuffer.kLightMod;

	public static bool isBatchingSupported => false;

	internal NativeArray<PerLight2D> nativeBuffer
	{
		get
		{
			if (lightBuffer[activeCount] == null)
			{
				lightBuffer[activeCount] = new LightBuffer();
			}
			return lightBuffer[activeCount].nativeBuffer;
		}
	}

	internal GraphicsBuffer graphicsBuffer
	{
		get
		{
			if (lightBuffer[activeCount] == null)
			{
				lightBuffer[activeCount] = new LightBuffer();
			}
			return lightBuffer[activeCount].graphicsBuffer;
		}
	}

	internal NativeArray<int> lightMarker
	{
		get
		{
			if (lightBuffer[activeCount] == null)
			{
				lightBuffer[activeCount] = new LightBuffer();
			}
			return lightBuffer[activeCount].lightMarkers;
		}
	}

	internal static int batchSlotIndex => (int)(batchRunningIndex * (float)LightBuffer.kLightMod);

	internal PerLight2D GetLight(int index)
	{
		return nativeBuffer[index];
	}

	internal void SetLight(int index, PerLight2D light)
	{
		NativeArray<PerLight2D> nativeArray = nativeBuffer;
		nativeArray[index] = light;
	}

	internal static float GetBatchColor()
	{
		return (float)batchSlotIndex / (float)batchLightMod;
	}

	internal static int GetBatchSlotIndex(float channelColor)
	{
		return (int)(channelColor * (float)LightBuffer.kLightMod);
	}

	private static int Hash(Light2D light, Material material)
	{
		return ((0x50C5D1F ^ material.GetHashCode()) * 16777619) ^ ((!(light.lightCookieSprite == null)) ? light.lightCookieSprite.GetHashCode() : 0);
	}

	private void Validate()
	{
	}

	private void OnAssemblyReload()
	{
		for (int i = 0; i < LightBuffer.kCount; i++)
		{
			lightBuffer[activeCount].Release();
		}
	}

	private void ResetInternals()
	{
		for (int i = 0; i < LightBuffer.kCount; i++)
		{
			if (lightBuffer[i] != null)
			{
				lightBuffer[i].Reset();
			}
		}
	}

	private void SetBuffer()
	{
		Validate();
		graphicsBuffer.SetData(nativeBuffer, lightCount, lightCount, math.min(LightBuffer.kBatchMax, LightBuffer.kMax - lightCount));
	}

	internal int SlotIndex(int x)
	{
		return lightCount + x;
	}

	internal void Reset()
	{
		if (isBatchingSupported)
		{
			maxIndex = 0;
			hashCode = 0;
			batchCount = 0;
			lightCount = 0;
			activeCount = 0;
			Shader.SetGlobalBuffer("_Light2DBuffer", graphicsBuffer);
		}
	}

	internal bool CanBatch(Light2D light, Material material, int index, out int lightHash)
	{
		lightHash = Hash(light, material);
		hashCode = ((hashCode == 0) ? lightHash : hashCode);
		if (batchCount == 0)
		{
			hashCode = lightHash;
		}
		else if (hashCode != lightHash || SlotIndex(index) >= LightBuffer.kMax || lightMarker[index] == 1)
		{
			hashCode = lightHash;
			return false;
		}
		return true;
	}

	internal bool AddBatch(Light2D light, Material material, Matrix4x4 mat, Mesh mesh, int subset, int lightHash, int index)
	{
		cachedLight = light;
		cachedMaterial = material;
		matrices[batchCount] = mat;
		lightMeshes[batchCount] = mesh;
		subsets[batchCount] = subset;
		batchCount++;
		maxIndex = math.max(maxIndex, index);
		NativeArray<int> nativeArray = lightMarker;
		nativeArray[index] = 1;
		return true;
	}

	internal void Flush(RasterCommandBuffer cmd)
	{
		if (batchCount > 0)
		{
			using (new ProfilingScope(cmd, profilingDrawBatched))
			{
				SetBuffer();
				cmd.SetGlobalInt(k_BufferOffset, lightCount);
				cmd.DrawMultipleMeshes(matrices, lightMeshes, subsets, batchCount, cachedMaterial, -1, null);
			}
			lightCount = lightCount + maxIndex + 1;
		}
		for (int i = 0; i < batchCount; i++)
		{
			lightMeshes[i] = null;
		}
		ResetInternals();
		batchCount = 0;
		maxIndex = 0;
	}
}
