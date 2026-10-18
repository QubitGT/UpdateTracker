using System;

namespace VYaml.Parser;

public class Anchor : IEquatable<Anchor>
{
	public string Name { get; }

	public int Id { get; }

	public Anchor(string name, int id)
	{
		Name = name;
		Id = id;
	}

	public bool Equals(Anchor? other)
	{
		if (other != null)
		{
			return Id == other.Id;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is Anchor other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id;
	}

	public override string ToString()
	{
		return $"{Name} Id={Id}";
	}
}
