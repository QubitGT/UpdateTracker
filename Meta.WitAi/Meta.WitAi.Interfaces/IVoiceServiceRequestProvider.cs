using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;

namespace Meta.WitAi.Interfaces;

public interface IVoiceServiceRequestProvider
{
	VoiceServiceRequest CreateRequest(WitRuntimeConfiguration requestSettings, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);
}
