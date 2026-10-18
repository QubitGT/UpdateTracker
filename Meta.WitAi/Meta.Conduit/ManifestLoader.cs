using System;
using System.IO;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.Conduit;

[LogCategory(LogCategory.Conduit)]
internal class ManifestLoader : IManifestLoader, ILogSource
{
	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Conduit);


	public Manifest LoadManifest(string manifestLocalPath)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(manifestLocalPath));
		if (textAsset == null)
		{
			VLog.E(GetType().Name, "No Manifest found at Resources/" + manifestLocalPath);
			return null;
		}
		return LoadManifestFromJson(textAsset.text);
	}

	public Manifest LoadManifestFromJson(string manifestText)
	{
		Manifest manifest = JsonConvert.DeserializeObject<Manifest>(manifestText);
		if (manifest.ResolveActions())
		{
			Logger.Info("Successfully Loaded Conduit manifest", null, null, null, null, "LoadManifestFromJson", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Conduit\\Data\\ManifestLoader.cs", 49);
			return manifest;
		}
		VLog.E(GetType().Name, "Failed to resolve actions from Conduit manifest");
		return manifest;
	}

	public async Task<Manifest> LoadManifestAsync(string manifestLocalPath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(manifestLocalPath);
		ResourceRequest jsonRequest = Resources.LoadAsync<TextAsset>(fileNameWithoutExtension);
		await TaskUtility.FromAsyncOp(jsonRequest);
		if (jsonRequest.asset is TextAsset textAsset)
		{
			return await LoadManifestFromJsonAsync(textAsset.text);
		}
		VLog.W(GetType().Name, "No Manifest found at Resources/" + manifestLocalPath + ", conduit will not be available.");
		return null;
	}

	public async Task<Manifest> LoadManifestFromJsonAsync(string manifestText)
	{
		Manifest manifest = await JsonConvert.DeserializeObjectAsync<Manifest>(manifestText);
		if (manifest == null)
		{
			VLog.E(GetType().Name, "Cannot decode Conduit manifest\n\n" + manifestText);
			return null;
		}
		await Task.Run(delegate
		{
			try
			{
				if (manifest.ResolveActions())
				{
					Logger.Info("Successfully Loaded Conduit manifest", null, null, null, null, "LoadManifestFromJsonAsync", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Conduit\\Data\\ManifestLoader.cs", 97);
				}
				else
				{
					VLog.E(GetType().Name, "Failed to decode actions from Conduit manifest");
				}
			}
			catch (Exception arg)
			{
				VLog.E(GetType().Name, $"Failed to decode actions from Conduit manifest\n{arg}");
			}
		});
		return manifest;
	}
}
