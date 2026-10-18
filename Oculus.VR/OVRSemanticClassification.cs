using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[DisallowMultipleComponent]
[RequireComponent(typeof(OVRSceneAnchor))]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSemanticClassification : MonoBehaviour, IOVRSceneComponent
{
	public const char LabelSeparator = ',';

	private readonly List<string> _labels = new List<string>();

	public IReadOnlyList<string> Labels => _labels;

	public bool Contains(string label)
	{
		foreach (string label2 in _labels)
		{
			if (label2 == label)
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		if (GetComponent<OVRSceneAnchor>().Space.Valid)
		{
			((IOVRSceneComponent)this).Initialize();
		}
	}

	void IOVRSceneComponent.Initialize()
	{
		if (OVRPlugin.GetSpaceSemanticLabels(GetComponent<OVRSceneAnchor>().Space, out var labels))
		{
			_labels.Clear();
			_labels.AddRange(ValidateAndUpgradeLabels(labels).Split(','));
		}
	}

	internal static string ValidateAndUpgradeLabels(string labels)
	{
		List<string> list;
		using (new OVRObjectPool.ListScope<string>(out list))
		{
			string[] array = labels.Split(',');
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				list.Add(text);
				switch (text)
				{
				case "TABLE":
					flag = true;
					break;
				case "DESK":
					flag2 = true;
					break;
				case "INVISIBLE_WALL_FACE":
					flag3 = true;
					break;
				case "WALL_FACE":
					flag4 = true;
					break;
				}
			}
			if (flag && !flag2)
			{
				list.Add("DESK");
			}
			else if (flag2 && !flag)
			{
				list.Add("TABLE");
			}
			if (flag3 && !flag4)
			{
				list.Add("WALL_FACE");
			}
			return string.Join(','.ToString(), list);
		}
	}
}
