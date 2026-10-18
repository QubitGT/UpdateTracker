using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public readonly struct OVRMarkerPayload : IOVRAnchorComponent<OVRMarkerPayload>, IEquatable<OVRMarkerPayload>
{
	public static readonly OVRMarkerPayload Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRMarkerPayload>.Type => Type;

	ulong IOVRAnchorComponent<OVRMarkerPayload>.Handle => Handle;

	public bool IsNull => Handle == 0;

	public bool IsEnabled
	{
		get
		{
			bool enabled = default(bool);
			bool changePending = default(bool);
			if (!IsNull && OVRPlugin.GetSpaceComponentStatus(Handle, Type, out enabled, out changePending) && enabled)
			{
				return !changePending;
			}
			return false;
		}
	}

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.MarkerPayload;

	internal ulong Handle { get; }

	public OVRMarkerPayloadType PayloadType
	{
		get
		{
			OVRPlugin.SpaceMarkerPayload payload = default(OVRPlugin.SpaceMarkerPayload);
			if (!OVRPlugin.GetSpaceMarkerPayload(Handle, ref payload).IsSuccess())
			{
				return OVRMarkerPayloadType.InvalidQRCode;
			}
			return (OVRMarkerPayloadType)payload.PayloadType;
		}
	}

	public ArraySegment<byte> Bytes
	{
		get
		{
			int byteCount = ByteCount;
			if (byteCount == 0)
			{
				return Array.Empty<byte>();
			}
			byte[] array = new byte[byteCount];
			return new ArraySegment<byte>(array, 0, GetBytes(array));
		}
	}

	public int ByteCount
	{
		get
		{
			OVRPlugin.SpaceMarkerPayload payload = default(OVRPlugin.SpaceMarkerPayload);
			if (!OVRPlugin.GetSpaceMarkerPayload(Handle, ref payload).IsSuccess())
			{
				return 0;
			}
			return (int)payload.BufferCountOutput;
		}
	}

	OVRMarkerPayload IOVRAnchorComponent<OVRMarkerPayload>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRMarkerPayload(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRMarkerPayload>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The MarkerPayload component cannot be enabled or disabled.");
	}

	public bool Equals(OVRMarkerPayload other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRMarkerPayload lhs, OVRMarkerPayload rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRMarkerPayload lhs, OVRMarkerPayload rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRMarkerPayload other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + ((int)Type).GetHashCode();
	}

	public override string ToString()
	{
		return $"{Handle}.MarkerPayload";
	}

	private OVRMarkerPayload(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	public unsafe string AsString()
	{
		if (PayloadType != OVRMarkerPayloadType.StringQRCode)
		{
			throw new InvalidOperationException(string.Format("{0} must be {1}.", "PayloadType", OVRMarkerPayloadType.StringQRCode));
		}
		using NativeArray<byte> nativeArray = new NativeArray<byte>(ByteCount, Allocator.Temp);
		void* unsafeReadOnlyPtr = nativeArray.GetUnsafeReadOnlyPtr();
		return Marshal.PtrToStringUTF8(new IntPtr(unsafeReadOnlyPtr), GetBytes(new Span<byte>(unsafeReadOnlyPtr, nativeArray.Length)));
	}

	public unsafe int GetBytes(Span<byte> buffer)
	{
		fixed (byte* buffer2 = buffer)
		{
			OVRPlugin.SpaceMarkerPayload spaceMarkerPayload = default(OVRPlugin.SpaceMarkerPayload);
			spaceMarkerPayload.BufferCapacityInput = (uint)buffer.Length;
			spaceMarkerPayload.Buffer = buffer2;
			OVRPlugin.SpaceMarkerPayload payload = spaceMarkerPayload;
			OVRPlugin.Result spaceMarkerPayload2 = OVRPlugin.GetSpaceMarkerPayload(Handle, ref payload);
			if (spaceMarkerPayload2 == OVRPlugin.Result.Failure_InsufficientSize)
			{
				throw new ArgumentException("buffer is not large enough to hold the payload data. It " + $"must be at least {payload.BufferCountOutput} but was {buffer.Length}.", "buffer");
			}
			if (!spaceMarkerPayload2.IsSuccess())
			{
				return 0;
			}
			return (int)payload.BufferCountOutput;
		}
	}
}
