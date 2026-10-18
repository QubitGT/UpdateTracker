using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Backtrace.Unity.Extensions;
using Backtrace.Unity.Json;
using UnityEngine.Networking;

namespace Backtrace.Unity.Model;

internal sealed class BacktraceHttpClient : IBacktraceHttpClient
{
	private const string DiagnosticFileName = "upload_file";

	private const int RequestTimeout = 15000;

	public bool IgnoreSslValidation { get; set; }

	public void Post(string submissionUrl, BacktraceJObject jObject, Action<long, bool, string> onComplete)
	{
		UnityWebRequest request = new UnityWebRequest(submissionUrl, "POST")
		{
			timeout = 15000
		};
		request.IgnoreSsl(IgnoreSslValidation);
		byte[] bytes = Encoding.UTF8.GetBytes(jObject.ToJson());
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetJsonContentType();
		request.SendWebRequest().completed += delegate
		{
			long responseCode = request.responseCode;
			string text = request.downloadHandler.text;
			bool arg = request.ReceivedNetworkError();
			request.Dispose();
			if (onComplete != null)
			{
				onComplete(responseCode, arg, text);
			}
		};
	}

	public UnityWebRequest Post(string submissionUrl, string json, IEnumerable<string> attachments, IDictionary<string, string> attributes)
	{
		return Post(submissionUrl, CreateJsonFormData(Encoding.UTF8.GetBytes(json), attachments, attributes));
	}

	public UnityWebRequest Post(string submissionUrl, byte[] minidump, IEnumerable<string> attachments, IDictionary<string, string> attributes)
	{
		return Post(submissionUrl, CreateMinidumpFormData(minidump, attachments, attributes));
	}

	private UnityWebRequest Post(string submissionUrl, List<IMultipartFormSection> formData)
	{
		byte[] array = UnityWebRequest.GenerateBoundary();
		UnityWebRequest unityWebRequest = UnityWebRequest.Post(submissionUrl, formData, array);
		unityWebRequest.timeout = 15000;
		unityWebRequest.IgnoreSsl(IgnoreSslValidation);
		unityWebRequest.SetMultipartFormData(array);
		return unityWebRequest;
	}

	private List<IMultipartFormSection> CreateJsonFormData(byte[] json, IEnumerable<string> attachments, IDictionary<string, string> attributes)
	{
		List<IMultipartFormSection> list = new List<IMultipartFormSection>
		{
			new MultipartFormFileSection("upload_file", json, string.Format("{0}.json", "upload_file"), "application/json")
		};
		AddAttributesToFormData(list, attributes);
		AddAttachmentToFormData(list, attachments);
		return list;
	}

	private List<IMultipartFormSection> CreateMinidumpFormData(byte[] minidump, IEnumerable<string> attachments, IDictionary<string, string> attributes)
	{
		List<IMultipartFormSection> list = new List<IMultipartFormSection>
		{
			new MultipartFormFileSection("upload_file", minidump)
		};
		AddAttributesToFormData(list, attributes);
		AddAttachmentToFormData(list, attachments);
		return list;
	}

	private void AddAttributesToFormData(List<IMultipartFormSection> formData, IDictionary<string, string> attributes)
	{
		if (attributes == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> attribute in attributes)
		{
			if (!string.IsNullOrEmpty(attribute.Value))
			{
				formData.Add(new MultipartFormDataSection(attribute.Key, attribute.Value));
			}
		}
	}

	private void AddAttachmentToFormData(List<IMultipartFormSection> formData, IEnumerable<string> attachments)
	{
		if (attachments == null)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>(attachments.Reverse());
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (string item in hashSet)
		{
			if (string.IsNullOrEmpty(item) || !File.Exists(item))
			{
				continue;
			}
			long length = new FileInfo(item).Length;
			if (length <= 10485760 && length != 0)
			{
				string text = Path.GetFileName(item);
				if (dictionary.ContainsKey(text))
				{
					dictionary[text]++;
					text = $"{Path.GetFileName(text)}({dictionary[text]}){Path.GetExtension(text)}";
				}
				else
				{
					dictionary[text] = 0;
				}
				formData.Add(new MultipartFormFileSection(string.Format("{0}{1}", "attachment_", text), File.ReadAllBytes(item)));
			}
		}
	}
}
