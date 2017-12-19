using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dingo.Roads
{
    [CreateAssetMenu(menuName = "sRoads/Connection Configuration")]
    public class ConnectionConfiguration : ScriptableObject, ISerializationCallbackReceiver
    {
        public Color Color = UnityEngine.Color.white;

        [AllowDerived]
        [ReorderableList]
        public List<ConnectionConfigurationBase> Components = new List<ConnectionConfigurationBase>();

        [HideInInspector]
        [SerializeField]
        private List<DerivedComponentJsonDataRow> ComponentsJSON = new List<DerivedComponentJsonDataRow>();
        
        public void OnBeforeSerialize()
        {
            ComponentsJSON.Clear();
            foreach (var connectionComponentConfigBase in Components)
            {
                var t = connectionComponentConfigBase.GetType();
                var jsonRow = new DerivedComponentJsonDataRow()
                {
                    AssemblyQualifiedName = t.AssemblyQualifiedName,
                };
                jsonRow.JsonText = JSONSerializer.Serialize(t, connectionComponentConfigBase, false,
                    jsonRow.SerializedObjects);
                ComponentsJSON.Add(jsonRow);
            }
        }

        public void OnAfterDeserialize()
        {
            Components.Clear();
            foreach (var row in ComponentsJSON)
            {
                Components.Add(
                    (ConnectionConfigurationBase)
                        JSONSerializer.Deserialize(Type.GetType(row.AssemblyQualifiedName), row.JsonText,
                            row.SerializedObjects));
            }
        }

        public ConnectionConfigurationBase GetComponentWithGUID(string componentGuid)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                var component = Components[i];
                if (component.GUID == componentGuid)
                {
                    return component;
                }
            }
            return null;
        }
    }
}
