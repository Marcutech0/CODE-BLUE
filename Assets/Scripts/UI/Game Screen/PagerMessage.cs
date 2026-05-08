using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class PagerMessage : SingletonBehaviour<PagerMessage>
    {
        [SerializeField] TextMeshProUGUI _messageText;
        [SerializeField] float _typewriterSpeed = 5; // 5 chars per second
        [SerializeField] float _closeDelay = 4;

        [SerializeField] Image _notifFlag;

        int _charTracker;

        private void Start()
        {
            transform.localScale = Vector3.zero;
            _notifFlag.color = new Color(1, 1, 1, 0);
        }

        public void ShowMessage(string message) => StartCoroutine(ShowMessageInternal(message));
        IEnumerator ShowMessageInternal(string message)
        {
            AudioManager.Instance.PlayGameSfx("new-message");
            transform.DOScale(1, 0.2f).SetEase(Ease.InFlash).From(0);
            _messageText.text = "! NEW MESSAGE !";

            yield return new WaitForSeconds(3);
            _messageText.text = "";

            var msg = "";
            foreach (char c in message)
            {
                msg += c;
                _messageText.text = msg;

                // play blip every 3 chars
                _charTracker++;
                if (_charTracker >= 3)
                {
                    AudioManager.Instance.PlayUISfx("blip");
                    _charTracker = 0;   
                }

                yield return new WaitForSeconds(_typewriterSpeed / 60);
            }

            yield return new WaitForSeconds(_closeDelay);
            transform.DOScale(0, 0.2f).SetEase(Ease.Linear).From(1);
        }

        Transform _flagFollow;

        public void ShowNotificationFlag(Transform target)
        {
            _notifFlag.color = Color.white;

            _flagFollow = target;
        }

        float _padding = 100;
        private void LateUpdate()
        {
            if (_flagFollow == null) return;

            var screenPos = Camera.main.WorldToScreenPoint(_flagFollow.localPosition);
            screenPos.z = 0;
            screenPos.x = Mathf.Clamp(screenPos.x, _padding, Screen.width - _padding);
            screenPos.y = Mathf.Clamp(screenPos.y, _padding, Screen.height - _padding);

            if ((screenPos - new Vector3(Screen.width, Screen.height)/2).sqrMagnitude < 250 * 250)
            {
                _notifFlag.color = new Color(1, 1, 1, 0);
                _flagFollow = null;
                return;
            }

            _notifFlag.transform.position = screenPos + Vector3.up;
        }
    }
}
