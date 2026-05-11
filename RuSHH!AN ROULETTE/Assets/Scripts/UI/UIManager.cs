using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject startUi;
    public GameObject startQuitUi;
    public GameObject mainUi;
    public GameObject mainQuitUi;
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

    }
    public void ChangeUi(GameObject to)
    {
        if (to != startQuitUi && to != mainQuitUi)
        {
            startUi.SetActive(false);
            mainUi.SetActive(false);
            to.SetActive(true);
        }
        else
        {
            mainQuitUi.SetActive(false);
            startQuitUi.SetActive(false);
            to.SetActive(true);
        }
    }
    private void DisableUi(bool disable)
    {
        if (disable)
        {
            startQuitUi.SetActive(false);
            mainQuitUi.SetActive(false);
            startUi.SetActive(false);
            mainUi.SetActive(false);
        }
    }
}
