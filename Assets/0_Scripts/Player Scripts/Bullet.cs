using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public Vector2 direction;
    public float damage;
    public float range;
    private float lifeTime = 0;

    private Vector3 originalPos;

    public Vector3 currentVel;

    float originalZRoot;

    public void KonoAwake(float _speed, float _damage, float _range, Vector2 dir)
    {
        originalPos = transform.position;
        lifeTime = 0;
        speed = _speed;
        damage = _damage;
        range = _range;
        direction = dir;
        currentVel = (direction.normalized * speed);
        originalZRoot = transform.localRotation.eulerAngles.z;
        RotateBullet();
    }

    private void Update()
    {
        if(lifeTime >= MasterManager.GameDataManager.bulletMaxLifeTime)
        {
            Destroy();
            return;
        }
        lifeTime += Time.deltaTime;

        float distanceTravelled = (transform.position - originalPos).magnitude;
        Debug.Log("distanceTravelled = " + distanceTravelled + "; range = " + range+ "; originalPos = "+ originalPos + "; transform.position = " + transform.position);

        if (distanceTravelled >= range)
        {
            Destroy();
        }

        MoveBullet();
    }

    void MoveBullet()
    {
        transform.position = transform.position + (currentVel * Time.deltaTime);
        RotateBullet();
    }

    void Destroy()
    {
        //Impact VFX

        //Impact sound

        //Explosion?

        Destroy(gameObject);
        Debug.Log("Destroy bullet!");
        return;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            //Damage enemy;
            Destroy();
            return;
        }
        else if(!other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Bullet collided with " + other.gameObject.name);
            Destroy();
            return;
        }
    }

    void RotateBullet()
    {
        //Rotate bullet
        float angle = Vector2.SignedAngle(Vector2.up, currentVel.normalized);
        transform.localRotation = Quaternion.Euler(0, 0, originalZRoot + angle);
    }
}
