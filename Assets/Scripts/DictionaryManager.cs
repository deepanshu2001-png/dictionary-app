    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;

    public class DictionaryManager : MonoBehaviour
    {
        public string baseUrl = "https://activity-api.aceplus.in/web/dictionary_words";
        public GameObject wordPanelPrefab;
        public Transform contentPanel;
        public Button[] letterButtons; // Array of buttons for each letter from A to Z
        public TMP_InputField searchInputField;
        [SerializeField] private Button seeMoreButton;
        public GameObject container;
        public GridLayoutGroup gridLayoutGroup;
        public float newHeight = 130f; // Set your desired height here
        public Dictionary<char, int> currentPage = new Dictionary<char, int>();
        public List<DictionaryEntry> allWords = new List<DictionaryEntry>(); // Store all fetched word entries
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
            seeMoreButton.onClick.AddListener(() => ResizeChildren());

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

                        // Clear existing word panels on the first page for the clicked letter
                        if (currentPage[letter] == 1)
                        {
                            foreach (Transform child in contentPanel)
                            {
                                Destroy(child.gameObject);
                            }
                        }
                        allWords.AddRange(response.data.list);


                        // Instantiate word panels for each word and meaning for the clicked letter
                        foreach (DictionaryEntry entry in response.data.list)
                        {
                            GameObject panelObject = Instantiate(wordPanelPrefab, contentPanel);
                            SetWordAndMeaning(panelObject, entry.game.word, entry.game.meaning);
                        }

                        // Increment page number if pagination is supported for the clicked letter
                        if (response.data.has_next)
                        {
                            currentPage[letter]++;
                        }
                        else
                        {
                            // Set hasNext to false if there are no more pages for the clicked letter
                            hasNext = false;
                        }
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
        public void SearchWord(string searchTerm)
        {
            // Clear existing word panels
            foreach (Transform child in contentPanel)
            {
                Destroy(child.gameObject);
            }

            searchTerm = searchTerm.ToLower(); // Convert search term to lowercase for case-insensitive search

            // Check if words starting with the search term's letter have been fetched
            char searchLetter = searchTerm[0];
            if (!currentPage.ContainsKey(searchLetter))
            {
                // Fetch words for the search term's letter if they haven't been fetched yet
                OnLetterButtonClicked(searchLetter);
            }
            else
            {
                // Check if the word list contains the searched word directly
                bool wordFound = false;
                foreach (DictionaryEntry entry in allWords)
                {
                    if (entry.game.word.ToLower().Contains(searchTerm) || entry.game.meaning.ToLower().Contains(searchTerm))
                    {
                        GameObject panelObject = Instantiate(wordPanelPrefab, contentPanel);
                        SetWordAndMeaning(panelObject, entry.game.word, entry.game.meaning);
                        wordFound = true; // Set flag to true once a match is found
                        break; // Stop searching once a match is found
                    }
                }

                if (!wordFound)
                {
                    // Fetch words for the search term's letter if the searched word is not found directly
                    OnLetterButtonClicked(searchLetter);
                }
            }
        }

        // Method to set the word and meaning texts on the instantiated panel
        // Method to set the word and meaning texts on the instantiated panel
        public void SetWordAndMeaning(GameObject panelObject, string word, string meaning)
        {
            // Find the Text components directly in panelObject
            TMP_Text wordText = panelObject.transform.Find("WordText")?.GetComponent<TMP_Text>();
            TMP_Text meaningText = panelObject.transform.Find("MeaningText")?.GetComponent<TMP_Text>();
            

            // Ensure that both WordText and MeaningText are found
            if (wordText == null || meaningText == null)
            {
                Debug.LogError("WordText or MeaningText component not found in panelObject: " + panelObject.name);
                return;
            }

            // Set the text values
            wordText.text = word;
            string formattedMeaning = FormatMeaningText(meaning, out bool isTruncated);
            meaningText.text = formattedMeaning;
            

    }

        public string FormatMeaningText(string meaning, out bool isTruncated)
        {
            // Split the meaning text into words
            string[] words = meaning.Split(' ');

            // Initialize variables
            string formattedMeaning = "";
            int wordCount = 0;
            int lineCount = 0;
            isTruncated = false;

            // Iterate through each word
            foreach (string word in words)
            {
                // Add the word to the formatted meaning text
                formattedMeaning += word + " ";
                wordCount++;

                // Add a new line if maximum words per line (4) reached
                if (wordCount >= 4)
                {
                    formattedMeaning += "\n";
                    wordCount = 0;
                    lineCount++;

                    // Break loop if maximum lines (3) reached
                    if (lineCount >= 3)
                    {
                        // Append ellipsis (...) if there are more words
                        if (words.Length > wordCount)
                        {
                            formattedMeaning = formattedMeaning.TrimEnd() + " ...";
                            isTruncated = true;
                        }
                        break;
                    }
                }
            }

            return formattedMeaning;
        }

    // Method to expand the meaning panel to show full meaning text
    //private void ExpandMeaningPanel(GameObject panelObject, TMP_Text meaningText)
    //{
    // Get the RectTransform of the word meaning box
    // RectTransform panelRect = panelObject.GetComponent<RectTransform>();

    // Set the preferred height of the meaning text
    // float preferredHeight = meaningText.preferredHeight;

    // Expand the height of the word meaning box
    // panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, preferredHeight);

    // Toggle between truncated and full text
    // ToggleTruncatedText(meaningText);
    // }

    void ResizeChildren()
    {
        // Loop through each child of the grid layout group
        foreach (RectTransform child in gridLayoutGroup.transform)
        {
            // Adjust the height of the child
            Vector2 newSize = child.sizeDelta;
            newSize.y = newHeight;
            child.sizeDelta = newSize;
        }

        // Optional: You may need to force a layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayoutGroup.GetComponent<RectTransform>());
    }
    public void UpdateScrollRect()
        {
            // Get the size of the content panel
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            Vector2 contentSize = new Vector2(contentRect.rect.width, contentRect.rect.height);

            // Update the Scroll Rect component with the new content size
            ScrollRect scrollRect = contentPanel.parent.GetComponent<ScrollRect>();
            scrollRect.content.sizeDelta = contentSize;
        }

    // Call this method after dynamically adding content



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
