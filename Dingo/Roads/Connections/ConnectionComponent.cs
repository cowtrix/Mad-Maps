using System;
using Dingo.Common;
using UnityEngine;

namespace Dingo.Roads
{
    [Serializable]
    public class ComponentConfigurationRef
    {
        public ConnectionConfiguration Configuration;
        public string ComponentGUID;

        public ComponentConfigurationRef()
        {
        }

        public ComponentConfigurationRef(ConnectionConfiguration configuration, string guid)
        {
            Configuration = configuration;
            ComponentGUID = guid;
        }

        public T GetConfig<T>() where T : ConnectionConfigurationBase
        {
            return GetConfig() as T;
        }

        public ConnectionConfigurationBase GetConfig()
        {
            if (Configuration == null)
            {
                return null;
            }
            return Configuration.GetComponentWithGUID(ComponentGUID);
        }
    }

    [StripComponentOnBuild]
    public abstract class ConnectionComponent : sBehaviour
    {
        public bool OverridePriority;
        public int Priority;

        public ComponentConfigurationRef Configuration;
        public NodeConnection NodeConnection;

        public RoadNetwork Network
        {
            get
            {
                if (NodeConnection == null)
                {
                    return null;
                }
                return NodeConnection.Network;
            }
        }

        public virtual void Think()
        {
            if (NodeConnection == null)
            {
                Destroy();
                DestroyImmediate(this);
            }
        }

        public virtual void CopyTo(ConnectionComponent otherRo)
        {
            otherRo.Configuration = Configuration.JSONClone();
        }

        public virtual void SetData(NodeConnection nodeConnection, ComponentConfigurationRef config)
        {
            Configuration = config;
            NodeConnection = nodeConnection;
        }
        
        public virtual void Destroy()
        {
        }

        [ContextMenu("Force Rebake")]
        public void ForceBake()
        {
            var prebake = this as IOnPrebakeCallback;
            if (prebake != null)
            {
                prebake.OnPrebake();
            }
            var bake = this as IOnBakeCallback;
            if (bake != null)
            {
                bake.OnBake();
            }
            var post = this as IOnPostBakeCallback;
            if (post != null)
            {
                post.OnPostBake();
            }
        }

        /// <summary>
        /// If a connection component wants to enforce that it should be done early or late
        /// </summary>
        /// <returns></returns>
        public virtual int GetPriority()
        {
            if (OverridePriority)
            {
                return Priority;
            }
            return Configuration != null && Configuration.GetConfig() != null ? Configuration.GetConfig().Priority : 0;
        }

        public void Strip()
        {
            DestroyImmediate(this);
        }
    }
}