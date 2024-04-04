using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

[System.Serializable]
public class WordData
{
    public string word;
    public string meaning;
}

public class WordSearch : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public Transform wordMeaningContainer;

    private List<WordData> allWords = new List<WordData>();
    private Transform[] wordBoxTransforms;

    private void Awake()
    {
        wordBoxTransforms = new Transform[wordMeaningContainer.childCount];
        for (int i = 0; i < wordMeaningContainer.childCount; i++)
        {
            wordBoxTransforms[i] = wordMeaningContainer.GetChild(i);
        }

        // Fetch words initially to populate word boxes
        FetchAllWords();
    }

    private void Start()
    {
    }


    private void FetchAllWords()
    {
        // Implement your logic to fetch words from the API and populate the allWords list
        for (char letter = 'a'; letter <= 'z'; letter++)
        {
            string apiUrl = "https://activity-api.aceplus.in/web/dictionary_words?page=1&per_page=30&q=&alphabet=" + letter;
            StartCoroutine(FetchWordsCoroutine(apiUrl));
        }


        // Once words are fetched, populate the word boxes initially
        PopulateWordBoxes();
    }

    private IEnumerator FetchWordsCoroutine(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the GET request
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Parse the JSON response and update word boxes
                string jsonResponse = webRequest.downloadHandler.text;
                ParseJSONResponse1(jsonResponse);
            }
        }
    }

    private void ParseJSONResponse1(string jsonResponse)
    {
        try
        {
            // Deserialize JSON
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);

            if (response != null && response.status && response.data != null && response.data.list != null)
            {
                foreach (var item in response.data.list)
                {
                    if (item.game != null)
                    {
                        // Add word and its meaning to the list
                        allWords.Add(new WordData { word = item.game.word, meaning = item.game.meaning });
                    }
                }
            }
            else
            {
                Debug.LogError("Invalid JSON response format or missing data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing JSON response: " + e.Message);
        }
    }



    private void PopulateWordBoxes()
    {
        int wordIndex = 0;
        foreach (Transform wordBox in wordBoxTransforms)
        {
            if (wordIndex >= allWords.Count)
            {
                break;
            }

            WordData wordData = allWords[wordIndex];
            TMP_Text wordText = wordBox.Find("heading").GetComponent<TMP_Text>();
            TMP_Text meaningText = wordBox.Find("meaning").GetComponent<TMP_Text>();
            wordText.text = wordData.word;
            meaningText.text = wordData.meaning;
            wordBox.gameObject.SetActive(true); // Activate the box

            wordIndex++;
        }
    }

    private void Search(string keyword)
    {
        // Don't hide boxes if the search keyword is empty
        if (string.IsNullOrEmpty(keyword))
        {
            return;
        }

        // Clear existing word boxes by setting them to inactive
        foreach (Transform child in wordMeaningContainer)
        {
            child.gameObject.SetActive(false);
        }

        // Filter search results
        List<WordData> searchResults = GetSearchResults(keyword);

        // Display search results
        int resultIndex = 0;
        foreach (WordData result in searchResults)
        {
            // Find the first inactive word meaning box
            if (resultIndex >= wordBoxTransforms.Length)
            {
                Debug.LogError("More search results than available word boxes!");
                break;
            }
            Transform wordBox = wordBoxTransforms[resultIndex];

            // Activate the box and update its text components
            wordBox.gameObject.SetActive(true);
            TMP_Text wordText = wordBox.Find("heading").GetComponent<TMP_Text>();
            TMP_Text meaningText = wordBox.Find("meaning").GetComponent<TMP_Text>();
            wordText.text = result.word;
            meaningText.text = result.meaning;

            resultIndex++;
        }
    }

    private List<WordData> GetSearchResults(string keyword)
    {
        List<WordData> results = new List<WordData>();

        foreach (WordData data in allWords)
        {
            if (data.word.ToLower().Contains(keyword.ToLower()) || data.meaning.ToLower().Contains(keyword.ToLower()))
            {
                results.Add(data);
            }
        }

        return results;
    }

    public void OnSearchButtonClicked()
    {
        string keyword = searchInputField.text.ToLower(); // Get the search keyword
        Search(keyword);
    }

    private void Update()
    {
        // Detect Enter key press in search input field
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string keyword = searchInputField.text.ToLower(); // Get the search keyword
            Search(keyword);
        }
    }
}