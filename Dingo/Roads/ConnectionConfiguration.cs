using System;
using System.Collections.Generic;
using Dingo.Common.Serialization;
using Dingo.Common.GenericEdtitor;
using UnityEngine;
using DerivedComponentJsonDataRow = Dingo.Common.Serialization.DerivedComponentJsonDataRow;

namespace Dingo.Roads
{
    [CreateAssetMenu(menuName = "sRoads/Connection Configuration")]
    public class ConnectionConfiguration : ScriptableObject, ISerializationCallbackReceiver
    {
        public Color Color = UnityEngine.Color.white;

        [ListGenericUI]
        public List<ConnectionConfigurationBase> Components = new List<ConnectionConfigurationBase>();

        [HideInInspector]
        [SerializeField]
        private List<Common.Serialization.DerivedComponentJsonDataRow> ComponentsJSON = new List<Common.Serialization.DerivedComponentJsonDataRow>();
        
        public void OnBeforeSerialize()
        {
            ComponentsJSON.Clear();
            foreach (var connectionComponentConfigBase in Components)
            {
                var t = connectionComponentConfigBase.GetType();
                var jsonRow = new Common.Serialization.DerivedComponentJsonDataRow()
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
                var type = Type.GetType(row.AssemblyQualifiedName) ?? LegacyManager.GetTypeFromLegacy(row.AssemblyQualifiedName);
                
                if (type == null)
                {
                    Debug.LogError("Couldn't get type " + row.AssemblyQualifiedName);
                }

                Components.Add(
                    (ConnectionConfigurationBase)
                        JSONSerializer.Deserialize(type, row.JsonText,
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
