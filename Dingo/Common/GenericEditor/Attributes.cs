using System;

namespace Dingo.Common
{
    public class NameAttribute : Attribute
    {
        public string Name;

        public NameAttribute(string name)
        {
            Name = name;
        }
    }

    public class ShowIfAttribute : Attribute
    {
        public string FieldName;
        public bool Invert;

        public ShowIfAttribute(string fieldName, bool invert = false)
        {
            FieldName = fieldName;
            Invert = invert;
        }
    }
}