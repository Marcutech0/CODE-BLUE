using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExpandablePanel : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float expandedHeight = 200f;
    [SerializeField] private float collapsedHeight = 50f;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private ExpandablePanelGroup panelGroup;

    private bool isExpanded = false;
    private float originalYPosition;

    private void Start()
    {
        panel.sizeDelta = new Vector2(panel.sizeDelta.x, collapsedHeight);
        originalYPosition = panel.anchoredPosition.y;
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        float targetHeight = isExpanded ? expandedHeight : collapsedHeight;
        float heightDifference = targetHeight - panel.sizeDelta.y;

        StartCoroutine(AnimateHeight(targetHeight));
        panelGroup.UpdatePanelPositions(this, heightDifference);
    }

    private IEnumerator AnimateHeight(float targetHeight)
    {
        float startHeight = panel.sizeDelta.y;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float newHeight = Mathf.Lerp(startHeight, targetHeight, elapsedTime);
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, newHeight);

            // Move downward only
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, originalYPosition - (newHeight - collapsedHeight) / 2);

            yield return null;
        }

        panel.sizeDelta = new Vector2(panel.sizeDelta.x, targetHeight);
    }
}
