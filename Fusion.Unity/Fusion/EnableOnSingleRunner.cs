using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion;

[AddComponentMenu("Fusion/Enable On Single Runner")]
public class EnableOnSingleRunner : Behaviour
{
	[InlineHelp]
	[SerializeField]
	public RunnerVisibilityLink.PreferredRunners PreferredRunner;

	[InlineHelp]
	public Component[] Components = new Component[0];

	[HideInInspector]
	[SerializeField]
	private string _guid = Guid.NewGuid().ToString().Substring(0, 19);

	private static readonly List<Component> reusableComponentsList = new List<Component>();

	private static readonly List<Component> reusableComponentsList2 = new List<Component>();

	internal void AddNodes(List<RunnerVisibilityLink> existingNodes)
	{
		for (int i = 0; i < Components.Length; i++)
		{
			bool flag = false;
			foreach (RunnerVisibilityLink existingNode in existingNodes)
			{
				if (existingNode.Component == Components[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RunnerVisibilityLink runnerVisibilityLink = Components[i].gameObject.AddComponent<RunnerVisibilityLink>();
				runnerVisibilityLink.Guid = _guid + i;
				runnerVisibilityLink.Component = Components[i];
				runnerVisibilityLink.SetupOnSingleRunnerLink(PreferredRunner);
				existingNodes.Add(runnerVisibilityLink);
			}
		}
	}

	[EditorButton("Find on GameObject", EditorButtonVisibility.EditMode, 0, true)]
	public void FindRecognizedTypes()
	{
		Components = FindRecognizedComponentsOnGameObject(base.gameObject);
	}

	[EditorButton("Find in Nested Children", EditorButtonVisibility.EditMode, 0, true)]
	public void FindNestedRecognizedTypes()
	{
		Components = FindRecognizedNestedComponents(base.gameObject);
	}

	private static Component[] FindRecognizedComponentsOnGameObject(GameObject go)
	{
		try
		{
			go.GetComponents(reusableComponentsList);
			reusableComponentsList2.Clear();
			foreach (Component reusableComponents in reusableComponentsList)
			{
				if (reusableComponents.GetType().IsRecognizedByRunnerVisibility())
				{
					reusableComponentsList2.Add(reusableComponents);
				}
			}
			return reusableComponentsList2.ToArray();
		}
		finally
		{
			reusableComponentsList.Clear();
			reusableComponentsList2.Clear();
		}
	}

	private static Component[] FindRecognizedNestedComponents(GameObject go)
	{
		try
		{
			go.transform.GetNestedComponentsInChildren<Component, NetworkObject>(reusableComponentsList);
			reusableComponentsList2.Clear();
			foreach (Component reusableComponents in reusableComponentsList)
			{
				if (reusableComponents.GetType().IsRecognizedByRunnerVisibility())
				{
					reusableComponentsList2.Add(reusableComponents);
				}
			}
			return reusableComponentsList2.ToArray();
		}
		finally
		{
			reusableComponentsList.Clear();
			reusableComponentsList2.Clear();
		}
	}
}
