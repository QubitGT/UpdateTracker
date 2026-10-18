using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;

namespace Modio;

public class TermsOfUse
{
	private static Dictionary<string, TermsOfUse> _termsCache = new Dictionary<string, TermsOfUse>();

	public string TermsText { get; private set; }

	public string AgreeText { get; private set; }

	public string DisagreeText { get; private set; }

	public TermsOfUseLink[] Links { get; private set; }

	public static async Task<(Error error, TermsOfUse result)> Get()
	{
		string langCode = ModioAPI.LanguageCodeResponse;
		if (_termsCache.TryGetValue(langCode, out var value))
		{
			return (error: Error.None, result: value);
		}
		var (error, termsObject) = await ModioAPI.Authentication.Terms();
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error getting Terms of Use: {error}");
			}
			return (error: error, result: null);
		}
		TermsOfUse termsOfUse = ConvertTermsObjectToTermsOfUse(termsObject.Value);
		_termsCache[langCode] = termsOfUse;
		return (error: Error.None, result: termsOfUse);
	}

	public TermsOfUseLink GetLink(LinkType type)
	{
		TermsOfUseLink[] links = Links;
		for (int i = 0; i < links.Length; i++)
		{
			TermsOfUseLink result = links[i];
			if (result.type == type)
			{
				return result;
			}
		}
		ModioLog.Error?.Log("Could not find " + type.ToString() + " link in Terms of Use! The API may have changed!");
		return default(TermsOfUseLink);
	}

	private static TermsOfUse ConvertTermsObjectToTermsOfUse(TermsObject termsObject)
	{
		TermsOfUse termsOfUse = new TermsOfUse();
		termsOfUse.TermsText = termsObject.Plaintext;
		termsOfUse.AgreeText = termsObject.Buttons.Agree.Text;
		termsOfUse.DisagreeText = termsObject.Buttons.Disagree.Text;
		TermsOfUseLink termsOfUseLink = default(TermsOfUseLink);
		termsOfUseLink.type = LinkType.Website;
		termsOfUseLink.text = termsObject.Links.Website.Text;
		termsOfUseLink.url = termsObject.Links.Website.Url;
		termsOfUseLink.required = termsObject.Links.Website.Required;
		TermsOfUseLink termsOfUseLink2 = termsOfUseLink;
		termsOfUseLink = default(TermsOfUseLink);
		termsOfUseLink.type = LinkType.Terms;
		termsOfUseLink.text = termsObject.Links.Terms.Text;
		termsOfUseLink.url = termsObject.Links.Terms.Url;
		termsOfUseLink.required = termsObject.Links.Terms.Required;
		TermsOfUseLink termsOfUseLink3 = termsOfUseLink;
		termsOfUseLink = default(TermsOfUseLink);
		termsOfUseLink.type = LinkType.Privacy;
		termsOfUseLink.text = termsObject.Links.Privacy.Text;
		termsOfUseLink.url = termsObject.Links.Privacy.Url;
		termsOfUseLink.required = termsObject.Links.Privacy.Required;
		TermsOfUseLink termsOfUseLink4 = termsOfUseLink;
		termsOfUseLink = default(TermsOfUseLink);
		termsOfUseLink.type = LinkType.Manage;
		termsOfUseLink.text = termsObject.Links.Manage.Text;
		termsOfUseLink.url = termsObject.Links.Manage.Url;
		termsOfUseLink.required = termsObject.Links.Manage.Required;
		TermsOfUseLink termsOfUseLink5 = termsOfUseLink;
		termsOfUse.Links = new TermsOfUseLink[4] { termsOfUseLink2, termsOfUseLink3, termsOfUseLink4, termsOfUseLink5 };
		return termsOfUse;
	}
}
