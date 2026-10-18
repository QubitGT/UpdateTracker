using System;
using System.Threading.Tasks;

namespace Modio.Unity.UI.Panels;

public abstract class ModioWaitingPanelBase : ModioPanelBase
{
	public async void OpenAndWaitFor<T>(Task<T> task, Action<T> action)
	{
		OpenPanel();
		await task;
		ClosePanel();
		action(task.Result);
	}

	public async Task<T> OpenAndWaitForAsync<T>(Task<T> task)
	{
		OpenPanel();
		await task;
		ClosePanel();
		return task.Result;
	}

	public async Task OpenAndWaitFor(Task task, Action action = null)
	{
		OpenPanel();
		await task;
		ClosePanel();
		action?.Invoke();
	}

	public override void DoDefaultSelection()
	{
		SetSelectedGameObject(null);
	}

	protected override void CancelPressed()
	{
	}
}
