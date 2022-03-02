using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GraphicRaycasting : MonoBehaviour
{
    private EventSystem systemEvent;


    private void Awake()
    {
        systemEvent = GetComponent<EventSystem>();
    }

    private void Update()
    {
        GraphicDetection();
    }


    private void GraphicDetection()
    {

        
            PointerEventData myPointerEventData = new PointerEventData(systemEvent);

            myPointerEventData.position = Input.mousePosition;

            Debug.Log("entro aca");

            List<RaycastResult> results = new List<RaycastResult>();

            foreach (RaycastResult result in results)
            {
                Debug.Log(" RESULT " + result.gameObject.name);
            }
        
      
    }
}
