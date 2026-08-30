using System.Collections;
using TMPro;
using UnityEngine;

public static class TextTypingUtils
{
    public static IEnumerator PlayTypewriterEffect(TextMeshProUGUI textObj, string fullText, float charsPerSecond = 30f)
    {
        textObj.text = fullText;
        textObj.maxVisibleCharacters = 0;
        int totalChars = fullText.Length;
        float interval = 1f / charsPerSecond;

        for (int i = 0; i <= totalChars; i++)
        {
            textObj.maxVisibleCharacters = i;
            yield return new WaitForSeconds(interval);
        }
        textObj.maxVisibleCharacters = totalChars;
    }
}
