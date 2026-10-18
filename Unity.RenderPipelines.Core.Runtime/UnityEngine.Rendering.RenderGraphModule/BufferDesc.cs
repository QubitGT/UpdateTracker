namespace UnityEngine.Rendering.RenderGraphModule;

public struct BufferDesc
{
	public int count;

	public int stride;

	public string name;

	public GraphicsBuffer.Target target;

	public GraphicsBuffer.UsageFlags usageFlags;

	public BufferDesc(int count, int stride)
	{
		this = default(BufferDesc);
		this.count = count;
		this.stride = stride;
		target = GraphicsBuffer.Target.Structured;
		usageFlags = GraphicsBuffer.UsageFlags.None;
	}

	public BufferDesc(int count, int stride, GraphicsBuffer.Target target)
	{
		this = default(BufferDesc);
		this.count = count;
		this.stride = stride;
		this.target = target;
		usageFlags = GraphicsBuffer.UsageFlags.None;
	}

	public override int GetHashCode()
	{
		HashFNV1A32 hashFNV1A = HashFNV1A32.Create();
		hashFNV1A.Append(in count);
		hashFNV1A.Append(in stride);
		int input = (int)target;
		hashFNV1A.Append(in input);
		input = (int)usageFlags;
		hashFNV1A.Append(in input);
		return hashFNV1A.value;
	}
}
