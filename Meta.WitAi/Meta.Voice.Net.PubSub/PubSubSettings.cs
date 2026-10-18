using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Voice.Net.PubSub;

[Serializable]
public struct PubSubSettings
{
	[Tooltip("The unique pubsub topic id to publish and/or subscribe to")]
	public string PubSubTopicId;

	[Tooltip("Toggles for publishing per response type.")]
	public PubSubResponseOptions PublishOptions;

	[Tooltip("Toggles for subscribing per response type.")]
	public PubSubResponseOptions SubscribeOptions;

	public PubSubSettings(string pubSubTopicId = "")
	{
		PubSubTopicId = pubSubTopicId;
		PublishOptions = GetDefaultOptions();
		SubscribeOptions = GetDefaultOptions();
	}

	public bool Equals(PubSubSettings other)
	{
		if (string.Equals(PubSubTopicId, other.PubSubTopicId) && PublishOptions.Equals(other.PublishOptions))
		{
			return SubscribeOptions.Equals(other.SubscribeOptions);
		}
		return false;
	}

	public void GetPublishTopics(Dictionary<string, string> topics)
	{
		GetTopics(topics, PubSubTopicId, PublishOptions);
	}

	public Dictionary<string, string> GetPublishTopics()
	{
		return GetTopics(PubSubTopicId, PublishOptions);
	}

	public void GetSubscribeTopics(Dictionary<string, string> topics)
	{
		GetTopics(topics, PubSubTopicId, SubscribeOptions);
	}

	public Dictionary<string, string> GetSubscribeTopics()
	{
		return GetTopics(PubSubTopicId, SubscribeOptions);
	}

	public static void GetTopics(Dictionary<string, string> topics, string topicId, PubSubResponseOptions options)
	{
		if (!string.IsNullOrEmpty(topicId))
		{
			if (options.transcriptionResponses)
			{
				SetTopicKey(topics, topicId, "1", "_ASR");
			}
			if (options.composerResponses)
			{
				SetTopicKey(topics, topicId, "2", "_COMP");
			}
		}
	}

	public static Dictionary<string, string> GetTopics(string topicId, PubSubResponseOptions options)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		GetTopics(dictionary, topicId, options);
		return dictionary;
	}

	private static void SetTopicKey(Dictionary<string, string> topics, string topicId, string key, string append)
	{
		topics[key] = topicId + append;
	}

	public bool IsSubscribedTopicId(string topicId)
	{
		if (string.IsNullOrEmpty(PubSubTopicId) || string.IsNullOrEmpty(topicId))
		{
			return false;
		}
		if (SubscribeOptions.transcriptionResponses && topicId.Equals(PubSubTopicId + "_ASR"))
		{
			return true;
		}
		if (SubscribeOptions.composerResponses && topicId.Equals(PubSubTopicId + "_COMP"))
		{
			return true;
		}
		return false;
	}

	private static PubSubResponseOptions GetDefaultOptions()
	{
		PubSubResponseOptions result = default(PubSubResponseOptions);
		result.transcriptionResponses = true;
		result.composerResponses = true;
		return result;
	}
}
