using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RoboRyanTron.SearchableEnum.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using SuperUnity.Hub.Configs;

[CustomPropertyDrawer(typeof(BaseStableEnum), true)]
public class StableEnumDrawer : PropertyDrawer
{
    /*    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Enum enumValue = (Enum)((BaseStableEnum)fieldInfo.GetValue(property.serializedObject.targetObject)).valueObject;

            enumValue = EditorGUI.EnumPopup(position, label, enumValue);
            valueProp.intValue = (int)Convert.ChangeType(enumValue, enumValue.GetType());
            proxyProp.stringValue = enumValue.ToString();
        }
    */

    /// <summary>
    /// Cache of the hash to use to resolve the ID for the drawer.
    /// </summary>
    private int idHash;

    public bool changed;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var proxyProp = property.FindPropertyRelative(BaseStableEnum.kProxyPropName);
        var valueProp = property.FindPropertyRelative(BaseStableEnum.kValuePropName);

        string[] list = Enum.GetNames(((BaseStableEnum)fieldInfo.GetValue(property.serializedObject.targetObject)).valueObject.GetType());

        // By manually creating the control ID, we can keep the ID for the
        // label and button the same. This lets them be selected together
        // with the keyboard in the inspector, much like a normal popup.
        if (idHash == 0) idHash = "StableEnumDrawer".GetHashCode();
        int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, id, label);

        proxyProp.stringValue = EditorGUI.TextField(new Rect(position.x, position.y, position.width - 22, position.height), proxyProp.stringValue);

        GUIContent buttonText;
        buttonText = new GUIContent();

        if (DropdownButton(id, new Rect(position.x + position.width - 20.0f, position.y, 20.0f, position.height), buttonText))
        {
            int index = Mathf.Max(0, Array.IndexOf(list, proxyProp.stringValue));

            Action<int> onSelect = i =>
            {
                /*                property.stringValue = list[i];
                                property.serializedObject.ApplyModifiedProperties();*/

                if (proxyProp.stringValue != list[i])
                {
                    valueProp.intValue = 0;
                    proxyProp.stringValue = list[i];
                    property.serializedObject.ApplyModifiedProperties();
                    changed = true;
                }
            };

            SearchablePopup.Show(position, list, index, onSelect);
        }
        EditorGUI.EndProperty();

        if (changed)
        {
            changed = false;
            GUI.changed = true;
        }

    }

    /// <summary>
    /// A custom button drawer that allows for a controlID so that we can
    /// sync the button ID and the label ID to allow for keyboard
    /// navigation like the built-in enum drawers.
    /// </summary>
    private static bool DropdownButton(int id, Rect position, GUIContent content)
    {
        Event current = Event.current;
        switch (current.type)
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && current.button == 0)
                {
                    Event.current.Use();
                    return true;
                }
                break;
            case EventType.KeyDown:
                if (GUIUtility.keyboardControl == id && current.character == '\n')
                {
                    Event.current.Use();
                    return true;
                }
                break;
            case EventType.Repaint:
                EditorStyles.popup.Draw(position, content, id, false);
                break;
        }
        return false;
    }
    // Draw the property inside the given rect

}
#endif