using Modio.Mods;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Modio.Unity.UI.Components;

public class ModioUIMod : MonoBehaviour, IModioUIPropertiesOwner, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	public UnityEvent onModUpdate;

	public UnityEvent<Mod> onClickOrSubmit;

	public UnityEvent<ModioUIMod> onDisplayMoreInfo;

	public Mod Mod { get; private set; }

	private void OnDestroy()
	{
		if (Mod != null)
		{
			Mod.OnModUpdated -= OnModUpdated;
		}
	}

	public void AddUpdatePropertiesListener(UnityAction listener)
	{
		onModUpdate.AddListener(listener);
	}

	public void RemoveUpdatePropertiesListener(UnityAction listener)
	{
		onModUpdate.RemoveListener(listener);
	}

	public void SetMod(Mod mod)
	{
		if (Mod != null)
		{
			Mod.OnModUpdated -= OnModUpdated;
		}
		Mod = mod;
		if (mod != null)
		{
			Mod.OnModUpdated += OnModUpdated;
			OnModUpdated();
		}
	}

	private void OnModUpdated()
	{
		onModUpdate?.Invoke();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnSubmit(eventData);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		onClickOrSubmit?.Invoke(Mod);
	}

	public void OnDisplayMoreInfoClicked()
	{
		onDisplayMoreInfo?.Invoke(this);
	}
}
