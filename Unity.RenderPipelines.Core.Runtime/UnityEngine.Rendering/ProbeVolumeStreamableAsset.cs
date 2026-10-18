using System;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[Serializable]
[MovedFrom(false, "UnityEngine.Rendering", "Unity.RenderPipelines.Core.Runtime", "ProbeVolumeBakingSet.StreamableAsset")]
internal class ProbeVolumeStreamableAsset
{
	[Serializable]
	[MovedFrom(false, "UnityEngine.Rendering", "Unity.RenderPipelines.Core.Runtime", "ProbeVolumeBakingSet.StreamableAsset.StreamableCellDesc")]
	public struct StreamableCellDesc
	{
		public int offset;

		public int elementCount;
	}

	[SerializeField]
	[FormerlySerializedAs("assetGUID")]
	private string m_AssetGUID = "";

	[SerializeField]
	[FormerlySerializedAs("streamableAssetPath")]
	private string m_StreamableAssetPath = "";

	[SerializeField]
	[FormerlySerializedAs("elementSize")]
	private int m_ElementSize;

	[SerializeField]
	[FormerlySerializedAs("streamableCellDescs")]
	private SerializedDictionary<int, StreamableCellDesc> m_StreamableCellDescs = new SerializedDictionary<int, StreamableCellDesc>();

	[SerializeField]
	private TextAsset m_Asset;

	private string m_FinalAssetPath;

	private FileHandle m_AssetFileHandle;

	public string assetGUID => m_AssetGUID;

	public TextAsset asset => m_Asset;

	public int elementSize => m_ElementSize;

	public SerializedDictionary<int, StreamableCellDesc> streamableCellDescs => m_StreamableCellDescs;

	public ProbeVolumeStreamableAsset(string apvStreamingAssetsPath, SerializedDictionary<int, StreamableCellDesc> cellDescs, int elementSize, string bakingSetGUID, string assetGUID)
	{
		m_AssetGUID = assetGUID;
		m_StreamableCellDescs = cellDescs;
		m_ElementSize = elementSize;
		m_StreamableAssetPath = Path.Combine(Path.Combine(apvStreamingAssetsPath, bakingSetGUID), m_AssetGUID + ".bytes");
	}

	internal void RefreshAssetPath()
	{
		m_FinalAssetPath = Path.Combine(Application.streamingAssetsPath, m_StreamableAssetPath);
	}

	public string GetAssetPath()
	{
		if (string.IsNullOrEmpty(m_FinalAssetPath))
		{
			RefreshAssetPath();
		}
		return m_FinalAssetPath;
	}

	internal bool HasValidAssetReference()
	{
		if (m_Asset != null)
		{
			return m_Asset.bytes != null;
		}
		return false;
	}

	public unsafe bool FileExists()
	{
		if (m_Asset != null)
		{
			return true;
		}
		FileInfoResult fileInfoResult = default(FileInfoResult);
		AsyncReadManager.GetFileInfo(GetAssetPath(), &fileInfoResult).JobHandle.Complete();
		return fileInfoResult.FileState == FileState.Exists;
	}

	public long GetFileSize()
	{
		return new FileInfo(GetAssetPath()).Length;
	}

	public bool IsOpen()
	{
		return m_AssetFileHandle.IsValid();
	}

	public FileHandle OpenFile()
	{
		if (m_AssetFileHandle.IsValid())
		{
			return m_AssetFileHandle;
		}
		m_AssetFileHandle = AsyncReadManager.OpenFileAsync(GetAssetPath());
		return m_AssetFileHandle;
	}

	public void CloseFile()
	{
		if (m_AssetFileHandle.IsValid() && m_AssetFileHandle.JobHandle.IsCompleted)
		{
			m_AssetFileHandle.Close();
		}
		m_AssetFileHandle = default(FileHandle);
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(m_AssetGUID);
	}

	public void Dispose()
	{
		if (m_AssetFileHandle.IsValid())
		{
			m_AssetFileHandle.Close().Complete();
			m_AssetFileHandle = default(FileHandle);
		}
	}
}
