namespace UnityEngine.ProBuilder;

public sealed class ActionResult
{
	public enum Status
	{
		Success,
		Failure,
		Canceled,
		NoChange
	}

	public Status status { get; private set; }

	public string notification { get; private set; }

	public static ActionResult Success => new ActionResult(Status.Success, "");

	public static ActionResult NoSelection => new ActionResult(Status.Canceled, "Nothing Selected");

	public static ActionResult UserCanceled => new ActionResult(Status.Canceled, "User Canceled");

	public ActionResult(Status status, string notification)
	{
		this.status = status;
		this.notification = notification;
	}

	public static implicit operator bool(ActionResult res)
	{
		if (res != null)
		{
			return res.status == Status.Success;
		}
		return false;
	}

	public bool ToBool()
	{
		return status == Status.Success;
	}

	public static bool FromBool(bool success)
	{
		return success ? Success : new ActionResult(Status.Failure, "Failure");
	}
}
