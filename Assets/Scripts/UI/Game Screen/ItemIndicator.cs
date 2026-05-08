using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class ItemIndicator : NetworkBehaviour
    {
        [SerializeField] private Image _itemIcon;

        public void UpdateItemIndicator(Sprite itemLogo, string itemName)
        {
            _itemIcon.sprite = itemLogo;
            
        }
    }
}