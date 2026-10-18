using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal class XRUIToolkitPokeHandler
{
	private GameObject m_VisualizersRoot;

	private Transform m_PokePointVisualizer;

	private Transform m_ClosestPointVisualizer;

	private Transform m_RayOriginVisualizer;

	private Transform m_NormalVisualizer;

	private bool m_VisualizersCreated;

	private readonly XRPokeInteractor m_Interactor;

	private bool m_UpdateDepth;

	public bool updateDepth
	{
		get
		{
			return m_UpdateDepth;
		}
		set
		{
			m_UpdateDepth = value;
		}
	}

	public XRUIToolkitPokeHandler(XRPokeInteractor interactor)
	{
		m_Interactor = interactor;
	}

	public void Dispose()
	{
		DestroyVisualizers();
	}

	public void ProcessPokeInteraction(Collider hitCollider, Transform interactableTransform, IXRInteractable interactable, bool useMultiPick, IXRPokeFilter pokeFilter = null)
	{
		if (!interactableTransform.TryGetComponent<UIDocument>(out var component))
		{
			return;
		}
		Transform transform = component.transform;
		Vector3 position = m_Interactor.GetAttachTransform(null).position;
		Vector3 vector = -transform.forward;
		new Plane(vector, transform.position).Raycast(new Ray(position, -vector), out var enter);
		Vector3 vector2 = position - vector * enter;
		Vector3 vector3 = vector2 + vector;
		UpdateVisualizers(position, vector2, vector3, vector, transform);
		VisualElement visualElement = PerformPick(component, vector3, -vector, m_Interactor.pokeWidth, useMultiPick);
		if (visualElement != null)
		{
			XRUIToolkitHandler.UpdateInteractorHitData(m_Interactor, new InteractorHitData
			{
				closestPoint = vector2,
				interactorOrigin = position,
				interactorDirection = vector,
				hitDocument = component
			});
			float num = 1f;
			if (pokeFilter != null && interactable is IXRSelectInteractable interactable2)
			{
				num = pokeFilter.Process(m_Interactor, interactable2, 0f);
			}
			bool isUiSelectInputActive = num > 0.99f;
			if (m_UpdateDepth)
			{
				XRUIToolkitHandler.SetZDepthForInteractor(visualElement, m_Interactor, 20f * (1f - num));
			}
			XRUIToolkitHandler.HandlePointerUpdate(m_Interactor, vector3, Quaternion.LookRotation((vector2 - vector3).normalized), isUiSelectInputActive, shouldReset: false);
		}
		else
		{
			ResetPointerState();
		}
	}

	public void ResetPointerState()
	{
		XRUIToolkitHandler.HandlePointerUpdate(m_Interactor, Vector3.zero, Quaternion.identity, isUiSelectInputActive: false, shouldReset: true);
		if (m_UpdateDepth)
		{
			XRUIToolkitHandler.ClearZDepthForInteractor(m_Interactor);
		}
	}

	private VisualElement PerformPick(UIDocument document, Vector3 center, Vector3 direction, float radius, bool useMultiPick)
	{
		VisualElement visualElement = WorldSpaceInput.Pick3D(document, new Ray(center, direction));
		if (visualElement != null)
		{
			return visualElement;
		}
		if (useMultiPick)
		{
			return PerformMultiPick(document, center, direction, radius);
		}
		return null;
	}

	private VisualElement PerformMultiPick(UIDocument document, Vector3 center, Vector3 direction, float radius)
	{
		Dictionary<VisualElement, float> dictionary = new Dictionary<VisualElement, float>();
		Matrix4x4 localToWorldMatrix = document.transform.localToWorldMatrix;
		Vector3 vector = localToWorldMatrix.GetColumn(0);
		Vector3 vector2 = localToWorldMatrix.GetColumn(1);
		for (int i = 0; i < 4; i++)
		{
			float f = (float)i * (MathF.PI / 2f);
			Vector3 vector3 = vector * (Mathf.Cos(f) * radius) + vector2 * (Mathf.Sin(f) * radius);
			Vector3 origin = center + vector3;
			VisualElement visualElement = WorldSpaceInput.Pick3D(document, new Ray(origin, direction));
			if (visualElement != null)
			{
				float sqrMagnitude = vector3.sqrMagnitude;
				if (!dictionary.TryGetValue(visualElement, out var value) || sqrMagnitude < value)
				{
					dictionary[visualElement] = sqrMagnitude;
				}
			}
		}
		VisualElement result = null;
		float num = float.MaxValue;
		foreach (KeyValuePair<VisualElement, float> item in dictionary)
		{
			if (item.Value < num)
			{
				num = item.Value;
				result = item.Key;
			}
		}
		return result;
	}

	public void UpdateVisualizersState()
	{
		if (m_Interactor.debugVisualizationsEnabled && m_Interactor.isActiveAndEnabled)
		{
			CreateVisualizers();
		}
		else
		{
			DestroyVisualizers();
		}
	}

	private void UpdateVisualizers(Vector3 pokePoint, Vector3 closestPoint, Vector3 rayOrigin, Vector3 normal, Transform parentTransform)
	{
		if (m_Interactor.debugVisualizationsEnabled && m_VisualizersCreated)
		{
			m_PokePointVisualizer.position = pokePoint;
			m_ClosestPointVisualizer.position = closestPoint;
			m_RayOriginVisualizer.position = rayOrigin;
			m_NormalVisualizer.position = closestPoint + normal * 0.025f;
			m_NormalVisualizer.up = normal;
			if (m_VisualizersRoot.transform.parent != parentTransform)
			{
				m_VisualizersRoot.transform.SetParent(parentTransform, worldPositionStays: false);
			}
		}
	}

	private void CreateVisualizers()
	{
		if (!m_VisualizersCreated)
		{
			m_VisualizersRoot = new GameObject("UIPokeVisualizers");
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.name = "PokePoint";
			gameObject.transform.SetParent(m_VisualizersRoot.transform);
			gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			if (gameObject.TryGetComponent<Collider>(out var component))
			{
				Object.Destroy(component);
			}
			gameObject.GetComponent<Renderer>().material.color = Color.blue;
			m_PokePointVisualizer = gameObject.transform;
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject2.name = "ClosestPoint";
			gameObject2.transform.SetParent(m_VisualizersRoot.transform);
			gameObject2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			if (gameObject2.TryGetComponent<Collider>(out var component2))
			{
				Object.Destroy(component2);
			}
			gameObject2.GetComponent<Renderer>().material.color = Color.green;
			m_ClosestPointVisualizer = gameObject2.transform;
			GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject3.name = "RayOrigin";
			gameObject3.transform.SetParent(m_VisualizersRoot.transform);
			gameObject3.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			if (gameObject3.TryGetComponent<Collider>(out var component3))
			{
				Object.Destroy(component3);
			}
			gameObject3.GetComponent<Renderer>().material.color = Color.yellow;
			m_RayOriginVisualizer = gameObject3.transform;
			GameObject gameObject4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			gameObject4.name = "DocumentNormal";
			gameObject4.transform.SetParent(m_VisualizersRoot.transform);
			gameObject4.transform.localScale = new Vector3(0.005f, 0.05f, 0.005f);
			if (gameObject4.TryGetComponent<Collider>(out var component4))
			{
				Object.Destroy(component4);
			}
			gameObject4.GetComponent<Renderer>().material.color = Color.red;
			m_NormalVisualizer = gameObject4.transform;
			m_VisualizersCreated = true;
		}
	}

	private void DestroyVisualizers()
	{
		if (m_VisualizersRoot != null)
		{
			Object.Destroy(m_VisualizersRoot);
			m_VisualizersCreated = false;
			m_PokePointVisualizer = null;
			m_ClosestPointVisualizer = null;
			m_RayOriginVisualizer = null;
			m_NormalVisualizer = null;
		}
	}
}
