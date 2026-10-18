using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

internal class UpdateTracker
{
	public enum UpdateClock
	{
		Fixed = 1,
		Late
	}

	private class UpdateStatus
	{
		private const int kWindowSize = 30;

		private int m_WindowStart;

		private int m_NumWindowLateUpdateMoves;

		private int m_NumWindowFixedUpdateMoves;

		private int m_NumWindows;

		private int m_LastFrameUpdated;

		private Matrix4x4 m_LastPos;

		public UpdateClock PreferredUpdate { get; private set; }

		public UpdateStatus(int currentFrame, Matrix4x4 pos)
		{
			m_WindowStart = currentFrame;
			m_LastFrameUpdated = Time.frameCount;
			PreferredUpdate = UpdateClock.Late;
			m_LastPos = pos;
		}

		public void OnUpdate(int currentFrame, UpdateClock currentClock, Matrix4x4 pos)
		{
			if (!(m_LastPos == pos))
			{
				if (currentClock == UpdateClock.Late)
				{
					m_NumWindowLateUpdateMoves++;
				}
				else if (m_LastFrameUpdated != currentFrame)
				{
					m_NumWindowFixedUpdateMoves++;
				}
				m_LastPos = pos;
				UpdateClock preferredUpdate = UpdateClock.Late;
				if (m_NumWindowFixedUpdateMoves > 3 && m_NumWindowLateUpdateMoves < m_NumWindowFixedUpdateMoves / 3)
				{
					preferredUpdate = UpdateClock.Fixed;
				}
				if (m_NumWindows == 0)
				{
					PreferredUpdate = preferredUpdate;
				}
				if (m_WindowStart + 30 <= currentFrame)
				{
					PreferredUpdate = preferredUpdate;
					m_NumWindows++;
					m_WindowStart = currentFrame;
					m_NumWindowLateUpdateMoves = ((PreferredUpdate == UpdateClock.Late) ? 1 : 0);
					m_NumWindowFixedUpdateMoves = ((PreferredUpdate == UpdateClock.Fixed) ? 1 : 0);
				}
			}
		}
	}

	private static Dictionary<Transform, UpdateStatus> s_UpdateStatus = new Dictionary<Transform, UpdateStatus>();

	private static List<Transform> s_ToDelete = new List<Transform>();

	private static object s_LastUpdateContext;

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeModule()
	{
		s_UpdateStatus.Clear();
	}

	private static void UpdateTargets(UpdateClock currentClock)
	{
		int frameCount = Time.frameCount;
		Dictionary<Transform, UpdateStatus>.Enumerator enumerator = s_UpdateStatus.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Transform, UpdateStatus> current = enumerator.Current;
			if (current.Key == null)
			{
				s_ToDelete.Add(current.Key);
			}
			else
			{
				current.Value.OnUpdate(frameCount, currentClock, current.Key.localToWorldMatrix);
			}
		}
		for (int num = s_ToDelete.Count - 1; num >= 0; num--)
		{
			s_UpdateStatus.Remove(s_ToDelete[num]);
		}
		s_ToDelete.Clear();
		enumerator.Dispose();
	}

	public static UpdateClock GetPreferredUpdate(Transform target)
	{
		if (Application.isPlaying && target != null)
		{
			if (s_UpdateStatus.TryGetValue(target, out var value))
			{
				return value.PreferredUpdate;
			}
			value = new UpdateStatus(Time.frameCount, target.localToWorldMatrix);
			s_UpdateStatus.Add(target, value);
		}
		return UpdateClock.Late;
	}

	public static void OnUpdate(UpdateClock currentClock, object context)
	{
		if (s_LastUpdateContext == null || s_LastUpdateContext == context)
		{
			s_LastUpdateContext = context;
			UpdateTargets(currentClock);
		}
	}

	public static void ForgetContext(object context)
	{
		if (s_LastUpdateContext == context)
		{
			s_LastUpdateContext = null;
		}
	}
}
