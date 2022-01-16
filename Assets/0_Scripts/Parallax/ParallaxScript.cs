
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ParallaxScript : MonoBehaviour
{
    public static ParallaxScript instance;
    public float spawnOffsetX = 20;
    public float spawnInitialtX = 20;
    public ParallaxLayer[] parallaxLayers;
    public ParallaxLayer defaultLayer;
    public bool update;
    public SpriteRendererPool spriteRendererPool;

    [Header("Jugador")]
    public GameObject jugador;

    public float distJugador;
    private float distAntJugador;
    private float incrementoDist;

    public int GetLayerIndex(ParallaxLayer parallaxLayer)
    {
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            if (parallaxLayer.parent == parallaxLayers[i].parent)
            {
                return i;
            }
        }
        return -1;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            instance = this;
        }

        //jugador = GameObject.FindGameObjectWithTag("Player");
        //camara = GameObject.FindGameObjectWithTag("MainCamera");

        if (!jugador) Destroy(this.gameObject);
    }

    public void Start()
    {
        SetupLayers();
        distAntJugador = distJugador = jugador.transform.position.x;

        //distCapaMas4 = 500;
        //distCapaMas5 = 500;

        //prof5 = profundidadCapa5;
        //prof4 = profundidadCapa5;
        //prof3 = profundidadCapa4;
        //prof1 = profundidadCapa1;
    }

    public void Update()
    {
            ProcessLayers();
        if (!Application.isPlaying)
        {
            if (update)
            {
                update = false;
                ProcessLayers();
            }
        }
    }

    public void SetupLayers()
    {
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            parallaxLayers[i].Setup(jugador.transform);
        }
        defaultLayer.Setup(jugador.transform);
    }

    void ProcessLayers()
    {
        //Debug.Log("Process layers");
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            //if(i==3 || i == 2)Debug.Log("Process Layer " + i);
            parallaxLayers[i].ProcessLayer();
        }
        defaultLayer.ProcessLayer();
    }

    //public void BuscarPersonaje()
    //{
    //    jugador = GameObject.FindGameObjectWithTag("Player");
    //}

    public void SetTransformInLayer(Transform tr, int layerIndex, float depthOffset=0)
    {
        tr.SetParent(parallaxLayers[layerIndex].parent);
        tr.SetAsLastSibling();
        tr.localPosition = new Vector3(tr.localPosition.x, tr.localPosition.y, 0+ depthOffset);
        tr.gameObject.SetActive(true);
    }

    public void SetTransformInDefaultLayer(Transform tr)
    {
        tr.SetParent(defaultLayer.parent);
        tr.SetAsLastSibling();
        tr.gameObject.SetActive(true);
    }
}
