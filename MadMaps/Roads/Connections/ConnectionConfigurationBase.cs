using System;

namespace MadMaps.Roads
{
    public abstract class ConnectionConfigurationBase
    {
        public int Priority;

        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(__guid))
                {
                    __guid = System.Guid.NewGuid().ToString();
                }
                return __guid;
            }
        }
        private string __guid;

        public abstract Type GetMonoType();
    }
}