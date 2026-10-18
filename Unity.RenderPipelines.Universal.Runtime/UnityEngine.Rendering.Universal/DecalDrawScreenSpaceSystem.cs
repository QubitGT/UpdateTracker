namespace UnityEngine.Rendering.Universal;

internal class DecalDrawScreenSpaceSystem : DecalDrawSystem
{
	public DecalDrawScreenSpaceSystem(DecalEntityManager entityManager)
		: base("DecalDrawScreenSpaceSystem.Execute", entityManager)
	{
	}

	protected override int GetPassIndex(DecalCachedChunk decalCachedChunk)
	{
		return decalCachedChunk.passIndexScreenSpace;
	}
}
