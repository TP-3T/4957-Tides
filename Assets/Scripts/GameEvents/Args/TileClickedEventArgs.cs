using TTT.DataClasses.HexData;
using UnityEngine;

namespace TTT.GameEvents
{
    public class TileClickedEventArgs : Object
    {
        public HexCell? SelectedCell { get; set; }
    }
}
