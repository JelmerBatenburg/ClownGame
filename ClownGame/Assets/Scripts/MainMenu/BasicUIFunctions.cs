using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicUIFunctions : MonoBehaviour
{
    [Header("FromToButton")]
    public GameObject[] from;
    public GameObject[] to;
    [Header("SizeChange")]
    public float sizeIncrease;
    private Vector3 basicSize;
    [Header("ChangeColor")]
    public Color highlightedColor;
    public Color nonHighlightedColor;
    [Header("SliderValueDisplay")]
    public Text displayText;

    public void FromToButton()
    {
        foreach (GameObject obj in from)
            obj.SetActive(false);
        foreach (GameObject obj in to)
            obj.SetActive(true);
    }

    public void SizeChangeEnter(GameObject obj)
    {
        basicSize = obj.transform.localScale;
        obj.transform.localScale = basicSize * sizeIncrease;
    }

    public void SizeChangeExit(GameObject obj)
    {
        obj.transform.localScale = basicSize;
    }

    public void HighlightColor(GameObject obj)
    {
        obj.GetComponent<Image>().color = highlightedColor;
    }

    public void DeHighlightColor(GameObject obj)
    {
        obj.GetComponent<Image>().color = nonHighlightedColor;
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }

    public void SliderChangeDisplay(float value)
    {
        displayText.text = value.ToString();
    }
}
