using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion.Photon.Realtime;

internal class MonoBehaviourEmpty : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass6_0
	{
		public IEnumerator coroutine;

		public MonoBehaviourEmpty _003C_003E4__this;
	}

	internal Action<RegionHandler> onCompleteCall;

	private RegionHandler obj;

	public static MonoBehaviourEmpty BuildInstance(string id = null)
	{
		GameObject gameObject = new GameObject(id ?? "MonoBehaviourEmpty");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		return gameObject.AddComponent<MonoBehaviourEmpty>();
	}

	public void SelfDestroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (obj != null)
		{
			onCompleteCall(obj);
			obj = null;
			onCompleteCall = null;
			SelfDestroy();
		}
	}

	public void CompleteOnMainThread(RegionHandler obj)
	{
		this.obj = obj;
	}

	public void StartCoroutineAndDestroy(IEnumerator coroutine)
	{
		_003C_003Ec__DisplayClass6_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass6_0();
		CS_0024_003C_003E8__locals0.coroutine = coroutine;
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		StartCoroutine(Routine());
		[IteratorStateMachine(typeof(_003C_003Ec__DisplayClass6_0._003C_003CStartCoroutineAndDestroy_003Eg__Routine_007C0_003Ed))]
		IEnumerator Routine()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003Ec__DisplayClass6_0._003C_003CStartCoroutineAndDestroy_003Eg__Routine_007C0_003Ed(0)
			{
				_003C_003E4__this = CS_0024_003C_003E8__locals0
			};
		}
	}
}
