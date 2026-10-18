using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Voxels;

[NetworkBehaviourWeaved(0)]
public class VoxelScoreBoard : NetworkComponent
{
	[Serializable]
	public struct VoxelScoreEntry
	{
		public int actorNumber;

		public int amount1;

		public int amount2;

		public int amount3;

		public VoxelScoreEntry(int actorNumber)
		{
			this.actorNumber = actorNumber;
			amount1 = 0;
			amount2 = 0;
			amount3 = 0;
		}

		public void Add(int[] resources)
		{
			amount1 += resources[0];
			if (resources.Length != 1)
			{
				amount2 += resources[1];
				if (resources.Length != 2)
				{
					amount3 += resources[2];
				}
			}
		}
	}

	[SerializeField]
	private VoxelMaterialSet materialSet;

	[SerializeField]
	private TMP_Text text;

	private string[] _materialNames;

	private List<VoxelScoreEntry> _entries = new List<VoxelScoreEntry>();

	private StringBuilder sb = new StringBuilder(220);

	public int lineLength = 50;

	public int nameWidth = 15;

	public int columnWidth = 10;

	public int columnOffset = -4;

	private new void Start()
	{
		_materialNames = new string[materialSet.Materials.Length];
		for (int i = 0; i < _materialNames.Length; i++)
		{
			_materialNames[i] = materialSet.Materials[i].name;
		}
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if (_entries.Count == 0)
		{
			text.text = "";
			return;
		}
		sb.Clear();
		int num = lineLength - (nameWidth + _materialNames.Length * columnWidth);
		AddText("    NAME", nameWidth + num);
		string[] materialNames = _materialNames;
		foreach (string text2 in materialNames)
		{
			AddText(text2, columnWidth);
		}
		sb.Append("\n");
		foreach (VoxelScoreEntry entry in _entries)
		{
			if (Utils.PlayerInRoom(entry.actorNumber))
			{
				string text3 = NetPlayer.Get(entry.actorNumber)?.NickName ?? "UNKNOWN";
				AddText(text3, nameWidth + num - columnOffset);
				AddTextRight((entry.amount1 / 100).ToString(), columnWidth);
				if (_materialNames.Length > 1)
				{
					AddTextRight((entry.amount2 / 100).ToString(), columnWidth);
				}
				if (_materialNames.Length > 2)
				{
					AddTextRight((entry.amount3 / 100).ToString(), columnWidth);
				}
				sb.Append("\n");
			}
		}
		text.alignment = TextAlignmentOptions.TopLeft;
		text.text = sb.ToString();
		void AddText(string text, int length)
		{
			if (text.Length > length)
			{
				sb.Append(text.Substring(0, length));
			}
			else
			{
				sb.Append(text);
				for (int k = 0; k < length - text.Length; k++)
				{
					sb.Append(" ");
				}
			}
		}
		void AddTextRight(string text, int length)
		{
			if (text.Length > length)
			{
				sb.Append(text.Substring(0, length));
			}
			else
			{
				for (int j = 0; j < length - text.Length; j++)
				{
					sb.Append(" ");
				}
				sb.Append(text);
			}
		}
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(_entries.Count);
		foreach (VoxelScoreEntry entry in _entries)
		{
			stream.SendNext(entry.actorNumber);
			stream.SendNext(entry.amount1);
			stream.SendNext(entry.amount2);
			stream.SendNext(entry.amount3);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient && !info.Sender.IsLocal)
		{
			return;
		}
		int num = (int)stream.ReceiveNext();
		if (num < 0 || num > 20)
		{
			return;
		}
		if (_entries.Count > num)
		{
			_entries.RemoveRange(num, _entries.Count - num);
		}
		for (int i = 0; i < num; i++)
		{
			VoxelScoreEntry voxelScoreEntry = default(VoxelScoreEntry);
			voxelScoreEntry.actorNumber = (int)stream.ReceiveNext();
			voxelScoreEntry.amount1 = (int)stream.ReceiveNext();
			voxelScoreEntry.amount2 = (int)stream.ReceiveNext();
			voxelScoreEntry.amount3 = (int)stream.ReceiveNext();
			VoxelScoreEntry voxelScoreEntry2 = voxelScoreEntry;
			if (i < _entries.Count)
			{
				_entries[i] = voxelScoreEntry2;
			}
			else
			{
				_entries.Add(voxelScoreEntry2);
			}
		}
		if (_materialNames != null)
		{
			UpdateDisplay();
		}
	}

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		VoxelEvents.OnResourcesMinedAuthority += OnResourcesMined;
		RoomSystem.JoinedRoomEvent += new Action(OnJoinedRoom);
		RoomSystem.LeftRoomEvent += new Action(OnLeftRoom);
		RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(OnPlayerEnteredRoom);
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(OnPlayerLeftRoom);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		VoxelEvents.OnResourcesMinedAuthority -= OnResourcesMined;
		RoomSystem.JoinedRoomEvent -= new Action(OnJoinedRoom);
		RoomSystem.LeftRoomEvent -= new Action(OnLeftRoom);
		RoomSystem.PlayerJoinedEvent -= new Action<NetPlayer>(OnPlayerEnteredRoom);
		RoomSystem.PlayerLeftEvent -= new Action<NetPlayer>(OnPlayerLeftRoom);
	}

	private void OnJoinedRoom()
	{
		if (base.IsLocallyOwned)
		{
			int scoreLine = GetScoreLine(PhotonUtils.LocalNetPlayer);
			for (int i = 0; i < _entries.Count; i++)
			{
				if (i != scoreLine)
				{
					_entries.RemoveAt(i--);
				}
			}
			NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
			foreach (NetPlayer player in allNetPlayers)
			{
				GetScoreLine(player);
			}
		}
		UpdateDisplay();
	}

	private void OnLeftRoom()
	{
		_entries.Clear();
		UpdateDisplay();
	}

	private void OnPlayerEnteredRoom(NetPlayer player)
	{
		if (base.IsLocallyOwned)
		{
			GetScoreLine(player);
		}
		UpdateDisplay();
	}

	private void OnPlayerLeftRoom(NetPlayer player)
	{
		if (base.IsLocallyOwned)
		{
			for (int i = 0; i < _entries.Count; i++)
			{
				if (_entries[i].actorNumber == player.ActorNumber)
				{
					_entries.RemoveAt(i);
					return;
				}
			}
		}
		UpdateDisplay();
	}

	private int GetScoreLine(NetPlayer player)
	{
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].actorNumber == player.ActorNumber)
			{
				return i;
			}
		}
		VoxelScoreEntry item = new VoxelScoreEntry(player.ActorNumber);
		_entries.Add(item);
		return _entries.Count - 1;
	}

	private void OnResourcesMined(NetPlayer player, VoxelWorld world, int[] resources)
	{
		if (!(world.MaterialSet != materialSet))
		{
			int scoreLine = GetScoreLine(player);
			VoxelScoreEntry value = _entries[scoreLine];
			value.Add(resources);
			_entries[scoreLine] = value;
			UpdateDisplay();
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
		base.CopyBackingFieldsToState(P_0);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}
}
