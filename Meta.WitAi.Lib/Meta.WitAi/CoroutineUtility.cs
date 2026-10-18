using System.Collections;
using System.Linq;
using UnityEngine;

namespace Meta.WitAi;

public static class CoroutineUtility
{
	public class CoroutinePerformer : MonoBehaviour
	{
		private bool _useUpdate;

		private IEnumerator _method;

		private Coroutine _coroutine;

		public bool IsRunning { get; private set; }

		private void Awake()
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public void CoroutineBegin(IEnumerator asyncMethod, bool useUpdate)
		{
			if (!IsRunning)
			{
				IsRunning = true;
				if (Application.isBatchMode)
				{
					useUpdate = true;
				}
				_useUpdate = useUpdate;
				_method = asyncMethod;
				if (_useUpdate)
				{
					CoroutineIterateUpdate();
				}
				else
				{
					_coroutine = StartCoroutine(CoroutineIterateEnumerator());
				}
			}
		}

		private IEnumerator CoroutineIterateEnumerator()
		{
			yield return _method;
			CoroutineComplete();
		}

		private void Update()
		{
			if (_useUpdate)
			{
				CoroutineIterateUpdate();
			}
		}

		private void CoroutineIterateUpdate()
		{
			if (this == null || _method == null)
			{
				CoroutineCancel();
			}
			else if (!MoveNext(_method))
			{
				CoroutineComplete();
			}
		}

		private bool MoveNext(IEnumerator method)
		{
			object current = method.Current;
			if (current != null && current.GetType().GetInterfaces().Contains(typeof(IEnumerator)) && MoveNext(current as IEnumerator))
			{
				return true;
			}
			return method.MoveNext();
		}

		private void OnDestroy()
		{
			CoroutineUnload();
		}

		public void CoroutineCancel()
		{
			CoroutineComplete();
		}

		private void CoroutineComplete()
		{
			if (IsRunning)
			{
				CoroutineUnload();
				if (this != null && base.gameObject != null)
				{
					base.gameObject.DestroySafely();
				}
			}
		}

		private void CoroutineUnload()
		{
			IsRunning = false;
			if (_method != null)
			{
				_method = null;
			}
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}
	}

	public static CoroutinePerformer StartCoroutine(IEnumerator asyncMethod, bool useUpdate = false)
	{
		CoroutinePerformer performer = GetPerformer();
		performer.CoroutineBegin(asyncMethod, useUpdate);
		return performer;
	}

	private static CoroutinePerformer GetPerformer()
	{
		CoroutinePerformer coroutinePerformer = new GameObject("Coroutine").AddComponent<CoroutinePerformer>();
		coroutinePerformer.gameObject.hideFlags = HideFlags.HideAndDontSave;
		return coroutinePerformer;
	}
}
