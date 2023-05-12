using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeImageOutliner : MonoBehaviour
{
    [SerializeField] private Image outerImage;
    [SerializeField] private Image innerImage;

    // Update is called once per frame
    void Update()
    {
        outerImage.sprite = innerImage.sprite;
    }
}
