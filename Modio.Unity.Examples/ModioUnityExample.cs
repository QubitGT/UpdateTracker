using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Modio;
using Modio.Authentication;
using Modio.Mods;
using Modio.Mods.Builder;
using Modio.Unity;
using Modio.Users;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ModioUnityExample : MonoBehaviour
{
	private readonly struct DummyModData
	{
		public readonly string name;

		public readonly string summary;

		public readonly Texture2D logo;

		public readonly string path;

		public DummyModData(string name, string summary, Texture2D logo, string path)
		{
			this.name = name;
			this.summary = summary;
			this.logo = logo;
			this.path = path;
		}
	}

	private static readonly byte[] Megabyte = new byte[1048576];

	private static readonly System.Random RandomBytes = new System.Random();

	[Header("Authentication")]
	[SerializeField]
	private GameObject authContainer;

	[SerializeField]
	private InputField authInput;

	[SerializeField]
	private Button authRequest;

	[SerializeField]
	private Button authSubmit;

	[Header("Terms of Use")]
	[SerializeField]
	private GameObject tosContainer;

	[SerializeField]
	private Button termsLink;

	[SerializeField]
	private Button privacyLink;

	[SerializeField]
	private Button denyButton;

	[SerializeField]
	private Button acceptButton;

	[Header("Random Mod")]
	[SerializeField]
	private GameObject randomContainer;

	[SerializeField]
	private Text randomName;

	[SerializeField]
	private Image randomLogo;

	[SerializeField]
	private Button randomButton;

	private Mod[] allMods;

	private Mod currentDownload;

	private float downloadProgress;

	private float timeToProgressCheck = 1f;

	private void Awake()
	{
		ModioServices.Bind<IModioAuthService>().FromInstance(new ModioEmailAuthService(GetAuthCode));
		authContainer.SetActive(value: false);
		tosContainer.SetActive(value: false);
		randomContainer.SetActive(value: false);
	}

	private void Start()
	{
		InitPlugin();
	}

	private async Task InitPlugin()
	{
		Error error = await ModioClient.Init();
		if ((bool)error)
		{
			Debug.LogError($"Error initializing mod.io: {error}");
			return;
		}
		Debug.Log("mod.io plugin initialized");
		OnInit();
	}

	private void OnInit()
	{
		if (User.Current.IsAuthenticated)
		{
			OnAuth();
			return;
		}
		authRequest.onClick.AddListener(delegate
		{
			Authenticate();
		});
	}

	private async Task Authenticate()
	{
		Error error = await ModioClient.AuthService.Authenticate(displayedTerms: true, (authInput.text.Length > 0) ? authInput.text : null);
		if ((bool)error)
		{
			Debug.LogError($"Error authenticating user: {error}");
		}
		else
		{
			OnAuth();
		}
	}

	private async Task<string> GetAuthCode()
	{
		bool codeEntered = false;
		authSubmit.onClick.AddListener(delegate
		{
			codeEntered = true;
		});
		while (!codeEntered)
		{
			await Task.Yield();
		}
		return authInput.text;
	}

	private async void OnAuth()
	{
		Debug.Log("Authenticated user: " + User.Current.Profile.Username);
		authContainer.SetActive(value: false);
		tosContainer.SetActive(value: false);
		await AddModsIfNone();
		allMods = await GetAllMods();
		Debug.Log("Available mods:\n" + string.Join("\n", allMods.Select((Mod mod) => $"{mod.Name} (id: {mod.Id})")));
		randomButton.onClick.AddListener(SetRandomMod);
		randomContainer.SetActive(value: true);
		SetRandomMod();
		while (User.Current.IsUpdating)
		{
			await Task.Yield();
		}
		Mod[] subscribedMods = GetSubscribedMods();
		Debug.Log("Subscribed mods:\n" + ((subscribedMods.Length != 0) ? string.Join("\n", subscribedMods.Select((Mod mod) => $"{mod.Name} (id: {mod.Id})")) : "None"));
		await SubscribeToMod(allMods[UnityEngine.Random.Range(0, allMods.Length - 1)]);
		WakeUpModManagement();
		ICollection<Mod> collection = await ModInstallationManagement.GetAllInstalledMods();
		Debug.Log("Installed mods:\n" + ((collection.Count > 0) ? string.Join("\n", collection.Select((Mod mod) => $"{mod.Name} (id: {mod.Id})")) : "None"));
	}

	private async Task AddModsIfNone()
	{
		var (error, modioPage) = await Mod.GetMods(new ModSearchFilter());
		if ((bool)error)
		{
			Debug.LogError($"Error getting mods: {error}");
			return;
		}
		if (modioPage.Data.Length != 0)
		{
			Debug.Log($"{modioPage.Data.Length} mods found. Not adding mods");
			return;
		}
		DummyModData dummyModData = await GenerateDummyMod("Cool Weapon", "A really cool weapon.", "24466B", "FDA576", 10);
		DummyModData dummyModData2 = await GenerateDummyMod("Funny Sound Pack", "You'll laugh a lot using this.", "B85675", "633E63", 50);
		DummyModData dummyModData3 = await GenerateDummyMod("Klingon Language Pack", "tlhIngan Hol Dajatlh'a'?", "93681C", "FFEAD0", 1);
		DummyModData dummyModData4 = await GenerateDummyMod("Ten New Missions", "Ported from the sequel to the prequel!", "FDA576", "D45B7A", 99);
		DummyModData[] array = new DummyModData[4] { dummyModData, dummyModData2, dummyModData3, dummyModData4 };
		DummyModData[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DummyModData dummyModData5 = array2[i];
			await UploadMod(dummyModData5.name, dummyModData5.summary, dummyModData5.logo, dummyModData5.path);
		}
	}

	private async Task UploadMod(string modName, string summary, Texture2D logo, string path)
	{
		Debug.Log("Starting upload: " + modName);
		ModBuilder modBuilder = Mod.Create();
		modBuilder.SetName(modName).SetSummary(summary).SetLogo(logo.EncodeToPNG(), ImageFormat.Png)
			.EditModfile()
			.SetSourceDirectoryPath(path)
			.FinishModfile();
		var (error, mod) = await modBuilder.Publish();
		if ((bool)error)
		{
			Debug.LogError($"Error uploading mod {modName}: {error}");
		}
		else
		{
			Debug.Log($"Successfully created mod {mod.Name} with Id {mod.Id}");
		}
	}

	private async Task<Mod[]> GetAllMods()
	{
		var (error, modioPage) = await Mod.GetMods(new ModSearchFilter());
		if ((bool)error)
		{
			Debug.LogError($"Error getting mods: {error}");
			return Array.Empty<Mod>();
		}
		return modioPage.Data;
	}

	private async void SetRandomMod()
	{
		Mod mod = allMods[UnityEngine.Random.Range(0, allMods.Length - 1)];
		randomName.text = mod.Name;
		var (error, texture2D) = await mod.Logo.DownloadAsTexture2D(Mod.LogoResolution.X320_Y180);
		if ((bool)error)
		{
			Debug.LogError($"Error downloading {mod.Name}'s logo: {error}");
		}
		else
		{
			randomLogo.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
		}
	}

	private static Mod[] GetSubscribedMods()
	{
		return User.Current.ModRepository.GetSubscribed().ToArray();
	}

	private async Task SubscribeToMod(Mod mod)
	{
		Error error = await mod.Subscribe();
		if ((bool)error)
		{
			Debug.LogError($"Error subscribing to {mod.Name}: {error}");
		}
		else
		{
			Debug.Log("Subscribed to mod: " + mod.Name);
		}
	}

	private void WakeUpModManagement()
	{
		ModInstallationManagement.ManagementEvents += HandleModManagementEvent;
		void HandleModManagementEvent(Mod mod, Modfile modfile, ModInstallationManagement.OperationType jobType, ModInstallationManagement.OperationPhase jobPhase)
		{
			Debug.Log($"{jobType} {jobPhase}: {mod.Name}");
			switch (jobPhase)
			{
			case ModInstallationManagement.OperationPhase.Started:
				if (jobType != ModInstallationManagement.OperationType.Uninstall)
				{
					currentDownload = mod;
				}
				break;
			case ModInstallationManagement.OperationPhase.Cancelled:
			case ModInstallationManagement.OperationPhase.Failed:
				currentDownload = null;
				break;
			case ModInstallationManagement.OperationPhase.Completed:
				if (jobType != ModInstallationManagement.OperationType.Uninstall)
				{
					Debug.Log("Mod " + mod.Name + " installed at " + mod.File.InstallLocation);
					currentDownload = null;
				}
				else
				{
					Debug.Log("Mod " + mod.Name + " uninstalled");
				}
				break;
			}
		}
	}

	private void Update()
	{
		if (currentDownload != null)
		{
			timeToProgressCheck -= Time.deltaTime;
			if (!(timeToProgressCheck > 0f))
			{
				Debug.Log($"Downloading {currentDownload.Name}: [{Mathf.RoundToInt(currentDownload.File.FileStateProgress * 100f)}%]");
				timeToProgressCheck += 1f;
			}
		}
	}

	private async Task<DummyModData> GenerateDummyMod(string dummyName, string summary, string backgroundColor, string textColor, int megabytes)
	{
		Debug.Log("Writing temporary mod file: " + dummyName);
		string path = Path.Combine(Application.dataPath, "../_temp_dummy_mods/" + dummyName);
		Directory.CreateDirectory(path);
		using (FileStream fs = File.OpenWrite(Path.Combine(path, dummyName + ".dummy")))
		{
			for (int i = 0; i < megabytes; i++)
			{
				RandomBytes.NextBytes(Megabyte);
				await fs.WriteAsync(Megabyte, 0, Megabyte.Length);
			}
		}
		return new DummyModData(dummyName, summary, await GenerateLogo(dummyName.Replace(' ', '+'), backgroundColor, textColor), path);
	}

	private async Task<Texture2D> GenerateLogo(string text, string backgroundColor, string textColor)
	{
		UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://placehold.co/512x288/" + backgroundColor + "/" + textColor + ".png?text=" + text);
		request.SendWebRequest();
		while (!request.isDone)
		{
			await Task.Yield();
		}
		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("GenerateLogo failed: " + request.error);
			return null;
		}
		return DownloadHandlerTexture.GetContent(request);
	}
}
