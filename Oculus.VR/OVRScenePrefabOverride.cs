using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
public class OVRScenePrefabOverride : ISerializationCallbackReceiver
{
	[FormerlySerializedAs("prefab")]
	public OVRSceneAnchor Prefab;

	[FormerlySerializedAs("classificationLabel")]
	public string ClassificationLabel = "";

	[FormerlySerializedAs("editorClassificationIndex")]
	[SerializeField]
	private int _editorClassificationIndex;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		UpdateEditorClassificationIndex();
	}

	internal void UpdateEditorClassificationIndex()
	{
		if (ClassificationLabel != "")
		{
			_editorClassificationIndex = IndexOf(ClassificationLabel, OVRSceneManager.Classification.List);
			if (_editorClassificationIndex < 0)
			{
				Debug.LogError("[OVRScenePrefabOverride] OnAfterDeserialize() " + ClassificationLabel + " not found. The Classification list in OVRSceneManager has likely changed");
			}
		}
		else
		{
			_editorClassificationIndex = 0;
		}
		static int IndexOf(string label, IEnumerable<string> collection)
		{
			int num = 0;
			foreach (string item in collection)
			{
				if (item == label)
				{
					return num;
				}
				num++;
			}
			return -1;
		}
	}
}
