using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using System;

namespace TTT.DataClasses
{
    public struct GlobalInformation : INetworkSerializable, IEquatable<GlobalInformation>
    {
        public bool FTTaken;
        public int Year;
        public FixedString32Bytes Season;
        public ulong CurrentPlayerId;
        public ulong NextPlayerId;

        public GlobalInformation(bool fttaken, int year, string season)
        {
            FTTaken = fttaken;
            Year = year;
            Season = season;
            CurrentPlayerId = ulong.MaxValue;
            NextPlayerId = ulong.MaxValue;
        }

        public bool Equals(GlobalInformation other)
        {
            return Year == other.Year
                && FTTaken == other.FTTaken
                && Season == other.Season;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Year);
            serializer.SerializeValue(ref FTTaken);
            serializer.SerializeValue(ref Season);
        }

        public override string ToString()
        {
            return $"{Year}, {Season}, {FTTaken}";
        }
    }
}
