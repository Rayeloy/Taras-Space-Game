using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerMovementCMF Player;

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
}
