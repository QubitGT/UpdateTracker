using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Center.Common;

[Serializable]
public class AnsweredQuestion
{
	public string QuestionId;

	public List<string> Answers;
}
