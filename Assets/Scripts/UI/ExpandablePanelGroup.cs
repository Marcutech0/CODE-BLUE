using System.Collections.Generic;
using UnityEngine;

public class ExpandablePanelGroup : MonoBehaviour
{
    [SerializeField] private List<ExpandablePanel> panels;

    public void UpdatePanelPositions(ExpandablePanel expandedPanel, float heightDifference)
    {
        bool movePanels = false;

        foreach (var panel in panels)
        {
            if (panel == expandedPanel)
            {
                movePanels = true; // Start moving panels below this one
                continue;
            }

            if (movePanels)
            {
                Vector2 newPosition = panel.GetComponent<RectTransform>().anchoredPosition;
                newPosition.y -= heightDifference; // Move down when above expands
                panel.GetComponent<RectTransform>().anchoredPosition = newPosition;
            }
        }
    }
}
