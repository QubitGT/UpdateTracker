using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Meta.XR.Acoustics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

internal class MetaXRAcousticMap : MonoBehaviour
{
	internal const string FILE_EXTENSION = "xramap";

	[SerializeField]
	internal MetaXRAcousticSceneGroup SceneGroup;

	[SerializeField]
	internal bool customPointsEnabled;

	[NonSerialized]
	internal bool IsLoaded;

	[SerializeField]
	internal AcousticMapFlags Flags = AcousticMapFlags.NO_FLOATING | AcousticMapFlags.DIFFRACTION;

	internal const float DISTANCE_PARAMETER_MAX = 10000f;

	[SerializeField]
	internal uint ReflectionCount = 6u;

	[SerializeField]
	[Range(0f, 10000f)]
	internal float MinSpacing = 1f;

	[SerializeField]
	[Range(0f, 10000f)]
	internal float MaxSpacing = 10f;

	[SerializeField]
	[Range(0f, 10000f)]
	internal float HeadHeight = 1.5f;

	[SerializeField]
	[Range(0f, 10000f)]
	internal float MaxHeight = 3f;

	[SerializeField]
	private Vector3 gravityVector = new Vector3(0f, -1f, 0f);

	[FormerlySerializedAs("relativeFilePath_")]
	[SerializeField]
	private string relativeFilePath = "";

	[NonSerialized]
	internal IntPtr mapHandle = IntPtr.Zero;

	[NonSerialized]
	private Action delayedEnable;

	internal const int Success = 0;

