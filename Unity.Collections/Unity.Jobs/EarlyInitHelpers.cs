using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Jobs;

public class EarlyInitHelpers
{
	public delegate void EarlyInitFunction();

	private static List<EarlyInitFunction> s_PendingDelegates;

	static EarlyInitHelpers()
	{
		FlushEarlyInits();
	}

	public static void FlushEarlyInits()
	{
		while (s_PendingDelegates != null)
		{
			List<EarlyInitFunction> list = s_PendingDelegates;
			s_PendingDelegates = null;
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					list[i]();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	public static void AddEarlyInitFunction(EarlyInitFunction func)
	{
		if (s_PendingDelegates == null)
		{
			s_PendingDelegates = new List<EarlyInitFunction>();
		}
		s_PendingDelegates.Add(func);
	}

	public static void JobReflectionDataCreationFailed(Exception ex)
	{
		Debug.LogError("Failed to create job reflection data. Please refer to callstack of exception for information on which job could not produce its reflection data.");
		Debug.LogException(ex);
	}
}
