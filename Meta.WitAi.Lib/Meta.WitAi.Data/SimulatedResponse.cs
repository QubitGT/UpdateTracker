using System;
using System.Collections.Generic;

namespace Meta.WitAi.Data;

[Serializable]
public class SimulatedResponse
{
	public int code;

	public List<SimulatedResponseMessage> messages = new List<SimulatedResponseMessage>();

	public string responseDescription;
}
