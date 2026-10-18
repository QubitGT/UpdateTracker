using System;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.HttpClient;
using Modio.API.Interfaces;
using Modio.Authentication;
using Modio.Errors;
using Modio.FileIO;
using Modio.Users;

namespace Modio;

public static class ModioClient
{
	private static TaskCompletionSource<Error> _initializingTCS;

	private static bool _hasBoundDefaultServices;

	public static IModioDataStorage DataStorage => ModioServices.Resolve<IModioDataStorage>();

	public static IModioAPIInterface Api => ModioServices.Resolve<IModioAPIInterface>();

	public static IModioAuthService AuthService => ModioServices.Resolve<IModioAuthService>();

	public static ModioSettings Settings => ModioServices.Resolve<ModioSettings>();

	public static bool IsInitialized { get; private set; }

	internal static bool IsCurrentlyInitializing => _initializingTCS != null;

	private static event Action InternalOnInitialized;

	public static event Action OnInitialized
	{
		add
		{
			InternalOnInitialized += value;
			if (IsInitialized)
			{
				value?.Invoke();
			}
		}
		remove
		{
			InternalOnInitialized -= value;
		}
	}

	public static event Action OnShutdown;

	public static Task<Error> Init(ModioSettings settings)
	{
		ModioServices.BindInstance(settings, ModioServicePriority.PlatformProvided);
		return Init();
	}

	public static async Task<Error> Init()
	{
		if (IsInitialized)
		{
			ModioLog.Error?.Log("Reinitializing mod.io SDK! Use ModioClient.Shutdown before initializing the SDK!");
			return new Error(ErrorCode.SDKALREADY_INITIALIZED);
		}
		BindDefaultServices();
		if (DataStorage == null || Api == null)
		{
			ModioLog.Error?.Log("mod.io SDK failed to find required components");
			return new Error(ErrorCode.MISSING_COMPONENTS);
		}
		if (_initializingTCS != null)
		{
			return await _initializingTCS.Task;
		}
		_initializingTCS = new TaskCompletionSource<Error>();
		try
		{
			ModioAPI.Init();
			ModioAPI.SetResponseLanguage(Settings.DefaultLanguage);
			Error error = await DataStorage.Init();
			if ((bool)error)
			{
				ModioLog.Error?.Log("mod.io SDK failed to init DataStorage module");
				_initializingTCS.TrySetResult(error);
				_initializingTCS = null;
				return error;
			}
			await User.InitializeNewUser();
			error = await ModInstallationManagement.Init();
			if ((bool)error)
			{
				ModioLog.Error?.Log($"mod.io SDK failed to Init {typeof(ModInstallationManagement)}");
				_initializingTCS.TrySetResult(error);
				_initializingTCS = null;
				return error;
			}
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log($"mod.io SDK exception during Init: {arg}");
			if (_initializingTCS != null)
			{
				_initializingTCS.TrySetResult(Error.Unknown);
				_initializingTCS = null;
			}
			return Error.Unknown;
		}
		IsInitialized = true;
		ModioClient.InternalOnInitialized?.Invoke();
		_initializingTCS.TrySetResult(Error.None);
		_initializingTCS = null;
		return Error.None;
	}

	public static async Task Shutdown()
	{
		IsInitialized = false;
		ModioClient.OnShutdown?.Invoke();
		await ModInstallationManagement.Shutdown();
		if (ModioServices.TryResolve<IModioDataStorage>(out var result))
		{
			await result.Shutdown();
		}
	}

	private static void BindDefaultServices()
	{
		if (!_hasBoundDefaultServices)
		{
			_hasBoundDefaultServices = true;
			ModioServices.Bind<IModioAPIInterface>().FromNew<ModioAPIHttpClient>(ModioServicePriority.Default);
			ModioServices.Bind<IModioDataStorage>().FromNew<BaseDataStorage>(ModioServicePriority.Default);
			ModioServices.Bind<ModioEmailAuthService>().WithInterfaces<IGetActiveUserIdentifier>().WithInterfaces<IModioAuthService>()
				.FromNew<ModioEmailAuthService>(ModioServicePriority.Default);
			ModioServices.BindErrorMessage<ModioSettings>("Please ensure you've bound a ModioSettings using ModioServices.BindInstance(settings); before trying to use Modio classes");
		}
	}
}