	internal bool StaticOnly
	{
		get
		{
			return (Flags & AcousticMapFlags.STATIC_ONLY) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= AcousticMapFlags.STATIC_ONLY;
			}
			else
			{
				Flags &= ~AcousticMapFlags.STATIC_ONLY;
			}
		}
	}

	internal bool NoFloating
	{
		get
		{
			return (Flags & AcousticMapFlags.NO_FLOATING) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= AcousticMapFlags.NO_FLOATING;
			}
			else
			{
				Flags &= ~AcousticMapFlags.NO_FLOATING;
			}
		}
	}

	internal bool Diffraction
	{
		get
		{
			return (Flags & AcousticMapFlags.DIFFRACTION) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= AcousticMapFlags.DIFFRACTION;
			}
			else
			{
				Flags &= ~AcousticMapFlags.DIFFRACTION;
			}
		}
	}

	internal Vector3 GravityVector
	{
		get
		{
			return gravityVector;
		}
		set
		{
			gravityVector = value.normalized;
		}
	}

	internal string RelativeFilePath => relativeFilePath;

	internal string AbsoluteFilePath
	{
		get
		{
			return Path.GetFullPath(Path.Combine(Application.dataPath, RelativeFilePath));
		}
		set
		{
			string text = value.Replace('\\', '/');
			if (text.StartsWith(Application.dataPath))
			{
				relativeFilePath = text.Substring(Application.dataPath.Length + 1);
				if (File.Exists(AbsoluteFilePath))
				{
					DestroyInternal();
					StartInternal();
				}
			}
			else
			{
				Debug.LogError("invalid path " + value + ", outside application path " + Application.dataPath);
			}
		}
	}

	private void Start()
	{
		StartInternal();
	}

	internal void StartInternal(bool autoLoad = true)
	{
		if (mapHandle != IntPtr.Zero)
		{
			return;
		}
		if (MetaXRAcousticNativeInterface.Interface.CreateAudioSceneIR(out mapHandle) != 0)
		{
			Debug.LogError("Unable to create internal Acoustic Map", base.gameObject);
			return;
		}
		if (Application.isPlaying)
		{
			if (!string.IsNullOrEmpty(relativeFilePath))
			{
				string text = relativeFilePath;
				if (relativeFilePath.StartsWith("StreamingAssets"))
				{
					string streamingAssetsSubPath = text.Substring("StreamingAssets/".Length);
					StartCoroutine(LoadMapAsync(streamingAssetsSubPath));
				}
			}
		}
		else if (autoLoad)
		{
			bool flag = !string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(base.name) && File.Exists(AbsoluteFilePath);
			if (flag)
			{
				Debug.Log("Loading Acoustic Map " + base.name + " from File " + AbsoluteFilePath);
			}
			int num = MetaXRAcousticNativeInterface.Interface.AudioSceneIRReadFile(mapHandle, AbsoluteFilePath);
			if (num != 0)
			{
				if (flag)
				{
					Debug.LogError($"Error {num}: Unable to load the Acoustic Map from file: {AbsoluteFilePath}");
				}
				return;
			}
			if (!flag)
			{
				Debug.Log("Found data in default location: " + RelativeFilePath);
				relativeFilePath = RelativeFilePath;
			}
		}
		ApplyTransform();
	}

	private IEnumerator LoadMapAsync(string streamingAssetsSubPath)
	{
		string text = Application.streamingAssetsPath + "/" + streamingAssetsSubPath;
		Debug.Log("Loading Acoustic Map " + base.name + " from StreamingAssets " + text);
		float startTime = Time.realtimeSinceStartup;
		UnityWebRequest unityWebRequest = UnityWebRequest.Get(text);
		yield return unityWebRequest.SendWebRequest();
		if (!string.IsNullOrEmpty(unityWebRequest.error))
		{
			Debug.LogError($"web request: done={unityWebRequest.isDone}: {unityWebRequest.error}", base.gameObject);
		}
		float num = Time.realtimeSinceStartup - startTime;
		Debug.Log($"Acoustic Map {base.name}, read time = {num}", base.gameObject);
		LoadMapFromMemory(unityWebRequest.downloadHandler.nativeData);
	}

	private unsafe async void LoadMapFromMemory(NativeArray<byte>.ReadOnly data)
	{
		if (data.Length == 0)
		{
			return;
		}
		float startTime = Time.realtimeSinceStartup;
		int result = -1;
		await Task.Run(delegate
		{
			IntPtr data2 = (IntPtr)data.GetUnsafeReadOnlyPtr();
			lock (this)
			{
				if (mapHandle != IntPtr.Zero)
				{
					result = MetaXRAcousticNativeInterface.Interface.AudioSceneIRReadMemory(mapHandle, data2, (ulong)data.Length);
					GC.KeepAlive(data);
				}
			}
		});
		if (result == 0)
		{
			float num = Time.realtimeSinceStartup - startTime;
			Debug.Log($"Sucessfully loaded Acoustic Map {base.name}, load time = {num}", base.gameObject);
			delayedEnable = delegate
			{
				MetaXRAcousticGeometry.OnAnyGeometryEnabled -= delayedEnable;
				Debug.Log("Delayed enable", base.gameObject);
				MetaXRAcousticNativeInterface.Interface.AudioSceneIRSetEnabled(mapHandle, base.isActiveAndEnabled);
			};
			if (MetaXRAcousticGeometry.EnabledGeometryCount > 0)
			{
				MetaXRAcousticNativeInterface.Interface.AudioSceneIRSetEnabled(mapHandle, base.isActiveAndEnabled);
			}
			else
			{
				MetaXRAcousticGeometry.OnAnyGeometryEnabled += delayedEnable;
			}
			IsLoaded = true;
		}
		else
		{
			Debug.LogError($"Error {result}: Unable to read the Acoustic Map.");
		}
	}

	private void OnDestroy()
	{
		DestroyInternal();
	}

	internal void DestroyInternal()
	{
		lock (this)
		{
			if (mapHandle != IntPtr.Zero)
			{
				if (MetaXRAcousticNativeInterface.Interface.DestroyAudioSceneIR(mapHandle) != 0)
				{
					Debug.LogError("Unable to destroy Acoustic Map", base.gameObject);
				}
				mapHandle = IntPtr.Zero;
			}
		}
	}

	private void OnEnable()
	{
		if (!(mapHandle == IntPtr.Zero))
		{
			Debug.Log("Enabling AcousticMap: " + RelativeFilePath);
			MetaXRAcousticNativeInterface.Interface.AudioSceneIRSetEnabled(mapHandle, enabled: true);
		}
	}

	private void OnDisable()
	{
		if (!(mapHandle == IntPtr.Zero))
		{
			MetaXRAcousticGeometry.OnAnyGeometryEnabled -= delayedEnable;
			Debug.Log("Disabling AcousticMap: " + RelativeFilePath);
			MetaXRAcousticNativeInterface.Interface.AudioSceneIRSetEnabled(mapHandle, enabled: false);
		}
	}

	private void LateUpdate()
	{
		if (!(mapHandle == IntPtr.Zero) && base.transform.hasChanged)
		{
			ApplyTransform();
			base.transform.hasChanged = false;
		}
	}

	private void ApplyTransform()
	{
		MetaXRAcousticNativeInterface.INativeInterface @interface = MetaXRAcousticNativeInterface.Interface;
		IntPtr sceneIR = mapHandle;
		Matrix4x4 matrix = base.transform.localToWorldMatrix;
		@interface.AudioSceneIRSetTransform(sceneIR, in matrix);
	}
}
