namespace Photon.Voice;

public struct ImageBufferInfo
{
	public struct StrideSet
	{
		private int stride0;

		private int stride1;

		private int stride2;

		private int stride3;

		public int this[int key]
		{
			get
			{
				return key switch
				{
					0 => stride0, 
					1 => stride1, 
					2 => stride2, 
					3 => stride3, 
					_ => 0, 
				};
			}
			set
			{
				switch (key)
				{
				case 0:
					stride0 = value;
					break;
				case 1:
					stride1 = value;
					break;
				case 2:
					stride2 = value;
					break;
				case 3:
					stride3 = value;
					break;
				}
			}
		}

		public int Length { get; private set; }

		public StrideSet(int length, int s0 = 0, int s1 = 0, int s2 = 0, int s3 = 0)
		{
			Length = length;
			stride0 = s0;
			stride1 = s1;
			stride2 = s2;
			stride3 = s3;
		}
	}

	public int Width { get; }

	public int Height { get; }

	public StrideSet Stride { get; }

	public ImageFormat Format { get; }

	public Rotation Rotation { get; set; }

	public Flip Flip { get; set; }

	public ImageBufferInfo(int width, int height, StrideSet stride, ImageFormat format)
	{
		Width = width;
		Height = height;
		Stride = stride;
		Format = format;
		Rotation = Rotation.Rotate0;
		Flip = Flip.None;
	}
}
