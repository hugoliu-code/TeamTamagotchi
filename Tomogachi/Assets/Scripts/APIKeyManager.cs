using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class APIKeyManager : MonoBehaviour
{
  
    [SerializeField] TMP_InputField GPTKey;
    [SerializeField] TMP_InputField AWSKey;
    private string fileSave = "APIKeys.json";
    private string filePath;
    public static APIKeyManager Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

    }
    private void Start()
    {
        Debug.Log(Application.persistentDataPath);
        filePath = System.IO.Path.Combine(Application.persistentDataPath, fileSave);
        GPTManager.Instance.UpdateAPIKey();
    }

    public void UpdateAPIKey()
    {
        KeyData data = new KeyData();
        if (System.IO.File.Exists(filePath))
        {
            // Read the entire file and save its contents.
            string fileContents = System.IO.File.ReadAllText(filePath);

            // Deserialize the JSON data 
            //  into a pattern matching the GameData class.
            data = JsonUtility.FromJson<KeyData>(fileContents);
        }
        if (GPTKey.text != "")
            data.GPTKey = GPTKey.text;
        if (AWSKey.text != "")
            data.AWSKey = AWSKey.text;
        string keys = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(filePath, keys);
        GPTKey.text = "";
        AWSKey.text = "";
        GPTManager.Instance.UpdateAPIKey();
    }

    public string GetAPIKey(string type)
    {
        if (System.IO.File.Exists(filePath))
        {
            // Read the entire file and save its contents.
            string fileContents = System.IO.File.ReadAllText(filePath);

            // Deserialize the JSON data 
            //  into a pattern matching the GameData class.
            KeyData data = JsonUtility.FromJson<KeyData>(fileContents);
            if (type.Equals("GPT"))
            {
                return data.GPTKey;
            }
            else if (type.Equals("AWS"))
            {
                return data.AWSKey;
            }
            else
            {
                Debug.LogError("Invalid API Key Type, must be AWS/GPT");
            }
        }
        Debug.LogError(filePath);
        return "";
    }

    [System.Serializable]
    public class KeyData
    {
        // Public lives
        public string GPTKey;

        // Public highScore
        public string AWSKey;
    }
}
