using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerMovementCMF myPlayerMovement;
    public float currentHealth;
    public float maxHealth;
    public float maxInvincibilityTime = 2;
    float currentInvincibilityTime = 0;
    public bool invincible = false;

    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovementCMF>();
        currentHealth = maxHealth;
        Debug.Log("Current health = " + currentHealth);
    }

    public void KonoUpdate()
    {
        ProcessDamaged();
        ProcessInvincibility();
        ProcessDeath();
    }

    public void ReceiveDamage(float damageAmount, Vector2 knockback)
    {
        if (invincible) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Player lost health: damage received = "+damageAmount+"; current health = " +currentHealth);
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
        //myPlayerMovement.noInput = true;
        StartInvincibility();
    }

    void ProcessDamaged()
    {
        //If damaged animation ended
        //myPlayerMovement.noInput = false;
    }

    public void StartPlayerDeath()
    {
        //Player death animation

       // myPlayerMovement.noInput = true;
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

    public void StartInvincibility()
    {
        if (!invincible)
        {
            invincible = true;
            currentInvincibilityTime = 0;
        }
    }

    void ProcessInvincibility()
    {
        if (invincible)
        {
            if(currentInvincibilityTime >= maxInvincibilityTime)
            {
                EndInvincibility();
            }
            currentInvincibilityTime += Time.deltaTime;
        }
    }

    void EndInvincibility()
    {
        if (invincible)
        {
            invincible = false;
        }
    }
}
