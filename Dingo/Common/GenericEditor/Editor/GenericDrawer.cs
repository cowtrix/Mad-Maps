using System;
using System.Reflection;

namespace Dingo.Common.GenericEditor
{
    public interface IGenericDrawer
    {
        object DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null);
    }

    public interface ITypedGenericDrawer<T> : IGenericDrawer
    {
    }

    public abstract class GenericDrawer<T> : ITypedGenericDrawer<T>
    {
        public object DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null)
        {
            return DrawGUIInternal((T)target, label, targetType, fieldInfo, context);
        }
        protected abstract T DrawGUIInternal(T target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null); 
    }
}