namespace UnityEngine.Rendering.Universal;

internal class DecalDrawFowardEmissiveSystem : DecalDrawSystem
{
	public DecalDrawFowardEmissiveSystem(DecalEntityManager entityManager)
		: base("DecalDrawFowardEmissiveSystem.Execute", entityManager)
	{
	}

	protected override int GetPassIndex(DecalCachedChunk decalCachedChunk)
	{
		return decalCachedChunk.passIndexEmissive;
	}
}
