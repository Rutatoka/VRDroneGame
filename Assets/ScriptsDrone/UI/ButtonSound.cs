using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour
{
    public void OnPointerClick(PointerEventData eventData)
    {
             AudioManager.Instance?.PlayButtonClick();
    }
}
