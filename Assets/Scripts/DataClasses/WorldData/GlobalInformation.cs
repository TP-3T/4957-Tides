using System;
using PlasticGui.Diff;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TTT.DataClasses
{
    public struct GlobalInformation : INetworkSerializable, IEquatable<GlobalInformation>
    {
        public int Year;
        public int CO2;
        public int Temp;
        public FixedString32Bytes Season;

        public GlobalInformation(int year, int co2, int temp, FixedString32Bytes season)
        {
            CO2 = co2;
            Year = year;
            Temp = temp;
            Season = season;
        }

        public bool Equals(GlobalInformation other)
        {
            return Year == other.Year
                && CO2 == other.CO2
                && Temp == other.Temp
                && Season == other.Season;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Year);
            serializer.SerializeValue(ref CO2);
            serializer.SerializeValue(ref Temp);
            serializer.SerializeValue(ref Season);
        }

        public override string ToString()
        {
            return $"{Year}, {CO2}, {Temp}, {Season}";
        }
    }
}
