using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private void Awake()
    {
        GameController.instance.managerTime = this;
    }

    private void Start()
    {
        Time.timeScale = 1;

    }

    public void ChangeTimeScale(float currentTime)
    {
        Time.timeScale = currentTime;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1;
    }

    public void FreezeTimeScale()
    {
        Time.timeScale = Mathf.Epsilon;
    }
}
