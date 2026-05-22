using UnityEngine;

public class DebugInput : MonoBehaviour
{
    public RoundManager roundManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) roundManager.HumanHit();
        if (Input.GetKeyDown(KeyCode.S)) roundManager.HumanStand();
    }
}