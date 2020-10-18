using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Resourcer.Editor
{
    public static class EditorGUIUtilityExtensions
    {
        #region Constants
        private const string cLoadIconMethodName = "LoadIcon";
        #endregion

        #region Fields
        private static readonly Type sEditorGUIUtilityType = typeof(EditorGUIUtility);
        private static readonly MethodInfo sLoadIconMethodInfo;
        #endregion

        static EditorGUIUtilityExtensions ()
        {
            sLoadIconMethodInfo = sEditorGUIUtilityType.GetMethod(
                name: cLoadIconMethodName,
                bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static Texture2D LoadIcon (string name) =>
            sLoadIconMethodInfo.Invoke(obj: null, parameters: new object[] {name}) as Texture2D;
    }
}