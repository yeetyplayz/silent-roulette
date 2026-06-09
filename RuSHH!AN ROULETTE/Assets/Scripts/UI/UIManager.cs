using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject startUi;
    public GameObject startQuitUi;
    public GameObject mainUi;
    public GameObject mainQuitUi;
    public GameObject mainSettingsUi;
    public GameObject ingameUi;
    public GameObject ingameQuitUi;
    private void Start()
    {
        DisableUi(true);
        ChangeUi(startUi);
    }
    private void Update()
    {
        if (startQuitUi.activeSelf && startUi.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Y)) { Application.Quit(); Debug.Log("U have Quit the game"); }
            if (Input.GetKeyDown(KeyCode.N)) { ChangeUi(startUi); }
        }
        if (mainQuitUi.activeSelf && mainUi.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Y)) { Application.Quit(); Debug.Log("U have Quit the game"); }
            if (Input.GetKeyDown(KeyCode.N)) { ChangeUi(mainUi); }
        }
        if (ingameQuitUi.activeSelf && ingameUi.activeSelf) 
        {
            if (Input.GetKeyDown(KeyCode.Y)) { ChangeUi(mainUi); }
            if (Input.GetKeyDown(KeyCode.N)) { ChangeUi(ingameUi); }
        }
    }
    public void ChangeUi(GameObject to)
    {
        if (to != mainSettingsUi && to != mainQuitUi && to != ingameQuitUi)
        {
            mainSettingsUi.SetActive(false);
            mainQuitUi.SetActive(false);
            startQuitUi.SetActive(false);
            ingameQuitUi.SetActive(false);
            to.SetActive(true);
        }
        else 
        {
            startUi.SetActive(false);
            mainUi.SetActive(false);
            ingameUi.SetActive(false);
        }
    }
    private void DisableUi(bool disable)
    {
        if (disable)
        {
            startQuitUi.SetActive(false);
            mainQuitUi.SetActive(false);
            ingameQuitUi.SetActive(false);
            startUi.SetActive(false);
            mainUi.SetActive(false);
            ingameUi.SetActive(false);
        }
    }
}
