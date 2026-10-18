using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering;

internal static class ProbeVolumeConstantRuntimeResources
{
	private static ComputeBuffer m_SkySamplingDirectionsBuffer = null;

	private static ComputeBuffer m_AntiLeakDataBuffer = null;

	private const int NB_SKY_PRECOMPUTED_DIRECTIONS = 255;

	private static Vector3[] k_SkyDirections = new Vector3[255];

	private static uint[] k_AntiLeakData = new uint[256]
	{
		38347995u, 38347849u, 38347852u, 38347851u, 38347873u, 38347865u, 38322764u, 38322763u, 38347876u, 38324297u,
		38347868u, 38324299u, 38347875u, 38324313u, 38322780u, 38347867u, 38348041u, 38347977u, 38408780u, 38408779u,
		38408801u, 38408793u, 69517900u, 69517899u, 38408804u, 38324425u, 38408796u, 69519435u, 38408803u, 69519449u,
		69517916u, 38408795u, 38348044u, 38410313u, 38347980u, 38410315u, 38410337u, 38410329u, 38322892u, 70304331u,
		38410340u, 70305865u, 38410332u, 70305867u, 38410339u, 70305881u, 70304348u, 38410331u, 38348043u, 38410441u,
		38408908u, 38347979u, 38322955u, 38409817u, 69518028u, 38322891u, 38324491u, 70305993u, 38409820u, 38324427u,
		38409827u, 26351193u, 25564764u, 38323915u, 38348065u, 38421065u, 38421068u, 38421067u, 38348001u, 38421081u,
		38312161u, 38388299u, 38421092u, 75810889u, 38421084u, 75810891u, 38421091u, 75810905u, 38388316u, 38421083u,
		38348057u, 38421193u, 38312217u, 38416971u, 38408929u, 38347993u, 69507297u, 38312153u, 38324505u, 75811017u,
		38416988u, 26358347u, 38416995u, 38324441u, 69583452u, 38320345u, 38421260u, 75896905u, 38421196u, 75896907u,
		38410465u, 75896921u, 38388428u, 70369867u, 75896932u, 70305865u, 75896924u, 70305867u, 75896931u, 70305881u,
		70369884u, 75896923u, 38421259u, 75897033u, 38417100u, 38421195u, 38409953u, 38410457u, 69583564u, 38377689u,
		75811083u, 70305993u, 75896412u, 75811019u, 75896419u, 70306009u, 70107740u, 70301913u, 38348068u, 38422601u,
		38422604u, 38422603u, 38422625u, 38422617u, 76595788u, 76595787u, 38348004u, 38310628u, 38422620u, 38389835u,
		38422627u, 38389849u, 76595804u, 38422619u, 38422793u, 38422729u, 76681804u, 76681803u, 76681825u, 76681817u,
		69517900u, 69517899u, 38408932u, 38389961u, 76681820u, 69584971u, 76681827u, 69584985u, 69517916u, 76681819u,
		38348060u, 38310684u, 38422732u, 38418507u, 38322972u, 38418521u, 76595916u, 25573451u, 38410468u, 70292196u,
		38347996u, 38310620u, 38418531u, 70371417u, 38322908u, 38318812u, 38422795u, 38418633u, 76681932u, 38422731u,
		76595979u, 76682841u, 69518028u, 76595915u, 38409956u, 70371529u, 38408924u, 38376156u, 76682851u, 70109273u,
		69518044u, 69513948u, 38348067u, 38310691u, 38312227u, 38422091u, 38422753u, 38422105u, 76585185u, 76661323u,
		38421220u, 75797220u, 38422108u, 75876427u, 38348003u, 38310627u, 38312163u, 38311651u, 38422809u, 38422217u,
		76585241u, 76689995u, 76681953u, 38422745u, 69507297u, 76585177u, 38417124u, 75876553u, 76690012u, 73779275u,
		38408931u, 38389977u, 69507299u, 76593369u, 38421276u, 75797276u, 38422220u, 75905099u, 38418657u, 75905113u,
		76661452u, 74564171u, 75897060u, 70292196u, 38421212u, 75797212u, 38410467u, 70292195u, 38388444u, 75805404u,
		38348059u, 38310683u, 38312219u, 38422219u, 38322971u, 38418649u, 25467163u, 76650713u, 38324507u, 26252059u,
		38417116u, 75862748u, 38409955u, 70371545u, 69583580u, 38347995u
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void GetRuntimeResources(ref ProbeReferenceVolume.RuntimeResources rr)
	{
		rr.SkyPrecomputedDirections = m_SkySamplingDirectionsBuffer;
		rr.QualityLeakReductionData = m_AntiLeakDataBuffer;
	}

	internal static void Initialize()
	{
		if (m_SkySamplingDirectionsBuffer == null)
		{
			k_SkyDirections = GenerateSkyDirections();
			m_SkySamplingDirectionsBuffer = new ComputeBuffer(k_SkyDirections.Length, 12);
			m_SkySamplingDirectionsBuffer.SetData(k_SkyDirections);
		}
		if (m_AntiLeakDataBuffer == null)
		{
			m_AntiLeakDataBuffer = new ComputeBuffer(k_AntiLeakData.Length, 4);
			m_AntiLeakDataBuffer.SetData(k_AntiLeakData);
		}
	}

	public static Vector3[] GetSkySamplingDirections()
	{
		return k_SkyDirections;
	}

	internal static void Cleanup()
	{
		CoreUtils.SafeRelease(m_SkySamplingDirectionsBuffer);
		m_SkySamplingDirectionsBuffer = null;
		CoreUtils.SafeRelease(m_AntiLeakDataBuffer);
		m_AntiLeakDataBuffer = null;
	}

	private static Vector3[] GenerateSkyDirections()
	{
		Vector3[] array = new Vector3[255];
		float num = Mathf.Sqrt(255f);
		float num2 = 0f;
		float a = 0f;
		float a2 = 0f;
		for (int i = 0; i < 255; i++)
		{
			float num3 = -1f + 2f * (float)i / 254f;
			float num4 = Mathf.Acos(num3);
			num2 = ((i != 254 && i != 0) ? (num2 + 3.6f / num * 1f / Mathf.Sqrt(1f - num3 * num3)) : 0f);
			Vector3 vector = new Vector3(Mathf.Sin(num4) * Mathf.Cos(num2), Mathf.Sin(num4) * Mathf.Sin(num2), Mathf.Cos(num4));
			vector.Normalize();
			array[i] = vector;
			a = Mathf.Max(a, num2);
			a2 = Mathf.Max(a2, num4);
		}
		return array;
	}
}
