using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class OVRTask
{
	internal readonly struct Builder
	{
		private readonly OVRPlugin.Result _synchronousResult;

		private readonly Guid _taskId;

		public Builder(OVRPlugin.Result synchronousResult, Guid taskId)
		{
			_synchronousResult = synchronousResult;
			_taskId = taskId;
		}

		public OVRTask<OVRPlugin.Result> ToTask()
		{
			return ToTask(_synchronousResult);
		}

		public OVRTask<TStatus> ToTask<TStatus>() where TStatus : struct, Enum
		{
			return ToTask(CastResult<TStatus>());
		}

		public OVRTask<TResult> ToTask<TResult>(TResult failureValue)
		{
			if (!_synchronousResult.IsSuccess())
			{
				return FromResult(failureValue);
			}
			return FromGuid<TResult>(_taskId);
		}

		public OVRTask<OVRResult<TStatus>> ToResultTask<TStatus>() where TStatus : struct, Enum
		{
			return ToTask(_synchronousResult.IsSuccess() ? default(OVRResult<TStatus>) : OVRResult<TStatus>.FromFailure(CastResult<TStatus>()));
		}

		public OVRTask<OVRResult<TValue, TStatus>> ToTask<TValue, TStatus>() where TStatus : struct, Enum
		{
			return ToTask(_synchronousResult.IsSuccess() ? default(OVRResult<TValue, TStatus>) : OVRResult<TValue, TStatus>.FromFailure(CastResult<TStatus>()));
		}

		private TResult CastResult<TResult>() where TResult : struct, Enum
		{
			Type enumUnderlyingType = typeof(TResult).GetEnumUnderlyingType();
			if (enumUnderlyingType != typeof(int) && enumUnderlyingType != typeof(uint))
			{
				throw new InvalidCastException(typeof(TResult).Name + " must have an underlying type of Int32 or UInt32.");
			}
			OVRPlugin.Result from = _synchronousResult;
			return UnsafeUtility.As<OVRPlugin.Result, TResult>(ref from);
		}
	}

	private class MultiTaskData<T> : OVRObjectPool.IPoolObject
	{
		protected OVRTask<T> CombinedTask;

		protected T Result;

		protected HashSet<Guid> Remaining;

		void OVRObjectPool.IPoolObject.OnGet()
		{
			CombinedTask = FromGuid<T>(Guid.NewGuid());
			Result = default(T);
			Remaining = OVRObjectPool.HashSet<Guid>();
		}

		void OVRObjectPool.IPoolObject.OnReturn()
		{
			Result = default(T);
			OVRObjectPool.Return(Remaining);
		}

		protected void AddTask(Guid id)
		{
			Remaining.Add(id);
		}

		protected void OnResult(Guid taskId)
		{
			Remaining.Remove(taskId);
			if (Remaining.Count != 0)
			{
				return;
			}
			try
			{
				CombinedTask.SetResult(Result);
			}
			finally
			{
				OVRObjectPool.Return(this);
			}
		}
	}

	private class MultiTaskData<T1, T2> : MultiTaskData<(T1, T2)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2)> Get(OVRTask<T1> task1, OVRTask<T2> task2)
		{
			MultiTaskData<T1, T2> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3> : MultiTaskData<(T1, T2, T3)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3)
		{
			MultiTaskData<T1, T2, T3> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3, T4> : MultiTaskData<(T1, T2, T3, T4)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3, T4>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3, T4>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3, T4>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3, T4>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3, T4>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3, T4>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T4, (Guid, MultiTaskData<T1, T2, T3, T4>)> _onResult4 = delegate(T4 result, (Guid, MultiTaskData<T1, T2, T3, T4>) data)
		{
			data.Item2.Result.Item4 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3, T4)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4)
		{
			MultiTaskData<T1, T2, T3, T4> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3, T4>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			multiTaskData.AddTask(task4._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			task4.ContinueWith(_onResult4, (task4._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3, T4, T5> : MultiTaskData<(T1, T2, T3, T4, T5)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3, T4, T5>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3, T4, T5>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3, T4, T5>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T4, (Guid, MultiTaskData<T1, T2, T3, T4, T5>)> _onResult4 = delegate(T4 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5>) data)
		{
			data.Item2.Result.Item4 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T5, (Guid, MultiTaskData<T1, T2, T3, T4, T5>)> _onResult5 = delegate(T5 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5>) data)
		{
			data.Item2.Result.Item5 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3, T4, T5)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5)
		{
			MultiTaskData<T1, T2, T3, T4, T5> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3, T4, T5>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			multiTaskData.AddTask(task4._id);
			multiTaskData.AddTask(task5._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			task4.ContinueWith(_onResult4, (task4._id, multiTaskData));
			task5.ContinueWith(_onResult5, (task5._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3, T4, T5, T6> : MultiTaskData<(T1, T2, T3, T4, T5, T6)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T4, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult4 = delegate(T4 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item4 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T5, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult5 = delegate(T5 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item5 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T6, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>)> _onResult6 = delegate(T6 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6>) data)
		{
			data.Item2.Result.Item6 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3, T4, T5, T6)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6)
		{
			MultiTaskData<T1, T2, T3, T4, T5, T6> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3, T4, T5, T6>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			multiTaskData.AddTask(task4._id);
			multiTaskData.AddTask(task5._id);
			multiTaskData.AddTask(task6._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			task4.ContinueWith(_onResult4, (task4._id, multiTaskData));
			task5.ContinueWith(_onResult5, (task5._id, multiTaskData));
			task6.ContinueWith(_onResult6, (task6._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3, T4, T5, T6, T7> : MultiTaskData<(T1, T2, T3, T4, T5, T6, T7)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T4, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult4 = delegate(T4 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item4 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T5, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult5 = delegate(T5 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item5 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T6, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult6 = delegate(T6 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item6 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T7, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>)> _onResult7 = delegate(T7 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7>) data)
		{
			data.Item2.Result.Item7 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3, T4, T5, T6, T7)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6, OVRTask<T7> task7)
		{
			MultiTaskData<T1, T2, T3, T4, T5, T6, T7> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3, T4, T5, T6, T7>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			multiTaskData.AddTask(task4._id);
			multiTaskData.AddTask(task5._id);
			multiTaskData.AddTask(task6._id);
			multiTaskData.AddTask(task7._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			task4.ContinueWith(_onResult4, (task4._id, multiTaskData));
			task5.ContinueWith(_onResult5, (task5._id, multiTaskData));
			task6.ContinueWith(_onResult6, (task6._id, multiTaskData));
			task7.ContinueWith(_onResult7, (task7._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private class MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8> : MultiTaskData<(T1, T2, T3, T4, T5, T6, T7, T8)>
	{
		private static Action<T1, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult1 = delegate(T1 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item1 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T2, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult2 = delegate(T2 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item2 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T3, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult3 = delegate(T3 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item3 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T4, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult4 = delegate(T4 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item4 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T5, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult5 = delegate(T5 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item5 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T6, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult6 = delegate(T6 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item6 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T7, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult7 = delegate(T7 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item7 = result;
			data.Item2.OnResult(data.Item1);
		};

		private static Action<T8, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>)> _onResult8 = delegate(T8 result, (Guid, MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>) data)
		{
			data.Item2.Result.Item8 = result;
			data.Item2.OnResult(data.Item1);
		};

		public static OVRTask<(T1, T2, T3, T4, T5, T6, T7, T8)> Get(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6, OVRTask<T7> task7, OVRTask<T8> task8)
		{
			MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8> multiTaskData = OVRObjectPool.Get<MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>>();
			multiTaskData.AddTask(task1._id);
			multiTaskData.AddTask(task2._id);
			multiTaskData.AddTask(task3._id);
			multiTaskData.AddTask(task4._id);
			multiTaskData.AddTask(task5._id);
			multiTaskData.AddTask(task6._id);
			multiTaskData.AddTask(task7._id);
			multiTaskData.AddTask(task8._id);
			task1.ContinueWith(_onResult1, (task1._id, multiTaskData));
			task2.ContinueWith(_onResult2, (task2._id, multiTaskData));
			task3.ContinueWith(_onResult3, (task3._id, multiTaskData));
			task4.ContinueWith(_onResult4, (task4._id, multiTaskData));
			task5.ContinueWith(_onResult5, (task5._id, multiTaskData));
			task6.ContinueWith(_onResult6, (task6._id, multiTaskData));
			task7.ContinueWith(_onResult7, (task7._id, multiTaskData));
			task8.ContinueWith(_onResult8, (task8._id, multiTaskData));
			return multiTaskData.CombinedTask;
		}
	}

	private const ulong HashModifier1 = 3573116690164977347uL;

	private const ulong HashModifier2 = 10871156337175269513uL;

	public static OVRTask<TResult[]> WhenAll<TResult>(IEnumerable<OVRTask<TResult>> tasks)
	{
		return OVRTask<TResult>.WhenAll(tasks);
	}

	public static OVRTask<List<TResult>> WhenAll<TResult>(IEnumerable<OVRTask<TResult>> tasks, List<TResult> results)
	{
		return OVRTask<TResult>.WhenAll(tasks, results);
	}

	internal static OVRTask<TResult> FromGuid<TResult>(Guid id)
	{
		return Create<TResult>(id);
	}

	[Obsolete("Consider OVRTask.Build instead.")]
	internal static OVRTask<TResult> FromRequest<TResult>(ulong id)
	{
		return Create<TResult>(GetId(id));
	}

	[Obsolete("Consider OVRTask.Build instead.")]
	internal static OVRTask<TResult> FromRequest<TResult>(ulong id, OVRPlugin.EventType eventType)
	{
		return Create<TResult>(GetId(id, eventType));
	}

	internal static Builder Build(bool success, ulong requestId)
	{
		return new Builder((!success) ? OVRPlugin.Result.Failure : OVRPlugin.Result.Success, GetId(requestId));
	}

	internal static Builder Build(OVRPlugin.Result result, ulong requestId)
	{
		return new Builder(result, GetId(requestId));
	}

	internal static Builder Build(OVRPlugin.Result result, ulong requestId, OVRPlugin.EventType eventType)
	{
		return new Builder(result, GetId(requestId, eventType));
	}

	public static OVRTask<TResult> FromResult<TResult>(TResult result)
	{
		OVRTask<TResult> result2 = Create<TResult>(Guid.NewGuid());
		result2.SetResult(result);
		return result2;
	}

	[Obsolete("This method does not ensure the task exists; it just returns an OVRTask with the given id. Use TryGetPending instead.", true)]
	internal static OVRTask<TResult> GetExisting<TResult>(Guid id)
	{
		return Get<TResult>(id);
	}

	internal static bool TryGetPendingTask<TResult>(Guid id, out OVRTask<TResult> task)
	{
		task = Get<TResult>(id);
		return task.IsPending;
	}

	[Obsolete("This method does not ensure the task exists; it just returns an OVRTask with the given id. Use TryGetPending instead.", true)]
	internal static OVRTask<TResult> GetExisting<TResult>(ulong id)
	{
		return Get<TResult>(GetId(id));
	}

	internal static bool TryGetPendingTask<TResult>(ulong id, out OVRTask<TResult> task)
	{
		return TryGetPendingTask(GetId(id), out task);
	}

	public static void SetResult<TResult>(Guid id, TResult result)
	{
		OVRTask<TResult> oVRTask = Get<TResult>(id);
		if (oVRTask.HasResult)
		{
			throw new InvalidOperationException($"Task {id} already has a result.");
		}
		oVRTask.SetResult(result);
	}

	internal static void SetResult<TResult>(ulong id, TResult result)
	{
		Get<TResult>(GetId(id)).SetResult(result);
	}

	private static OVRTask<TResult> Get<TResult>(Guid id)
	{
		return new OVRTask<TResult>(id);
	}

	public static OVRTask<TResult> Create<TResult>(Guid taskId)
	{
		RegisterType<TResult>();
		OVRTask<TResult> result = Get<TResult>(taskId);
		if (!result.AddToPending())
		{
			throw new ArgumentException($"The task with id {taskId} already exists.", "taskId");
		}
		return result;
	}

	internal unsafe static Guid GetId(ulong part1, ulong part2)
	{
		return *(Guid*)stackalloc long[2]
		{
			(long)(part1 + 3573116690164977347L),
			(long)part2 + -7575587736534282103L
		};
	}

	internal static Guid GetId(ulong handle, OVRPlugin.EventType eventType)
	{
		return GetId(handle, (ulong)eventType);
	}

	internal static Guid GetId(ulong value)
	{
		return GetId(value, 0uL);
	}

	internal static ulong GetId(Guid value)
	{
		return GetIdParts(value).Item1;
	}

	internal unsafe static (ulong, ulong) GetIdParts(Guid id)
	{
		ulong* ptr = stackalloc ulong[2];
		UnsafeUtility.MemCpy(ptr, &id, sizeof(Guid));
		return (*ptr - 3573116690164977347L, ptr[1] - 10871156337175269513uL);
	}

	internal static void RegisterType<TResult>()
	{
	}

	public static OVRTask<(T1, T2)> WhenAll<T1, T2>(OVRTask<T1> task1, OVRTask<T2> task2)
	{
		return MultiTaskData<T1, T2>.Get(task1, task2);
	}

	public static OVRTask<(T1, T2, T3)> WhenAll<T1, T2, T3>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3)
	{
		return MultiTaskData<T1, T2, T3>.Get(task1, task2, task3);
	}

	public static OVRTask<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4)
	{
		return MultiTaskData<T1, T2, T3, T4>.Get(task1, task2, task3, task4);
	}

	public static OVRTask<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5)
	{
		return MultiTaskData<T1, T2, T3, T4, T5>.Get(task1, task2, task3, task4, task5);
	}

	public static OVRTask<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6)
	{
		return MultiTaskData<T1, T2, T3, T4, T5, T6>.Get(task1, task2, task3, task4, task5, task6);
	}

	public static OVRTask<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6, OVRTask<T7> task7)
	{
		return MultiTaskData<T1, T2, T3, T4, T5, T6, T7>.Get(task1, task2, task3, task4, task5, task6, task7);
	}

	public static OVRTask<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(OVRTask<T1> task1, OVRTask<T2> task2, OVRTask<T3> task3, OVRTask<T4> task4, OVRTask<T5> task5, OVRTask<T6> task6, OVRTask<T7> task7, OVRTask<T8> task8)
	{
		return MultiTaskData<T1, T2, T3, T4, T5, T6, T7, T8>.Get(task1, task2, task3, task4, task5, task6, task7, task8);
	}
}
[AsyncMethodBuilder(typeof(OVRTaskBuilder<>))]
public readonly struct OVRTask<TResult> : IEquatable<OVRTask<TResult>>, IDisposable
{
	private delegate void ContinueWithInvoker(Guid guid, TResult result);

	private delegate bool ContinueWithRemover(Guid guid);

	private delegate bool InternalDataRemover(Guid guid);

	private static class InternalData<T>
	{
		private static readonly Dictionary<Guid, T> Data = new Dictionary<Guid, T>();

		private static readonly InternalDataRemover Remover = Remove;

		private static readonly Action Clearer = Clear;

		public static bool TryGet(Guid taskId, out T data)
		{
			return Data.TryGetValue(taskId, out data);
		}

		public static void Set(Guid taskId, T data)
		{
			Data[taskId] = data;
			OVRTask<TResult>.InternalDataRemovers.Add(taskId, Remover);
			OVRTask<TResult>.InternalDataClearers.Add(Clearer);
		}

		private static bool Remove(Guid taskId)
		{
			return Data.Remove(taskId);
		}

		private static void Clear()
		{
			Data.Clear();
		}
	}

	private static class IncrementalResultSubscriber<T>
	{
		private static readonly Dictionary<Guid, Action<T>> Subscribers = new Dictionary<Guid, Action<T>>();

		private static readonly Action<Guid> Remover = Remove;

		private static readonly Action Clearer = Clear;

		public static void Set(Guid taskId, Action<T> subscriber)
		{
			Subscribers[taskId] = subscriber;
			OVRTask<TResult>.IncrementalResultSubscriberRemovers[taskId] = Remover;
			OVRTask<TResult>.IncrementalResultSubscriberClearers.Add(Clearer);
		}

		public static void Notify(Guid taskId, T result)
		{
			if (Subscribers.TryGetValue(taskId, out var value))
			{
				value(result);
			}
		}

		private static void Remove(Guid id)
		{
			Subscribers.Remove(id);
		}

		private static void Clear()
		{
			Subscribers.Clear();
		}
	}

	private readonly struct CombinedTaskData : IDisposable
	{
		public readonly OVRTask<List<TResult>> Task;

		private readonly HashSet<Guid> _remainingTaskIds;

		private readonly List<Guid> _originalTaskOrder;

		private readonly Dictionary<Guid, TResult> _completedTasks;

		private readonly List<TResult> _userOwnedResultList;

		private static readonly Action<TResult, CombinedTaskDataWithCompletedTaskId> _onSingleTaskCompleted = delegate(TResult result, CombinedTaskDataWithCompletedTaskId data)
		{
			data.CombinedData.OnSingleTaskCompleted(data.CompletedTaskId, result);
		};

		private void OnSingleTaskCompleted(Guid taskId, TResult result)
		{
			_completedTasks.Add(taskId, result);
			_remainingTaskIds.Remove(taskId);
			if (_remainingTaskIds.Count != 0)
			{
				return;
			}
			using (this)
			{
				_userOwnedResultList.Clear();
				foreach (Guid item in _originalTaskOrder)
				{
					_userOwnedResultList.Add(_completedTasks[item]);
				}
				Task.SetResult(_userOwnedResultList);
			}
		}

		public CombinedTaskData(IEnumerable<OVRTask<TResult>> tasks, List<TResult> userOwnedResultList)
		{
			Task = OVRTask.FromGuid<List<TResult>>(Guid.NewGuid());
			_remainingTaskIds = OVRObjectPool.HashSet<Guid>();
			_originalTaskOrder = OVRObjectPool.List<Guid>();
			_completedTasks = OVRObjectPool.Dictionary<Guid, TResult>();
			_userOwnedResultList = userOwnedResultList;
			_userOwnedResultList.Clear();
			List<OVRTask<TResult>> list;
			using (new OVRObjectPool.ListScope<OVRTask<TResult>>(out list))
			{
				foreach (OVRTask<TResult> item in tasks.ToNonAlloc())
				{
					list.Add(item);
					_remainingTaskIds.Add(item._id);
					_originalTaskOrder.Add(item._id);
				}
				if (list.Count == 0)
				{
					Task.SetResult(_userOwnedResultList);
					return;
				}
				foreach (OVRTask<TResult> item2 in list)
				{
					item2.ContinueWith(_onSingleTaskCompleted, new CombinedTaskDataWithCompletedTaskId
					{
						CompletedTaskId = item2._id,
						CombinedData = this
					});
				}
			}
		}

		public void Dispose()
		{
			OVRObjectPool.Return(_remainingTaskIds);
			OVRObjectPool.Return(_originalTaskOrder);
			OVRObjectPool.Return(_completedTasks);
		}
	}

	private struct CombinedTaskDataWithCompletedTaskId
	{
		public Guid CompletedTaskId;

		public CombinedTaskData CombinedData;
	}

	private class TaskSource : IValueTaskSource<TResult>, OVRObjectPool.IPoolObject
	{
		private ManualResetValueTaskSourceCore<TResult> _manualSource;

		public ValueTask<TResult> Task { get; private set; }

		public TResult GetResult(short token)
		{
			try
			{
				return _manualSource.GetResult(token);
			}
			finally
			{
				OVRObjectPool.Return(this);
			}
		}

		public ValueTaskSourceStatus GetStatus(short token)
		{
			return _manualSource.GetStatus(token);
		}

		public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
		{
			_manualSource.OnCompleted(continuation, state, token, flags);
		}

		void OVRObjectPool.IPoolObject.OnGet()
		{
			_manualSource.Reset();
			Task = new ValueTask<TResult>(this, _manualSource.Version);
		}

		void OVRObjectPool.IPoolObject.OnReturn()
		{
		}

		public void SetResult(TResult result)
		{
			_manualSource.SetResult(result);
		}

		public void SetException(Exception exception)
		{
			_manualSource.SetException(exception);
		}
	}

	private class AwaitableSource : AwaitableCompletionSource<TResult>, OVRObjectPool.IPoolObject
	{
		public void OnGet()
		{
			Reset();
		}

		public void OnReturn()
		{
		}

		public void SetResultAndReturnToPool(in TResult result)
		{
			try
			{
				SetResult(in result);
			}
			finally
			{
				OVRObjectPool.Return(this);
			}
		}
	}

	public readonly struct Awaiter : INotifyCompletion
	{
		private readonly OVRTask<TResult> _task;

		public bool IsCompleted => _task.IsCompleted;

		internal Awaiter(OVRTask<TResult> task)
		{
			_task = task;
		}

		void INotifyCompletion.OnCompleted(Action continuation)
		{
			_task.WithContinuation(continuation);
		}

		public TResult GetResult()
		{
			return _task.GetResult();
		}
	}

	private readonly struct Callback
	{
		private static readonly Dictionary<Guid, Callback> Callbacks = new Dictionary<Guid, Callback>();

		private readonly Action<TResult> _delegate;

		public static readonly ContinueWithInvoker Invoker = Invoke;

		public static readonly ContinueWithRemover Remover = Remove;

		public static readonly Action Clearer = Clear;

		private static void Invoke(Guid taskId, TResult result)
		{
			if (Callbacks.TryGetValue(taskId, out var value))
			{
				Callbacks.Remove(taskId);
				value.Invoke(result);
			}
		}

		private static bool Remove(Guid taskId)
		{
			return Callbacks.Remove(taskId);
		}

		private static void Clear()
		{
			Callbacks.Clear();
		}

		private void Invoke(TResult result)
		{
			_delegate(result);
		}

		private Callback(Action<TResult> @delegate)
		{
			_delegate = @delegate;
		}

		public static void Add(Guid taskId, Action<TResult> @delegate)
		{
			Callbacks.Add(taskId, new Callback(@delegate));
			OVRTask<TResult>.ContinueWithInvokers.Add(taskId, Invoker);
			OVRTask<TResult>.ContinueWithRemovers.Add(taskId, Remover);
			OVRTask<TResult>.ContinueWithClearers.Add(Clearer);
		}
	}

	private readonly struct CallbackWithState<T>
	{
		private static readonly Dictionary<Guid, CallbackWithState<T>> Callbacks = new Dictionary<Guid, CallbackWithState<T>>();

		private readonly T _data;

		private readonly Action<TResult, T> _delegate;

		private static readonly ContinueWithInvoker Invoker = Invoke;

		private static readonly ContinueWithRemover Remover = Remove;

		private static readonly Action Clearer = Clear;

		private static void Invoke(Guid taskId, TResult result)
		{
			if (Callbacks.TryGetValue(taskId, out var value))
			{
				Callbacks.Remove(taskId);
				value.Invoke(result);
			}
		}

		private CallbackWithState(T data, Action<TResult, T> @delegate)
		{
			_data = data;
			_delegate = @delegate;
		}

		private static void Clear()
		{
			Callbacks.Clear();
		}

		private static bool Remove(Guid taskId)
		{
			return Callbacks.Remove(taskId);
		}

		private void Invoke(TResult result)
		{
			_delegate(result, _data);
		}

		public static void Add(Guid taskId, T data, Action<TResult, T> callback)
		{
			Callbacks.Add(taskId, new CallbackWithState<T>(data, callback));
			OVRTask<TResult>.ContinueWithInvokers.Add(taskId, Invoker);
			OVRTask<TResult>.ContinueWithRemovers.Add(taskId, Remover);
			OVRTask<TResult>.ContinueWithClearers.Add(Clearer);
		}
	}

	private static readonly HashSet<Guid> Pending;

	private static readonly Dictionary<Guid, TResult> Results;

	private static readonly Dictionary<Guid, Exception> Exceptions;

	private static readonly Dictionary<Guid, TaskSource> Sources;

	private static readonly Dictionary<Guid, AwaitableSource> AwaitableSources;

	private static readonly Dictionary<Guid, Action> Continuations;

	private static readonly Dictionary<Guid, ContinueWithInvoker> ContinueWithInvokers;

	private static readonly Dictionary<Guid, ContinueWithRemover> ContinueWithRemovers;

	private static readonly HashSet<Action> ContinueWithClearers;

	private static readonly Dictionary<Guid, InternalDataRemover> InternalDataRemovers;

	private static readonly HashSet<Action> InternalDataClearers;

	private static readonly Dictionary<Guid, Action<Guid>> IncrementalResultSubscriberRemovers;

	private static readonly HashSet<Action> IncrementalResultSubscriberClearers;

	internal static readonly Action Clear;

	internal readonly Guid _id;

	private static readonly Action<List<TResult>, OVRTask<TResult[]>> _onCombinedTaskCompleted;

	internal bool IsPending => Pending.Contains(_id);

	public bool IsCompleted => !IsPending;

	public bool IsFaulted => Exceptions.ContainsKey(_id);

	public bool HasResult => Results.ContainsKey(_id);

	internal OVRTask(Guid id)
	{
		_id = id;
	}

	static OVRTask()
	{
		Pending = new HashSet<Guid>();
		Results = new Dictionary<Guid, TResult>();
		Exceptions = new Dictionary<Guid, Exception>();
		Sources = new Dictionary<Guid, TaskSource>();
		AwaitableSources = new Dictionary<Guid, AwaitableSource>();
		Continuations = new Dictionary<Guid, Action>();
		ContinueWithInvokers = new Dictionary<Guid, ContinueWithInvoker>();
		ContinueWithRemovers = new Dictionary<Guid, ContinueWithRemover>();
		ContinueWithClearers = new HashSet<Action>();
		InternalDataRemovers = new Dictionary<Guid, InternalDataRemover>();
		InternalDataClearers = new HashSet<Action>();
		IncrementalResultSubscriberRemovers = new Dictionary<Guid, Action<Guid>>();
		IncrementalResultSubscriberClearers = new HashSet<Action>();
		Clear = delegate
		{
			Results.Clear();
			Continuations.Clear();
			Pending.Clear();
			Exceptions.Clear();
			ContinueWithInvokers.Clear();
			foreach (Action continueWithClearer in ContinueWithClearers)
			{
				continueWithClearer();
			}
			ContinueWithClearers.Clear();
			ContinueWithRemovers.Clear();
			foreach (Action internalDataClearer in InternalDataClearers)
			{
				internalDataClearer();
			}
			InternalDataClearers.Clear();
			InternalDataRemovers.Clear();
			foreach (Action incrementalResultSubscriberClearer in IncrementalResultSubscriberClearers)
			{
				incrementalResultSubscriberClearer();
			}
			IncrementalResultSubscriberClearers.Clear();
			IncrementalResultSubscriberRemovers.Clear();
			foreach (TaskSource value in Sources.Values)
			{
				OVRObjectPool.Return(value);
			}
			Sources.Clear();
			foreach (AwaitableSource value2 in AwaitableSources.Values)
			{
				OVRObjectPool.Return(value2);
			}
			AwaitableSources.Clear();
		};
		_onCombinedTaskCompleted = delegate(List<TResult> resultsFromPool, OVRTask<TResult[]> task)
		{
			TResult[] result = resultsFromPool.ToArray();
			OVRObjectPool.Return(resultsFromPool);
			task.SetResult(result);
		};
		OVRTask.RegisterType<TResult>();
	}

	internal bool AddToPending()
	{
		return Pending.Add(_id);
	}

	internal void SetInternalData<T>(T data)
	{
		InternalData<T>.Set(_id, data);
	}

	internal OVRTask<TResult> WithInternalData<T>(T data)
	{
		if (!HasResult)
		{
			InternalData<T>.Set(_id, data);
		}
		return this;
	}

	internal bool TryGetInternalData<T>(out T data)
	{
		return InternalData<T>.TryGet(_id, out data);
	}

	internal void SetException(Exception exception)
	{
		if (AwaitableSources.Remove(_id, out var value))
		{
			value.SetException(exception);
			return;
		}
		if (Sources.Remove(_id, out var value2))
		{
			value2.SetException(exception);
			return;
		}
		if (TryRemoveInternalData())
		{
			if (ContinueWithInvokers.Remove(_id, out var _))
			{
				ExceptionDispatchInfo.Capture(exception).Throw();
			}
			Exceptions.Add(_id, exception);
			TryInvokeContinuation();
			return;
		}
		throw new InvalidOperationException($"The exception {exception} cannot be set on task {_id} because it is not a valid task.", exception);
	}

	private bool TryRemoveInternalData()
	{
		if (!Pending.Remove(_id))
		{
			return false;
		}
		if (InternalDataRemovers.Remove(_id, out var value))
		{
			value(_id);
		}
		if (IncrementalResultSubscriberRemovers.Remove(_id, out var value2))
		{
			value2(_id);
		}
		return true;
	}

	private bool TryInvokeContinuation()
	{
		if (Continuations.Remove(_id, out var value))
		{
			value();
			return true;
		}
		return false;
	}

	internal void SetResult(TResult result)
	{
		TaskSource value2;
		if (AwaitableSources.Remove(_id, out var value))
		{
			value.SetResultAndReturnToPool(in result);
		}
		else if (Sources.Remove(_id, out value2))
		{
			value2.SetResult(result);
		}
		else if (TryRemoveInternalData())
		{
			if (ContinueWithInvokers.Remove(_id, out var value3))
			{
				value3(_id, result);
				return;
			}
			Results.Add(_id, result);
			TryInvokeContinuation();
		}
	}

	internal void SetIncrementalResultCallback<TIncrementalResult>(Action<TIncrementalResult> onIncrementalResultAvailable)
	{
		if (onIncrementalResultAvailable == null)
		{
			throw new ArgumentNullException("onIncrementalResultAvailable");
		}
		IncrementalResultSubscriber<TIncrementalResult>.Set(_id, onIncrementalResultAvailable);
	}

	internal void NotifyIncrementalResult<TIncrementalResult>(TIncrementalResult incrementalResult)
	{
		IncrementalResultSubscriber<TIncrementalResult>.Notify(_id, incrementalResult);
	}

	internal static OVRTask<List<TResult>> WhenAll(IEnumerable<OVRTask<TResult>> tasks, List<TResult> results)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		return new CombinedTaskData(tasks, results).Task;
	}

	internal static OVRTask<TResult[]> WhenAll(IEnumerable<OVRTask<TResult>> tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		OVRTask<TResult[]> oVRTask = OVRTask.FromGuid<TResult[]>(Guid.NewGuid());
		List<TResult> results = OVRObjectPool.List<TResult>();
		WhenAll(tasks, results).ContinueWith(_onCombinedTaskCompleted, oVRTask);
		return oVRTask;
	}

	public Exception GetException()
	{
		if (!Exceptions.Remove(_id, out var value))
		{
			throw new InvalidOperationException(string.Format("Task {0} is not in a faulted state. Check with {1}", _id, "IsFaulted"));
		}
		return value;
	}

	public TResult GetResult()
	{
		if (Exceptions.Remove(_id, out var value))
		{
			ExceptionDispatchInfo.Capture(value).Throw();
		}
		if (!TryGetResult(out var result))
		{
			throw new InvalidOperationException($"Task {_id} doesn't have any available result.");
		}
		return result;
	}

	public bool TryGetResult(out TResult result)
	{
		return Results.Remove(_id, out result);
	}

	public ValueTask<TResult> ToValueTask()
	{
		TResult value;
		bool flag = Results.TryGetValue(_id, out value);
		if (!Pending.Contains(_id) && !flag)
		{
			throw new InvalidOperationException($"Task {_id} is not a valid task.");
		}
		if (Continuations.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used by an await call.");
		}
		if (ContinueWithInvokers.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used with ContinueWith.");
		}
		using (this)
		{
			if (flag)
			{
				Results.Remove(_id);
				return new ValueTask<TResult>(value);
			}
			TaskSource taskSource = OVRObjectPool.Get<TaskSource>();
			Sources.Add(_id, taskSource);
			return taskSource.Task;
		}
	}

	public Awaitable<TResult> ToAwaitable()
	{
		TResult value;
		bool flag = Results.TryGetValue(_id, out value);
		if (!Pending.Contains(_id) && !flag)
		{
			throw new InvalidOperationException($"Task {_id} is not a valid task.");
		}
		if (Continuations.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used by an await call.");
		}
		if (ContinueWithInvokers.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used with ContinueWith.");
		}
		using (this)
		{
			AwaitableSource awaitableSource = OVRObjectPool.Get<AwaitableSource>();
			if (flag)
			{
				awaitableSource.SetResult(in value);
			}
			else
			{
				AwaitableSources.Add(_id, awaitableSource);
			}
			return awaitableSource.Awaitable;
		}
	}

	public Awaiter GetAwaiter()
	{
		return new Awaiter(this);
	}

	private void WithContinuation(Action continuation)
	{
		ValidateDelegateAndThrow(continuation, "continuation");
		Continuations[_id] = continuation;
	}

	public void ContinueWith(Action<TResult> onCompleted)
	{
		ValidateDelegateAndThrow(onCompleted, "onCompleted");
		if (IsCompleted)
		{
			onCompleted(GetResult());
		}
		else
		{
			Callback.Add(_id, onCompleted);
		}
	}

	public void ContinueWith<T>(Action<TResult, T> onCompleted, T state)
	{
		ValidateDelegateAndThrow(onCompleted, "onCompleted");
		if (IsCompleted)
		{
			onCompleted(GetResult(), state);
		}
		else
		{
			CallbackWithState<T>.Add(_id, state, onCompleted);
		}
	}

	private void ValidateDelegateAndThrow(object @delegate, string paramName)
	{
		if (@delegate == null)
		{
			throw new ArgumentNullException(paramName);
		}
		if (Continuations.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used by an await call.");
		}
		if (ContinueWithInvokers.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used with ContinueWith.");
		}
		if (Sources.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used as a ValueTask.");
		}
		if (AwaitableSources.ContainsKey(_id))
		{
			throw new InvalidOperationException($"Task {_id} is already being used as an Awaitable.");
		}
	}

	public void Dispose()
	{
		Results.Remove(_id);
		Continuations.Remove(_id);
		Pending.Remove(_id);
		ContinueWithInvokers.Remove(_id);
		if (ContinueWithRemovers.TryGetValue(_id, out var value))
		{
			ContinueWithRemovers.Remove(_id);
			value(_id);
		}
		if (InternalDataRemovers.TryGetValue(_id, out var value2))
		{
			InternalDataRemovers.Remove(_id);
			value2(_id);
		}
		if (IncrementalResultSubscriberRemovers.TryGetValue(_id, out var value3))
		{
			IncrementalResultSubscriberRemovers.Remove(_id);
			value3(_id);
		}
	}

	public bool Equals(OVRTask<TResult> other)
	{
		return _id == other._id;
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRTask<TResult> other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(OVRTask<TResult> lhs, OVRTask<TResult> rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRTask<TResult> lhs, OVRTask<TResult> rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		return _id.GetHashCode();
	}

	public override string ToString()
	{
		return _id.ToString();
	}
}
