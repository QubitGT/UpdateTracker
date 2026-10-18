using Oculus.Interaction.DebugTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.PoseDetection.Debug;

public class ActiveStateNodeUIVertical : MonoBehaviour, INodeUI<IActiveState>
{
	[SerializeField]
	private RectTransform _childArea;

	[SerializeField]
	private RectTransform _connectingLine;

	[SerializeField]
	private TextMeshProUGUI _label;

	[SerializeField]
	private Image _activeImage;

	[SerializeField]
	private Color _activeColor = Color.green;

	[SerializeField]
	private Color _inactiveColor = Color.red;

	private const string OBJNAME_FORMAT = "<color=#dddddd><size=85%>{0}</size></color>";

	private ITreeNode<IActiveState> _boundNode;

	private bool _isRoot;

	private bool _isDuplicate;

	public RectTransform ChildArea => _childArea;

	public void Bind(ITreeNode<IActiveState> node, bool isRoot, bool isDuplicate)
	{
		_isRoot = isRoot;
		_isDuplicate = isDuplicate;
		_boundNode = node;
		_label.text = GetLabelText(node);
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		_activeImage.color = (_boundNode.Value.Active ? _activeColor : _inactiveColor);
		_childArea.gameObject.SetActive(_childArea.childCount > 0);
		_connectingLine.gameObject.SetActive(!_isRoot);
	}

	private string GetLabelText(ITreeNode<IActiveState> node)
	{
		string text = (_isDuplicate ? "<i>" : "");
		if (node.Value is Object @object)
		{
			text = text + @object.name + " - ";
		}
		return text + $"<color=#dddddd><size=85%>{node.Value.GetType().Name}</size></color>";
	}
}
