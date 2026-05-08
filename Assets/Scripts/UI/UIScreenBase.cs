using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIScreenBase : NetworkBehaviour
{
    protected CanvasGroup _cg;

    public virtual void SetScreenVisibility(bool isVisible)
    {
        if (_cg == null) _cg = GetComponent<CanvasGroup>();
        _cg.interactable = isVisible;
        _cg.alpha = isVisible ? 1 : 0;
        transform.localPosition = !isVisible ? Vector2.up * 4000 : Vector2.zero;
    }
}
