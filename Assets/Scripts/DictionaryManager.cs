using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.UIElements.Experimental;

public class DictionaryManager : MonoBehaviour
{
  
    public string baseUrl = "https://activity-api.aceplus.in/web/dictionary_words";
    public GameObject wordPanelPrefab;
    public Transform contentPanel;
    public Button[] letterButtons; // Array of buttons for each letter from A to Z
    public TMP_InputField searchInputField;
    public VerticalLayoutGroup verticalLayoutGroup;
    private TMP_Text searchResultText;
    public GameObject horizontalRowPrefab; // Prefab for horizontal row containing 3 boxes
    public Transform verticalLayoutParent; // Parent transform for vertical layout group
    public Dictionary<char, int> currentPage = new Dictionary<char, int>();
    public List<DictionaryEntry> allWords = new List<DictionaryEntry>(); // Store all fetched word entries
    public float expandedHeight = 130f; // Height to expand the word box to
    public RectTransform canvasRectTransform; // Reference to the canvas object's RectTransform
    private Dictionary<char, List<DictionaryEntry>> allWordsDataForLetter = new Dictionary<char, List<DictionaryEntry>>();


    private void Awake()
    {
        // Find or add the VerticalLayoutGroup component
        verticalLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();


    }

    public void Start()
    {
        for (char c = 'a'; c <= 'z'; c++)
        {
            currentPage[char.ToLower(c)] = 1; ;
        }
        // Initialize currentPage dictionary with default page number for each letter
        foreach (Button button in letterButtons)
        {
            char letter = char.ToLower(button.GetComponentInChildren<TMP_Text>().text[0]); // Ensure lowercase letter
            button.onClick.AddListener(() => OnLetterButtonClicked(letter));
        }
        searchInputField.onEndEdit.AddListener(SearchWord);
        searchResultText = GameObject.Find("searchresult").GetComponent<TextMeshProUGUI>();
        searchResultText.gameObject.SetActive(false); // Initially hide the search result text
    }

    public void OnLetterButtonClicked(char letter)
    {
        currentPage[letter] = 1;
        StartCoroutine(FetchWordsForLetter(letter));
    }

