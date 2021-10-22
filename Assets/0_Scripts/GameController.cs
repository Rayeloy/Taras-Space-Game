using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerMovementCMF Player;
    public float slowmo = 1;

    private void Awake()
    {
        Player.KonoAwake();
    }

    // Start is called before the first frame update
    void Start()
    {
        Player.KonoStart();
    }

    // Update is called once per frame
    void Update()
    {
        Player.KonoUpdate();
    }

    private void FixedUpdate()
    {
        Player.KonoFixedUpdate();
    }

    private void LateUpdate()
    {
        Player.KonoLateUpdate();
    }

    public void SlowmoCheck()
    {
            switch (slowmo)
            {
                case 1:
                    slowmo = 0.5f;
                    break;
                case 0.5f:
                    slowmo = 0.25f;
                    break;
                case 0.25f:
                    slowmo = 0.125f;
                    break;
                case 0.125f:
                    slowmo = 1;
                    break;
            }
            Time.timeScale = slowmo;
        Debug.Log("SLOWMO CHANGE : new slowmo = " + slowmo);
    }
}
