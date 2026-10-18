using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion;

[ScriptHelp]
[FusionGlobalScriptableObject("Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion", DefaultContentsGeneratorMethod = "GenerateDefaultContents")]
public class NetworkProjectConfigAsset : FusionGlobalScriptableObject<NetworkProjectConfigAsset>
{
	[Serializable]
	public struct SerializableSimulationBehaviourMeta
	{
		public SerializableType<SimulationBehaviour> Type;

		public int ExecutionOrder;
	}

	[SerializeField]
	[DrawInline]
	public NetworkProjectConfig Config = new NetworkProjectConfig();

	[ResolveNetworkPrefabSource]
	[SerializeReference]
	[HideArrayElementLabel]
	[InlineHelp]
	public List<INetworkPrefabSource> Prefabs = new List<INetworkPrefabSource>();

	public NetworkPrefabTableOptions PrefabOptions = NetworkPrefabTableOptions.Default;

	[ReadOnly]
	[InlineHelp]
	[SerializeField]
	public SerializableSimulationBehaviourMeta[] BehaviourMeta = Array.Empty<SerializableSimulationBehaviourMeta>();

	public static NetworkProjectConfigAsset Global => FusionGlobalScriptableObject<NetworkProjectConfigAsset>.GlobalInternal;

	public static bool IsGlobalLoaded => FusionGlobalScriptableObject<NetworkProjectConfigAsset>.IsGlobalLoadedInternal;

	public static bool TryGetGlobal(out NetworkProjectConfigAsset global)
	{
		return FusionGlobalScriptableObject<NetworkProjectConfigAsset>.TryGetGlobalInternal(out global);
	}

	public static void UnloadGlobal()
	{
		FusionGlobalScriptableObject<NetworkProjectConfigAsset>.UnloadGlobalInternal();
	}

	private void OnEnable()
	{
		Config.PrefabTable.Clear();
		Config.ExecutionOrderOverrides.Clear();
		NetworkPrefabTable prefabTable = Config.PrefabTable;
		foreach (INetworkPrefabSource prefab in Prefabs)
		{
			if (!prefabTable.TryAddSource(prefab, out var _))
			{
				InternalLogStreams.LogError?.Log($"Failed to add prefab asset {prefab.AssetGuid}, there is already a prefab entry with same guid");
			}
		}
		prefabTable.Options = PrefabOptions;
		SerializableSimulationBehaviourMeta[] behaviourMeta = BehaviourMeta;
		for (int i = 0; i < behaviourMeta.Length; i++)
		{
			SerializableSimulationBehaviourMeta serializableSimulationBehaviourMeta = behaviourMeta[i];
			Type type = serializableSimulationBehaviourMeta.Type;
			if (type == null)
			{
				InternalLogStreams.LogError?.Log($"Failed to resolve type: {serializableSimulationBehaviourMeta.Type}");
			}
			else
			{
				Config.ExecutionOrderOverrides.Add(type, serializableSimulationBehaviourMeta.ExecutionOrder);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Config.PrefabTable?.Clear();
		Config.ExecutionOrderOverrides.Clear();
	}

	private static string GenerateDefaultContents()
	{
		return JsonUtility.ToJson(new NetworkProjectConfig(), prettyPrint: true);
	}
}
