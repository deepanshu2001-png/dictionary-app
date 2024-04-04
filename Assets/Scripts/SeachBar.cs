using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeachBar : MonoBehaviour
{
    public InputField searchField; // Public reference to the InputField component

    public void OnSearch() // This function gets called when the user presses enter or submits the search
    {
        string searchTerm = searchField.text; // Get the text entered in the search field

        // Implement your search logic here based on the searchTerm
        Debug.Log("Search term: " + searchTerm); // For now, log the term to the console
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
