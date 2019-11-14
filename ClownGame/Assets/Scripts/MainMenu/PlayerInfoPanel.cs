using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    [Header("Information")]
    public Text nameInput;
    public Image playerClownClass;
    public GameObject ready;

    public void DisplayInfo(bool isReady, string playerName, int clownClassIndex)
    {
        ready.SetActive(isReady ? true : false);
        nameInput.text = playerName;
    }
}
