using TTT.DataClasses;
using TTT.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Amount<>), useForChildren: true)]
public class AmountDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;

        var thingProp = property.FindRealPropertyRelative("Thing");
        var countProp = property.FindRealPropertyRelative("Count");

        if (thingProp == null || countProp == null)
        {
            Debug.LogWarning("Missing properties");
            return container;
        }

        var label = new Label("Object & Amount");
        label.AddToClassList("unity-base-field__label");

        var thingPropField = new PropertyField(thingProp);
        thingPropField.style.width = new(new Length(50, LengthUnit.Percent));
        thingPropField.style.marginLeft = new(StyleKeyword.Auto);
        thingPropField.label = string.Empty;

        var countPropField = new PropertyField(countProp);
        countPropField.style.width = new(new Length(20, LengthUnit.Percent));
        countPropField.style.marginLeft = new(StyleKeyword.Auto);
        countPropField.label = string.Empty;

        container.Add(label);
        container.Add(thingPropField);
        container.Add(countPropField);

        return container;
    }
}
