using System;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(13)]
public struct NetworkSceneInfo : INetworkStruct, IEquatable<NetworkSceneInfo>
{
	public const int WORD_COUNT = 13;

	public const int SIZE = 52;

	public const int MaxScenes = 8;

	[FieldOffset(0)]
	private NetworkSceneInfoDefaultFlags _flags;

	[FieldOffset(4)]
	private SceneRef _scene0;

	[FieldOffset(8)]
	private SceneRef _scene1;

	[FieldOffset(12)]
	private SceneRef _scene2;

	[FieldOffset(16)]
	private SceneRef _scene3;

	[FieldOffset(20)]
	private SceneRef _scene4;

	[FieldOffset(24)]
	private SceneRef _scene5;

	[FieldOffset(28)]
	private SceneRef _scene6;

	[FieldOffset(32)]
	private SceneRef _scene7;

	[FieldOffset(36)]
	private NetworkLoadSceneParameters _sceneMeta0;

	[FieldOffset(38)]
	private NetworkLoadSceneParameters _sceneMeta1;

	[FieldOffset(40)]
	private NetworkLoadSceneParameters _sceneMeta2;

	[FieldOffset(42)]
	private NetworkLoadSceneParameters _sceneMeta3;

	[FieldOffset(44)]
	private NetworkLoadSceneParameters _sceneMeta4;

	[FieldOffset(46)]
	private NetworkLoadSceneParameters _sceneMeta5;

	[FieldOffset(48)]
	private NetworkLoadSceneParameters _sceneMeta6;

	[FieldOffset(50)]
	private NetworkLoadSceneParameters _sceneMeta7;

	public FixedArray<SceneRef> Scenes => FixedArray.Create(ref _scene0, SceneCount);

	public FixedArray<NetworkLoadSceneParameters> SceneParams => FixedArray.Create(ref _sceneMeta0, SceneCount);

	public int SceneCount
	{
		get
		{
			return (int)(_flags & NetworkSceneInfoDefaultFlags.SceneCountMask);
		}
		private set
		{
			_flags = (NetworkSceneInfoDefaultFlags)((uint)(_flags & ~NetworkSceneInfoDefaultFlags.SceneCountMask) | ((uint)value & 0xFu));
		}
	}

	public int Version
	{
		get
		{
			return (int)(_flags & NetworkSceneInfoDefaultFlags.ConterMask) >> 4;
		}
		private set
		{
			_flags = (NetworkSceneInfoDefaultFlags)((uint)(_flags & ~NetworkSceneInfoDefaultFlags.ConterMask) | ((uint)(value << 4) & 0xFFFF0u));
		}
	}

	public int IndexOf(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
	{
		for (int i = 0; i < SceneCount; i++)
		{
			if (Scenes[i] == sceneRef && SceneParams[i] == sceneParams)
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf((SceneRef SceneRef, NetworkLoadSceneParameters SceneParams) scene)
	{
		return IndexOf(scene.SceneRef, scene.SceneParams);
	}

	public int AddSceneRef(SceneRef sceneRef, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeOnLoad = false)
	{
		return AddSceneRef(sceneRef, (NetworkLoadSceneParametersFlags)(((loadSceneMode == LoadSceneMode.Single) ? 1u : 0u) | (uint)(((localPhysicsMode & LocalPhysicsMode.Physics2D) != 0) ? NetworkLoadSceneParametersFlags.LocalPhysics2D : ((NetworkLoadSceneParametersFlags)0)) | (uint)(((localPhysicsMode & LocalPhysicsMode.Physics3D) != 0) ? NetworkLoadSceneParametersFlags.LocalPhysics3D : ((NetworkLoadSceneParametersFlags)0)) | (uint)(activeOnLoad ? NetworkLoadSceneParametersFlags.ActiveOnLoad : ((NetworkLoadSceneParametersFlags)0))));
	}

	internal int AddSceneRef(SceneRef sceneRef, NetworkLoadSceneParametersFlags flags)
	{
		if ((flags & NetworkLoadSceneParametersFlags.Single) != 0)
		{
			Scenes.Clear();
			SceneParams.Clear();
			SceneCount = 0;
		}
		if (SceneCount >= 8)
		{
			return -1;
		}
		int num = SceneCount++;
		SceneParams[num] = new NetworkLoadSceneParameters(new NetworkSceneLoadId((byte)Version), flags);
		Scenes[num] = sceneRef;
		int version = Version + 1;
		Version = version;
		return num;
	}

	public bool RemoveSceneRef(SceneRef sceneRef)
	{
		for (int i = 0; i < SceneCount; i++)
		{
			FixedArray<SceneRef> scenes = Scenes;
			if (scenes[i] == sceneRef)
			{
				FixedArray<NetworkLoadSceneParameters> sceneParams;
				for (int j = i + 1; j < SceneCount; j++)
				{
					scenes = Scenes;
					ref SceneRef reference = ref scenes[j - 1];
					scenes = Scenes;
					reference = scenes[j];
					sceneParams = SceneParams;
					ref NetworkLoadSceneParameters reference2 = ref sceneParams[j - 1];
					sceneParams = SceneParams;
					reference2 = sceneParams[j];
				}
				scenes = Scenes;
				scenes[SceneCount - 1] = default(SceneRef);
				sceneParams = SceneParams;
				sceneParams[SceneCount - 1] = default(NetworkLoadSceneParameters);
				int version = Version + 1;
				Version = version;
				version = SceneCount - 1;
				SceneCount = version;
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return "[Scenes: " + string.Join(", ", Scenes) + "]";
	}

	public unsafe bool Equals(NetworkSceneInfo other)
	{
		fixed (NetworkSceneInfo* ptr = &this)
		{
			return Native.MemCmp(ptr, &other, 52) == 0;
		}
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkSceneInfo other && Equals(other);
	}

	public unsafe override int GetHashCode()
	{
		fixed (NetworkSceneInfo* data = &this)
		{
			return HashCodeUtilities.GetHashCodeDeterministic(data);
		}
	}

	public static implicit operator NetworkSceneInfo(SceneRef sceneRef)
	{
		NetworkSceneInfo result = default(NetworkSceneInfo);
		result.AddSceneRef(sceneRef);
		return result;
	}
}
