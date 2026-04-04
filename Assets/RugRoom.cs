//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEditor.ShaderGraph.Serialization;
//using UnityEngine;

//public class RugRoom : MonoBehaviour
//{
//    [SerializeField] TMP_Text _placeTextRug;
//    private int cubsRug = 0;
//    private int cubsRugMax;
//    public DoorsScript _doors;
//    private PlayerEvent _playerEvent;
//    // Start is called before the first frame update
//    void Start()
//    {

     
//        if (_placeTextRug == null)
//        {
//            _placeTextRug = GameObject.Find("RugTxt")?.GetComponent<TMP_Text>();
//        }
//        if (_doors == null)
//        {
//            _doors = GameObject.Find("Room_Modern_Door_Oppens")?.GetComponent<DoorsScript>();
//        }
//        if (_playerEvent == null)
//        {
//            _playerEvent = GameObject.Find("XR Rig")?.GetComponent<PlayerEvent>();
//        }
//        cubsRugMax = _playerEvent.cubes.Length;

//    }
//    private void OnTriggerEnter(Collider collision)
//    {
       
//        if (collision.CompareTag("Cube"))
//        {
//            CubsCount(+1);
           
//        }
//    }
//    public void CubsCount( int cub)
//    {
//        cubsRug += cub;
//        _placeTextRug.text = "Положено кубов:" + cubsRug;
//        if (cubsRug >= cubsRugMax)
//        {
//            _doors.Win();
//        }
//    }
//    private void OnTriggerExit(Collider collision)
//    {
//        if (collision.CompareTag("Cube"))
//        {
//            CubsCount(-1);
          
//        }
//    }

//}
