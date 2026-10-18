using System;
using System.Collections;
using Meta.XR.Util;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#what-does-ovrscenemanager-do")]
[RequireComponent(typeof(OVRSceneManager))]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSceneModelLoader : MonoBehaviour
{
	private const float RetryingReminderDelay = 10f;

	private bool _sceneCaptureRequested;

	protected OVRSceneManager SceneManager { get; private set; }

	protected virtual void Start()
	{
		OVRTelemetry.SendEvent(163059869);
		SceneManager = GetComponent<OVRSceneManager>();
		OVRSceneManager sceneManager = SceneManager;
		sceneManager.SceneModelLoadedSuccessfully = (Action)Delegate.Combine(sceneManager.SceneModelLoadedSuccessfully, new Action(OnSceneModelLoadedSuccessfully));
		OVRSceneManager sceneManager2 = SceneManager;
		sceneManager2.NoSceneModelToLoad = (Action)Delegate.Combine(sceneManager2.NoSceneModelToLoad, new Action(OnNoSceneModelToLoad));
		OVRSceneManager sceneManager3 = SceneManager;
		sceneManager3.NewSceneModelAvailable = (Action)Delegate.Combine(sceneManager3.NewSceneModelAvailable, new Action(OnNewSceneModelAvailable));
		SceneManager.LoadSceneModelFailedPermissionNotGranted += OnLoadSceneModelFailedPermissionNotGranted;
		OVRSceneManager sceneManager4 = SceneManager;
		sceneManager4.SceneCaptureReturnedWithoutError = (Action)Delegate.Combine(sceneManager4.SceneCaptureReturnedWithoutError, new Action(OnSceneCaptureReturnedWithoutError));
		OVRSceneManager sceneManager5 = SceneManager;
		sceneManager5.UnexpectedErrorWithSceneCapture = (Action)Delegate.Combine(sceneManager5.UnexpectedErrorWithSceneCapture, new Action(OnUnexpectedErrorWithSceneCapture));
		OnStart();
	}

	private IEnumerator AttemptToLoadSceneModel()
	{
		float timeSinceReminder = 0f;
		do
		{
			timeSinceReminder += Time.deltaTime;
			if (timeSinceReminder >= 10f)
			{
				timeSinceReminder = 0f;
			}
			yield return null;
		}
		while (!SceneManager.LoadSceneModel());
	}

	protected virtual void OnStart()
	{
		LoadSceneModel();
	}

	protected static OVRTask<bool> RequestScenePermissionAsync()
	{
		return OVRTask.FromResult(result: true);
	}

	protected virtual async void OnLoadSceneModelFailedPermissionNotGranted()
	{
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "Requesting permission com.oculus.permission.USE_SCENE");
		if (await RequestScenePermissionAsync())
		{
			SceneManager.Verbose?.Log("OVRSceneModelLoader", "Permission com.oculus.permission.USE_SCENE granted. Attempting to load scene model.");
			LoadSceneModel();
		}
		else
		{
			SceneManager.Verbose?.Log("OVRSceneModelLoader", "Permission com.oculus.permission.USE_SCENE denied. Scene model will not be loaded.");
		}
	}

	private void LoadSceneModel()
	{
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "OnStart() calling OVRSceneManager.LoadSceneModel()");
		if (!SceneManager.LoadSceneModel() && OVRManager.isHmdPresent)
		{
			StartCoroutine(AttemptToLoadSceneModel());
		}
	}

	protected virtual void OnSceneModelLoadedSuccessfully()
	{
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "OVRSceneManager.LoadSceneModel() completed successfully.");
	}

	protected virtual void OnNoSceneModelToLoad()
	{
		if (_sceneCaptureRequested)
		{
			SceneManager.Verbose?.Log("OVRSceneModelLoader", "OnSceneCaptureReturnedWithoutError() There is no scene model, but we have already requested scene capture once. No further action will be taken.");
			return;
		}
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "OnNoSceneModelToLoad() calling OVRSceneManager.RequestSceneCapture()");
		_sceneCaptureRequested = SceneManager.RequestSceneCapture();
	}

	protected virtual void OnNewSceneModelAvailable()
	{
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "OnNewSceneModelAvailable() calling OVRSceneManager.LoadSceneModel()");
		SceneManager.LoadSceneModel();
	}

	protected virtual void OnSceneCaptureReturnedWithoutError()
	{
		SceneManager.Verbose?.Log("OVRSceneModelLoader", "Room setup returned without errors.");
	}

	protected virtual void OnUnexpectedErrorWithSceneCapture()
	{
		SceneManager.Verbose?.LogError("OVRSceneModelLoader", "Requesting the Room Setup failed. The Scene Model cannot be loaded.");
	}
}
