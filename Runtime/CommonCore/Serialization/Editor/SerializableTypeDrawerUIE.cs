using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace CommonCore
{
    [CustomPropertyDrawer(typeof(SerializableType<>), true)]
    public class SerializableTypeDrawerUIE : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty InProperty)
        {
            List<System.Type> SupportedTypes = new();

            System.Type BoxedValueType = InProperty.boxedValue.GetType();
            System.Type FilterType = BoxedValueType.GetGenericArguments()[0];

            // find all of the child types
            foreach (var Assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var Type in Assembly.GetTypes())
                {
                    if (Type.IsSubclassOf(FilterType) && !Type.IsAbstract)
                        SupportedTypes.Add(Type);
                }
            }

            // sort by name
            SupportedTypes.Sort((LHS, RHS) => LHS.Name.CompareTo(RHS.Name));

            var PropertyContainer = new VisualElement();

            bool bIsInContainer = System.Type.GetType(InProperty.displayName) != null;

            // Setup the type selector options
            var TypeSelector = new DropdownField(bIsInContainer ? string.Empty : InProperty.displayName);
            TypeSelector.choices.Add("None");
            foreach (var Type in SupportedTypes)
            {
                TypeSelector.choices.Add(Type.Name);
            }

            // attempt to select the name
            SerializedProperty NameProp = InProperty.FindPropertyRelative("AssemblyQualifiedName");

            if (string.IsNullOrEmpty(NameProp.stringValue))
                TypeSelector.index = 0;
            else
            {
                // search for the name
                bool bFound = false;
                for (int TypeIndex = 0; TypeIndex < SupportedTypes.Count; TypeIndex++)
                {
                    if (SupportedTypes[TypeIndex].AssemblyQualifiedName == NameProp.stringValue)
                    {
                        bFound = true;
                        TypeSelector.index = TypeIndex + 1;
                        break;
                    }
                }

                if (!bFound)
                    TypeSelector.index = 0;
            }

            // setup the callback
            TypeSelector.RegisterValueChangedCallback(CallbackData =>
            {
                NameProp.stringValue = TypeSelector.index == 0 ? string.Empty : SupportedTypes[TypeSelector.index - 1].AssemblyQualifiedName;
                NameProp.serializedObject.ApplyModifiedProperties();
            });

            PropertyContainer.Add(TypeSelector);

            return PropertyContainer;
        }
    }
}
