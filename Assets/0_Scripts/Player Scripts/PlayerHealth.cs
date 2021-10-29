using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerMovementCMF myPlayerMovement;
    public float currentHealth;
    public float maxHealth;

    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovementCMF>();
        currentHealth = maxHealth;
    }

    public void ReceiveDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0)
        {
            StartPlayerDeath();
        }
        else
        {
            StartDamaged();

        }
    }

    void StartDamaged()
    {
        //Player damaged animation
        myPlayerMovement.noInput = true;
    }

    void ProcessDamaged()
    {
        //If damaged animation ended
        myPlayerMovement.noInput = false;
    }

    public void StartPlayerDeath()
    {
        //Player death animation

        myPlayerMovement.noInput = true;
    }

    void ProcessDeath()
    {
        //if death animation ended
        //Death menu
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        myPlayerMovement.noInput = false;
    }
}
