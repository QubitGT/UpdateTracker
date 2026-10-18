namespace PlayFab.SharedModels;

public class PlayFabBaseModel
{
	public string ToJson()
	{
		return PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).SerializeObject(this);
	}
}
