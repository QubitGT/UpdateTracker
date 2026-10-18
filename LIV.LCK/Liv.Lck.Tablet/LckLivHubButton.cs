using System;
using System.Collections;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckLivHubButton : MonoBehaviour
{
	private const long PRODUCTION_APPID = 24199129276346881L;

	[SerializeField]
	private GameObject _livHubButtonGameObject;

	private void Start()
	{
		if (Application.platform != RuntimePlatform.Android && !Application.isEditor && (bool)_livHubButtonGameObject)
		{
			_livHubButtonGameObject.SetActive(value: false);
		}
	}

	public void OpenMetaStoreApp()
	{
		StartCoroutine(OpenStoreAppCoroutine());
	}

	private IEnumerator OpenStoreAppCoroutine()
	{
		if (Application.platform != RuntimePlatform.Android || Application.isEditor)
		{
			yield break;
		}
		AndroidJavaClass androidJavaClass = null;
		AndroidJavaObject androidJavaObject = null;
		AndroidJavaObject androidJavaObject2 = null;
		AndroidJavaObject androidJavaObject3 = null;
		AndroidJavaObject androidJavaObject4 = null;
		try
		{
			androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if (androidJavaClass == null)
			{
				throw new Exception("Failed to create UnityPlayer class");
			}
			androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			if (androidJavaObject == null)
			{
				throw new Exception("Failed to get current activity");
			}
			androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getPackageManager", Array.Empty<object>());
			if (androidJavaObject2 == null)
			{
				throw new Exception("Failed to get package manager");
			}
			androidJavaObject4 = androidJavaObject2.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { "tv.liv.controlcenter" });
			if (androidJavaObject4 != null)
			{
				androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { "com.oculus.vrshell" });
				if (androidJavaObject3 == null)
				{
					throw new Exception("Failed to find com.oculus.vrshell package.");
				}
				androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2] { "intent_data", "tv.liv.controlcenter/.MainActivity" });
				if (androidJavaObject3 == null)
				{
					throw new Exception("Failed to add extra intent data tv.liv.controlcenter/.MainActivity");
				}
				androidJavaObject3.Call<AndroidJavaObject>("addFlags", new object[1] { 268697600 });
				androidJavaObject.Call("startActivity", androidJavaObject3);
			}
			else
			{
				androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { "com.oculus.vrshell" });
				if (androidJavaObject3 == null)
				{
					throw new Exception("Failed to set launch intent to com.oculus.vrshell");
				}
				androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2] { "intent_data", "com.oculus.store/.StoreActivity" });
				if (androidJavaObject3 == null)
				{
					throw new Exception("Failed to put extra intent data com.oculus.store/.StoreActivity");
				}
				androidJavaObject3.Call<AndroidJavaObject>("putExtra", new object[2]
				{
					"uri",
					"/item/" + 24199129276346881L
				});
				if (androidJavaObject3 == null)
				{
					throw new Exception("Failed to put extra intent data appID: " + 24199129276346881L);
				}
				androidJavaObject.Call("startActivity", androidJavaObject3);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to launch store app: " + ex.Message + "\n" + ex.StackTrace);
		}
		androidJavaObject3?.Dispose();
		androidJavaObject4?.Dispose();
		androidJavaObject2?.Dispose();
		androidJavaObject?.Dispose();
		androidJavaClass?.Dispose();
	}
}
