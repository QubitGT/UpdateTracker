#define DEBUG
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
public struct TickRate
{
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct Selection
	{
		[FieldOffset(0)]
		public int Client;

		[FieldOffset(4)]
		public int ServerIndex;

		[FieldOffset(8)]
		public int ClientSendIndex;

		[FieldOffset(12)]
		public int ServerSendIndex;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Resolved
	{
		public const int SIZE = 16;

		public const int WORDS = 4;

		[FieldOffset(0)]
		public int Client;

		[FieldOffset(4)]
		public int ClientSend;

		[FieldOffset(8)]
		public int Server;

		[FieldOffset(12)]
		public int ServerSend;

		public double ServerTickDelta => Inverse(Server);

		public double ServerSendDelta => Inverse(ServerSend);

		public int ServerTickStride => Client / Server;

		public double ClientTickDelta => Inverse(Client);

		public double ClientSendDelta => Inverse(ClientSend);

		public int ClientTickStride => 1;

		internal Resolved(int client, int clientSend, int server, int serverSend)
		{
			Client = client;
			ClientSend = clientSend;
			Server = server;
			ServerSend = serverSend;
		}

		public override string ToString()
		{
			return $"[ClientTickRate = {Client}, ClientSendRate = {ClientSend}, ServerTickRate = {Server}, ServerSendRate = {ServerSend}]";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double Inverse(int rate)
		{
			return (rate == 0) ? 0.0 : (1.0 / (double)rate);
		}
	}

	public enum ValidateResult
	{
		Ok,
		Error,
		NotFound,
		InvalidTickRate,
		ServerIndexOutOfRange,
		ClientSendIndexOutOfRange,
		ServerSendIndexOutOfRange,
		ServerSendRateLargerThanTickRate
	}

	[FieldOffset(0)]
	private int _count;

	[FieldOffset(4)]
	private unsafe fixed int _rates[4];

	private static TickRate[] _valid;

	private static ReadOnlyCollection<TickRate> _validReadOnly;

	private static Dictionary<int, TickRate> _lookup;

	public unsafe int Client => _rates[0];

	public int Count => _count;

	public int this[int index] => GetTickRate(index);

	internal static Selection Default
	{
		get
		{
			Selection result = default(Selection);
			result.Client = 64;
			result.ClientSendIndex = 1;
			result.ServerIndex = 0;
			result.ServerSendIndex = 1;
			return result;
		}
	}

	internal static Selection Shared
	{
		get
		{
			Selection result = default(Selection);
			result.Client = 32;
			result.ClientSendIndex = 1;
			result.ServerIndex = 0;
			result.ServerSendIndex = 1;
			return result;
		}
	}

	public static IReadOnlyList<TickRate> Available
	{
		get
		{
			InitChecked();
			return _validReadOnly;
		}
	}

	private unsafe TickRate(params int[] rates)
	{
		Assert.Check(rates.Length >= 1 && rates.Length <= 4);
		_count = rates.Length;
		for (int i = 0; i < rates.Length; i++)
		{
			_rates[i] = rates[i];
		}
	}

	public int GetDivisor(int index)
	{
		Assert.Check((uint)index < _count);
		Assert.Check(Client == Client / GetTickRate(index) * GetTickRate(index));
		return Client / GetTickRate(index);
	}

	public unsafe int GetTickRate(int index)
	{
		Assert.Check((uint)index < _count);
		return _rates[index];
	}

	public int[] ToArray()
	{
		int[] array = new int[_count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetTickRate(i);
		}
		return array;
	}

	private unsafe bool Validate()
	{
		if (_count == 0)
		{
			return false;
		}
		if (_count > 4)
		{
			return false;
		}
		if (_rates[0] <= 0)
		{
			return false;
		}
		for (int i = 1; i < _count; i++)
		{
			if (_rates[0] != _rates[0] / GetTickRate(i) * GetTickRate(i))
			{
				return false;
			}
		}
		return true;
	}

	public Selection ClampSelection(Selection selection)
	{
		if (!Validate())
		{
			return default(Selection);
		}
		selection.ServerIndex = Maths.Clamp(selection.ServerIndex, 0, _count - 1);
		selection.ServerSendIndex = Maths.Clamp(selection.ServerSendIndex, selection.ServerIndex, _count - 1);
		selection.ClientSendIndex = Maths.Clamp(selection.ClientSendIndex, 0, _count - 1);
		return selection;
	}

	public ValidateResult ValidateSelection(Selection selected)
	{
		if (!Validate())
		{
			return ValidateResult.InvalidTickRate;
		}
		if (Client != selected.Client)
		{
			return ValidateResult.Error;
		}
		if ((uint)selected.ServerIndex >= (uint)_count)
		{
			return ValidateResult.ServerIndexOutOfRange;
		}
		if ((uint)selected.ServerSendIndex >= (uint)_count)
		{
			return ValidateResult.ServerSendIndexOutOfRange;
		}
		if ((uint)selected.ClientSendIndex >= (uint)_count)
		{
			return ValidateResult.ClientSendIndexOutOfRange;
		}
		if (selected.ServerSendIndex < selected.ServerIndex)
		{
			return ValidateResult.ServerSendRateLargerThanTickRate;
		}
		return ValidateResult.Ok;
	}

	static TickRate()
	{
		Init();
	}

	private static void InitChecked()
	{
		if (_valid == null || _valid.Length == 0 || _lookup == null || _lookup.Count <= 0)
		{
			Init();
		}
	}

	public static void Init()
	{
		_valid = new TickRate[15]
		{
			new TickRate(8, 4),
			new TickRate(10, 5),
			new TickRate(16, 8, 4),
			new TickRate(20, 10, 5),
			new TickRate(24, 12, 6),
			new TickRate(30, 15),
			new TickRate(32, 16, 8),
			new TickRate(50, 25),
			new TickRate(60, 30, 15),
			new TickRate(64, 32, 16),
			new TickRate(100, 50, 25),
			new TickRate(120, 60, 30),
			new TickRate(128, 64, 32),
			new TickRate(240, 120, 60, 30),
			new TickRate(256, 128, 64, 32)
		};
		_validReadOnly = new ReadOnlyCollection<TickRate>(_valid);
		_lookup = new Dictionary<int, TickRate>();
		TickRate[] valid = _valid;
		for (int i = 0; i < valid.Length; i++)
		{
			TickRate value = valid[i];
			_lookup.Add(value.Client, value);
		}
	}

	public static bool IsValid(TickRate rate)
	{
		return IsValid(rate.Client);
	}

	public static bool IsValid(int rate)
	{
		InitChecked();
		return _lookup.ContainsKey(rate);
	}

	public static TickRate Get(int rate)
	{
		InitChecked();
		if (_lookup.TryGetValue(rate, out var value))
		{
			return value;
		}
		throw new InvalidOperationException("invalid tickrate");
	}

	public static Resolved Resolve(Selection selection)
	{
		InitChecked();
		Assert.Always(IsValid(selection.Client), "IsValid(selection.Client)");
		TickRate tickRate = _lookup[selection.Client];
		ValidateResult validateResult = tickRate.ValidateSelection(selection);
		Assert.Always(validateResult == ValidateResult.Ok, "result != ValidateResult.Ok");
		return new Resolved(tickRate.Client, tickRate.GetTickRate(selection.ClientSendIndex), tickRate.GetTickRate(selection.ServerIndex), tickRate.GetTickRate(selection.ServerSendIndex));
	}
}
