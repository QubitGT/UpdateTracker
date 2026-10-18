using Unity.Collections;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

public struct FixedAttachmentArray<DataType> where DataType : unmanaged
{
	public static FixedAttachmentArray<DataType> Empty = new FixedAttachmentArray<DataType>(0);

	public const int MaxAttachments = 8;

	private DataType a0;

	private DataType a1;

	private DataType a2;

	private DataType a3;

	private DataType a4;

	private DataType a5;

	private DataType a6;

	private DataType a7;

	private int activeAttachments;

	public int size => activeAttachments;

	public unsafe ref DataType this[int index]
	{
		get
		{
			fixed (FixedAttachmentArray<DataType>* ptr = &this)
			{
				DataType* ptr2 = (DataType*)ptr;
				return ref ptr2[index];
			}
		}
	}

	public FixedAttachmentArray(int numAttachments)
	{
		a0 = (a1 = (a2 = (a3 = (a4 = (a5 = (a6 = (a7 = new DataType())))))));
		activeAttachments = numAttachments;
	}

	public FixedAttachmentArray(DataType[] attachments)
		: this(attachments.Length)
	{
		for (int i = 0; i < activeAttachments; i++)
		{
			this[i] = attachments[i];
		}
	}

	public FixedAttachmentArray(NativeArray<DataType> attachments)
		: this(attachments.Length)
	{
		for (int i = 0; i < activeAttachments; i++)
		{
			this[i] = attachments[i];
		}
	}

	public void Clear()
	{
		activeAttachments = 0;
	}

	public unsafe int Add(in DataType data)
	{
		int num = activeAttachments;
		fixed (FixedAttachmentArray<DataType>* ptr = &this)
		{
			DataType* ptr2 = (DataType*)ptr;
			ptr2[num] = data;
		}
		activeAttachments++;
		return num;
	}
}
