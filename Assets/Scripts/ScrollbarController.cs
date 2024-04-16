using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;

    private RectTransform contentRectTransform;

    void Start()
    {
        // Get the RectTransform component of the content
        contentRectTransform = scrollRect.content.GetComponent<RectTransform>();

        // Subscribe to content size changes
        LayoutRebuilder.MarkLayoutForRebuild(contentRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        ContentSizeChanged();
    }

    void Update()
    {
        // Check if content size has changed
        if (contentRectTransform.sizeDelta != lastContentSize)
        {
            ContentSizeChanged();
            lastContentSize = contentRectTransform.sizeDelta;
        }
    }

    private Vector2 lastContentSize;

    void ContentSizeChanged()
    {
        // Get the height of the ScrollRect
        float viewportHeight = scrollRect.GetComponent<RectTransform>().rect.height;

        // Get the height of the content
        float contentHeight = contentRectTransform.rect.height;

        // Calculate the scrollbar handle size
        float handleSize = viewportHeight / contentHeight;

        // Ensure handle size is within valid range
        handleSize = Mathf.Clamp01(handleSize);

        // Update the scrollbar handle size
        scrollbar.size = handleSize;

        // Update the scrollbar interactability based on whether scrolling is needed
        scrollbar.interactable = contentHeight > viewportHeight;
    }
}
