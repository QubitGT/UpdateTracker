using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Multiplayer.Center.Common;

[Serializable]
public class AnswerData
{
	public List<AnsweredQuestion> Answers = new List<AnsweredQuestion>();

	public AnswerData Clone()
	{
		return JsonUtility.FromJson(JsonUtility.ToJson(this), typeof(AnswerData)) as AnswerData;
	}
}
