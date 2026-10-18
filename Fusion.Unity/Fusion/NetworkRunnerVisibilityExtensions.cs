using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fusion;

public static class NetworkRunnerVisibilityExtensions
{
	private class RunnerVisibility
	{
		public LinkedList<RunnerVisibilityLink> Nodes = new LinkedList<RunnerVisibilityLink>();

		public bool IsVisible { get; set; } = true;

	}

	private static readonly string[] RecognizedBehaviourNames;

	private static readonly Type[] RecognizedBehaviourTypes;

	private static readonly Dictionary<NetworkRunner, RunnerVisibility> DictionaryLookup;

	private static bool _commonLinksWithMissingInputAuthNeedRefresh;

	private static readonly Dictionary<string, List<RunnerVisibilityLink>> CommonObjectLookup;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetAllSimulationStatics()
	{
		ResetStatics();
	}

	static NetworkRunnerVisibilityExtensions()
	{
		RecognizedBehaviourNames = new string[1] { "EventSystem" };
		RecognizedBehaviourTypes = new Type[6]
		{
			typeof(IRunnerVisibilityRecognizedType),
			typeof(Renderer),
			typeof(AudioListener),
			typeof(Camera),
			typeof(Canvas),
			typeof(Light)
		};
		CommonObjectLookup = new Dictionary<string, List<RunnerVisibilityLink>>();
		DictionaryLookup = new Dictionary<NetworkRunner, RunnerVisibility>();
	}

	public static void RetryRefreshCommonLinks()
	{
		_commonLinksWithMissingInputAuthNeedRefresh = false;
		RefreshCommonObjectVisibilities();
	}

	public static void EnableVisibilityExtension(this NetworkRunner runner)
	{
		if ((bool)runner && !DictionaryLookup.ContainsKey(runner))
		{
			DictionaryLookup.Add(runner, new RunnerVisibility());
		}
	}

	public static void DisableVisibilityExtension(this NetworkRunner runner)
	{
		if ((bool)runner && DictionaryLookup.ContainsKey(runner))
		{
			DictionaryLookup.Remove(runner);
		}
	}

	public static bool HasVisibilityEnabled(this NetworkRunner runner)
	{
		return DictionaryLookup.ContainsKey(runner);
	}

	public static bool GetVisible(this NetworkRunner runner)
	{
		if (runner == null)
		{
			return false;
		}
		if (!DictionaryLookup.TryGetValue(runner, out var value))
		{
			return true;
		}
		return value.IsVisible;
	}

	public static void SetVisible(this NetworkRunner runner, bool isVisibile)
	{
		runner.GetVisibilityInfo().IsVisible = isVisibile;
		RefreshRunnerVisibility(runner);
	}

	private static LinkedList<RunnerVisibilityLink> GetVisibilityNodes(this NetworkRunner runner)
	{
		if (!runner)
		{
			return null;
		}
		return runner.GetVisibilityInfo()?.Nodes;
	}

	private static RunnerVisibility GetVisibilityInfo(this NetworkRunner runner)
	{
		if (!DictionaryLookup.TryGetValue(runner, out var value))
		{
			return null;
		}
		return value;
	}

	public static void AddVisibilityNodes(this NetworkRunner runner, GameObject go)
	{
		runner.EnableVisibilityExtension();
		if (!go.GetComponent<RunnerVisibilityLinksRoot>())
		{
			go.AddComponent<RunnerVisibilityLinksRoot>();
			EnableOnSingleRunner[] componentsInChildren = go.transform.GetComponentsInChildren<EnableOnSingleRunner>(includeInactive: true);
			List<RunnerVisibilityLink> existingNodes = go.GetComponentsInChildren<RunnerVisibilityLink>(includeInactive: false).ToList();
			EnableOnSingleRunner[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AddNodes(existingNodes);
			}
			CollectBehavioursAndAddNodes(go, runner, existingNodes);
			RefreshRunnerVisibility(runner);
		}
	}

	private static void CollectBehavioursAndAddNodes(GameObject go, NetworkRunner runner, List<RunnerVisibilityLink> existingNodes)
	{
		bool flag = false;
		Component[] componentsInChildren = go.transform.GetComponentsInChildren<Component>(includeInactive: true);
		foreach (Component component in componentsInChildren)
		{
			bool flag2 = false;
			if (component == null)
			{
				continue;
			}
			foreach (RunnerVisibilityLink existingNode in existingNodes)
			{
				if (existingNode.Component == component)
				{
					flag2 = true;
					if (existingNode.IsOnSingleRunner)
					{
						AddNodeToCommonLookup(existingNode);
						RegisterNode(existingNode, runner, component);
						flag = true;
					}
					break;
				}
			}
			if (!flag2 && component.GetType().IsRecognizedByRunnerVisibility())
			{
				RegisterNode(component.gameObject.AddComponent<RunnerVisibilityLink>(), runner, component);
			}
		}
		if (flag)
		{
			_commonLinksWithMissingInputAuthNeedRefresh = true;
			RefreshCommonObjectVisibilities();
		}
	}

