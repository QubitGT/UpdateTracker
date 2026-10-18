using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Pool;

namespace UnityEngine.Localization.Pseudo;

public class Message
{
	internal static readonly ObjectPool<Message> Pool = new ObjectPool<Message>(() => new Message(), null, null, null, collectionCheck: false);

	public string Original { get; private set; }

	public List<MessageFragment> Fragments { get; private set; } = new List<MessageFragment>();


	public int Length
	{
		get
		{
			int num = 0;
			foreach (MessageFragment fragment in Fragments)
			{
				num += fragment.Length;
			}
			return num;
		}
	}

	public WritableMessageFragment CreateTextFragment(string original, int start, int end)
	{
		WritableMessageFragment writableMessageFragment = WritableMessageFragment.Pool.Get();
		writableMessageFragment.Initialize(this, original, start, end);
		return writableMessageFragment;
	}

	public WritableMessageFragment CreateTextFragment(string original)
	{
		WritableMessageFragment writableMessageFragment = WritableMessageFragment.Pool.Get();
		writableMessageFragment.Initialize(this, original);
		return writableMessageFragment;
	}

	public ReadOnlyMessageFragment CreateReadonlyTextFragment(string original, int start, int end)
	{
		ReadOnlyMessageFragment readOnlyMessageFragment = ReadOnlyMessageFragment.Pool.Get();
		readOnlyMessageFragment.Initialize(this, original, start, end);
		return readOnlyMessageFragment;
	}

	public ReadOnlyMessageFragment CreateReadonlyTextFragment(string original)
	{
		ReadOnlyMessageFragment readOnlyMessageFragment = ReadOnlyMessageFragment.Pool.Get();
		readOnlyMessageFragment.Initialize(this, original);
		return readOnlyMessageFragment;
	}

	public void ReplaceFragment(MessageFragment original, MessageFragment replacement)
	{
		int num = Fragments.IndexOf(original);
		if (num == -1)
		{
			throw new Exception("Can not replace Fragment " + original.ToString() + " that is not part of the message.");
		}
		Fragments[num] = replacement;
		ReleaseFragment(original);
	}

	public void ReleaseFragment(MessageFragment fragment)
	{
		if (fragment is WritableMessageFragment element)
		{
			WritableMessageFragment.Pool.Release(element);
		}
		else if (fragment is ReadOnlyMessageFragment element2)
		{
			ReadOnlyMessageFragment.Pool.Release(element2);
		}
	}

	internal static Message CreateMessage(string text)
	{
		Message message = Pool.Get();
		message.Fragments.Add(message.CreateTextFragment(text));
		message.Original = text;
		return message;
	}

	internal void Release()
	{
		foreach (MessageFragment fragment in Fragments)
		{
			ReleaseFragment(fragment);
		}
		Fragments.Clear();
		Pool.Release(this);
	}

	public override string ToString()
	{
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			foreach (MessageFragment fragment in Fragments)
			{
				fragment.BuildString(value);
			}
			return value.ToString();
		}
	}
}