    public IEnumerator FetchWordsForLetter(char letter)
    {
        bool hasNext = true;

        while (hasNext)
        {
            // Prepare API URL with appropriate parameters for the clicked letter and current page
            string apiUrl = $"{baseUrl}?page={currentPage[letter]}&per_page=30&q=&alphabet={letter}";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Parse JSON response
                    DictionaryResponse response = JsonUtility.FromJson<DictionaryResponse>(request.downloadHandler.text);

                    if (!allWordsDataForLetter.ContainsKey(letter))
                    {
                        allWordsDataForLetter[letter] = new List<DictionaryEntry>();
                    }
                    allWordsDataForLetter[letter].AddRange(response.data.list);

                    // Clear existing word panels on the first page for the clicked letter
                    if (currentPage[letter] == 1)
                    {
                        ClearContentPanel();
                        allWords.Clear(); // Clear allWords list on the first page
                    }
                    allWords.AddRange(response.data.list);

                    int wordCount = 0;
                    foreach (DictionaryEntry entry in response.data.list)
                    {
                        if (wordCount % 3 == 0)
                        {
                            GameObject horizontalRow = Instantiate(horizontalRowPrefab, verticalLayoutParent);
                        }
                        Transform rowTransform = verticalLayoutParent.GetChild(verticalLayoutParent.childCount - 1); // Get the last row
                        GameObject panelObject = rowTransform.GetChild(wordCount % 3).gameObject;
                        SetWordAndMeaning(panelObject, entry.game.word, entry.game.meaning, false, "");
                        wordCount++;

                        if (wordCount >= response.data.list.Count)
                        {
                            // If we have reached the end of the words list, check hasNext
                            if (!response.data.has_next)
                            {
                                // If there's no next page and the last row is not filled completely, remove empty boxes
                                int remainingBoxes = wordCount % 3;
                                if (remainingBoxes > 0)
                                {
                                    for (int i = remainingBoxes; i < 3; i++)
                                    {
                                        Destroy(rowTransform.GetChild(i).gameObject);
                                    }
                                }
                                hasNext = false;
                            }
                            break;
                        }
                    }

                    // Increment page number regardless of pagination
                    currentPage[letter]++;
                }
                else
                {
                    Debug.LogError($"Failed to fetch data: {request.error}");
                    // Break the loop if request fails
                    break;
                }
            }
        }
    }

    void ClearContentPanel()
    {
        foreach (Transform child in verticalLayoutParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void SearchWord(string searchTerm)
    {
        ClearContentPanel();

        searchTerm = searchTerm.ToLower(); // Convert search term to lowercase for case-insensitive search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            verticalLayoutGroup.spacing = -20;
            searchResultText.gameObject.SetActive(true);
            searchResultText.text = $"Showing results with words containing '{searchTerm}'";
            canvasRectTransform.anchoredPosition = new Vector2(canvasRectTransform.anchoredPosition.x, -1894.21f);

            Scrollbar scrollbar = contentPanel.GetComponentInChildren<Scrollbar>();
            if (scrollbar != null)
            {
                scrollbar.gameObject.SetActive(false);
            }


            // Flag to track if any matching word is found
            bool wordFound = false;

            // Populate the content panel with search results
            foreach (char letter in allWordsDataForLetter.Keys)
            {
                foreach (DictionaryEntry entry in allWordsDataForLetter[letter])
                {
                    if (entry.game.word.ToLower().Contains(searchTerm) || entry.game.meaning.ToLower().Contains(searchTerm))
                    {
                        GameObject horizontalRow = Instantiate(horizontalRowPrefab, verticalLayoutParent);
                        GameObject panelObject = horizontalRow.transform.GetChild(0).gameObject; // Always use the first box in the row
                        SetWordAndMeaning(panelObject, entry.game.word, entry.game.meaning, true, searchTerm); // Pass searchTerm to SetWordAndMeaning // Pass true to indicate it's a search result
                        wordFound = true; // Set flag to true once a match is found
                                          // Destroy the other two boxes in the horizontal row
                        Destroy(horizontalRow.transform.GetChild(1).gameObject);
                        Destroy(horizontalRow.transform.GetChild(2).gameObject);
                    }
                }
            }
            // If no matching word is found, display a message
            if (!wordFound)
            {
                GameObject horizontalRow = Instantiate(horizontalRowPrefab, verticalLayoutParent);
                GameObject panelObject = horizontalRow.transform.GetChild(0).gameObject; // Always use the first box in the row
                SetWordAndMeaning(panelObject, "Word not found", "", true, ""); // Display a message indicating word not found

                // Destroy the other two boxes in the horizontal row
                Destroy(horizontalRow.transform.GetChild(1).gameObject);
                Destroy(horizontalRow.transform.GetChild(2).gameObject);
            }
        }
        else
        {
            verticalLayoutGroup.spacing = 10;

            searchResultText.gameObject.SetActive(false);
            Scrollbar scrollbar = GetComponentInChildren<Scrollbar>();
            if (scrollbar != null)
            {
                scrollbar.gameObject.SetActive(true);
            }
            canvasRectTransform.anchoredPosition = new Vector2(0.24997f, -1872.392f);
        }
    }



    // Method to set the word and meaning texts on the instantiated panel
    public void SetWordAndMeaning(GameObject panelObject, string word, string meaning, bool isSearchResult, string searchterm)
    {
        // Find the Text components directly in panelObject
        TMP_Text wordText = panelObject.transform.Find("WordText")?.GetComponent<TMP_Text>();
        TMP_Text meaningText = panelObject.transform.Find("MeaningText")?.GetComponent<TMP_Text>();

        Button seeMoreButton = panelObject.transform.Find("SeeMoreButton")?.GetComponent<Button>();

        // Ensure that both WordText and MeaningText are found
        if (wordText == null || meaningText == null)
        {
            Debug.LogError("WordText or MeaningText component not found in panelObject: " + panelObject.name);
            return;
        }

        // Set the text values
        wordText.text = word;

        // Set the alignment of the word heading to left if it's a search result
        if (isSearchResult)
        {
            wordText.alignment = TextAlignmentOptions.Left;
            wordText.rectTransform.anchorMin = new Vector2(0, 1);
            wordText.rectTransform.anchorMax = new Vector2(0, 1);
            wordText.rectTransform.pivot = new Vector2(0, 1);
        }
        if(isSearchResult)
        {
            seeMoreButton.gameObject.SetActive(false);
        }
        string originalMeaning = meaning;

        // Split the meaning into words
        string[] words = meaning.Split(' ');

        // Display the meaning text in a single line if it's a search result
        if (isSearchResult)
        {
            meaningText.alignment = TextAlignmentOptions.Left;
            meaningText.rectTransform.anchorMin = new Vector2(0, 0.5f); // Middle Left
            meaningText.rectTransform.anchorMax = new Vector2(0, 0.5f); // Middle Left
            meaningText.rectTransform.pivot = new Vector2(0, 0.5f); // Middle Left
            meaningText.rectTransform.sizeDelta = new Vector2(655.43f, meaningText.rectTransform.sizeDelta.y);
            meaningText.text = string.Join(" ", words);
            string highlightedMeaning = HighlightSearchTerm(meaning, searchterm);
            meaningText.text = highlightedMeaning;
        }
            List<string> lines = new List<string>();

            // Arrange words into lines with maximum four words per line
            for (int i = 0; i < words.Length; i += 4)
            {
                int wordCount = Mathf.Min(4, words.Length - i);
                string line = string.Join(" ", words, i, wordCount);
                lines.Add(line);
            }

            // Limit the number of lines to three
            if (lines.Count > 3)
            {
                lines.RemoveRange(3, lines.Count - 3);
                lines[2] += "..."; // Add ellipsis to the end of the third line
                seeMoreButton.gameObject.SetActive(true);
                // Add an event listener to the See More button
                seeMoreButton.onClick.AddListener(() => OnSeeMoreClicked(panelObject, originalMeaning));
            }
            else
            {
                // Hide the See More button
                seeMoreButton.gameObject.SetActive(false);
            }

            // Set the meaning text
            meaningText.text = string.Join("\n", lines);

            float lineHeight = meaningText.fontSize * meaningText.lineSpacing;
            float height = lines.Count * lineHeight;
            meaningText.rectTransform.sizeDelta = new Vector2(meaningText.rectTransform.sizeDelta.x, height);
        
    }
    private string HighlightSearchTerm(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return text;
        }

        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].ToLower().Contains(searchTerm))
            {
                words[i] = "<color=yellow>" + words[i] + "</color>";
            }
        }

        return string.Join(" ", words);
    }


    private void OnSeeMoreClicked(GameObject panelObject, string fullMeaning)
    {
        TMP_Text meaningText = panelObject.transform.Find("MeaningText")?.GetComponent<TMP_Text>();
        Button seeMoreButton = panelObject.transform.Find("SeeMoreButton")?.GetComponent<Button>();
        Button seeLessButton = panelObject.transform.Find("SeeLessButton")?.GetComponent<Button>();

        if (meaningText == null || seeMoreButton == null)
        {
            Debug.LogError("MeaningText or SeeMoreButton component not found in panelObject: " + panelObject.name);
            return;
        }

        meaningText.text = fullMeaning;

        // Change the property of the content size fitter to preferred size
        ContentSizeFitter contentSizeFitter = panelObject.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Hide the See More button after expanding
        seeLessButton.gameObject.SetActive(true);
        seeMoreButton.gameObject.SetActive(false);

        seeLessButton.onClick.AddListener(() => OnSeeLessClicked(panelObject));
        Debug.Log("See More button clicked for panel: " + panelObject.name);

    }

    private void OnSeeLessClicked(GameObject panelObject)
    {
        TMP_Text meaningText = panelObject.transform.Find("MeaningText")?.GetComponent<TMP_Text>();
        Button seeMoreButton = panelObject.transform.Find("SeeMoreButton")?.GetComponent<Button>();
        Button seeLessButton = panelObject.transform.Find("SeeLessButton")?.GetComponent<Button>();

        if (meaningText == null || seeMoreButton == null || seeLessButton == null)
        {
            Debug.LogError("MeaningText, SeeMoreButton, or SeeLessButton component not found in panelObject: " + panelObject.name);
            return;
        }
        string[] words = meaningText.text.Split(' ');
        List<string> lines = new List<string>();

        // Arrange words into lines with maximum four words per line
        for (int i = 0; i < words.Length; i += 4)
        {
            int wordCount = Mathf.Min(4, words.Length - i);
            string line = string.Join(" ", words, i, wordCount);
            lines.Add(line);
        }

        // Limit the number of lines to three
        if (lines.Count > 3)
        {
            lines.RemoveRange(3, lines.Count - 3);
            lines[2] += "..."; // Add ellipsis to the end of the third line
            
        }

        // Set the meaning text
        meaningText.text = string.Join("\n", lines);
        RectTransform meaningRectTransform = meaningText.rectTransform;
        meaningRectTransform.sizeDelta = new Vector2(183f, 54f); // Adjust width and height as needed

        // Change the property of the content size fitter back to unconstrained
        ContentSizeFitter contentSizeFitter = panelObject.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        // Hide the See Less button and show the See More button after collapsing
        seeLessButton.gameObject.SetActive(false);
        seeMoreButton.gameObject.SetActive(true);

        Debug.Log("See Less button clicked for panel: " + panelObject.name);
    }
}

[System.Serializable]
public class DictionaryResponse
{
    public DictionaryData data;
}

[System.Serializable]
public class DictionaryData
{
    public List<DictionaryEntry> list;
    public bool has_next;
}

[System.Serializable]
public class DictionaryEntry
{
    public GameData game;
}

[System.Serializable]
public class GameData
{
    public string word;
    public string meaning;
}