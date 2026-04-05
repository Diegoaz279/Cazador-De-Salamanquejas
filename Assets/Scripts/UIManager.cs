using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoVidas;
    [SerializeField] private TextMeshProUGUI textoOleada;
    [SerializeField] private TextMeshProUGUI textoContador;

    [Header("Mensaje central")]
    [SerializeField] private GameObject      panelMensaje;
    [SerializeField] private TextMeshProUGUI textoMensaje;

    [Header("Flash de daño")]
    [SerializeField] private Image imagenFlash;

    [Header("Pausa")]
    [SerializeField] private GameObject panelPausa;

    [Header("Iconos de vidas (opcional)")]
    [SerializeField] private GameObject[] iconosVidas;

    void Start()
    {
        panelPausa?.SetActive(false);
        panelMensaje?.SetActive(false);
        if (imagenFlash != null)
        {
            Color c = imagenFlash.color; c.a = 0f; imagenFlash.color = c;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePausa();
    }

    // Llamado por GameManager
    public void Actualizar(int puntos, int vidas, int oleada, int eliminados, int objetivo)
    {
        if (textoPuntuacion != null) textoPuntuacion.text = $"RD$ {puntos}";
        if (textoOleada     != null) textoOleada.text     = $"Oleada {oleada}";
        if (textoVidas      != null) textoVidas.text      = $"Vidas: {vidas}";
        if (textoContador   != null) textoContador.text   = $"Salamanquejas: {eliminados} / {objetivo}";

        if (iconosVidas != null)
            for (int i = 0; i < iconosVidas.Length; i++)
                if (iconosVidas[i] != null) iconosVidas[i].SetActive(i < vidas);
    }

    public void MostrarMensaje(string msg)
    {
        if (textoMensaje != null) textoMensaje.text = msg;
        panelMensaje?.SetActive(true);
        StopCoroutine(nameof(OcultarMensaje));
        StartCoroutine(nameof(OcultarMensaje));
    }

    IEnumerator OcultarMensaje()
    {
        yield return new WaitForSeconds(2.5f);
        panelMensaje?.SetActive(false);
    }

    public void MostrarFlash()
    {
        if (imagenFlash != null) StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float dur = 0.15f, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            Color c = imagenFlash.color; c.a = Mathf.Lerp(0f, 0.5f, t / dur); imagenFlash.color = c;
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            Color c = imagenFlash.color; c.a = Mathf.Lerp(0.5f, 0f, t / dur); imagenFlash.color = c;
            yield return null;
        }
    }

    void TogglePausa()
    {
        if (panelPausa == null) return;
        bool pausado = panelPausa.activeSelf;
        panelPausa.SetActive(!pausado);
        if (pausado) GameManager.Instance?.ReanudarJuego();
        else         GameManager.Instance?.PausarJuego();
    }

    public void ReanudarJuego()
    {
        panelPausa?.SetActive(false);
        GameManager.Instance?.ReanudarJuego();
    }

    // Compatibilidad con botones viejos
    public void ActualizarUI(int p, int v, int o, int e, int obj) => Actualizar(p, v, o, e, obj);
    public void MostrarFlashDanio() => MostrarFlash();
    public void MostrarMensajeOleada(string m) => MostrarMensaje(m);
    public void ActualizarContador(int e, int obj) { if (textoContador != null) textoContador.text = $"Salamanquejas: {e} / {obj}"; }
}
