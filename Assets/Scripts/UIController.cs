using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    //[SerializeField] GameObject root;
    [SerializeField] GameObject Left;
    [SerializeField] GameObject Right;

    [SerializeField] GameObject StuckButton;

    public void TurnUI(bool canLeft, bool canRight)
    {
        //root.SetActive(true);
        // enable left/right icons here
        Left.SetActive(canLeft);
        Right.SetActive(canRight);
    }

    public void Hide()
    {
        //root.SetActive(false);
        Left.SetActive(false);
        Right.SetActive(false);
        StuckButton.SetActive(false);
    }

    public void StuckUI(bool state)
    {
        StuckButton.SetActive(state);
    }

}
