using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    PlayerMovementCMF myPlayerMovement;
    public float currentHealth;
    public float maxHealth;
    public float maxInvincibilityTime = 2;
    float currentInvincibilityTime = 0;
    public bool invincible = false;
    [SerializeField] private Image[] heartImage;
    private bool _isDeadBool;
    public bool isDeadBool
    {
        get { return _isDeadBool; }
    }

    private int _indexCurrentHeart;
    private Dictionary<Image, float> _damageImageUmbralDictionary = new Dictionary<Image, float>();
   
    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovementCMF>();
        currentHealth = maxHealth;
    }

    public void KonoStart()
    {
        _indexCurrentHeart = 0;

        for(int i = 0; i < heartImage.Length; i++)
        {
            float max = (maxHealth - ((maxHealth / heartImage.Length) * i));
            _damageImageUmbralDictionary.Add(heartImage[i], max);
        }
    }

    public void KonoUpdate()
    {
        ProcessDamaged();
        ProcessInvincibility();
        ProcessDeath();
    }

    public void ReceiveDamage(float damageAmount, Vector2 knockback)
    {
        if (invincible || _isDeadBool) return;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        ChangeHeartState();

        //  Debug.Log("Player lost health: damage received = "+damageAmount+"; current health = " +currentHealth);
        if (currentHealth <= 0)
        {
            StartPlayerDeath();
        }
        else
        {
            StartDamaged();
        }
    }

   
    /// <summary>
    /// Change the current heart state in the HUD
    /// </summary>
    void ChangeHeartState()
    {
       if((_damageImageUmbralDictionary[heartImage[_indexCurrentHeart]] - (maxHealth / heartImage.Length)) > ((currentHealth / maxHealth) * 100) )
        {
            heartImage[_indexCurrentHeart].fillAmount = 0;
            _indexCurrentHeart += 1;
            currentHealth = _damageImageUmbralDictionary[heartImage[_indexCurrentHeart]];

        }
        else if(_damageImageUmbralDictionary[heartImage[_indexCurrentHeart]] < ((currentHealth / maxHealth) * 100))
        {
            heartImage[_indexCurrentHeart].fillAmount = 1;

            _indexCurrentHeart -= 1;

            currentHealth = _damageImageUmbralDictionary[heartImage[_indexCurrentHeart]];

        }

        _indexCurrentHeart = Mathf.Clamp(_indexCurrentHeart, 0, heartImage.Length - 1);

        float currentMaxHealth = (maxHealth);
       float calculus = (currentMaxHealth - currentHealth) / maxHealth;
        calculus *= maxHealth;

        float div = calculus / (maxHealth / (heartImage.Length));
        if (div < 0)
            div *= -1;
       
        div -= _indexCurrentHeart;
        heartImage[_indexCurrentHeart].fillAmount = 1 - div;

    }

    [ContextMenu("test damage")]
    public void Test_Damage()
    {
        ReceiveDamage(10, Vector2.zero);
    }

    [ContextMenu("ADD HEALTH")]
    public void Add_Health()
    {
        ReceiveDamage(-10, Vector2.zero);

    }
    void StartDamaged()
    {
        //Player damaged animation
        //myPlayerMovement.noInput = true;
        //StartInvincibility();
    }

    void ProcessDamaged()
    {
        //If damaged animation ended
        //myPlayerMovement.noInput = false;
    }

    public void StartPlayerDeath()
    {
        //Player death animation
        _isDeadBool = true;
        myPlayerMovement.myPlayerAnimations.SetAnimationState(PlayerAnimationState.Dying);
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
