using System;

namespace Dingo.Terrains
{
    /// <summary>
    /// TODO
    /// </summary>
    public class UserInstantiableAttribute : Attribute
    {
        public bool IsUserInstantiable;
        public UserInstantiableAttribute(bool val)
        {
            IsUserInstantiable = val;
        }
    }
}