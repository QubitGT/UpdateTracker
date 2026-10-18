using System.Threading.Tasks;

namespace Meta.WitAi.Requests;

internal interface IVRequestDownloadDecoder
{
	TaskCompletionSource<bool> Completion { get; }

	event VRequestResponseDelegate OnFirstResponse;

	event VRequestResponseDelegate OnResponse;

	event VRequestProgressDelegate OnProgress;
}
