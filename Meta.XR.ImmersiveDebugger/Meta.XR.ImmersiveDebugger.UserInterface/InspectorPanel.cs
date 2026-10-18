using System;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class InspectorPanel : DebugPanel, IDebugUIPanel
{
	private ScrollView _scrollView;

	private ScrollView _categoryScrollView;

	private ScrollView _hierarchyScrollView;

	private readonly Dictionary<Category, Dictionary<Type, Dictionary<InstanceHandle, Inspector>>> _registries = new Dictionary<Category, Dictionary<Type, Dictionary<InstanceHandle, Inspector>>>();

	private readonly Dictionary<Category, CategoryButton> _categories = new Dictionary<Category, CategoryButton>();

	private readonly Dictionary<Item, HierarchyItemButton> _items = new Dictionary<Item, HierarchyItemButton>();

	private CategoryButton _selectedCategory;

	private HierarchyItemButton _selectedItem;

	private Background _categoryBackground;

	private Vector3 _currentPosition;

	private Vector3 _targetPosition;

	private readonly float _lerpSpeed = 10f;

	private bool _lerpCompleted = true;

	private ImageStyle _categoryBackgroundImageStyle;

	private DebugInterface _debugInterface;

	private Flex _buttonsAnchor;

	private Label _selectedModeTitle;

	private Toggle _hierarchyIcon;

	private Toggle _categoriesIcon;

	private Flex _categoryDiv;

	private Flex Flex => _scrollView.Flex;

	internal ScrollView ScrollView => _scrollView;

	private Flex CategoryFlex => _categoryScrollView.Flex;

	private Flex HierarchyFlex => _hierarchyScrollView.Flex;

	public ImageStyle CategoryBackgroundStyle
	{
		set
		{
			_categoryBackground.Sprite = value.sprite;
			_categoryBackground.Color = value.color;
			_categoryBackground.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_debugInterface = UnityEngine.Object.FindObjectOfType<DebugInterface>();
		Flex flex = Append<Flex>("div");
		flex.LayoutStyle = Style.Load<LayoutStyle>("InspectorDivFlex");
		_categoryDiv = flex.Append<Flex>("categories_div");
		_categoryDiv.LayoutStyle = Style.Load<LayoutStyle>("CategoriesDiv");
		_categoryBackground = _categoryDiv.Append<Background>("background");
		_categoryBackground.LayoutStyle = Style.Load<LayoutStyle>("CategoriesDivBackground");
		_categoryBackgroundImageStyle = Style.Load<ImageStyle>("CategoriesDivBackground");
		CategoryBackgroundStyle = _categoryBackgroundImageStyle;
		_buttonsAnchor = _categoryDiv.Append<Flex>("header");
		_buttonsAnchor.LayoutStyle = Style.Load<LayoutStyle>("ConsoleButtons");
		_hierarchyIcon = RegisterControl("Hierarchy", Resources.Load<Texture2D>("Textures/hierarchy_icon"), Style.Load<ImageStyle>("InspectorModeIcon"), SelectHierarchyMode);
		_categoriesIcon = RegisterControl("Categories", Resources.Load<Texture2D>("Textures/categories_icon"), Style.Load<ImageStyle>("InspectorModeIcon"), SelectCategoryMode);
		_selectedModeTitle = _buttonsAnchor.Append<Label>("title");
		_selectedModeTitle.LayoutStyle = Style.Load<LayoutStyle>("InspectorModeTitle");
		_selectedModeTitle.TextStyle = Style.Load<TextStyle>("MemberTitle");
		_categoryScrollView = _categoryDiv.Append<ScrollView>("categories");
		_categoryScrollView.LayoutStyle = Style.Load<LayoutStyle>("CategoriesScrollView");
		CategoryFlex.LayoutStyle = Style.Load<LayoutStyle>("InspectorCategoryFlex");
		_hierarchyScrollView = _categoryDiv.Append<ScrollView>("categories");
		_hierarchyScrollView.LayoutStyle = Style.Load<LayoutStyle>("CategoriesScrollView");
		HierarchyFlex.LayoutStyle = Style.Load<LayoutStyle>("InspectorCategoryFlex");
		_scrollView = flex.Append<ScrollView>("main");
		_scrollView.LayoutStyle = Style.Load<LayoutStyle>("PanelScrollView");
		Flex.LayoutStyle = Style.Load<LayoutStyle>("InspectorMainFlex");
		SelectCategoryMode();
	}

	private Toggle RegisterControl(string buttonName, Texture2D icon, ImageStyle style, Action callback)
	{
		if (buttonName == null)
		{
			throw new ArgumentNullException("buttonName");
		}
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		Toggle toggle = _buttonsAnchor.Append<Toggle>(buttonName);
		toggle.LayoutStyle = Style.Load<LayoutStyle>("ConsoleButton");
		toggle.Icon = icon;
		toggle.IconStyle = (style ? style : Style.Default<ImageStyle>());
		toggle.Callback = callback;
		return toggle;
	}

	private void SelectCategoryMode()
	{
		_selectedModeTitle.Content = "Custom Inspectors";
		_categoryDiv.Forget(_hierarchyScrollView);
		_categoryDiv.Remember(_categoryScrollView);
		_categoriesIcon.State = true;
		_hierarchyIcon.State = false;
	}

	private void SelectHierarchyMode()
	{
		_selectedModeTitle.Content = "Hierarchy View";
		_categoryDiv.Forget(_categoryScrollView);
		_categoryDiv.Remember(_hierarchyScrollView);
		_hierarchyIcon.State = true;
		_categoriesIcon.State = false;
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_categoryBackground.Color = (base.Transparent ? _categoryBackgroundImageStyle.colorOff : _categoryBackgroundImageStyle.color);
	}

	public IInspector RegisterInspector(InstanceHandle instanceHandle, Category category)
	{
		if (instanceHandle.Instance != null && !(instanceHandle.Instance is Component))
		{
			GetHierarchyItemButton(category.Item, create: true).Counter++;
			return null;
		}
		Inspector inspectorInternal = GetInspectorInternal(instanceHandle, category, createRegistries: true, out var registry);
		if (inspectorInternal != null)
		{
			return inspectorInternal;
		}
		float progress = _scrollView.Progress;
		UnityEngine.Object instance = instanceHandle.Instance;
		string childName = ((instance != null) ? instance.name : instanceHandle.Type.Name);
		inspectorInternal = Flex.Append<Inspector>(childName);
		inspectorInternal.LayoutStyle = Style.Load<LayoutStyle>("Inspector");
		inspectorInternal.InstanceHandle = instanceHandle;
		registry.Add(instanceHandle, inspectorInternal);
		_scrollView.Progress = progress;
		if (category.Item != null)
		{
			inspectorInternal.Foldout.State = false;
			HierarchyItemButton hierarchyItemButton = GetHierarchyItemButton(category.Item, create: true);
			hierarchyItemButton.Counter++;
			if (!_hierarchyScrollView.Visibility || _selectedItem != hierarchyItemButton)
			{
				Flex.Forget(inspectorInternal);
			}
		}
		else
		{
			CategoryButton categoryButton = GetCategoryButton(category, create: true);
			categoryButton.Counter++;
			if (!_categoryScrollView.Visibility || _selectedCategory != categoryButton)
			{
				Flex.Forget(inspectorInternal);
			}
		}
		return inspectorInternal;
	}

	public void UnregisterInspector(InstanceHandle instanceHandle, Category category, bool allCategories)
	{
		if (allCategories)
		{
			foreach (var (category3, dictionary2) in _registries)
			{
				if (dictionary2.TryGetValue(instanceHandle.Type, out var value) && value.TryGetValue(instanceHandle, out var value2))
				{
					value.Remove(instanceHandle);
					RemoveInspector(category3, value2);
				}
			}
			return;
		}
		if (!(instanceHandle.Instance is Component))
		{
			TryRemoveHierarchyItemButton(category.Item);
			return;
		}
		Dictionary<InstanceHandle, Inspector> registry;
		Inspector inspectorInternal = GetInspectorInternal(instanceHandle, category, createRegistries: false, out registry);
		if (!(inspectorInternal == null))
		{
			registry?.Remove(instanceHandle);
			RemoveInspector(category, inspectorInternal);
		}
	}

	private void RemoveInspector(Category category, Inspector inspector)
	{
		float progress = _scrollView.Progress;
		Flex.Remove(inspector, destroy: true);
		_scrollView.Progress = progress;
		if (category.Item != null)
		{
			TryRemoveHierarchyItemButton(category.Item);
			return;
		}
		CategoryButton categoryButton = GetCategoryButton(category);
		if (categoryButton != null)
		{
			categoryButton.Counter--;
		}
	}

	public IInspector GetInspector(InstanceHandle instanceHandle, Category category)
	{
		Dictionary<InstanceHandle, Inspector> registry;
		return GetInspectorInternal(instanceHandle, category, createRegistries: false, out registry);
	}

	public Inspector GetInspectorInternal(InstanceHandle instanceHandle, Category category, bool createRegistries, out Dictionary<InstanceHandle, Inspector> registry)
	{
		Inspector value = null;
		if (!_registries.TryGetValue(category, out var value2) && createRegistries)
		{
			value2 = new Dictionary<Type, Dictionary<InstanceHandle, Inspector>>();
			_registries.Add(category, value2);
		}
		if (value2 == null)
		{
			registry = null;
			return null;
		}
		if (!value2.TryGetValue(instanceHandle.Type, out registry))
		{
			if (!createRegistries)
			{
				return value;
			}
			registry = new Dictionary<InstanceHandle, Inspector>();
			value2.Add(instanceHandle.Type, registry);
		}
		registry.TryGetValue(instanceHandle, out value);
		return value;
	}

	private CategoryButton GetCategoryButton(Category category, bool create = false)
	{
		if (_categories.TryGetValue(category, out var button) || !create)
		{
			return button;
		}
		button = CategoryFlex.Append<CategoryButton>(category.Id);
		button.LayoutStyle = Style.Instantiate<LayoutStyle>("CategoryButton");
		button.Category = category;
		button.Callback = delegate
		{
			SelectCategoryButton(button);
		};
		_categories.Add(category, button);
		if (_selectedCategory == null)
		{
			SelectCategoryButton(button);
		}
		return button;
	}

	private Controller ComputeIdealPreviousItem(Item item)
	{
		Item parent = item.Parent;
		if (parent == null || parent is SceneRegistry)
		{
			return null;
		}
		HierarchyItemButton hierarchyItemButton = GetHierarchyItemButton(item.Parent, create: true);
		Controller controller = null;
		foreach (Controller child in HierarchyFlex.Children)
		{
			if (child is HierarchyItemButton hierarchyItemButton2)
			{
				if (hierarchyItemButton2.Item.Parent == item.Parent || hierarchyItemButton2 == hierarchyItemButton)
				{
					controller = hierarchyItemButton2;
				}
				else if (controller != null)
				{
					break;
				}
			}
		}
		return controller;
	}

	private HierarchyItemButton GetHierarchyItemButton(Item item, bool create = false)
	{
		if (_items.TryGetValue(item, out var button) || !create)
		{
			return button;
		}
		Controller controller = ComputeIdealPreviousItem(item);
		button = ((controller != null) ? HierarchyFlex.InsertAfter<HierarchyItemButton>(item.Label, controller) : HierarchyFlex.Append<HierarchyItemButton>(item.Label));
		button.LayoutStyle = Style.Instantiate<LayoutStyle>("HierarchyItemButton");
		button.Item = item;
		button.LayoutStyle.SetIndent((item.Depth - 1) * 10);
		button.LayoutStyle.SetWidth(button.LayoutStyle.size.x - (float)(item.Depth * 10));
		button.Label.Callback = delegate
		{
			SelectHierarchyItemButton(button);
		};
		button.Foldout.Callback = delegate
		{
			ToggleFoldItem(button);
		};
		_items.Add(item, button);
		return button;
	}

	private void TryRemoveHierarchyItemButton(Item item)
	{
		HierarchyItemButton hierarchyItemButton = GetHierarchyItemButton(item);
		if (!(hierarchyItemButton == null))
		{
			hierarchyItemButton.Counter--;
			if (hierarchyItemButton.Counter == 0)
			{
				_items.Remove(item);
				HierarchyFlex.Remove(hierarchyItemButton, destroy: true);
			}
		}
	}

	private void SelectCategoryButton(CategoryButton categoryButton)
	{
		if (!(_selectedCategory == categoryButton))
		{
			SelectHierarchyItemButton(null);
			Flex.ForgetAll();
			if (_selectedCategory != null)
			{
				_selectedCategory.State = false;
			}
			_selectedCategory = categoryButton;
			if (_selectedCategory != null)
			{
				_selectedCategory.State = true;
				SelectCategory(categoryButton.Category);
			}
			_scrollView.Progress = 1f;
		}
	}

	private void SelectCategory(Category category)
	{
		if (!_registries.TryGetValue(category, out var value))
		{
			return;
		}
		foreach (KeyValuePair<Type, Dictionary<InstanceHandle, Inspector>> item in value)
		{
			foreach (KeyValuePair<InstanceHandle, Inspector> item2 in item.Value)
			{
				Flex.Remember(item2.Value);
				if ((bool)_debugInterface)
				{
					_debugInterface.SetTransparencyRecursive(item2.Value, !_debugInterface.OpacityOverride);
				}
			}
		}
	}

	private void ToggleFoldItem(HierarchyItemButton button)
	{
		if (!(button == null))
		{
			if (button.Foldout.State)
			{
				FoldItem(button);
			}
			else
			{
				UnfoldItem(button);
			}
		}
	}

	private void FoldItem(HierarchyItemButton button)
	{
		button.Foldout.State = false;
	}

	private void UnfoldItem(HierarchyItemButton button)
	{
		button.Foldout.State = true;
	}

	private void SelectHierarchyItemButton(HierarchyItemButton button)
	{
		if (_selectedItem == button)
		{
			ToggleFoldItem(button);
			return;
		}
		SelectCategoryButton(null);
		Flex.ForgetAll();
		if (_selectedItem != null)
		{
			_selectedItem.Item?.ClearContent();
			_selectedItem.Label.State = false;
		}
		_selectedItem = button;
		if (_selectedItem != null)
		{
			_selectedItem.Label.State = true;
			SelectItem(_selectedItem.Item);
			UnfoldItem(button);
		}
		_scrollView.Progress = 1f;
	}

	private void SelectItem(Item item)
	{
		item.BuildContent();
		SelectCategory(item.Category);
	}

	internal void SetPanelPosition(RuntimeSettings.DistanceOption distanceOption, bool skipAnimation = false)
	{
		ValueContainer<Vector3> valueContainer = ValueContainer<Vector3>.Load("InspectorsPanelPositions");
		_targetPosition = distanceOption switch
		{
			RuntimeSettings.DistanceOption.Close => valueContainer["Close"], 
			RuntimeSettings.DistanceOption.Far => valueContainer["Far"], 
			_ => valueContainer["Default"], 
		};
		if (skipAnimation)
		{
			base.SphericalCoordinates = _targetPosition;
			_currentPosition = _targetPosition;
		}
		else
		{
			_lerpCompleted = false;
		}
	}

	private void Update()
	{
		if (_hierarchyIcon.State)
		{
			DebugManagerAddon<Meta.XR.ImmersiveDebugger.Hierarchy.Manager>.Instance?.Refresh();
		}
		if (!_lerpCompleted)
		{
			_currentPosition = Utils.LerpPosition(_currentPosition, _targetPosition, _lerpSpeed);
			_lerpCompleted = _currentPosition == _targetPosition;
			base.SphericalCoordinates = _currentPosition;
		}
	}
}
