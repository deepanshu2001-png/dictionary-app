using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class seemorebutton : MonoBehaviour
{
    public GameObject boxPrefab; // Reference to your box prefab
    public int increaseAmount; // Amount to increase the height by
    public int maxBoxes; // Maximum number of boxes to display before removing the oldest one

    private GridLayoutGroup grid;
    private RectTransform gridRectTransform;

    private bool isButtonClicked = false; // Flag to track if the button is clicked

    private void Start()
    {
        // Find the GridLayoutGroup component in the hierarchy
        grid = GetComponentInChildren<GridLayoutGroup>();
        gridRectTransform = grid.GetComponent<RectTransform>();
    }

    public void OnSeeMoreButtonClick()
    {
        isButtonClicked = true;

        // Instantiate a new box prefab only if the button is clicked properly
        if (isButtonClicked)
        {
            // Instantiate a new word meaning box prefab
            GameObject newBox = Instantiate(boxPrefab, grid.transform);

            // Check if the number of boxes exceeds the maximum limit
            if (grid.transform.childCount > maxBoxes)
            {
                // Remove the oldest box
                Destroy(grid.transform.GetChild(0).gameObject);
            }

            // Increase the height of the grid layout group
            float newHeight = gridRectTransform.rect.height + increaseAmount;
            gridRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }
        else
        {
            Debug.LogError("Button not clicked properly!");
        }
    }
}
