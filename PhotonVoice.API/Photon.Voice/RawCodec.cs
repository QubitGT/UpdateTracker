using System;
using System.Runtime.InteropServices;

namespace Photon.Voice;

public class RawCodec
{
	public class Encoder<T> : IEncoderDirect<T[]>, IEncoder, IDisposable
	{
		private int sizeofT = Marshal.SizeOf(default(T));

		private byte[] byteBuf = new byte[0];

		private static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(new byte[0]);

		public string Error { get; private set; }

		public Action<ArraySegment<byte>, FrameFlags> Output { get; set; }

		public ArraySegment<byte> DequeueOutput(out FrameFlags flags)
		{
			flags = (FrameFlags)0;
			return EmptyBuffer;
		}

		public void EndOfStream()
		{
		}

		public I GetPlatformAPI<I>() where I : class
		{
			return null;
		}

		public void Dispose()
		{
		}

		public void Input(T[] buf)
		{
			if (Error != null)
			{
				return;
			}
			if (Output == null)
			{
				Error = "RawCodec.Encoder: Output action is not set";
			}
			else if (buf != null && buf.Length != 0)
			{
				int num = buf.Length * sizeofT;
				if (byteBuf.Length < num)
				{
					byteBuf = new byte[num];
				}
				Buffer.BlockCopy(buf, 0, byteBuf, 0, num);
				Output(new ArraySegment<byte>(byteBuf, 0, num), (FrameFlags)0);
			}
		}
	}

	public class Decoder<T> : IDecoder, IDisposable
	{
		private T[] buf = new T[0];

		private int sizeofT = Marshal.SizeOf(default(T));

		private Action<FrameOut<T>> output;

		public string Error { get; private set; }

		public Decoder(Action<FrameOut<T>> output)
		{
			this.output = output;
		}

		public void Open(VoiceInfo info)
		{
		}

		public void Input(ref FrameBuffer byteBuf)
		{
			if (byteBuf.Array != null && byteBuf.Length != 0)
			{
				int num = byteBuf.Length / sizeofT;
				if (buf.Length < num)
				{
					buf = new T[num];
				}
				Buffer.BlockCopy(byteBuf.Array, byteBuf.Offset, buf, 0, byteBuf.Length);
				output(new FrameOut<T>(buf, endOfStream: false));
			}
		}

		public void Dispose()
		{
		}
	}

	public class ShortToFloat
	{
		private Action<FrameOut<float>> output;

		private float[] buf = new float[0];

		public ShortToFloat(Action<FrameOut<float>> output)
		{
			this.output = output;
		}

		public void Output(FrameOut<short> shortBuf)
		{
			if (buf.Length < shortBuf.Buf.Length)
			{
				buf = new float[shortBuf.Buf.Length];
			}
			AudioUtil.Convert(shortBuf.Buf, buf, shortBuf.Buf.Length);
			output(new FrameOut<float>(buf, endOfStream: false));
		}
	}
}
