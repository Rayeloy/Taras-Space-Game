using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderBridge : MonoBehaviour
{
    public List<string> enterTag = new List<string>();
    public List<string> stayTag = new List<string>();
    public List<string> exitTag = new List<string>();

    [HideInInspector] public Collider2D enterCollider;
    [HideInInspector] public Collider2D stayCollider;
    [HideInInspector] public Collider2D exitCollider;

    [HideInInspector] public Collision2D enterCol;
    [HideInInspector] public Collision2D stayCol;
    [HideInInspector] public Collision2D exitCol;
    public UnityEvent enterEvent;
    public UnityEvent stayEvent;
    public UnityEvent exitEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enterTag.Contains(other.tag))
        {
            string currentTag = other.tag;
            if (other.CompareTag(currentTag))
            {
                enterCollider = other;
                enterEvent.Invoke();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (stayTag.Contains(other.tag))
        {
            string currentTag = other.tag;

            if (other.CompareTag(currentTag))
            {
                stayCollider = other;
                stayEvent.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (exitTag.Contains(other.tag))
        {
            string currentTag = other.tag;

            if (other.CompareTag(currentTag))
            {
                exitCollider = other;
                exitEvent.Invoke();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enterTag.Contains(collision.gameObject.tag))
        {
            string currentTag = collision.gameObject.tag;

            if (collision.gameObject.CompareTag(currentTag))
            {
                enterCol = collision;
                enterEvent.Invoke();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (stayTag.Contains(collision.gameObject.tag))
        {
            string currentTag = collision.gameObject.tag;

            if (collision.gameObject.CompareTag(currentTag))
            {
                stayCol = collision;
                stayEvent.Invoke();
            }
        }
    }

    private void OnCollisionExit2d(Collision2D collision)
    {
        if (exitTag.Contains(collision.gameObject.tag))
        {
            string currentTag = collision.gameObject.tag;

            if (collision.gameObject.CompareTag(currentTag))
            {
                exitCol = collision;
                exitEvent.Invoke();
            }
        }
    }
}
