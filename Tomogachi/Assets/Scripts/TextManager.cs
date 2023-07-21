using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextManager : MonoBehaviour
{
    [SerializeField] TMP_InputField InputText;
    public void RespondToText()
    {
        if (InputText.text != "")
            GPTManager.Instance.AskChatGPT(InputText.text);
        InputText.text = "";
    }

}
