using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParallaxLayerMode
{
    None,
    Endless,
    Random
}

[System.Serializable]
public class ParallaxLayer
{
    public bool debug = false;
    public Transform parent;
    public float depth;
    public GameObject[] prefabsToSpawn;
    public bool visible;
    public ParallaxLayerMode mode;
    public string sortingLayer;
    public bool followPlayerY;
    [Header("--- Random Mode ---")]
    public bool spawnOnFloor = false;
    public Vector2 onFloorLayerOffset;
    public float minDistRandomSpawn = 10, maxDistRandomSpawn = 20;
    public float minYRandomSpawn = 20, maxYRandomSpawn = 50;
    public bool avoidSpawnOnChasm = false;
    public bool avoidSpawnOnChunkBorders = false;
    public bool avoidSpawnOnFirstChunk = false;



    float lastSpawnPos;
    Transform player;
    float nextSpawnPos;
    int lastOrder = 0;
    int lastPrefabIndex = -1;

    Vector3 cameraRightLimit
    {
        get
        {
            float distanceToCamera = depth - Camera.main.transform.position.z;
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 0, distanceToCamera));
        }
    }


    List<SpawnQueueElement> spawnQueue;

    float cameraYDiff;

    public ParallaxLayer()
    {
        depth = 0;
        visible = true;
        mode = ParallaxLayerMode.None;
    }

    public ParallaxLayer(Transform _parent, float _depth, bool _visible)
    {
        parent = _parent;
        depth = _depth;
        visible = _visible;
        mode = ParallaxLayerMode.None;
    }

    public void SetVisible(bool _visible)
    {
        if (parent.gameObject.activeInHierarchy == _visible) return;

        visible = _visible;
        parent.gameObject.SetActive(visible);
    }

    public void ProcessLayer()
    {
        SetVisible(visible);
        if (!visible)
        {
            return;
        }
        if (mode != ParallaxLayerMode.None)
        {
            if (Application.isPlaying && prefabsToSpawn.Length > 0)
            {
                //TrySpawnQueuedElements();
                CheckIfNeedSpawn();
            }

            if (parent.localPosition.z != depth) parent.localPosition = new Vector3(0, 0, depth);
            //Debug.Log("Setting the sorting layer of layer " + this+ " to " + sortingLayer);
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {

                    SpriteRenderer sRend = child.GetComponentInChildren<SpriteRenderer>();
                    //Debug.Log("Setting the sorting layer of "+child.name+" from " + sRend.sortingLayerID + " to " + sortingLayer);
                    EloyExtensions.UnityExtensions.SetSortingLayer_r(child, sortingLayer);
                }
            }
            if (followPlayerY && player != null)
            {
                parent.position = new Vector3(parent.position.x, Camera.main.transform.position.y + cameraYDiff, parent.position.z);
            }
        }

        CheckIfElementsTooFarAway();
    }

    public void Setup(Transform _player)
    {
        spawnQueue = new List<SpawnQueueElement>();
        player = _player;
        switch (mode)
        {
            case ParallaxLayerMode.Random:
                nextSpawnPos = cameraRightLimit.x + Random.Range(minDistRandomSpawn, maxDistRandomSpawn) /*ParallaxScript.instance.spawnInitialtX*/;
                nextSpawnPos = Mathf.Clamp(nextSpawnPos, ParallaxScript.instance.spawnInitialtX, float.MaxValue);//This is to avoid the middleground layers from spawning too early
                if (sortingLayer == "Background+2") Debug.Log("SETUP-> NextSpawnPos = " + nextSpawnPos + " cameraRightLimit = " + cameraRightLimit);
                break;
            case ParallaxLayerMode.Endless:
                cameraYDiff = Camera.main.transform.position.y - parent.position.y;
                break;
        }
    }

    void CheckIfNeedSpawn()
    {
        switch (mode)
        {
            case ParallaxLayerMode.Random:
                float currentDist = (cameraRightLimit.x + ParallaxScript.instance.spawnOffsetX + (depth / 8));
                //if(sortingLayer == "Background+2")Debug.Log("currentDist = " + currentDist + "; nextSpawnPos = " + nextSpawnPos + "; cameraRightLimit = " + cameraRightLimit);
                if (currentDist >= nextSpawnPos)
                {
                    TrySpawnRandomElement();
                }
                break;
            case ParallaxLayerMode.Endless:
                Transform lastElementSpawned = parent.GetChild(parent.childCount - 1);
                EloyExtensions.UnityExtensions.ParentRendBounds parentRendBounds = new EloyExtensions.UnityExtensions.ParentRendBounds(lastElementSpawned);

                Vector3 lastElementRightLimit = new Vector3(parentRendBounds.Center.x + parentRendBounds.Extents.x, parentRendBounds.Center.y, lastElementSpawned.position.z);
                //Debug.LogWarning("lastElementSpawned = "+ lastElementSpawned .name+ "; parentSRendBounds.Center = " + parentSRendBounds.Center + "; parentSRendBounds.Extents = " + parentSRendBounds.Extents);

                if (sortingLayer == "Background+2") Debug.LogWarning("lastElementRightLimit = " + lastElementRightLimit + "; Camera.main.WorldToViewportPoint(lastElementRightLimit).x = " + Camera.main.WorldToViewportPoint(lastElementRightLimit).x);
                if (Camera.main.WorldToViewportPoint(lastElementRightLimit).x < 1.2f)
                {
                    SpawnEndlessElement();
                }
                break;
        }

    }

    GameObject SelectNextPrefabToSpawn(float spawnX)
    {
        if (prefabsToSpawn.Length >= 2)
        {
            //Copy array except last prefab spawned
            List<GameObject> prefabsList = new List<GameObject>();
            for (int i = 0; i < prefabsToSpawn.Length; i++)
            {
                if (i != lastPrefabIndex)
                {
                    ParallaxElement pElement = prefabsToSpawn[i].GetComponent<ParallaxElement>();
                    if (spawnOnFloor)
                    {
                        //Slope
                        float slopeAngle = 0;
                        //if (sortingLayer == "Middleground+1") Debug.LogError("Getting the slopeAngle at " + spawnX+" = "+slopeAngle);
                        if (pElement.CanSpawnAtAngle(slopeAngle))
                        {
                            //if (sortingLayer == "Middleground+1") Debug.Log("The current slope Angle is " + slopeAngle + " which is inside the parameters of the parallax element " + pElement.transform.name +
                            //      " (" + pElement.minSlopeAngle + "," + pElement.maxSlopeAngle + ")");
                            prefabsList.Add(prefabsToSpawn[i]);
                        }
                        //                    else if (sortingLayer == "Middleground+1") Debug.Log("The current slope Angle is " + slopeAngle + " which is NOT inside the parameters of the parallax element " + pElement.transform.name +
                        //" (" + pElement.minSlopeAngle + "," + pElement.maxSlopeAngle + ")");
                    }
                    else
                    {
                        prefabsList.Add(prefabsToSpawn[i]);
                    }
                }
            }

            int random = Random.Range(0, prefabsList.Count);
            bool found = false;
            for (int i = 0; i < prefabsToSpawn.Length && !found; i++)
            {
                if (prefabsToSpawn[i] == prefabsList[random])
                {
                    lastPrefabIndex = i;
                    found = true;
                }
            }
            return prefabsList[random];
        }
        else return prefabsToSpawn[0];
    }

    GameObject SelectNextEndlessPrefabToSpawn()
    {
        if (prefabsToSpawn.Length >= 2)
        {
            //Copy array except last prefab spawned
            List<GameObject> prefabsList = new List<GameObject>();
            for (int i = 0; i < prefabsToSpawn.Length; i++)
            {
                if (i != lastPrefabIndex)
                {
                    prefabsList.Add(prefabsToSpawn[i]);
                }
            }

            int random = Random.Range(0, prefabsList.Count);
            bool found = false;
            for (int i = 0; i < prefabsToSpawn.Length && !found; i++)
            {
                if (prefabsToSpawn[i] == prefabsList[random])
                {
                    lastPrefabIndex = i;
                    found = true;
                }
            }
            return prefabsList[random];
        }
        else return prefabsToSpawn[0];
    }

    void TrySpawnRandomElement()
    {
        //Debug.Log("Try spawn Element " + random);


        //Check if spawn is possible inside last chunk;

//        if (spawnOnFloor)
//        {
//            if (nextSpawnPos < ParallaxScript.instance.terrainController.GetLastChunk().MyStartPosition.x)
//            {
//                Debug.LogError("For some reason we tried to spawn the object before the chunk! (Too far left); spawnX = " + nextSpawnPos + "; Chunk start X = " + ParallaxScript.instance.terrainController.GetLastChunk().MyStartPosition.x);
//                return;
//            }
//            else if (nextSpawnPos > ParallaxScript.instance.terrainController.GetLastChunk().MyEndPosition.x)
//            {
//                //Debug.Log("Queue Element, chunk not yet created");
//                spawnQueue.Add(new SpawnQueueElement(nextSpawnPos));
//                OnSpawnDecided();
//                return;
//            }
//            nextSpawnPos = Mathf.Clamp(nextSpawnPos, ParallaxScript.instance.terrainController.GetLastChunk().MyStartPosition.x,
//ParallaxScript.instance.terrainController.GetLastChunk().MyEndPosition.x);
//        }
        SpawnObject(nextSpawnPos);
        OnSpawnDecided();
    }

    void OnSpawnDecided()
    {
        switch (mode)
        {
            case ParallaxLayerMode.Random:
                lastSpawnPos = nextSpawnPos;
                nextSpawnPos = Random.Range(minDistRandomSpawn, maxDistRandomSpawn) + lastSpawnPos;
                //if (sortingLayer == "Background+2") Debug.Log("OnSpawnDecided-> NextSpawnPos = " + nextSpawnPos);
                break;
        }
    }

    void SpawnObject(float spawnX)
    {
        //ProceduralTerrainChunk chunk = ParallaxScript.instance.terrainController.GetLastChunk() as ProceduralTerrainChunk;
        //if (!chunk) return;
        GameObject prefab = SelectNextPrefabToSpawn(spawnX);
        ParallaxElement pElement = prefab.GetComponentInChildren<ParallaxElement>();
        Vector3 spawnPosition = new Vector3(spawnX, 0, 0);

        if (pElement != null)
            spawnPosition.z += pElement.order * 1;

        if (spawnOnFloor)
        {
            ////if (sortingLayer == "Foreground") Debug.Log("chunk = " + chunk.gameObject.name + "; chunk.isAroundChasm = " + chunk.isAroundChasm);
            //if (avoidSpawnOnChasm && chunk.isAroundChasm) return;
            //else if (avoidSpawnOnFirstChunk && ParallaxScript.instance.terrainController.IsFirstChunk(chunk)) return;


            //if (avoidSpawnOnChunkBorders)
            //{
            //    float chunkWidth = chunk.MyStartPosition.x - chunk.MyEndPosition.x;
            //    float minx = chunk.MyStartPosition.x + chunkWidth * 0.4f;
            //    float maxX = chunk.MyEndPosition.x - chunkWidth * 0.4f;
            //    spawnX = Mathf.Clamp(spawnX, minx, maxX);
            //}
            //else
            //{
            //    spawnX = Mathf.Clamp(spawnX, chunk.MyStartPosition.x, chunk.MyEndPosition.x);
            //}
            //float finalHeight = chunk.GetWorldFloorHeightAt(spawnX);
            //spawnPosition.y = finalHeight;
            //spawnPosition += (Vector3)onFloorLayerOffset;
        }
        else
        {
            spawnPosition.y = Random.Range(minYRandomSpawn, maxYRandomSpawn);
        }

        if (pElement != null)
            spawnPosition += (Vector3)pElement.spawnOffset;

        //Order to avoid elements to overlap each other (visual bug)
        switch (lastOrder)
        {
            case 0:
                lastOrder = 1;
                break;
            case 1:
                lastOrder = -1;
                break;
            case -1:
                lastOrder = 0;
                break;
        }

        //SPAWN OBJECT
        Vector2 scale = prefab.transform.localScale;
        Sprite sprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;

        SpriteRenderer sRend = ParallaxScript.instance.spriteRendererPool.GetElement();
        sRend.transform.localScale = scale;
        if (pElement != null && pElement.rotateWithFloor)
        {
            //float rot = chunk.GetSlopeAngle(spawnPosition.x);
            ////Debug.Log("prefab = " + prefab.name + "; Rotate with floor = " + pElement.rotateWithFloor + "; rot = " + rot);
            //sRend.transform.localRotation = Quaternion.Euler(0, 0, -rot);
        }
        else sRend.transform.localRotation = Quaternion.Euler(0, 0, 0);
        sRend.sprite = sprite;
        sRend.sortingOrder = lastOrder;
        sRend.transform.SetParent(parent);
        sRend.transform.SetAsLastSibling();
        sRend.transform.position = spawnPosition;
        //if (sortingLayer == "Background+2") Debug.LogError("SpawnPosition = " + spawnPosition + "; spawnX = " + spawnX);
        sRend.transform.localPosition = new Vector3(sRend.transform.localPosition.x, sRend.transform.localPosition.y, 0);
        sRend.name = prefab.name;
        //if(sortingLayer == "Background")Debug.Log("Prefab "+prefab.name+" Spawn Position = " + spawnPosition);
    }

    //void TrySpawnQueuedElements()
    //{
    //    List<SpawnQueueElement> spawnQueueCopy = new List<SpawnQueueElement>();
    //    spawnQueueCopy = spawnQueue;
    //    for (int i = 0; i < spawnQueue.Count; i++)
    //    {
    //        if (spawnQueue[i].spawnX < ParallaxScript.instance.terrainController.GetLastChunk().MyStartPosition.x)
    //        {
    //            Debug.LogError("For some reason we tried to spawn the object before the chunk! (Too far left)");
    //            return;
    //        }
    //        else if (spawnQueue[i].spawnX > ParallaxScript.instance.terrainController.GetLastChunk().MyEndPosition.x)
    //        {
    //            return;
    //        }
    //        else
    //        {
    //            SpawnObject(spawnQueue[i].spawnX);
    //            spawnQueueCopy.RemoveAt(i);
    //        }
    //    }
    //    spawnQueue = spawnQueueCopy;
    //}

    void SpawnEndlessElement()
    {
        if (mode == ParallaxLayerMode.Endless)
        {
            if (parent.childCount > 1)
            {
                //Debug.LogError("Spawning Endless Element");
                int randomIndex = Random.Range(0, prefabsToSpawn.Length);
                Transform newElement = GameObject.Instantiate(SelectNextEndlessPrefabToSpawn(), parent).transform;
                newElement.transform.SetAsLastSibling();

                Transform lastElementSpawned = parent.GetChild(parent.childCount - 2);
                EloyExtensions.UnityExtensions.ParentRendBounds parentRendBounds = new EloyExtensions.UnityExtensions.ParentRendBounds(lastElementSpawned);
                EloyExtensions.UnityExtensions.ParentRendBounds newParentRendBounds = new EloyExtensions.UnityExtensions.ParentRendBounds(newElement);


                Vector3 lastElementRightLimit = new Vector3(parentRendBounds.Max.x, parentRendBounds.Center.y, lastElementSpawned.position.z);
                Vector3 spriteOffset = newElement.position - newParentRendBounds.Center;

                Vector3 newPos = lastElementRightLimit + (Vector3.right * newParentRendBounds.Extents.x) + spriteOffset;
                newElement.transform.position = new Vector3(newPos.x, newPos.y, depth);
                //if (sortingLayer == "Background") Debug.Log("Prefab " + newElement.name);
                //Debug.Log("New Endless Element spawned; newPos = "+ newPos + "; lastElementRightLimit = "+ lastElementRightLimit +
                //    "; newParentSRendBounds.extents.x = "+ newParentSRendBounds.Extents.x + "; parentSRendBounds.center.x = " + parentSRendBounds.Center.x + "; spriteOffsett = " + spriteOffset);
            }
            else
            {
                Debug.LogError("Too few child elements in the layer " + parent.name);
            }
        }
    }

    void CheckIfElementsTooFarAway()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            EloyExtensions.UnityExtensions.ParentRendBounds parentRendBounds = new EloyExtensions.UnityExtensions.ParentRendBounds(parent.GetChild(i));

            Vector3 lastElementRightLimit = new Vector3(parentRendBounds.Center.x + parentRendBounds.Extents.x, parentRendBounds.Center.y, parent.GetChild(i).position.z);
            //Debug.LogWarning("gameobject = "+ parent.GetChild(i)+"; parentSRendBounds.Center = " + parentRendBounds.Center + "; parentSRendBounds.Extents = " + parentRendBounds.Extents);

            //Debug.LogWarning("lastElementRightLimit = "+ lastElementRightLimit + "; Camera.main.WorldToViewportPoint(lastElementRightLimit).x = " + Camera.main.WorldToViewportPoint(lastElementRightLimit).x);
            if (debug) Debug.Log("lastElementRightLimit = "+lastElementRightLimit.ToString("F4"));
            if (Camera.main.WorldToViewportPoint(lastElementRightLimit).x < -0.2f)
            {
               
               // parent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class SpawnQueueElement
    {
        public float spawnX;

        public SpawnQueueElement(float _spawnX)
        {
            spawnX = _spawnX;
        }
    }
}
