using System.Threading.Tasks;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;

namespace Meta.WitAi;

public interface IVoiceActivationHandler
{
	bool Active { get; }

	Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	void Deactivate();

	void DeactivateAndAbortRequest();

	void DeactivateAndAbortRequest(VoiceServiceRequest request);
}
