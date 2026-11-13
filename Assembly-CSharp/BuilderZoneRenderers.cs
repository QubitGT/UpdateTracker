using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderZoneRenderers : MonoBehaviour
{
	public List<Renderer> renderers;

	public List<Canvas> canvases;

	public List<GameObject> rootObjects;

	private bool inBuilderZone;

	private List<Renderer> allRenderers = new List<Renderer>(200);

	private void Start()
	{
		allRenderers.Clear();
		foreach (Renderer renderer in renderers)
		{
			if (renderer != null)
			{
				allRenderers.Add(renderer);
			}
		}
		foreach (GameObject rootObject in rootObjects)
		{
			if (rootObject != null)
			{
				allRenderers.AddRange(rootObject.GetComponentsInChildren<Renderer>(includeInactive: true));
			}
		}
		canvases.RemoveAll((Canvas c) => c == null);
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(OnZoneChanged));
		inBuilderZone = true;
		OnZoneChanged();
	}

	private void OnDestroy()
	{
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(OnZoneChanged));
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.instance.IsZoneActive(GTZone.monkeBlocks);
		if (flag != inBuilderZone)
		{
			inBuilderZone = flag;
			SetVisible(flag);
		}
	}

	private void SetVisible(bool visible)
	{
		foreach (Renderer allRenderer in allRenderers)
		{
			if (allRenderer != null)
			{
				allRenderer.enabled = visible;
			}
		}
		foreach (Canvas canvase in canvases)
		{
			if (canvase != null)
			{
				canvase.enabled = visible;
			}
		}
	}
}
