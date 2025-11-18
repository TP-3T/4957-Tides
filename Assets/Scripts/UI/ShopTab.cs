using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TTT.UI
{
    public class ShopTab : MonoBehaviour
    {
        [field: SerializeField]
        public TextMeshProUGUI TextArea { get; private set; }

        [field: SerializeField]
        public Button Button { get; private set; }
    }
}
