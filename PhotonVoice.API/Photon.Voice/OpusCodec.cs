using System;
using POpusCodec;
using POpusCodec.Enums;

namespace Photon.Voice;

public class OpusCodec
{
	public enum FrameDuration
	{
		Frame2dot5ms = 2500,
		Frame5ms = 5000,
		Frame10ms = 10000,
		Frame20ms = 20000,
		Frame40ms = 40000,
		Frame60ms = 60000
	}

	public static class Factory
	{
		public static IEncoder CreateEncoder<B>(VoiceInfo i, ILogger logger)
		{
			if (typeof(B) == typeof(float[]))
			{
				return new EncoderFloat(i, logger);
			}
			if (typeof(B) == typeof(short[]))
			{
				return new EncoderShort(i, logger);
			}
			throw new UnsupportedCodecException("Factory.CreateEncoder<" + typeof(B)?.ToString() + ">", i.Codec);
		}
	}

	public static class DecoderFactory
	{
		public static IEncoder Create<T>(VoiceInfo i, ILogger logger)
		{
			T[] array = new T[1];
			if (array[0].GetType() == typeof(float))
			{
				return new EncoderFloat(i, logger);
			}
			if (array[0].GetType() == typeof(short))
			{
				return new EncoderShort(i, logger);
			}
			throw new UnsupportedCodecException("EncoderFactory.Create<" + array[0].GetType()?.ToString() + ">", i.Codec);
		}
	}

	public abstract class Encoder<T> : IEncoderDirect<T[]>, IEncoder, IDisposable
	{
		protected OpusEncoder encoder;

		protected bool disposed;

		private static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(new byte[0]);

		public string Error { get; private set; }

		public Action<ArraySegment<byte>, FrameFlags> Output { get; set; }

		protected Encoder(VoiceInfo i, ILogger logger)
		{
			try
			{
				encoder = new OpusEncoder((SamplingRate)i.SamplingRate, (Channels)i.Channels, i.Bitrate, OpusApplicationType.Voip, (Delay)(i.FrameDurationUs * 2 / 1000));
				logger.LogInfo("[PV] OpusCodec.Encoder created. Opus version " + Version + ". Bitrate " + encoder.Bitrate + ". EncoderDelay " + encoder.EncoderDelay);
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in OpusCodec.Encoder constructor";
				}
				logger.LogError("[PV] OpusCodec.Encoder: " + Error);
			}
		}

		public void Input(T[] buf)
		{
			if (Error != null)
			{
				return;
			}
			if (Output == null)
			{
				Error = "OpusCodec.Encoder: Output action is not set";
				return;
			}
			lock (this)
			{
				if (!disposed && Error == null)
				{
					ArraySegment<byte> arg = encodeTyped(buf);
					if (arg.Count != 0)
					{
						Output(arg, (FrameFlags)0);
					}
				}
			}
		}

		public void EndOfStream()
		{
			lock (this)
			{
				if (!disposed && Error == null)
				{
					Output(EmptyBuffer, FrameFlags.EndOfStream);
				}
			}
		}

		public ArraySegment<byte> DequeueOutput(out FrameFlags flags)
		{
			flags = (FrameFlags)0;
			return EmptyBuffer;
		}

		protected abstract ArraySegment<byte> encodeTyped(T[] buf);

		public I GetPlatformAPI<I>() where I : class
		{
			return null;
		}

		public void Dispose()
		{
			lock (this)
			{
				if (encoder != null)
				{
					encoder.Dispose();
				}
				disposed = true;
			}
		}
	}

	public class EncoderFloat : Encoder<float>
	{
		internal EncoderFloat(VoiceInfo i, ILogger logger)
			: base(i, logger)
		{
		}

		protected override ArraySegment<byte> encodeTyped(float[] buf)
		{
			return encoder.Encode(buf);
		}
	}

	public class EncoderShort : Encoder<short>
	{
		internal EncoderShort(VoiceInfo i, ILogger logger)
			: base(i, logger)
		{
		}

		protected override ArraySegment<byte> encodeTyped(short[] buf)
		{
			return encoder.Encode(buf);
		}
	}

	public class Decoder<T> : IDecoder, IDisposable
	{
		protected OpusDecoder<T> decoder;

		private ILogger logger;

		private Action<FrameOut<T>> output;

		private FrameOut<T> frameOut = new FrameOut<T>(null, endOfStream: false);

		public string Error { get; private set; }

		public Decoder(Action<FrameOut<T>> output, ILogger logger)
		{
			this.output = output;
			this.logger = logger;
		}

		public void Open(VoiceInfo i)
		{
			try
			{
				decoder = new OpusDecoder<T>((SamplingRate)i.SamplingRate, (Channels)i.Channels);
				logger.LogInfo("[PV] OpusCodec.Decoder created. Opus version " + Version);
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in OpusCodec.Decoder constructor";
				}
				logger.LogError("[PV] OpusCodec.Decoder: " + Error);
			}
		}

		public void Dispose()
		{
			if (decoder != null)
			{
				decoder.Dispose();
			}
		}

		public void Input(ref FrameBuffer buf)
		{
			if (Error != null)
			{
				return;
			}
			if ((buf.Flags & FrameFlags.EndOfStream) != 0)
			{
				T[] array = null;
				if (buf.Array == null && buf.Length > 0)
				{
					array = decoder.DecodePacket(ref buf);
				}
				T[] array2 = decoder.DecodeEndOfStream();
				if (array != null && array.Length == 0)
				{
					if (array2 != null && array2.Length != 0)
					{
						output(frameOut.Set(array, endOfStream: false));
					}
					else
					{
						array2 = array;
					}
				}
				output(frameOut.Set(array2, endOfStream: true));
			}
			else
			{
				T[] array3 = decoder.DecodePacket(ref buf);
				if (array3.Length != 0)
				{
					output(frameOut.Set(array3, endOfStream: false));
				}
			}
		}
	}

	public class Util
	{
		internal static int bestEncoderSampleRate(int f)
		{
			int num = int.MaxValue;
			int result = 48000;
			foreach (object value in Enum.GetValues(typeof(SamplingRate)))
			{
				int num2 = Math.Abs((int)value - f);
				if (num2 < num)
				{
					num = num2;
					result = (int)value;
				}
			}
			return result;
		}
	}

	public static string Version => OpusLib.Version;
}
