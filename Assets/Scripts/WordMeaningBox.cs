using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using TMPro;
using System.ComponentModel;



[System.Serializable]
public class Game
{
    public int id;
    public string word;
    public string meaning;
    public int length;
}

[System.Serializable]
public class Item
{
    public Game game;
}

[System.Serializable]
public class Data
{
    public List<Item> list;
}

[System.Serializable]
public class ApiResponse
{
    public bool status;
    public Data data;
}

public class WordMeaningBox : MonoBehaviour
{
    public TMP_Text wordHeadingText; // Reference to the Text component for the word heading
    public TMP_Text wordMeaningText; // Reference to the Text component for the word meaning
    public Transform container;
    public Button[] alphabetButtons;

    private string apiBaseUrl = "https://activity-api.aceplus.in/web/dictionary_words?page=1&per_page=30&q=&alphabet=";

    private void Awake()
    {
        // Find and reference the Text components dynamically
        //wordHeadingText = transform.Find("heading").GetComponent<TMP_Text>();
        // wordMeaningText = transform.Find("meaning").GetComponent<TMP_Text>();
         // FetchWordMeanings('a');

    }

    private void Start()
    {
        // Add click listeners to alphabet buttons
        foreach (Button button in alphabetButtons)
        {
            char letter = button.GetComponentInChildren<TMP_Text>().text[0];
            button.onClick.AddListener(() => OnButtonClick(letter));
        }
    }
    // Method to fetch word meanings from the API
    public void FetchWordMeanings(char alphabet)
    {
        // Construct the API URL based on the selected alphabet
        string apiUrl = apiBaseUrl + alphabet;

        StartCoroutine(FetchWordMeaningsCoroutine(apiUrl));

    }

    private IEnumerator FetchWordMeaningsCoroutine(string url)
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
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                ParseJSONResponse(jsonResponse);
            }
        }
    }

    private void ParseJSONResponse(string jsonResponse)
    {
        try
        {
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);

            if (response != null && response.status && response.data != null && response.data.list != null)
            {
                for (int i = 0; i < response.data.list.Count && i < container.childCount; i++)
                {
                    var item = response.data.list[i];
                    if (item.game != null)
                    {
                        // Get references to the Text components in the word box
                        Transform wordBox = container.GetChild(i);
                        TMP_Text wordText = wordBox.Find("heading").GetComponent<TMP_Text>();
                        TMP_Text meaningText = wordBox.Find("meaning").GetComponent<TMP_Text>();

                        // Set word and meaning
                        wordText.text = item.game.word;
                        meaningText.text = item.game.meaning;
                        
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
    public void OnButtonClick(char alphabet)
    {
        FetchWordMeanings(alphabet);
    }

    

}
