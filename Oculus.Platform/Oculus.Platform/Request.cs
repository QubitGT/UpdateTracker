using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Oculus.Platform;

public sealed class Request<T> : Request
{
	private TaskCompletionSource<Message<T>> tcs_;

	private Message<T>.Callback callback_;

	public Request(ulong requestID)
		: base(requestID)
	{
	}

	public Request<T> OnComplete(Message<T>.Callback callback)
	{
		if (callback_ != null)
		{
			throw new UnityException("Attempted to attach multiple handlers to a Request.  This is not allowed.");
		}
		if (tcs_ != null)
		{
			throw new UnityException("Attempted to attach multiple handlers to a Request.  This is not allowed.");
		}
		callback_ = callback;
		Callback.AddRequest(this);
		return this;
	}

	public new async Task<Message<T>> Gen()
	{
		if (callback_ != null || tcs_ != null)
		{
			throw new UnityException("Attempted to attach multiple handlers to a Request.  This is not allowed.");
		}
		tcs_ = new TaskCompletionSource<Message<T>>();
		Callback.AddRequest(this);
		return await tcs_.Task;
	}

	public new TaskAwaiter<Message<T>> GetAwaiter()
	{
		return Gen().GetAwaiter();
	}

	public override void HandleMessage(Message msg)
	{
		if (!(msg is Message<T>))
		{
			Debug.LogError("Unable to handle message: " + msg.GetType());
			return;
		}
		if (tcs_ != null)
		{
			tcs_.SetResult((Message<T>)msg);
			return;
		}
		if (callback_ != null)
		{
			callback_((Message<T>)msg);
			return;
		}
		throw new UnityException("Request with no handler.  This should never happen.");
	}
}
public class Request
{
	private TaskCompletionSource<Message> tcs_;

	private Message.Callback callback_;

	public ulong RequestID { get; set; }

	public Request(ulong requestID)
	{
		RequestID = requestID;
	}

	public Request OnComplete(Message.Callback callback)
	{
		callback_ = callback;
		Callback.AddRequest(this);
		return this;
	}

	public async Task<Message> Gen()
	{
		tcs_ = new TaskCompletionSource<Message>();
		Callback.AddRequest(this);
		return await tcs_.Task;
	}

	public TaskAwaiter<Message> GetAwaiter()
	{
		return Gen().GetAwaiter();
	}

	public virtual void HandleMessage(Message msg)
	{
		if (tcs_ != null)
		{
			tcs_.SetResult(msg);
			return;
		}
		if (callback_ != null)
		{
			callback_(msg);
			return;
		}
		throw new UnityException("Request with no handler.  This should never happen.");
	}

	public static void RunCallbacks(uint limit = 0u)
	{
		if (limit == 0)
		{
			Callback.RunCallbacks();
		}
		else
		{
			Callback.RunLimitedCallbacks(limit);
		}
	}
}
