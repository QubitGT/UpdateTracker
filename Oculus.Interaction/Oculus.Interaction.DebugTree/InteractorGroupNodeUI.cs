using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.DebugTree;

public class InteractorGroupNodeUI : MonoBehaviour, INodeUI<IInteractor>
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
	private Color _selectColor = Color.green;

	[SerializeField]
	private Color _hoverColor = Color.blue;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _disabledColor = Color.grey;

	private const string OBJNAME_FORMAT = "<color=#dddddd><size=85%>{0}</size></color>";

	private ITreeNode<IInteractor> _boundNode;

	private bool _isRoot;

	private bool _isDuplicate;

	public RectTransform ChildArea => _childArea;

	public void Bind(ITreeNode<IInteractor> node, bool isRoot, bool isDuplicate)
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
		switch (_boundNode.Value.State)
		{
		case InteractorState.Select:
			_activeImage.color = _selectColor;
			break;
		case InteractorState.Hover:
			_activeImage.color = _hoverColor;
			break;
		case InteractorState.Normal:
			_activeImage.color = _normalColor;
			break;
		case InteractorState.Disabled:
			_activeImage.color = _disabledColor;
			break;
		}
		_childArea.gameObject.SetActive(_childArea.childCount > 0);
		_connectingLine.gameObject.SetActive(!_isRoot);
	}

	private string GetLabelText(ITreeNode<IInteractor> node)
	{
		string text = (_isDuplicate ? "<i>" : "");
		if (node.Value is Object @object)
		{
			text = text + @object.name + " - ";
		}
		return text + $"<color=#dddddd><size=85%>{node.Value.GetType().Name}</size></color>";
	}
}
