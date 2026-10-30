using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts;

public class ChristmasTree : NetworkComponent
{
	public GameObject hangers;

	public GameObject lights;

	public GameObject topOrnament;

	public float spinSpeed = 60f;

	private readonly List<AttachPoint> attachPointsList = new List<AttachPoint>();

	private MeshRenderer[] lightRenderers;

	private bool wasActive;

	private bool isActive;

	private bool spinTheTop;

	[SerializeField]
	private Material lightsOffMaterial;

	[SerializeField]
	private Material[] lightsOnMaterials;

	protected override void Awake()
	{
		base.Awake();
		AttachPoint[] componentsInChildren = hangers.GetComponentsInChildren<AttachPoint>();
		foreach (AttachPoint attachPoint in componentsInChildren)
		{
			attachPointsList.Add(attachPoint);
			attachPoint.onHookedChanged = (UnityAction)Delegate.Combine(attachPoint.onHookedChanged, new UnityAction(UpdateHangers));
		}
		lightRenderers = lights.GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = lightRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = lightsOffMaterial;
		}
		wasActive = false;
		isActive = false;
	}

	private void Update()
	{
		if (spinTheTop && (bool)topOrnament)
		{
			topOrnament.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
		}
	}

	private void OnDestroy()
	{
		foreach (AttachPoint attachPoints in attachPointsList)
		{
			attachPoints.onHookedChanged = (UnityAction)Delegate.Remove(attachPoints.onHookedChanged, new UnityAction(UpdateHangers));
		}
		attachPointsList.Clear();
	}

	private void UpdateHangers()
	{
		if (attachPointsList.Count == 0)
		{
			return;
		}
		foreach (AttachPoint attachPoints in attachPointsList)
		{
			if (attachPoints.IsHooked())
			{
				if (base.IsMine)
				{
					updateLight(enable: true);
				}
				return;
			}
		}
		if (base.IsMine)
		{
			updateLight(enable: false);
		}
	}

	private void updateLight(bool enable)
	{
		isActive = enable;
		for (int i = 0; i < lightRenderers.Length; i++)
		{
			lightRenderers[i].material = (enable ? lightsOnMaterials[i % lightsOnMaterials.Length] : lightsOffMaterial);
		}
		spinTheTop = enable;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender.IsMasterClient)
		{
			stream.SendNext(isActive);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender.IsMasterClient)
		{
			wasActive = isActive;
			isActive = (bool)stream.ReceiveNext();
			if (wasActive != isActive)
			{
				updateLight(isActive);
			}
		}
	}
}
