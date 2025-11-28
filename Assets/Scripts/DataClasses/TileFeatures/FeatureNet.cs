using System;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Windows.Forms;

namespace TTT.DataClasses.Assets.Scripts.DataClasses.TileFeatures
{
    public struct FeatureNet : INetworkSerializable, IEquatable<FeatureNet>
    {
        public FixedString32Bytes FeatureId;
        public Vector3 FeaturePosition;

        public FeatureNet(FixedString32Bytes fid, Vector3 fpos)
        {
            FeatureId = fid;
            FeaturePosition = fpos;
        }

        public bool Equals(FeatureNet other)
        {
            return FeatureId.Equals(other.FeatureId)
                && FeaturePosition.Equals(other.FeaturePosition);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref FeatureId);
            serializer.SerializeValue(ref FeaturePosition);
        }
    }
}

