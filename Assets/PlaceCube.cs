using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceCube : MonoBehaviour
{
    [SerializeField] GameObject _cubeText;
    [SerializeField] TMP_Text _placeTextBox;
    public DoorsScript _doors;
    string _placeTextOld;
    Color _placeColorOld;
    public bool isPressedPlatform;
    private void Start()
    {
        if (_cubeText == null)
        {
            Transform childTransform = transform.Find("txtScream"); 
            if (childTransform != null)
                _cubeText = childTransform.gameObject;
        }

        if (_placeTextBox == null)
        {
            _placeTextBox = GameObject.Find("TriggerTxt")?.GetComponent<TMP_Text>();
        }

       

        if (_doors == null)
        {
            _doors = GameObject.Find("Room_Modern_Door_Oppens")?.GetComponent<DoorsScript>();
        }

        if (_placeTextBox != null)
        {
            _placeTextOld = _placeTextBox.text;
            _placeColorOld = _placeTextBox.color;
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("TrigBox"))
        {
            isPressedPlatform=true;

            _placeTextBox.text = "AXAXAXA";
            _placeTextBox.color = Color.red;
            _cubeText.SetActive(true);
            _doors.OnEventDoor(isPressedPlatform);
        }
        
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("TrigBox"))
        {
            isPressedPlatform = false;
            _placeTextBox.text = _placeTextOld;
            _placeTextBox.color = _placeColorOld;
            _cubeText.SetActive(false);
            _doors.OnEventDoor(isPressedPlatform);
        }
        
    }
}
