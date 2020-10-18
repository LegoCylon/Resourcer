using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Resourcer.Editor
{

    public class ResourceWindow : EditorWindow
    {
        #region Constants
        private const string cEverythingName = "Everything";
        private const string cNothingName = "Nothing";

        private static readonly string[] sBuiltInAssetPaths =
        {
            "Library/unity default resources",
            "Library/unity editor resources",
        };
        #endregion

        #region Fields
        private readonly List<Object> _FilteredAssetList = new List<Object>();
        private readonly List<Object> _FullAssetList = new List<Object>();
        private readonly HashSet<Type> _FilteredTypeSet = new HashSet<Type>();
        private readonly HashSet<Type> _FullTypeSet = new HashSet<Type>();
        private ListView _ListView;
        private readonly List<Object> _SelectedAssetList = new List<Object>();
        private Toolbar _Toolbar;
        private ToolbarPopupSearchField _ToolbarPopupSearchField;
        private ToolbarButton _ToolbarCopySelectedButton;
        #endregion

        [MenuItem(itemName: "Window/General/Resources")]
        private static ResourceWindow Get () => GetWindow<ResourceWindow>(title: "Resources");

        protected void OnEnable ()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            SetupToolbar(parent: rootVisualElement);
            SetupListView(parent: rootVisualElement);

            if (_ToolbarCopySelectedButton != null)
            {
                _ToolbarCopySelectedButton.SetEnabled(value: _SelectedAssetList.Count > 0);
            }

            FindResources();
        }

        private void SetupToolbar (VisualElement parent)
        {
            if (_Toolbar != null)
            {
                _Toolbar.RemoveFromHierarchy();
            }

            _Toolbar = new Toolbar();
            parent.Add(child: _Toolbar);

            if (_ToolbarPopupSearchField != null)
            {
                _ToolbarPopupSearchField.RemoveFromHierarchy();
            }

            _ToolbarPopupSearchField = new ToolbarPopupSearchField();
            _ToolbarPopupSearchField.RegisterValueChangedCallback(callback: _ => RefreshListView());
            _Toolbar.Add(child: _ToolbarPopupSearchField);

            if (_ToolbarCopySelectedButton != null)
            {
                _ToolbarCopySelectedButton.RemoveFromHierarchy();
            }

            _ToolbarCopySelectedButton = new ToolbarButton(clickEvent: () =>
            {
                EditorGUIUtility.systemCopyBuffer = string.Join(
                    separator: $",{Environment.NewLine}",
                    values: _SelectedAssetList.Select(obj => $"\"{obj.name}\""));
            });
            _ToolbarCopySelectedButton.text = "Copy Selected Names";
            _Toolbar.Add(child: _ToolbarCopySelectedButton);
        }

        private void SetupListView (VisualElement parent)
        {
            if (_ListView != null)
            {
                _ListView.RemoveFromHierarchy();
            }

            _SelectedAssetList.Clear();
            _ListView = new ListView
            {
                itemsSource = _FilteredAssetList,
                makeItem = MakeResourceListItem,
                bindItem = BindResourceListItem,
                style =
                {
                    flexBasis = 1,
                    flexGrow = 1,
                }
            };
            _ListView.selectionType = SelectionType.Multiple;
            _ListView.onSelectionChanged += list =>
            {
                _SelectedAssetList.Clear();
                _SelectedAssetList.AddRange(collection: list.Select(selector: obj => obj as Object));
                Selection.objects = _SelectedAssetList.ToArray();
                if (_ToolbarCopySelectedButton != null)
                {
                    _ToolbarCopySelectedButton.SetEnabled(value: _SelectedAssetList.Count > 0);
                }
            };
            parent.Add(child: _ListView);

            RefreshListView();
        }

        private void FindResources ()
        {
            _SelectedAssetList.Clear();
            _FullAssetList.Clear();
            _FullTypeSet.Clear();
            foreach (string builtInPath in sBuiltInAssetPaths)
            {
                foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(assetPath: builtInPath))
                {
                    if (obj == null || string.IsNullOrEmpty(value: obj.name))
                    {
                        continue;
                    }

                    _FullAssetList.Add(item: obj);
                    _FullTypeSet.Add(item: obj.GetType());
                }
            }
            _FullAssetList.Sort(
                comparison: (lhs, rhs) => string.Compare(
                    strA: lhs.name,
                    strB: rhs.name,
                    comparisonType: StringComparison.CurrentCultureIgnoreCase));

            RefreshTypeMenu();
        }

        private void RefreshTypeMenu ()
        {
            while (_ToolbarPopupSearchField.menu.MenuItems().Count > 0)
            {
                _ToolbarPopupSearchField.RemoveAt(index: _ToolbarPopupSearchField.menu.MenuItems().Count - 1);
            }

            _ToolbarPopupSearchField.menu.AppendAction(
                actionName: cEverythingName,
                action: menuItem =>
                {
                    _FilteredTypeSet.UnionWith(other: _FullTypeSet);
                    RefreshListView();
                });
            _ToolbarPopupSearchField.menu.AppendAction(
                actionName: cNothingName,
                action: menuItem =>
                {
                    _FilteredTypeSet.Clear();
                    RefreshListView();
                });
            foreach (Type type in _FullTypeSet.OrderBy(keySelector: type => type.Name))
            {
                _ToolbarPopupSearchField.menu.AppendAction(
                    actionName: type.Name,
                    action: action =>
                    {
                        if (!_FilteredTypeSet.Add(item: type))
                        {
                            _FilteredTypeSet.Remove(item: type);
                        }

                        RefreshListView();
                    },
                    actionStatusCallback: action =>
                        _FilteredTypeSet.Contains(item: type)
                            ? DropdownMenuAction.Status.Checked
                            : DropdownMenuAction.Status.Normal);
            }

            _FilteredTypeSet.UnionWith(other: _FullTypeSet);
            RefreshListView();
        }

        private void RefreshListView ()
        {
            _FilteredAssetList.Clear();
            _FilteredAssetList.AddRange(
                collection: _FullAssetList.Where(
                    predicate: obj =>
                    {
                        if (!_FilteredTypeSet.Contains(item: obj.GetType()))
                        {
                            return false;
                        }
                        return obj.name.IndexOf(
                                value: _ToolbarPopupSearchField.value,
                                comparisonType: StringComparison.CurrentCultureIgnoreCase) >=
                            0;
                    }));
            _ListView.Refresh();
        }

        private VisualElement MakeResourceListItem () => new NamedObjectElement();

        private void BindResourceListItem (VisualElement element, int index)
        {
            if (element is NamedObjectElement namedObjectElement)
            {
                namedObjectElement.Bind(
                    obj: index < 0 || index >= _FilteredAssetList.Count
                        ? null
                        : _FilteredAssetList[index: index]);
            }
        }
    }
}
