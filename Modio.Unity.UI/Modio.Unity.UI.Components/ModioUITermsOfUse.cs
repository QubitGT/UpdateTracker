using System.Threading.Tasks;
using Modio.Extensions;
using Modio.Platforms;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUITermsOfUse : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _termsOfUseText;

	[SerializeField]
	private TMP_Text _agreeText;

	[SerializeField]
	private TMP_Text _disagreeText;

	[SerializeField]
	private TMP_Text _termsOfUseLinkButtonText;

	[SerializeField]
	private TMP_Text _privacyPolicyLinkButtonText;

	private static TermsOfUse _termsOfUse;

	private static IWebBrowserHandler _browserHandler;

	public void Start()
	{
		if (_termsOfUse != null)
		{
			ApplyTermsOfUse();
		}
		else
		{
			ModioClient.OnInitialized += OnPluginReady;
		}
	}

	private void OnPluginReady()
	{
		GetTermsOfUse().ForgetTaskSafely();
		_browserHandler = ModioServices.Resolve<IWebBrowserHandler>();
	}

	private async Task GetTermsOfUse()
	{
		var (error, termsOfUse) = await TermsOfUse.Get();
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log("Error getting terms of use: " + error.GetMessage());
			}
		}
		else
		{
			_termsOfUse = termsOfUse;
			ApplyTermsOfUse();
		}
	}

	private void ApplyTermsOfUse()
	{
		if (_termsOfUse == null)
		{
			ModioLog.Error?.Log("Attempted to apply terms of use before they were loaded");
			return;
		}
		if (_termsOfUseText != null)
		{
			_termsOfUseText.text = _termsOfUse.TermsText;
		}
		if (_agreeText != null)
		{
			_agreeText.text = _termsOfUse.AgreeText;
		}
		if (_disagreeText != null)
		{
			_disagreeText.text = _termsOfUse.DisagreeText;
		}
		if (_termsOfUseLinkButtonText != null)
		{
			_termsOfUseLinkButtonText.text = GetLinkButtonText(LinkType.Terms);
		}
		if (_privacyPolicyLinkButtonText != null)
		{
			_privacyPolicyLinkButtonText.text = GetLinkButtonText(LinkType.Privacy);
		}
	}

	public void HyperLinkToTOS()
	{
		HyperlinkTo(LinkType.Terms, "https://mod.io/terms");
	}

	public void HyperLinkToPrivacyPolicy()
	{
		HyperlinkTo(LinkType.Privacy, "https://mod.io/privacy");
	}

	public void HyperLinkToRefundPolicy()
	{
		HyperlinkTo(LinkType.Refund, "https://mod.io/refund");
	}

	private string GetLinkButtonText(LinkType type)
	{
		if (_termsOfUse == null)
		{
			ModioLog.Error?.Log($"Attempted to get button text for {type} before terms of use loaded");
			return type.ToString();
		}
		TermsOfUseLink[] links = _termsOfUse.Links;
		for (int i = 0; i < links.Length; i++)
		{
			TermsOfUseLink termsOfUseLink = links[i];
			if (termsOfUseLink.type == type)
			{
				return termsOfUseLink.text;
			}
		}
		ModioLog.Error?.Log($"Couldn't find TermsOfUseLink button text for {type}");
		return type.ToString();
	}

	private static void HyperlinkTo(LinkType type, string fallbackLink)
	{
		string text = null;
		if (_termsOfUse == null)
		{
			Debug.LogError($"Attempted to open hyperlink to {type} before terms of use loaded. Using fallback");
		}
		else
		{
			text = _termsOfUse.GetLink(type).url;
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			text = fallbackLink;
		}
		_browserHandler?.OpenUrl(text);
	}
}
