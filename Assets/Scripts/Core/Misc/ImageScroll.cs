using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue.Misc 
{ 
    public class ImageScroll : MonoBehaviour
    {
        [SerializeField] RawImage _image;
        [SerializeField] Vector2 _scrollOffset;

        private void Update()
        {
            _image.uvRect = new(_image.uvRect.position + _scrollOffset * Time.deltaTime, _image.uvRect.size);
        }
    }
}
