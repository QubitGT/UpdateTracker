using System.Threading;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Breadcrumbs;

namespace Backtrace.Unity.Runtime.Native.Base;

internal abstract class NativeClientBase
{
	internal const string AnrMessage = "ANRException: Blocked thread detected.";

	protected const string HangType = "Hang";

	protected const string CrashType = "Crash";

	protected const string ErrorTypeAttribute = "error.type";

	protected int AnrWatchdogTimeout = 5000;

	protected volatile bool LogAnr;

	protected internal volatile float LastUpdateTime;

	internal volatile bool PreventAnr;

	internal volatile bool StopAnr;

	internal Thread AnrThread;

	protected bool CaptureNativeCrashes;

	protected bool HandlerANR;

	protected readonly BacktraceConfiguration _configuration;

	protected readonly BacktraceBreadcrumbs _breadcrumbs;

	private readonly bool _shouldLogAnrsInBreadcrumbs;

	private object _lockObject = new object();

	internal NativeClientBase(BacktraceConfiguration configuration, BacktraceBreadcrumbs breadcrumbs)
	{
		_configuration = configuration;
		_breadcrumbs = breadcrumbs;
		_shouldLogAnrsInBreadcrumbs = ShouldStoreAnrBreadcrumbs();
		AnrWatchdogTimeout = ((configuration.AnrWatchdogTimeout > 1000) ? configuration.AnrWatchdogTimeout : 5000);
	}

	public void Update(float time)
	{
		LastUpdateTime = time;
		if (!_shouldLogAnrsInBreadcrumbs || !LogAnr || !Monitor.TryEnter(_lockObject))
		{
			return;
		}
		try
		{
			if (_shouldLogAnrsInBreadcrumbs && LogAnr)
			{
				_breadcrumbs.AddBreadcrumbs("ANRException: Blocked thread detected.", BreadcrumbLevel.System, UnityEngineLogLevel.Warning);
				LogAnr = false;
			}
		}
		finally
		{
			Monitor.Exit(_lockObject);
		}
	}

	internal void OnAnrDetection()
	{
		LogAnr = _shouldLogAnrsInBreadcrumbs;
	}

	public void PauseAnrThread(bool stopAnr)
	{
		PreventAnr = stopAnr;
	}

	public virtual void Disable()
	{
		if (AnrThread != null)
		{
			StopAnr = true;
		}
	}

	private bool ShouldStoreAnrBreadcrumbs()
	{
		if (_breadcrumbs == null)
		{
			return false;
		}
		if (_breadcrumbs.BreadcrumbsLevel.HasFlag(BacktraceBreadcrumbType.System))
		{
			return _breadcrumbs.UnityLogLevel.HasFlag(UnityEngineLogLevel.Warning);
		}
		return false;
	}
}
