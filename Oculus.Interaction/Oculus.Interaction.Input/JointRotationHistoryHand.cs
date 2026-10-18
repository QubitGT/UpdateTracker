using UnityEngine;

namespace Oculus.Interaction.Input;

public class JointRotationHistoryHand : Hand
{
	[SerializeField]
	private int _historyLength = 60;

	[SerializeField]
	private int _historyOffset = 5;

	private Quaternion[][] _jointHistory = new Quaternion[26][];

	private int _historyIndex;

	private int _capturedDataVersion;

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < _jointHistory.Length; i++)
		{
			_jointHistory[i] = new Quaternion[_historyLength];
			for (int j = 0; j < _historyLength; j++)
			{
				_jointHistory[i][j] = Quaternion.identity;
			}
		}
	}

	protected override void Apply(HandDataAsset data)
	{
		if (!data.IsDataValid)
		{
			return;
		}
		if (_capturedDataVersion != ModifyDataFromSource.CurrentDataVersion)
		{
			_capturedDataVersion = ModifyDataFromSource.CurrentDataVersion;
			_historyIndex = (_historyIndex + 1) % _historyLength;
			for (int i = 0; i < _jointHistory.Length; i++)
			{
				_jointHistory[i][_historyIndex] = data.Joints[i];
			}
		}
		_historyOffset = Mathf.Clamp(_historyOffset, 0, _historyLength);
		int num = (_historyIndex + _historyLength - _historyOffset) % _historyLength;
		for (int j = 0; j < _jointHistory.Length; j++)
		{
			data.Joints[j] = _jointHistory[j][num];
		}
	}

	public void SetHistoryOffset(int offset)
	{
		_historyOffset = offset;
		MarkInputDataRequiresUpdate();
	}

	public void InjectAllJointHistoryHand(UpdateModeFlags updateMode, IDataSource updateAfter, DataModifier<HandDataAsset> modifyDataFromSource, bool applyModifier, int historyLength, int historyOffset)
	{
		InjectAllHand(updateMode, updateAfter, modifyDataFromSource, applyModifier);
		InjectHistoryLength(historyLength);
		SetHistoryOffset(historyOffset);
	}

	public void InjectHistoryLength(int historyLength)
	{
		_historyLength = historyLength;
	}
}
