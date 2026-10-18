using System.Runtime.CompilerServices;
using System.Text;

namespace Fusion;

public static class LogUtils
{
	public readonly struct DumpDeferredPtr<T> where T : unmanaged, ILogDumpable
	{
		[CompilerGenerated]
		private unsafe readonly T* _003Cptr_003EP;

		public unsafe DumpDeferredPtr(T* ptr)
		{
			_003Cptr_003EP = ptr;
		}

		public unsafe override string ToString()
		{
			if (_003Cptr_003EP != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				_003Cptr_003EP->Dump(stringBuilder);
				return stringBuilder.ToString();
			}
			return "null";
		}
	}

	public readonly struct DumpDeferredStruct<T> where T : unmanaged, ILogDumpable
	{
		[CompilerGenerated]
		private readonly T _003Cobj_003EP;

		public DumpDeferredStruct(T obj)
		{
			_003Cobj_003EP = obj;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			_003Cobj_003EP.Dump(stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public readonly struct DumpDeferredClass
	{
		public readonly ILogDumpable Obj;

		public DumpDeferredClass(ILogDumpable obj)
		{
			Obj = obj;
		}

		public override string ToString()
		{
			if (Obj != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				Obj.Dump(stringBuilder);
				return stringBuilder.ToString();
			}
			return "null";
		}
	}

	public unsafe static DumpDeferredPtr<T> GetDump<T>(T* ptr) where T : unmanaged, ILogDumpable
	{
		return new DumpDeferredPtr<T>(ptr);
	}

	public static DumpDeferredClass GetDump<T>(T obj) where T : class, ILogDumpable
	{
		return new DumpDeferredClass(obj);
	}
}
