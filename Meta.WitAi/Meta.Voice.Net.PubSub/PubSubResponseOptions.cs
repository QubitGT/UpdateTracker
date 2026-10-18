using System;
using UnityEngine;

namespace Meta.Voice.Net.PubSub;

[Serializable]
public struct PubSubResponseOptions
{
	[Header("Responses returned from audio interactions.")]
	public bool transcriptionResponses;

	[Header("Responses returned from composer results.")]
	public bool composerResponses;

	public bool Equals(PubSubResponseOptions other)
	{
		if (transcriptionResponses == other.transcriptionResponses)
		{
			return composerResponses == other.composerResponses;
		}
		return false;
	}
}
