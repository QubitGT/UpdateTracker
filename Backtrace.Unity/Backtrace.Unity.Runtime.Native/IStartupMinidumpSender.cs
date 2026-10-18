using System.Collections;
using System.Collections.Generic;
using Backtrace.Unity.Interfaces;

namespace Backtrace.Unity.Runtime.Native;

internal interface IStartupMinidumpSender
{
	IEnumerator SendMinidumpOnStartup(ICollection<string> clientAttachments, IBacktraceApi backtraceApi);
}
