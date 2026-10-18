using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ActivateGO : MonoBehaviour
{
	public enum ActivateGOMode
	{
		EnableRenderers,
		ActivateGameObjects
	}

	[SerializeField]
	private GameObject targetGO;

	[SerializeField]
	private PlayerPrefFlags.Flag flag;

	[SerializeField]
	private bool invertFlag;

	[SerializeField]
	private int flashes;

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private ActivateGOMode mode;

	private bool active;

	private bool flashing;

	private void OnEnable()
	{
		active = PlayerPrefFlags.Check(flag) != invertFlag;
		SetGOsActive(0);
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Combine(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(OnFlagChange));
	}

	private void OnDisable()
	{
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Remove(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(OnFlagChange));
	}

	private void OnDestroy()
	{
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Remove(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(OnFlagChange));
	}

	public void OnFlagChange(PlayerPrefFlags.Flag f, bool value)
	{
		if (f == flag)
		{
			if (invertFlag)
			{
				active = !value;
			}
			else
			{
				active = value;
			}
			SetGOsActive(flashes);
		}
	}

	private async void SetGOsActive(int fls)
	{
		if (flashing)
		{
			return;
		}
		if (mode == ActivateGOMode.EnableRenderers)
		{
			List<Renderer> renderers = new List<Renderer>();
			renderers.AddRange(targetGO.GetComponentsInChildren<MeshRenderer>(includeInactive: true));
			renderers.AddRange(targetGO.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true));
			FirstPersonToggleOverride[] componentsInChildren = targetGO.GetComponentsInChildren<FirstPersonToggleOverride>(includeInactive: true);
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				if (componentsInChildren[k].Toggle && !renderers.Contains(componentsInChildren[k].Renderer))
				{
					renderers.Add(componentsInChildren[k].Renderer);
				}
				else if (!componentsInChildren[k].Toggle && renderers.Contains(componentsInChildren[k].Renderer))
				{
					renderers.Remove(componentsInChildren[k].Renderer);
				}
			}
			for (int i = 0; i < fls; i++)
			{
				flashing = true;
				toggle(renderers, active);
				await Task.Delay(150);
				toggle(renderers, !active);
				await Task.Delay(100);
			}
			toggle(renderers, active);
			flashing = false;
		}
		else
		{
			for (int i = 0; i < fls; i++)
			{
				flashing = true;
				targetGO.SetActive(active);
				await Task.Delay(150);
				targetGO.SetActive(!active);
				await Task.Delay(100);
			}
			targetGO.SetActive(active);
			flashing = false;
		}
	}

	private void toggle(List<Renderer> renderers, bool state)
	{
		for (int i = 0; i < renderers.Count; i++)
		{
			if ((layerMask.value & (1 << renderers[i].gameObject.layer)) != 0)
			{
				renderers[i].forceRenderingOff = !state;
			}
		}
	}
}
