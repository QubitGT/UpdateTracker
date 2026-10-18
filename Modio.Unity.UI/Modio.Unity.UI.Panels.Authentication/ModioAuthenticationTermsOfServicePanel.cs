using Modio.Extensions;

namespace Modio.Unity.UI.Panels.Authentication;

public class ModioAuthenticationTermsOfServicePanel : ModioPanelBase
{
	public void OnPressAgreeTOS()
	{
		ModioPanelManager.GetPanelOfType<ModioAuthenticationPanel>().AttemptSso(agreedToTerms: true).ForgetTaskSafely();
	}
}
