using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DayNightCycle : MonoBehaviour
{
	public struct LerpBakedLightingJob : IJob
	{
		public NativeArray<Color> fromPixels;

		public NativeArray<Color> toPixels;

		public NativeArray<Color> mixedPixels;

		public float lerpValue;

		public void Execute()
		{
			for (int i = 0; i < fromPixels.Length; i++)
			{
				mixedPixels[i] = Color.Lerp(fromPixels[i], toPixels[i], 0.5f);
			}
		}
	}

	public Texture2D _dayMap;

	private Texture2D fromMap;

	public Texture2D _sunriseMap;

	private Texture2D toMap;

	public LerpBakedLightingJob job;

	public JobHandle jobHandle;

	public bool isComplete;

	private float startTime;

	public float timeTakenStartingJob;

	public float timeTakenPostJob;

	public float timeTakenDuringJob;

	public LightmapData newData;

	private Color[] fromPixels;

	private Color[] toPixels;

	private Color[] mixedPixels;

	private LightmapData[] newDatas;

	public Texture2D newTexture;

	public int textureWidth;

	public int textureHeight;

	private Color[] workBlockFrom;

	private Color[] workBlockTo;

	private Color[] workBlockMix;

	public int subTextureSize = 1024;

	public Texture2D[] subTextureArray;

	public bool startCoroutine;

	public bool startedCoroutine;

	public bool finishedCoroutine;

	public bool startJob;

	public float switchTimeTaken;

	public bool jobStarted;

	public float lerpAmount;

	public int currentRow;

	public int currentColumn;

	public int currentSubTexture;

	public int currentRowInSubtexture;

	public void Awake()
	{
		fromMap = new Texture2D(_sunriseMap.width, _sunriseMap.height);
		fromMap = LightmapSettings.lightmaps[0].lightmapColor;
		toMap = new Texture2D(_dayMap.width, _dayMap.height);
		toMap.SetPixels(_dayMap.GetPixels());
		toMap.Apply();
		workBlockMix = new Color[subTextureSize * subTextureSize];
		newTexture = new Texture2D(fromMap.width, fromMap.height, fromMap.graphicsFormat, TextureCreationFlags.None);
		newData = new LightmapData();
		textureHeight = fromMap.height;
		textureWidth = fromMap.width;
		subTextureArray = new Texture2D[(int)Mathf.Pow(textureHeight / subTextureSize, 2f)];
		Debug.Log("aaaa " + fromMap.format);
		Debug.Log("aaaa " + fromMap.graphicsFormat);
		startJob = false;
		startCoroutine = false;
		startedCoroutine = false;
		finishedCoroutine = false;
	}

	public void Update()
	{
		if (startJob)
		{
			startJob = false;
			startTime = Time.realtimeSinceStartup;
			StartCoroutine(UpdateWork());
			timeTakenStartingJob = Time.realtimeSinceStartup - startTime;
			startTime = Time.realtimeSinceStartup;
		}
		if (jobStarted && jobHandle.IsCompleted)
		{
			timeTakenDuringJob = Time.realtimeSinceStartup - startTime;
			startTime = Time.realtimeSinceStartup;
			jobHandle.Complete();
			jobStarted = false;
			newTexture.SetPixels(job.mixedPixels.ToArray());
			newData.lightmapDir = LightmapSettings.lightmaps[0].lightmapDir;
			LightmapSettings.lightmaps = new LightmapData[1] { newData };
			job.fromPixels.Dispose();
			job.toPixels.Dispose();
			job.mixedPixels.Dispose();
			timeTakenPostJob = Time.realtimeSinceStartup - startTime;
		}
		if (startCoroutine)
		{
			startCoroutine = false;
			startTime = Time.realtimeSinceStartup;
			newTexture = new Texture2D(fromMap.width, fromMap.height);
			StartCoroutine(UpdateWork());
		}
		if (startedCoroutine && finishedCoroutine)
		{
			startedCoroutine = false;
			finishedCoroutine = false;
			timeTakenDuringJob = Time.realtimeSinceStartup - startTime;
			startTime = Time.realtimeSinceStartup;
			newData = LightmapSettings.lightmaps[0];
			newData.lightmapColor = fromMap;
			LightmapData[] lightmaps = LightmapSettings.lightmaps;
			lightmaps[0].lightmapColor = fromMap;
			LightmapSettings.lightmaps = lightmaps;
			timeTakenPostJob = Time.realtimeSinceStartup - startTime;
		}
	}

	public IEnumerator UpdateWork()
	{
		yield return 0;
		timeTakenStartingJob = Time.realtimeSinceStartup - startTime;
		startTime = Time.realtimeSinceStartup;
		startedCoroutine = true;
		currentSubTexture = 0;
		for (int k = 0; k < subTextureArray.Length; k++)
		{
			subTextureArray[k] = new Texture2D(subTextureSize, subTextureSize, fromMap.graphicsFormat, TextureCreationFlags.None);
			yield return 0;
		}
		for (int k = 0; k < textureWidth / subTextureSize; k++)
		{
			currentColumn = k;
			for (int l = 0; l < textureHeight / subTextureSize; l++)
			{
				currentRow = l;
				workBlockFrom = fromMap.GetPixels(k * subTextureSize, l * subTextureSize, subTextureSize, subTextureSize);
				workBlockTo = toMap.GetPixels(k * subTextureSize, l * subTextureSize, subTextureSize, subTextureSize);
				for (int m = 0; m < subTextureSize * subTextureSize - 1; m++)
				{
					workBlockMix[m] = Color.Lerp(workBlockFrom[m], workBlockTo[m], lerpAmount);
				}
				subTextureArray[l * (textureWidth / subTextureSize) + k].SetPixels(0, 0, subTextureSize, subTextureSize, workBlockMix);
				yield return 0;
			}
		}
		for (int k = 0; k < subTextureArray.Length; k++)
		{
			currentSubTexture = k;
			subTextureArray[k].Apply();
			yield return 0;
			Graphics.CopyTexture(subTextureArray[k], 0, 0, 0, 0, subTextureSize, subTextureSize, newTexture, 0, 0, k * subTextureSize % textureHeight, (int)Mathf.Floor(subTextureSize * k / textureHeight) * subTextureSize);
			yield return 0;
		}
		finishedCoroutine = true;
	}
}
