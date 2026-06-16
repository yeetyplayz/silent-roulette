using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private int speed = 100;
    public GameObject startUi;
    public GameObject startQuitUi;
    public GameObject mainUi;
    public GameObject mainQuitUi;
    //public GameObject mainSettingsUi;
    //public GameObject ingameUi;
    //public GameObject ingameQuitUi;
    private void Start()
    {
        DisableUi();
        ChangeUi(startUi);
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.C)) { while (speed > 0) { speed--; Spin(); speed = 100; } }
        if (startQuitUi.activeSelf && startUi.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Y)) { Application.Quit(); Debug.Log("U have Quit the game"); }
            if (Input.GetKeyDown(KeyCode.N)) { startQuitUi.SetActive(false); }
        }
        if (mainQuitUi.activeSelf && mainUi.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Y)) { Application.Quit(); Debug.Log("U have Quit the game"); }
            if (Input.GetKeyDown(KeyCode.N)) { mainQuitUi.SetActive(false); }
        }
        //if (ingameQuitUi.activeSelf && ingameUi.activeSelf) 
        //{
        //    if (Input.GetKeyDown(KeyCode.Y)) { ChangeUi(mainUi); }
        //    if (Input.GetKeyDown(KeyCode.N)) { ChangeUi(ingameUi); }
        //}
    }
    public void ChangeUi(GameObject to)
    {
        if (to != startUi && to != mainUi)// && to != ingameUi)
        {
            //mainSettingsUi.SetActive(false);
            mainQuitUi.SetActive(false);
            startQuitUi.SetActive(false);
            //ingameQuitUi.SetActive(false);
            to.SetActive(true);
        }
        else
        {
            startUi.SetActive(false);
            mainUi.SetActive(false);
            //ingameUi.SetActive(false);
            to.SetActive(true);
        }
    }
    private void DisableUi()
    {
            startQuitUi.SetActive(false);
            mainQuitUi.SetActive(false);
            //ingameQuitUi.SetActive(false);
            startUi.SetActive(false);
            mainUi.SetActive(false);
            //ingameUi.SetActive(false);
            //mainSettingsUi.SetActive(false);
    }
    private void Spin()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
