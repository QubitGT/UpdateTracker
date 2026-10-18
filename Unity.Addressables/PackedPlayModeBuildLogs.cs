using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PackedPlayModeBuildLogs
{
	[Serializable]
	public struct RuntimeBuildLog
	{
		public LogType Type;

		public string Message;

		public RuntimeBuildLog(LogType type, string message)
		{
			Type = type;
			Message = message;
		}
	}

	[SerializeField]
	private List<RuntimeBuildLog> m_RuntimeBuildLogs = new List<RuntimeBuildLog>();

	public List<RuntimeBuildLog> RuntimeBuildLogs
	{
		get
		{
			return m_RuntimeBuildLogs;
		}
		set
		{
			m_RuntimeBuildLogs = value;
		}
	}
}
