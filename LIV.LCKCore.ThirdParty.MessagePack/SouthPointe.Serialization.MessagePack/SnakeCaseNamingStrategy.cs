using System.Text;

namespace SouthPointe.Serialization.MessagePack;

public class SnakeCaseNamingStrategy : IMapNamingStrategy
{
	public string OnPack(string name, MapDefinition definition)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char c = '\0';
		foreach (char c2 in name)
		{
			if (char.IsUpper(c2))
			{
				if (char.IsLower(c))
				{
					stringBuilder.Append('_');
				}
				stringBuilder.Append(char.ToLower(c2));
			}
			else
			{
				stringBuilder.Append(c2);
			}
			c = c2;
		}
		return stringBuilder.ToString();
	}

	public string OnUnpack(string name, MapDefinition definition)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		for (int i = 0; i < name.Length; i++)
		{
			if (name[i] == '_')
			{
				flag = true;
			}
			else if (flag)
			{
				stringBuilder.Append(char.ToUpper(name[i]));
				flag = false;
			}
			else
			{
				stringBuilder.Append(name[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
