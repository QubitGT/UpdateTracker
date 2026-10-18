using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun;

public class PhotonStream
{
	private List<object> writeData;

	private object[] readData;

	private int currentItem;

	public bool IsWriting { get; private set; }

	public bool IsReading => !IsWriting;

	public int Count
	{
		get
		{
			if (!IsWriting)
			{
				return readData.Length;
			}
			return writeData.Count;
		}
	}

	public PhotonStream(bool write, object[] incomingData)
	{
		IsWriting = write;
		if (!write && incomingData != null)
		{
			readData = incomingData;
		}
	}

	public void SetReadStream(object[] incomingData, int pos = 0)
	{
		readData = incomingData;
		currentItem = pos;
		IsWriting = false;
	}

	internal void SetWriteStream(List<object> newWriteData, int pos = 0)
	{
		if (pos != newWriteData.Count)
		{
			throw new Exception("SetWriteStream failed, because count does not match position value. pos: " + pos + " newWriteData.Count:" + newWriteData.Count);
		}
		writeData = newWriteData;
		currentItem = pos;
		IsWriting = true;
	}

	internal List<object> GetWriteStream()
	{
		return writeData;
	}

	[Obsolete("Either SET the writeData with an empty List or use Clear().")]
	internal void ResetWriteStream()
	{
		writeData.Clear();
	}

	public object ReceiveNext()
	{
		if (IsWriting)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		object result = readData[currentItem];
		currentItem++;
		return result;
	}

	public object PeekNext()
	{
		if (IsWriting)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		return readData[currentItem];
	}

	public void SendNext(object obj)
	{
		if (!IsWriting)
		{
			Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
		}
		else
		{
			writeData.Add(obj);
		}
	}

	[Obsolete("writeData is a list now. Use and re-use it directly.")]
	public bool CopyToListAndClear(List<object> target)
	{
		if (!IsWriting)
		{
			return false;
		}
		target.AddRange(writeData);
		writeData.Clear();
		return true;
	}

	public object[] ToArray()
	{
		if (!IsWriting)
		{
			return readData;
		}
		return writeData.ToArray();
	}

	public void Serialize(ref bool myBool)
	{
		if (IsWriting)
		{
			writeData.Add(myBool);
		}
		else if (readData.Length > currentItem)
		{
			myBool = (bool)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref int myInt)
	{
		if (IsWriting)
		{
			writeData.Add(myInt);
		}
		else if (readData.Length > currentItem)
		{
			myInt = (int)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref string value)
	{
		if (IsWriting)
		{
			writeData.Add(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (string)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref char value)
	{
		if (IsWriting)
		{
			writeData.Add(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (char)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref short value)
	{
		if (IsWriting)
		{
			writeData.Add(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (short)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref float obj)
	{
		if (IsWriting)
		{
			writeData.Add(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (float)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Player obj)
	{
		if (IsWriting)
		{
			writeData.Add(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Player)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Vector3 obj)
	{
		if (IsWriting)
		{
			writeData.Add(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Vector3)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Vector2 obj)
	{
		if (IsWriting)
		{
			writeData.Add(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Vector2)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Quaternion obj)
	{
		if (IsWriting)
		{
			writeData.Add(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Quaternion)readData[currentItem];
			currentItem++;
		}
	}
}
