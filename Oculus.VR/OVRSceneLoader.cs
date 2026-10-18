using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Feature(Feature.Scene)]
public class OVRSceneLoader : MonoBehaviour
{
	private struct SceneInfo
	{
		public List<string> scenes;

		public long version;

		public SceneInfo(List<string> sceneList, long currentSceneEpochVersion)
		{
			scenes = sceneList;
			version = currentSceneEpochVersion;
		}
	}

	public const string externalStoragePath = "/sdcard/Android/data";

	public const string sceneLoadDataName = "SceneLoadData.txt";

	public const string resourceBundleName = "asset_resources";

	public float sceneCheckIntervalSeconds = 1f;

	public float logCloseTime = 5f;

	public Canvas mainCanvas;

	public Text logTextBox;

	private AsyncOperation loadSceneOperation;

	private string formattedLogText;

	private float closeLogTimer;

	private bool closeLogDialogue;

	private bool canvasPosUpdated;

	private string scenePath = "";

	private string sceneLoadDataPath = "";

	private List<AssetBundle> loadedAssetBundles = new List<AssetBundle>();

	private SceneInfo currentSceneInfo;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		string path = Path.Combine("/sdcard/Android/data", Application.identifier);
		scenePath = Path.Combine(path, "cache/scenes");
		sceneLoadDataPath = Path.Combine(scenePath, "SceneLoadData.txt");
		closeLogDialogue = false;
		StartCoroutine(DelayCanvasPosUpdate());
		currentSceneInfo = GetSceneInfo();
		if (currentSceneInfo.version != 0L && !string.IsNullOrEmpty(currentSceneInfo.scenes[0]))
		{
			LoadScene(currentSceneInfo);
		}
	}

	private void LoadScene(SceneInfo sceneInfo)
	{
		AssetBundle assetBundle = null;
		Debug.Log("[OVRSceneLoader] Loading main scene: " + sceneInfo.scenes[0] + " with version " + sceneInfo.version);
		Text text = logTextBox;
		text.text = text.text + "Target Scene: " + sceneInfo.scenes[0] + "\n";
		Text text2 = logTextBox;
		text2.text = text2.text + "Version: " + sceneInfo.version + "\n";
		Debug.Log("[OVRSceneLoader] Loading scene bundle files.");
		string[] files = Directory.GetFiles(scenePath, "*_*");
		Text text3 = logTextBox;
		text3.text = text3.text + "Loading " + files.Length + " bundle(s) . . . ";
		string text4 = "scene_" + sceneInfo.scenes[0].ToLower();
		try
		{
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				AssetBundle assetBundle2 = AssetBundle.LoadFromFile(array[i]);
				if (assetBundle2 != null)
				{
					Debug.Log(("[OVRSceneLoader] Loading file bundle: " + assetBundle2.name == null) ? "null" : assetBundle2.name);
					loadedAssetBundles.Add(assetBundle2);
				}
				else
				{
					Debug.LogError("[OVRSceneLoader] Loading file bundle failed");
				}
				if (assetBundle2.name == text4)
				{
					assetBundle = assetBundle2;
				}
				if (assetBundle2.name == "asset_resources")
				{
					OVRResources.SetResourceBundle(assetBundle2);
				}
			}
		}
		catch (Exception ex)
		{
			Text text5 = logTextBox;
			text5.text = text5.text + "<color=red>" + ex.Message + "</color>";
			return;
		}
		logTextBox.text += "<color=green>DONE\n</color>";
		if (assetBundle != null)
		{
			logTextBox.text += "Loading Scene: {0:P0}\n";
			formattedLogText = logTextBox.text;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetBundle.GetAllScenePaths()[0]);
			loadSceneOperation = SceneManager.LoadSceneAsync(fileNameWithoutExtension);
			loadSceneOperation.completed += LoadSceneOperation_completed;
		}
		else
		{
			logTextBox.text += "<color=red>Failed to get main scene bundle.\n</color>";
		}
	}

	private void LoadSceneOperation_completed(AsyncOperation obj)
	{
		StartCoroutine(onCheckSceneCoroutine());
		StartCoroutine(DelayCanvasPosUpdate());
		closeLogTimer = 0f;
		closeLogDialogue = true;
		logTextBox.text += "Log closing in {0} seconds.\n";
		formattedLogText = logTextBox.text;
	}

	public void Update()
	{
		if (loadSceneOperation != null && !loadSceneOperation.isDone)
		{
			logTextBox.text = string.Format(formattedLogText, loadSceneOperation.progress + 0.1f);
			if (loadSceneOperation.progress >= 0.9f)
			{
				logTextBox.text = formattedLogText.Replace("{0:P0}", "<color=green>DONE</color>");
				logTextBox.text += "Transitioning to new scene.\nLoad times will vary depending on scene complexity.\n";
			}
		}
		UpdateCanvasPosition();
		if (closeLogDialogue)
		{
			if (closeLogTimer < logCloseTime)
			{
				closeLogTimer += Time.deltaTime;
				logTextBox.text = string.Format(formattedLogText, (int)(logCloseTime - closeLogTimer));
			}
			else
			{
				mainCanvas.gameObject.SetActive(value: false);
				closeLogDialogue = false;
			}
		}
	}

	private void UpdateCanvasPosition()
	{
		if (mainCanvas.worldCamera != Camera.main)
		{
			mainCanvas.worldCamera = Camera.main;
			if (Camera.main != null)
			{
				Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
				base.gameObject.transform.position = position;
				base.gameObject.transform.rotation = Camera.main.transform.rotation;
			}
		}
	}

	private SceneInfo GetSceneInfo()
	{
		SceneInfo result = default(SceneInfo);
		try
		{
			StreamReader streamReader = new StreamReader(sceneLoadDataPath);
			result.version = Convert.ToInt64(streamReader.ReadLine());
			List<string> list = new List<string>();
			while (!streamReader.EndOfStream)
			{
				list.Add(streamReader.ReadLine());
			}
			result.scenes = list;
		}
		catch
		{
			logTextBox.text += "<color=red>Failed to get scene info data.\n</color>";
		}
		return result;
	}

	private IEnumerator DelayCanvasPosUpdate()
	{
		yield return new WaitForSeconds(0.1f);
		UpdateCanvasPosition();
	}

	private IEnumerator onCheckSceneCoroutine()
	{
		while (GetSceneInfo().version == currentSceneInfo.version)
		{
			yield return new WaitForSeconds(sceneCheckIntervalSeconds);
		}
		Debug.Log("[OVRSceneLoader] Scene change detected.");
		foreach (AssetBundle loadedAssetBundle in loadedAssetBundles)
		{
			if (loadedAssetBundle != null)
			{
				loadedAssetBundle.Unload(unloadAllLoadedObjects: true);
			}
		}
		loadedAssetBundles.Clear();
		int sceneCount = SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
		}
		DestroyAllGameObjects();
		SceneManager.LoadSceneAsync("OVRTransitionScene");
	}

	private void DestroyAllGameObjects()
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
	}
}
