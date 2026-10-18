namespace UnityEngine.Rendering.Universal;

internal class DecalDrawDBufferSystem : DecalDrawSystem
{
	public DecalDrawDBufferSystem(DecalEntityManager entityManager)
		: base("DecalDrawIntoDBufferSystem.Execute", entityManager)
	{
	}

	protected override int GetPassIndex(DecalCachedChunk decalCachedChunk)
	{
		return decalCachedChunk.passIndexDBuffer;
	}
}
