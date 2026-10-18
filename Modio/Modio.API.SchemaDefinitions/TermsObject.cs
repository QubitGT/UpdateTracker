using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct TermsObject
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal readonly struct EmbeddedButtons
	{
		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedAgree
		{
			internal readonly string Text;

			[JsonConstructor]
			public EmbeddedAgree(string text)
			{
				Text = text;
			}
		}

		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedDisagree
		{
			internal readonly string Text;

			[JsonConstructor]
			public EmbeddedDisagree(string text)
			{
				Text = text;
			}
		}

		internal readonly EmbeddedAgree Agree;

		internal readonly EmbeddedDisagree Disagree;

		[JsonConstructor]
		public EmbeddedButtons(EmbeddedAgree agree, EmbeddedDisagree disagree)
		{
			Agree = agree;
			Disagree = disagree;
		}
	}

	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal readonly struct EmbeddedLinks
	{
		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedWebsite
		{
			internal readonly string Text;

			internal readonly string Url;

			internal readonly bool Required;

			[JsonConstructor]
			public EmbeddedWebsite(string text, string url, bool required)
			{
				Text = text;
				Url = url;
				Required = required;
			}
		}

		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedTerms
		{
			internal readonly string Text;

			internal readonly string Url;

			internal readonly bool Required;

			[JsonConstructor]
			public EmbeddedTerms(string text, string url, bool required)
			{
				Text = text;
				Url = url;
				Required = required;
			}
		}

		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedPrivacy
		{
			internal readonly string Text;

			internal readonly string Url;

			internal readonly bool Required;

			[JsonConstructor]
			public EmbeddedPrivacy(string text, string url, bool required)
			{
				Text = text;
				Url = url;
				Required = required;
			}
		}

		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedRefund
		{
			internal readonly string Text;

			internal readonly string Url;

			internal readonly bool Required;

			[JsonConstructor]
			public EmbeddedRefund(string text, string url, bool required)
			{
				Text = text;
				Url = url;
				Required = required;
			}
		}

		[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
		internal readonly struct EmbeddedManage
		{
			internal readonly string Text;

			internal readonly string Url;

			internal readonly bool Required;

			[JsonConstructor]
			public EmbeddedManage(string text, string url, bool required)
			{
				Text = text;
				Url = url;
				Required = required;
			}
		}

		internal readonly EmbeddedWebsite Website;

		internal readonly EmbeddedTerms Terms;

		internal readonly EmbeddedPrivacy Privacy;

		internal readonly EmbeddedRefund Refund;

		internal readonly EmbeddedManage Manage;

		[JsonConstructor]
		public EmbeddedLinks(EmbeddedWebsite website, EmbeddedTerms terms, EmbeddedPrivacy privacy, EmbeddedRefund refund, EmbeddedManage manage)
		{
			Website = website;
			Terms = terms;
			Privacy = privacy;
			Refund = refund;
			Manage = manage;
		}
	}

	internal readonly string Plaintext;

	internal readonly string Html;

	internal readonly EmbeddedButtons Buttons;

	internal readonly EmbeddedLinks Links;

	[JsonConstructor]
	public TermsObject(string plaintext, string html, EmbeddedButtons buttons, EmbeddedLinks links)
	{
		Plaintext = plaintext;
		Html = html;
		Buttons = buttons;
		Links = links;
	}
}
