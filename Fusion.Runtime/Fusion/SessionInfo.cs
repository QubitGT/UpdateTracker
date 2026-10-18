using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fusion;

public class SessionInfo
{
	internal bool _isValid;

	internal bool _isOpen;

	internal bool _isVisible;

	private readonly NetworkRunner _runner;

	public bool IsValid => _isValid;

	public string Name { get; internal set; }

	public string Region { get; internal set; }

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (!(_runner == null))
			{
				if (_runner.IsSinglePlayer)
				{
					_isVisible = value;
				}
				else if ((_runner?._cloudServices?.UpdateRoomIsVisible(value)).GetValueOrDefault())
				{
					_isValid = false;
				}
			}
		}
	}

	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			if (!(_runner == null))
			{
				if (_runner.IsSinglePlayer)
				{
					_isOpen = value;
				}
				else if ((_runner?._cloudServices?.UpdateRoomIsOpen(value)).GetValueOrDefault())
				{
					_isValid = false;
				}
			}
		}
	}

	public ReadOnlyDictionary<string, SessionProperty> Properties { get; internal set; }

	public int PlayerCount { get; internal set; }

	public int MaxPlayers { get; internal set; }

	public static implicit operator bool(SessionInfo sessionInfo)
	{
		return sessionInfo?.IsValid ?? false;
	}

	internal SessionInfo(NetworkRunner runner = null)
	{
		_runner = runner;
		if (runner != null && runner.IsSinglePlayer)
		{
			_isValid = true;
		}
	}

	public bool UpdateCustomProperties(Dictionary<string, SessionProperty> customProperties)
	{
		if (_runner == null)
		{
			return false;
		}
		if (_runner.IsSinglePlayer)
		{
			Dictionary<string, SessionProperty> dictionary = new Dictionary<string, SessionProperty>();
			ReadOnlyDictionary<string, SessionProperty> properties = Properties;
			if (properties != null && properties.Count > 0)
			{
				dictionary = new Dictionary<string, SessionProperty>(Properties);
			}
			foreach (KeyValuePair<string, SessionProperty> customProperty in customProperties)
			{
				dictionary[customProperty.Key] = customProperty.Value;
			}
			Properties = new ReadOnlyDictionary<string, SessionProperty>(dictionary);
			return true;
		}
		if ((_runner?._cloudServices?.UpdateRoomProperties(customProperties)).GetValueOrDefault())
		{
			_isValid = false;
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[SessionInfo: ");
		stringBuilder.Append(string.Format("{0}={1}, ", "IsValid", IsValid));
		stringBuilder.Append("Name=" + Name + ", ");
		stringBuilder.Append(string.Format("{0}={1}, ", "IsOpen", IsOpen));
		stringBuilder.Append(string.Format("{0}={1}, ", "IsVisible", IsVisible));
		stringBuilder.Append("Region=" + Region + ", ");
		stringBuilder.Append(string.Format("{0}={1}, ", "PlayerCount", PlayerCount));
		stringBuilder.Append(string.Format("{0}={1}, ", "MaxPlayers", MaxPlayers));
		stringBuilder.Append("Properties=");
		if (Properties != null)
		{
			foreach (KeyValuePair<string, SessionProperty> property in Properties)
			{
				stringBuilder.Append($"{property.Key}={property.Value?.PropertyValue},");
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
