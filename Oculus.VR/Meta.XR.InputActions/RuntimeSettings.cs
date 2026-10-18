using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.InputActions;

public class RuntimeSettings : OVRRuntimeAssetsBase
{
	[Tooltip("A list of input action definitions, which define how certain input values can be obtained from third party devices.")]
	public List<UserInputActionSet> InputActionDefinitions = new List<UserInputActionSet>();

	[Tooltip("Allows for the inclusion of Input Actions defined in an InputActionSet Serializable Object, such as those provided in third party device samples.")]
	public List<InputActionSet> InputActionSets = new List<InputActionSet>();

	internal static string InstanceAssetName = "InputActions";

	private static RuntimeSettings _instance;

	public static RuntimeSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				OVRRuntimeAssetsBase.LoadAsset(out RuntimeSettings assetInstance, InstanceAssetName, (Action<RuntimeSettings>)null);
				_instance = assetInstance;
			}
			return _instance;
		}
	}
}
