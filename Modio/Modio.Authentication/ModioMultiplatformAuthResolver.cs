using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;

namespace Modio.Authentication;

public class ModioMultiplatformAuthResolver : IModioAuthService, IGetActiveUserIdentifier, IPotentialModioEmailAuthService
{
	private const ModioServicePriority SERVICE_BINDING_PRIORITY = (ModioServicePriority)50;

	private static bool _resolveUsingThis;

	private static bool _hasInitialized;

	public static IModioAuthService ServiceOverride { get; set; }

	public static IReadOnlyList<IModioAuthService> AuthBindings { get; private set; }

	public bool IsEmailPlatform
	{
		get
		{
			if (Get<IModioAuthService>() is IPotentialModioEmailAuthService potentialModioEmailAuthService)
			{
				return potentialModioEmailAuthService.IsEmailPlatform;
			}
			return false;
		}
	}

	public ModioAPI.Portal Portal => Get<IModioAuthService>()?.Portal ?? ModioAPI.Portal.None;

	public static void Initialize()
	{
		if (!_hasInitialized)
		{
			_hasInitialized = true;
			AuthBindings = (from tuple in ModioServices.GetBindings<IModioAuthService>().ResolveAll()
				orderby tuple.Item2 descending
				select tuple into platformPair
				select platformPair.Item1 into platform
				where platform is IGetActiveUserIdentifier
				select platform).ToList();
			ServiceOverride = AuthBindings.FirstOrDefault();
			ModioServices.Bind<ModioMultiplatformAuthResolver>().WithInterfaces<IModioAuthService>(IsActiveForConditional).WithInterfaces<IGetActiveUserIdentifier>(IsActiveForConditional)
				.FromNew<ModioMultiplatformAuthResolver>((ModioServicePriority)50);
			_resolveUsingThis = true;
		}
	}

	private static bool IsActiveForConditional()
	{
		return _resolveUsingThis;
	}

	public Task<Error> Authenticate(bool displayedTerms, string thirdPartyEmail = null)
	{
		return Get<IModioAuthService>().Authenticate(displayedTerms, thirdPartyEmail);
	}

	public Task<string> GetActiveUserIdentifier()
	{
		return Get<IGetActiveUserIdentifier>().GetActiveUserIdentifier();
	}

	private static T Get<T>()
	{
		IModioAuthService serviceOverride = ServiceOverride;
		if (serviceOverride is T)
		{
			return (T)serviceOverride;
		}
		_resolveUsingThis = false;
		T result = ModioServices.Resolve<T>();
		_resolveUsingThis = true;
		return result;
	}
}
