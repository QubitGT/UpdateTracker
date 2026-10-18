using System;
using Fusion;
using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

[NetworkBehaviourWeaved(904)]
public class AvatarBehaviourFusion : NetworkBehaviour, IAvatarBehaviour
{
	private const float LERP_TIME = 0.5f;

	private const int AvatarDataStreamMaxCapacity = 900;

	private Transform _cameraRig;

	private byte[] _buffer;

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("OculusId", 0, 2)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private ulong _OculusId;

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("LocalAvatarIndex", 2, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private int _LocalAvatarIndex;

	[WeaverGenerated]
	[DefaultForProperty("AvatarDataStreamLength", 3, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private int _AvatarDataStreamLength;

	[WeaverGenerated]
	[DefaultForProperty("AvatarDataStream", 4, 900)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private byte[] _AvatarDataStream;

	[Networked]
	[OnChangedRender("OnAvatarIdChanged")]
	[NetworkedWeaved(0, 2)]
	public unsafe ulong OculusId
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.OculusId. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(ulong*)((byte*)base.Ptr + 0);
		}
		set
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.OculusId. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(ulong*)((byte*)base.Ptr + 0) = value;
		}
	}

	[Networked]
	[OnChangedRender("OnAvatarIdChanged")]
	[NetworkedWeaved(2, 1)]
	public unsafe int LocalAvatarIndex
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.LocalAvatarIndex. Networked properties can only be accessed when Spawned() has been called.");
			}
			return base.Ptr[2];
		}
		set
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.LocalAvatarIndex. Networked properties can only be accessed when Spawned() has been called.");
			}
			base.Ptr[2] = value;
		}
	}

	[Networked]
	[NetworkedWeaved(3, 1)]
	private unsafe int AvatarDataStreamLength
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.AvatarDataStreamLength. Networked properties can only be accessed when Spawned() has been called.");
			}
			return base.Ptr[3];
		}
		set
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.AvatarDataStreamLength. Networked properties can only be accessed when Spawned() has been called.");
			}
			base.Ptr[3] = value;
		}
	}

	[Networked]
	[Capacity(900)]
	[OnChangedRender("OnAvatarDataStreamChanged")]
	[NetworkedWeaved(4, 900)]
	[NetworkedWeavedArray(900, 1, typeof(global::Fusion.ElementReaderWriterByte))]
	private unsafe NetworkArray<byte> AvatarDataStream
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AvatarBehaviourFusion.AvatarDataStream. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkArray<byte>((byte*)(base.Ptr + 4), 900, global::Fusion.ElementReaderWriterByte.GetInstance());
		}
	}

	public override void Spawned()
	{
		if ((bool)OVRManager.instance)
		{
			_cameraRig = OVRManager.instance.GetComponentInChildren<OVRCameraRig>().transform;
		}
	}

	private void OnAvatarIdChanged()
	{
	}

	private void OnAvatarDataStreamChanged()
	{
	}

	public override void FixedUpdateNetwork()
	{
		if (base.Object.HasStateAuthority && !(_cameraRig == null))
		{
			Transform transform = base.transform;
			base.transform.position = Vector3.Lerp(transform.position, _cameraRig.position, 0.5f);
			base.transform.rotation = Quaternion.Lerp(transform.rotation, _cameraRig.rotation, 0.5f);
		}
	}

	public void ReceiveStreamData(byte[] bytes)
	{
		if (bytes.Length > 900)
		{
			Debug.LogError(string.Format("[{0}] Cannot send a stream data of length {1} greater than the max capacity of {2}", "AvatarBehaviourFusion", bytes.Length, 900));
			return;
		}
		AvatarDataStreamLength = bytes.Length;
		AvatarDataStream.CopyFrom(bytes, 0, bytes.Length);
	}

	bool IAvatarBehaviour.get_HasInputAuthority()
	{
		return base.HasInputAuthority;
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
		OculusId = _OculusId;
		LocalAvatarIndex = _LocalAvatarIndex;
		AvatarDataStreamLength = _AvatarDataStreamLength;
		NetworkBehaviourUtils.InitializeNetworkArray(AvatarDataStream, _AvatarDataStream, "AvatarDataStream");
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		_OculusId = OculusId;
		_LocalAvatarIndex = LocalAvatarIndex;
		_AvatarDataStreamLength = AvatarDataStreamLength;
		NetworkBehaviourUtils.CopyFromNetworkArray(AvatarDataStream, ref _AvatarDataStream);
	}
}
