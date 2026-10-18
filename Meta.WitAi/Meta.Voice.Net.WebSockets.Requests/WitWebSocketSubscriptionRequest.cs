using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketSubscriptionRequest : WitWebSocketJsonRequest
{
	public WitWebSocketSubscriptionType SubscriptionType { get; }

	public WitWebSocketSubscriptionRequest(string topicId, WitWebSocketSubscriptionType subscriptionType)
		: base(GetSubscriptionNode(topicId, subscriptionType))
	{
		base.TopicId = topicId;
		SubscriptionType = subscriptionType;
	}

	public override string ToString()
	{
		return $"{base.ToString()}\nSubscription Type: {SubscriptionType}";
	}

	private static WitResponseNode GetSubscriptionNode(string topicId, WitWebSocketSubscriptionType subscriptionType)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		WitResponseClass witResponseClass2 = new WitResponseClass();
		WitResponseClass value = new WitResponseClass { ["topic"] = topicId };
		witResponseClass2[GetSubscriptionNodeKey(subscriptionType)] = value;
		witResponseClass["data"] = witResponseClass2;
		return witResponseClass;
	}

	private static string GetSubscriptionNodeKey(WitWebSocketSubscriptionType subscriptionType)
	{
		return subscriptionType switch
		{
			WitWebSocketSubscriptionType.Subscribe => "subscribe", 
			WitWebSocketSubscriptionType.Unsubscribe => "unsubscribe", 
			_ => string.Empty, 
		};
	}
}
