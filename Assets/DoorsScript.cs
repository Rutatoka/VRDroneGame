using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.Shapes;

public class DoorsScript : MonoBehaviour
{
    Animator anim;
   public GameObject winPanel;
    private void Start()
    {
        if (winPanel == null)
        {
            winPanel = GameObject.Find("WinPanel")?.GetComponent<GameObject>();
        }
        anim = GetComponent<Animator>();
    }
    public void OnEventDoor(bool bollen)
    {
        anim.SetBool("isOpen", bollen); 
    }
    public void Win()
    {
        winPanel.SetActive(true);
    }
    public void OkBtn()
    {
        SceneManager.LoadScene(0);
    }

}