	internal static bool IsRecognizedByRunnerVisibility(this Type type)
	{
		Type[] recognizedBehaviourTypes = RecognizedBehaviourTypes;
		for (int i = 0; i < recognizedBehaviourTypes.Length; i++)
		{
			if (recognizedBehaviourTypes[i].IsAssignableFrom(type))
			{
				return true;
			}
		}
		string name = type.Name;
		string[] recognizedBehaviourNames = RecognizedBehaviourNames;
		foreach (string value in recognizedBehaviourNames)
		{
			if (name.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private static void RegisterNode(RunnerVisibilityLink link, NetworkRunner runner, Component comp)
	{
		runner.GetVisibilityNodes().AddLast(link);
		link.Initialize(comp, runner);
	}

	public static void UnregisterNode(this RunnerVisibilityLink link)
	{
		if (link == null || link._runner == null)
		{
			return;
		}
		NetworkRunner runner = link._runner;
		bool flag = !runner;
		if (!flag && link._runner.GetVisibilityNodes() == null)
		{
			return;
		}
		if (!flag && runner.GetVisibilityNodes().Contains(link))
		{
			runner.GetVisibilityNodes().Remove(link);
		}
		if (link.Guid != null && CommonObjectLookup.TryGetValue(link.Guid, out var value))
		{
			if (value.Contains(link))
			{
				value.Remove(link);
			}
			if (value.Count == 0)
			{
				CommonObjectLookup.Remove(link.Guid);
			}
		}
	}

	private static void AddNodeToCommonLookup(RunnerVisibilityLink link)
	{
		string guid = link.Guid;
		if (!string.IsNullOrEmpty(guid))
		{
			if (!CommonObjectLookup.TryGetValue(guid, out var value))
			{
				value = new List<RunnerVisibilityLink>();
				CommonObjectLookup.Add(guid, value);
			}
			value.Add(link);
		}
	}

	private static void RefreshRunnerVisibility(NetworkRunner runner, bool refreshCommonObjects = true)
	{
		if (runner.GetVisibilityNodes() == null)
		{
			return;
		}
		bool visible = runner.GetVisible();
		foreach (RunnerVisibilityLink visibilityNode in runner.GetVisibilityNodes())
		{
			if (!(visibilityNode == null))
			{
				visibilityNode.SetEnabled(visible);
			}
		}
		if (refreshCommonObjects)
		{
			RefreshCommonObjectVisibilities();
		}
	}

	internal static void RefreshCommonObjectVisibilities()
	{
		List<NetworkRunner>.Enumerator instancesEnumerator = NetworkRunner.GetInstancesEnumerator();
		NetworkRunner networkRunner = null;
		NetworkRunner networkRunner2 = null;
		NetworkRunner networkRunner3 = null;
		bool flag = false;
		while (instancesEnumerator.MoveNext())
		{
			NetworkRunner current = instancesEnumerator.Current;
			if (current.IsRunning && current.GetVisible() && !current.IsShutdown)
			{
				if (current.IsServer)
				{
					networkRunner = current;
				}
				if (!networkRunner2 && current.GameMode != GameMode.Server)
				{
					networkRunner2 = current;
				}
				if (!networkRunner3)
				{
					networkRunner3 = current;
				}
			}
		}
		foreach (KeyValuePair<string, List<RunnerVisibilityLink>> item in CommonObjectLookup)
		{
			List<RunnerVisibilityLink> value = item.Value;
			if (value.Count <= 0)
			{
				continue;
			}
			RunnerVisibilityLink runnerVisibilityLink = value[0];
			NetworkRunner networkRunner4 = runnerVisibilityLink.PreferredRunner switch
			{
				RunnerVisibilityLink.PreferredRunners.Server => networkRunner, 
				RunnerVisibilityLink.PreferredRunners.Client => networkRunner2, 
				RunnerVisibilityLink.PreferredRunners.Auto => networkRunner3, 
				_ => null, 
			};
			flag = false;
			foreach (RunnerVisibilityLink item2 in value)
			{
				if (item2.PreferredRunner == RunnerVisibilityLink.PreferredRunners.InputAuthority)
				{
					bool flag2 = item2.IsInputAuth();
					item2.Enabled = flag2 && item2._runner.GetVisible();
					flag = flag || flag2;
				}
				else
				{
					item2.Enabled = (object)item2._runner == networkRunner4;
				}
			}
			if (runnerVisibilityLink.PreferredRunner == RunnerVisibilityLink.PreferredRunners.InputAuthority && !flag && _commonLinksWithMissingInputAuthNeedRefresh)
			{
				_commonLinksWithMissingInputAuthNeedRefresh = false;
				runnerVisibilityLink.InvokeRefreshCommonObjectVisibilities(1f);
			}
		}
	}

	internal static void ResetStatics()
	{
		CommonObjectLookup.Clear();
	}
}
