using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal class TriggerContactMonitor
{
	private readonly Dictionary<Collider, IXRInteractable> m_EnteredColliders = new Dictionary<Collider, IXRInteractable>();

	private readonly HashSet<IXRInteractable> m_UnorderedInteractables = new HashSet<IXRInteractable>();

	private readonly HashSet<Collider> m_EnteredUnassociatedColliders = new HashSet<Collider>();

	private static readonly List<Collider> s_ScratchColliders = new List<Collider>();

	private static readonly List<Collider> s_ExitedColliders = new List<Collider>();

	public XRInteractionManager interactionManager { get; set; }

	public event Action<IXRInteractable> contactAdded;

	public event Action<IXRInteractable> contactRemoved;

	public void AddCollider(Collider collider)
	{
		if (interactionManager == null)
		{
			return;
		}
		if (!interactionManager.TryGetInteractableForCollider(collider, out var interactable))
		{
			m_EnteredUnassociatedColliders.Add(collider);
			return;
		}
		m_EnteredColliders[collider] = interactable;
		if (m_UnorderedInteractables.Add(interactable))
		{
			this.contactAdded?.Invoke(interactable);
		}
	}

	public void RemoveCollider(Collider collider)
	{
		if (m_EnteredUnassociatedColliders.Remove(collider) || !m_EnteredColliders.TryGetValue(collider, out var value))
		{
			return;
		}
		m_EnteredColliders.Remove(collider);
		if (value == null)
		{
			return;
		}
		foreach (KeyValuePair<Collider, IXRInteractable> enteredCollider in m_EnteredColliders)
		{
			if (enteredCollider.Value == value && enteredCollider.Key != null)
			{
				return;
			}
		}
		if (m_UnorderedInteractables.Remove(value))
		{
			this.contactRemoved?.Invoke(value);
		}
	}

	public void ResolveUnassociatedColliders()
	{
		m_EnteredUnassociatedColliders.RemoveWhere(IsDestroyed);
		if (m_EnteredUnassociatedColliders.Count == 0 || interactionManager == null)
		{
			return;
		}
		s_ScratchColliders.Clear();
		foreach (Collider enteredUnassociatedCollider in m_EnteredUnassociatedColliders)
		{
			if (interactionManager.TryGetInteractableForCollider(enteredUnassociatedCollider, out var interactable))
			{
				s_ScratchColliders.Add(enteredUnassociatedCollider);
				m_EnteredColliders[enteredUnassociatedCollider] = interactable;
				if (m_UnorderedInteractables.Add(interactable))
				{
					this.contactAdded?.Invoke(interactable);
				}
			}
		}
		s_ScratchColliders.ForEach(RemoveFromUnassociatedColliders);
		s_ScratchColliders.Clear();
	}

	private void RemoveFromUnassociatedColliders(Collider col)
	{
		m_EnteredUnassociatedColliders.Remove(col);
	}

	public void ResolveUnassociatedColliders(IXRInteractable interactable)
	{
		m_EnteredUnassociatedColliders.RemoveWhere(IsDestroyed);
		if (m_EnteredUnassociatedColliders.Count == 0 || interactionManager == null)
		{
			return;
		}
		int i = 0;
		for (int count = interactable.colliders.Count; i < count; i++)
		{
			Collider collider = interactable.colliders[i];
			if (!(collider == null) && m_EnteredUnassociatedColliders.Contains(collider) && interactionManager.TryGetInteractableForCollider(collider, out var interactable2) && interactable2 == interactable)
			{
				m_EnteredUnassociatedColliders.Remove(collider);
				m_EnteredColliders[collider] = interactable;
				if (m_UnorderedInteractables.Add(interactable))
				{
					this.contactAdded?.Invoke(interactable);
				}
			}
		}
	}

	public void UpdateStayedColliders(HashSet<Collider> stayedColliders)
	{
		s_ExitedColliders.Clear();
		if (m_EnteredColliders.Count > 0)
		{
			foreach (Collider key in m_EnteredColliders.Keys)
			{
				if (!stayedColliders.Contains(key))
				{
					s_ExitedColliders.Add(key);
				}
				else
				{
					stayedColliders.Remove(key);
				}
			}
		}
		if (stayedColliders.Count > 0)
		{
			foreach (Collider stayedCollider in stayedColliders)
			{
				AddCollider(stayedCollider);
			}
		}
		if (s_ExitedColliders.Count > 0)
		{
			s_ExitedColliders.ForEach(RemoveCollider);
			s_ExitedColliders.Clear();
		}
	}

	public bool IsContacting(IXRInteractable interactable)
	{
		return m_UnorderedInteractables.Contains(interactable);
	}

	private static bool IsDestroyed(Collider col)
	{
		return col == null;
	}
}
