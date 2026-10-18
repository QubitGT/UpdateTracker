using System.Threading.Tasks;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.WitAi;

public class Wit : VoiceService, IWitRuntimeConfigProvider, IWitRuntimeConfigSetter
{
	[SerializeField]
	private WitRuntimeConfiguration witRuntimeConfiguration;

	private WitService witService;

	public WitRuntimeConfiguration RuntimeConfiguration
	{
		get
		{
			return witRuntimeConfiguration;
		}
		set
		{
			witRuntimeConfiguration = value;
		}
	}

	public override bool Active
	{
		get
		{
			if (!base.Active)
			{
				if (null != witService)
				{
					return witService.Active;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsRequestActive
	{
		get
		{
			if (!base.IsRequestActive)
			{
				if (null != witService)
				{
					return witService.IsRequestActive;
				}
				return false;
			}
			return true;
		}
	}

	public override ITranscriptionProvider TranscriptionProvider
	{
		get
		{
			return witService.TranscriptionProvider;
		}
		set
		{
			witService.TranscriptionProvider = value;
		}
	}

	public override bool MicActive
	{
		get
		{
			if (null != witService)
			{
				return witService.MicActive;
			}
			return false;
		}
	}

	protected override bool ShouldSendMicData
	{
		get
		{
			if (!witRuntimeConfiguration.sendAudioToWit)
			{
				return TranscriptionProvider == null;
			}
			return true;
		}
	}

	public override string GetSendError()
	{
		if (!(RuntimeConfiguration?.witConfiguration))
		{
			return "Your " + GetType().Name + " \"" + base.gameObject.name + "\" does not have a wit configuration assigned.   Voice interactions are not possible without the configuration.";
		}
		if (string.IsNullOrEmpty(RuntimeConfiguration.witConfiguration.GetClientAccessToken()))
		{
			return "The configuration \"" + RuntimeConfiguration.witConfiguration.name + "\" is not setup with a valid client access token.   Voice interactions are not possible without the token.";
		}
		return base.GetSendError();
	}

	public override string GetActivateAudioError()
	{
		if (!AudioBuffer.Instance.IsInputAvailable)
		{
			return "No Microphone(s)/recording devices found.  You will be unable to capture audio on this device.";
		}
		return base.GetActivateAudioError();
	}

	public override Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return witService.Activate(text, requestOptions, requestEvents);
	}

	public override VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return witService.Activate(requestOptions, requestEvents);
	}

	public override VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return witService.ActivateImmediately(requestOptions, requestEvents);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		witService.Deactivate();
	}

	public override void DeactivateAndAbortRequest()
	{
		base.DeactivateAndAbortRequest();
		witService.DeactivateAndAbortRequest();
	}

	protected override void Awake()
	{
		base.Awake();
		witService = base.gameObject.AddComponent<WitService>();
		witService.VoiceEventProvider = this;
		witService.TelemetryEventsProvider = this;
		witService.ConfigurationProvider = this;
	}
}
