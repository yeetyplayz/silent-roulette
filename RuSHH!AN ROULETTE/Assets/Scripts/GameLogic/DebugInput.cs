using UnityEngine;

public class DebugInput : MonoBehaviour
{
    public RoundManager roundManager;
    public RouletteManager rouletteManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) roundManager.HumanHit();
        if (Input.GetKeyDown(KeyCode.S)) roundManager.HumanStand();
        if (Input.GetKeyDown(KeyCode.F)) rouletteManager.HumanPullTrigger();
    }
}