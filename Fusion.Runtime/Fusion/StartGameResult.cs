using System;
using Fusion.Photon.Realtime.Async;

namespace Fusion;

public class StartGameResult
{
	public bool Ok => ShutdownReason == ShutdownReason.Ok;

	public ShutdownReason ShutdownReason { get; private set; }

	public string ErrorMessage { get; private set; }

	public string StackTrace { get; private set; }

	internal StartGameResult(ShutdownReason reason = ShutdownReason.Ok, string message = null, string stackTrace = null)
	{
		ShutdownReason = reason;
		ErrorMessage = message ?? reason.ToString();
		StackTrace = stackTrace;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}:{2}, {3}: {4}, {5}={6}, {7}={8}]", "StartGameResult", "Ok", Ok, "ShutdownReason", ShutdownReason, "ErrorMessage", ErrorMessage, "StackTrace", StackTrace);
	}

	internal static StartGameResult BuildGameResultFromException(Exception e)
	{
		ShutdownReason reason = ((e is StartGameException ex) ? ex.ShutdownReason : ((e is DisconnectException ex2) ? DisconnectCauseExt.ConvertToShutdownReason(ex2.Cause) : ((e is AuthenticationFailedException) ? ShutdownReason.CustomAuthenticationFailed : ((e is OperationException ex3) ? ErrorCodeExt.ConvertToShutdownReason(ex3.ErrorCode) : ((e is OperationStartException) ? ShutdownReason.Error : ((!(e is OperationTimeoutException) && !(e is TimeoutException)) ? ((!(e is OperationCanceledException)) ? ShutdownReason.Error : ShutdownReason.OperationCanceled) : ShutdownReason.OperationTimeout))))));
		return new StartGameResult(reason, e.Message, e.StackTrace);
	}
}
