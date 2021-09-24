using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificacionesScript : MonoBehaviour
{
    [Header("NuevaInterfaz")]
    public GameObject menuNotificaciones;
    public Image cajaTextoNueva;
    public TMPro.TextMeshProUGUI nuevoTextoRetos;
    public Text contadorDeRetos;
    public Ease gaugeEasingFunction;
    public Ease gaugeEasingFunction2;
    public float velocidadAnimacion;
    public float velocidadAnimacion2;
    private Vector3 posInicialCaja;
    private bool calcularRetos;
    private bool animandoLaCaja;
    private bool finAnimacionCaja;

    [Header("Acciones")]
    public bool notificarReto;
    public bool notificando;
    private bool calcularLongitudLineaRoja = true;

    [Header("UI")]
    public Image cajaRetos;
    public Text textoRetos;
    public Image lineaRoja;
   

    private Color colorTexto;
    public float deltaTime2;
    private float lastInterval;
    public List<string> notificaciones = new List<string>();

    void Start()
    {
        textoRetos.text = "Recorre 500 metros";
        colorTexto = new Color(1, 1, 1, 0);
        lineaRoja.transform.localScale = new Vector3(0, 1, 1);
        posInicialCaja = cajaTextoNueva.transform.localPosition;
    }

    
    void Update()
    {     
        //if(GeneralPauseScript.pause.estadoJuego == GameState.jugando && !ControlPulsarSobreManchaScript.instance.pulsandoArbol &&
        //    !TextoRetosInicialesScript.instance.mostrandoRetosEnPantalla && GeneralPauseScript.pause.tipoDePartida == TipoDePartida.completarMisiones)
        //{
        //    if(!calcularRetos)
        //    {
        //        contadorDeRetos.text = ContarRetos().ToString("F0") + "/3";
        //        cajaTextoNueva.color = new Color(1, 1, 1);
        //        calcularRetos = true;
        //    }
        //    menuNotificaciones.transform.localScale = Vector3.Lerp(menuNotificaciones.transform.localScale,Vector3.one,.3f);
        //}
        //else
        //{
        //    menuNotificaciones.transform.localScale = Vector3.zero;
        //    calcularRetos = false;
        //}
        deltaTime2 = Time.realtimeSinceStartup - lastInterval;
        lastInterval = Time.realtimeSinceStartup;

        if (notificarReto)
        {
            if (notificaciones.Count > 0)
            {
                if(!notificando)
                {
                    nuevoTextoRetos.text = notificaciones[0];
                    nuevoTextoRetos.color = Color.black;
                    notificando = true;
                }
               
                if (calcularLongitudLineaRoja)
                {
                    lineaRoja.rectTransform.sizeDelta = new Vector2(nuevoTextoRetos.text.Length * 35, 7.5f);
                    cajaRetos.rectTransform.sizeDelta = new Vector2(nuevoTextoRetos.text.Length * 35, 150);
                    calcularLongitudLineaRoja = false;
                }
                velocidadAnimacion += Time.unscaledDeltaTime;
                if (velocidadAnimacion < 1)
                {
                    cajaTextoNueva.transform.localPosition = new Vector3(EasingFunction.SelectEasingFunction(gaugeEasingFunction, -1300, 0, velocidadAnimacion), posInicialCaja.y, posInicialCaja.z);
                }
                else
                {
                    if (!animandoLaCaja) StartCoroutine(MostrarRetoFlama());
                }
                
                if (finAnimacionCaja)
                {
                    velocidadAnimacion2 += Time.unscaledDeltaTime;
                    if (velocidadAnimacion2 < 1)
                    {
                        cajaTextoNueva.transform.localPosition = new Vector3(EasingFunction.SelectEasingFunction(gaugeEasingFunction2, 0, -1300, velocidadAnimacion2), posInicialCaja.y, posInicialCaja.z);
                    }
                    else
                    {
                        notificando = false;
                        notificarReto = false;
                        colorTexto.a = 0;
                        cajaTextoNueva.transform.localPosition = new Vector3(-1300, posInicialCaja.y, posInicialCaja.z);
                        velocidadAnimacion = 0;
                        velocidadAnimacion2 = 0;
                        string prov = notificaciones[0];
                        notificaciones.Remove(prov);
                        contadorDeRetos.text = ContarRetos().ToString("F0") + "/3";
                        cajaTextoNueva.color = new Color(1, 1, 1);
                        finAnimacionCaja = false;
                        animandoLaCaja = false;
                        //Debug.LogError(notificaciones.Count);
                        if (notificaciones.Count == 0)
                        {

                        }
                        else
                        {
                            //notificarReto = true;
                        }
                    }                   
                }
            }
            else
            {
                notificarReto = false;
            }
        }
        else
        {
            calcularLongitudLineaRoja = true;
            if(notificaciones.Count > 0)
            {
                notificarReto = true;
            }
        }

        textoRetos.color = colorTexto;
        cajaRetos.color = new Color(1, 1, 1, colorTexto.a);        
    }

    public int ContarRetos()
    {
        int numeroRetos = 0;

        if (MasterManager.MissionsDataManager.currentMission.challenges[0].completed) numeroRetos++;
        if (MasterManager.MissionsDataManager.currentMission.challenges[1].completed) numeroRetos++;
        if (MasterManager.MissionsDataManager.currentMission.challenges[2].completed) numeroRetos++;
        return numeroRetos;
    }

    public IEnumerator MostrarRetoFlama()
    {
        animandoLaCaja = true;
        yield return new WaitForSeconds(.5f);
        while (cajaTextoNueva.transform.localScale.y > 0.0001f)
        {
            cajaTextoNueva.transform.localScale = Vector3.Lerp(cajaTextoNueva.transform.localScale, new Vector3(1, 0, 1), .6f);
            yield return null;
        }
        cajaTextoNueva.color = new Color(.5f, .66f, 0);
        nuevoTextoRetos.text = "<s>" + notificaciones[0] + "</s>";
        nuevoTextoRetos.color = Color.white;
        while (cajaTextoNueva.transform.localScale.y < 0.9999f)
        {
            cajaTextoNueva.transform.localScale = Vector3.Lerp(cajaTextoNueva.transform.localScale, new Vector3(1, 1, 1), .2f);
            yield return null;
        }

        finAnimacionCaja = true;
    }
}
