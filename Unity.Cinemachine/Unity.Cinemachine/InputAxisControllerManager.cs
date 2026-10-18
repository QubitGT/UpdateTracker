using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
internal class InputAxisControllerManager<T> where T : IInputAxisReader, new()
{
	public delegate void DefaultInitializer(in IInputAxisOwner.AxisDescriptor axis, InputAxisControllerBase<T>.Controller controller);

	[NonReorderable]
	public List<InputAxisControllerBase<T>.Controller> Controllers = new List<InputAxisControllerBase<T>.Controller>();

	private readonly List<IInputAxisOwner.AxisDescriptor> m_Axes = new List<IInputAxisOwner.AxisDescriptor>();

	private readonly List<IInputAxisOwner> m_AxisOwners = new List<IInputAxisOwner>();

	private readonly List<IInputAxisResetSource> m_AxisResetters = new List<IInputAxisResetSource>();

	public void Validate()
	{
		for (int i = 0; i < Controllers.Count; i++)
		{
			if (Controllers[i] != null)
			{
				Controllers[i].Driver.Validate();
			}
		}
	}

	public void OnDisable()
	{
		for (int i = 0; i < m_AxisResetters.Count; i++)
		{
			if (m_AxisResetters[i] as UnityEngine.Object != null)
			{
				m_AxisResetters[i].UnregisterResetHandler(OnResetInput);
			}
		}
		m_Axes.Clear();
		m_AxisOwners.Clear();
		m_AxisResetters.Clear();
	}

	public void Reset()
	{
		OnDisable();
		Controllers.Clear();
	}

	private void OnResetInput()
	{
		for (int i = 0; i < Controllers.Count; i++)
		{
			Controllers[i].Driver.Reset(ref m_Axes[i].DrivenAxis());
		}
	}

	public void CreateControllers(GameObject root, bool scanRecursively, bool enabled, DefaultInitializer defaultInitializer)
	{
		OnDisable();
		if (scanRecursively)
		{
			root.GetComponentsInChildren(m_AxisOwners);
		}
		else
		{
			root.GetComponents(m_AxisOwners);
		}
		for (int num = Controllers.Count - 1; num >= 0; num--)
		{
			if (!m_AxisOwners.Contains(Controllers[num].Owner as IInputAxisOwner))
			{
				Controllers.RemoveAt(num);
			}
		}
		List<InputAxisControllerBase<T>.Controller> list2 = new List<InputAxisControllerBase<T>.Controller>();
		for (int i = 0; i < m_AxisOwners.Count; i++)
		{
			IInputAxisOwner inputAxisOwner = m_AxisOwners[i];
			int count = m_Axes.Count;
			inputAxisOwner.GetInputAxes(m_Axes);
			for (int j = count; j < m_Axes.Count; j++)
			{
				int num2 = GetControllerIndex(Controllers, inputAxisOwner, m_Axes[j].Name);
				if (num2 < 0)
				{
					InputAxisControllerBase<T>.Controller controller = new InputAxisControllerBase<T>.Controller
					{
						Enabled = true,
						Name = m_Axes[j].Name,
						Owner = (inputAxisOwner as UnityEngine.Object),
						Input = new T()
					};
					if (defaultInitializer != null)
					{
						IInputAxisOwner.AxisDescriptor axis = m_Axes[j];
						defaultInitializer(in axis, controller);
					}
					list2.Add(controller);
				}
				else
				{
					list2.Add(Controllers[num2]);
					Controllers.RemoveAt(num2);
				}
			}
		}
		Controllers = list2;
		if (enabled)
		{
			RegisterResetHandlers(root, scanRecursively);
		}
		static int GetControllerIndex(List<InputAxisControllerBase<T>.Controller> list, IInputAxisOwner owner, string axisName)
		{
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].Owner as IInputAxisOwner == owner && list[k].Name == axisName)
				{
					return k;
				}
			}
			return -1;
		}
	}

	private void RegisterResetHandlers(GameObject root, bool scanRecursively)
	{
		m_AxisResetters.Clear();
		if (scanRecursively)
		{
			root.GetComponentsInChildren(m_AxisResetters);
		}
		else
		{
			root.GetComponents(m_AxisResetters);
		}
		for (int i = 0; i < m_AxisResetters.Count; i++)
		{
			m_AxisResetters[i].UnregisterResetHandler(OnResetInput);
			m_AxisResetters[i].RegisterResetHandler(OnResetInput);
		}
	}

	public void UpdateControllers(UnityEngine.Object context, float deltaTime)
	{
		for (int i = 0; i < Controllers.Count; i++)
		{
			InputAxisControllerBase<T>.Controller controller = Controllers[i];
			if (controller.Enabled && controller.Input != null)
			{
				IInputAxisOwner.AxisDescriptor.Hints hint = ((i < m_Axes.Count) ? m_Axes[i].Hint : IInputAxisOwner.AxisDescriptor.Hints.Default);
				if (controller.Input != null)
				{
					controller.InputValue = controller.Input.GetValue(context, hint);
				}
				controller.Driver.ProcessInput(ref m_Axes[i].DrivenAxis(), controller.InputValue, deltaTime);
			}
		}
	}
}
