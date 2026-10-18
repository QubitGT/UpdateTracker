using System;

namespace UnityEngine.Formats.Fbx.Exporter;

[Serializable]
internal struct StringPair
{
	private string m_fbxObjectName;

	private string m_unityObjectName;

	public string FBXObjectName
	{
		get
		{
			return m_fbxObjectName;
		}
		set
		{
			m_fbxObjectName = value;
		}
	}

	public string UnityObjectName
	{
		get
		{
			return m_unityObjectName;
		}
		set
		{
			m_unityObjectName = value;
		}
	}
}
