using System;
using UnityEngine;

namespace Dingo.WorldStamp
{
    [Serializable]
    public struct PrefabObjectData
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public GameObject Prefab;
        public string Guid;
        public bool AbsoluteHeight;
    }
}