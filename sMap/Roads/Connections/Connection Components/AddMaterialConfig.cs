using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace sMap.Roads.Connections
{
    public class AddMaterialConfig : ConnectionComponent, IOnPrebakeCallback, IOnBakeCallback
    {
        [Name("Hurtworld/Material Config")]
        public class Config : ConnectionConfigurationBase
        {
            public EMaterialType Material;
            public override Type GetMonoType()
            {
                return typeof(AddMaterialConfig);
            }
        }

        private MaterialConfig _materialConfig;
        
        public void OnPrebake()
        {
            if (!NodeConnection)
            {
                Debug.LogError("ConnectionMaterialComponent missing NodeConnection!", this);
                enabled = false;
                return;
            }
            _materialConfig = NodeConnection.GetDataContainer()
                .GetOrAddComponent<MaterialConfig>();
        }

        public void OnBake()
        {
            if (Configuration == null)
            {
                return;
            }
            var config = Configuration.GetConfig<Config>();
            if (config == null)
            {
                Debug.LogError("Invalid configuration! Expected ConnectionMaterialConfiguration");
                return;
            }
            _materialConfig.Material = config.Material;
        }
    }
}