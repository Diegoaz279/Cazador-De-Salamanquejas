using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Pon este script en un GameObject vacio llamado "TransicionNivel"
// Necesita una Image negra que cubra toda la pantalla (en el Canvas)
public class TransicionNivel : MonoBehaviour
{
    public static TransicionNivel Instance { get; private set; }

    [Header("Panel negro para el fade")]
    [SerializeField] private Image panelNegro; // Image negro que cubre toda la pantalla

    [Header("Velocidad del fade")]
    [SerializeField] private float velocidadFade = 1.5f;

    private bool transicionando = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Empieza completamente transparente
        SetAlpha(0f);
    }

    // Llamado cuando el jugador toca la zona de la puerta
    public void IniciarTransicion()
    {
        if (transicionando) return;
        transicionando = true;
        StartCoroutine(FadeYCambiarEscena());
    }

    IEnumerator FadeYCambiarEscena()
    {
        // Fade a negro
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadFade;
            SetAlpha(Mathf.Clamp01(t));
            yield return null;
        }

        SetAlpha(1f);

        // Pequena pausa en negro
        yield return new WaitForSeconds(0.3f);

        // Cambiar de escena
        GameManager.Instance?.PasarDeNivel();
    }

    void SetAlpha(float a)
    {
        if (panelNegro == null) return;
        Color c = panelNegro.color;
        c.a = a;
        panelNegro.color = c;
    }
}
