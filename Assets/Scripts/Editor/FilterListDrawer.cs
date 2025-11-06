using TTT.DataClasses;
using TTT.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(FilterList<>), useForChildren: true)]
public class FilterListDrawer : PropertyDrawer
{
    public const string WHITELIST = "Terrain Whitelist";
    public const string BLACKLIST = "Terrain Blacklist";

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.Center;

        var modeProp = property.FindRealPropertyRelative("Mode");
        var listProp = property.FindRealPropertyRelative("List");

        if (modeProp == null || listProp == null)
        {
            Debug.LogWarning("Missing properties");
            return container;
        }

        var label = new Label(property.displayName);
        container.Add(label);

        var box = new VisualElement();
        box.style.marginLeft = StyleKeyword.Auto;
        box.Add(new Label(" Mode"));

        var modePropField = new PropertyField(modeProp);
        modePropField.label = string.Empty;

        box.Add(modePropField);

        var listPropField = new PropertyField(listProp);
        listPropField.style.marginLeft = StyleKeyword.Auto;
        listPropField.style.width = new(new Length(40, LengthUnit.Percent));
        void updateLabel()
        {
            listPropField.label = modeProp.intValue == 1 ? BLACKLIST : WHITELIST;
        }
        updateLabel();
        modePropField.RegisterValueChangeCallback((_) => updateLabel());

        container.Add(box);
        container.Add(listPropField);

        return container;
    }
}
