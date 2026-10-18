using System.Collections.Generic;

namespace UnityEngine.Formats.Fbx.Exporter;

internal class FbxPrefab : MonoBehaviour
{
	[SerializeField]
	private string m_fbxHistory;

	[SerializeField]
	private List<StringPair> m_nameMapping = new List<StringPair>();

	[SerializeField]
	[Tooltip("Which FBX file does this refer to?")]
	private GameObject m_fbxModel;

	[Tooltip("Should we auto-update this prefab when the FBX file is updated?")]
	[SerializeField]
	private bool m_autoUpdate = true;

	public string FbxHistory
	{
		get
		{
			return m_fbxHistory;
		}
		set
		{
			m_fbxHistory = value;
		}
	}

	public List<StringPair> NameMapping => m_nameMapping;

	public GameObject FbxModel
	{
		get
		{
			return m_fbxModel;
		}
		set
		{
			m_fbxModel = value;
		}
	}

	public bool AutoUpdate
	{
		get
		{
			return m_autoUpdate;
		}
		set
		{
			m_autoUpdate = value;
		}
	}

	public static event HandleUpdate OnUpdate;

	public static void CallOnUpdate(FbxPrefab instance, IEnumerable<GameObject> updatedObjects)
	{
		if (FbxPrefab.OnUpdate != null)
		{
			FbxPrefab.OnUpdate(instance, updatedObjects);
		}
	}
}
