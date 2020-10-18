using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Resourcer.Editor
{
    public class NamedObjectElement : VisualElement
    {
        #region Fields
        private readonly Image _Image = new Image
        {
            scaleMode = ScaleMode.ScaleToFit,
            style =
            {
                alignSelf = Align.Center,
                width = 28,
                height = 20,
            }
        };

        private readonly Label _Name = new Label
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft,
                width = 200,
            }
        };

        private readonly Label _Type = new Label
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleRight,
                width = 200,
            }
        };

        private readonly Label _Path = new Label
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleRight,
                flexGrow = 1,
                flexBasis = 1,
            }
        };
        #endregion

        public NamedObjectElement ()
        {
            // Tile horizontally
            style.flexDirection = FlexDirection.Row;

            Add(child: _Image);
            Add(child: _Name);
            Add(child: _Type);
            Add(child: _Path);
        }

        public void Bind (Object obj)
        {
            if (obj is Texture2D objTexture)
            {
                _Image.image = objTexture;
            }
            else if (obj != null &&
                EditorGUIUtilityExtensions.LoadIcon(name: $"{obj.GetType().Name} Icon") is Texture2D typeIcon)
            {
                _Image.image = typeIcon;
            }
            else
            {
                _Image.image = null;
            }
            _Name.text = obj != null ? obj.name : string.Empty;
            _Type.text = obj != null ? obj.GetType().Name : string.Empty;
            _Path.text = obj != null ? AssetDatabase.GetAssetPath(assetObject: obj) : string.Empty;
        }
    }
}