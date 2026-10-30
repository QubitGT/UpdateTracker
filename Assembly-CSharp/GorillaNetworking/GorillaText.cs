using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaNetworking;

[Serializable]
public class GorillaText
{
	public string currentText;

	private string originalText = string.Empty;

	private StringBuilder stringBuilder = new StringBuilder();

	private bool modified;

	private bool failedState;

	private Material[] originalMaterials;

	private Material failureMaterial;

	private Material[] failureMaterials;

	internal Material[] currentMaterials;

	private UnityEvent<string> updateTextCallback;

	private UnityEvent<Material[]> updateMaterialCallback;

	public void Initialize(Material[] originalMaterials, Material failureMaterial, UnityEvent<string> callback = null, UnityEvent<Material[]> materialCallback = null)
	{
		this.failureMaterial = failureMaterial;
		this.originalMaterials = originalMaterials;
		failureMaterials = null;
		currentMaterials = originalMaterials;
		updateTextCallback = callback;
		updateMaterialCallback = materialCallback;
		GorillaTextManager.RegisterText(this);
	}

	public void InvokeIfUpdated()
	{
		if (modified)
		{
			modified = false;
			if (currentText == null || !stringBuilder.Equals(currentText.AsSpan()))
			{
				currentText = stringBuilder.ToString();
				updateTextCallback?.Invoke(currentText);
			}
		}
	}

	public void EnableFailedState(string failText)
	{
		if (!failedState)
		{
			originalText = currentText;
			failedState = true;
		}
		updateTextCallback?.Invoke(failText);
		currentText = failText;
		if (failureMaterials == null)
		{
			failureMaterials = (Material[])originalMaterials.Clone();
			failureMaterials[0] = failureMaterial;
		}
		currentMaterials = failureMaterials;
		updateMaterialCallback?.Invoke(currentMaterials);
	}

	public void DisableFailedState()
	{
		failedState = false;
		updateTextCallback?.Invoke(originalText);
		currentText = originalText;
		currentMaterials = originalMaterials;
		updateMaterialCallback?.Invoke(currentMaterials);
	}

	public void Append(string str)
	{
		modified = true;
		stringBuilder.Append(str);
	}

	public void Set(string str)
	{
		modified = true;
		stringBuilder.Clear();
		stringBuilder.Append(str);
	}
}
