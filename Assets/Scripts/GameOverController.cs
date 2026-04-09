using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("Textos de valores (solo el numero/valor cambia)")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion; // Campo vacio bajo PUNTUACION FINAL
    [SerializeField] private TextMeshProUGUI textoOleada;     // Campo vacio bajo OLEADA ALCANZADA
    [SerializeField] private TextMeshProUGUI textoRecord;     // Texto NUEVO RECORD (se activa/oculta)

    [Header("Escenas")]
    [SerializeField] private string escenaJuego = "Level_Sala";
    [SerializeField] private string escenaMenu  = "MainMenu";

    void Start()
    {
        int puntos = PlayerPrefs.GetInt("PuntuacionFinal", 0);
        int oleada = PlayerPrefs.GetInt("OleadaFinal", 1);
        int record = PlayerPrefs.GetInt("Record", 0);
        bool esNuevoRecord = puntos > record;

        // Guardar nuevo record si aplica
        if (esNuevoRecord)
        {
            PlayerPrefs.SetInt("Record", puntos);
            PlayerPrefs.Save();
        }

        // Solo actualizar el valor numerico, no el titulo
        if (textoPuntuacion != null)
            textoPuntuacion.text = $"RD$ {puntos}";

        if (textoOleada != null)
            textoOleada.text = $"{oleada}";

        // Mostrar u ocultar NUEVO RECORD
        if (textoRecord != null)
            textoRecord.gameObject.SetActive(esNuevoRecord);
    }

    // ── BOTONES ───────────────────────────────────────────────
    public void Reiniciar()
    {
        AudioManager.Instance?.SonarBoton();
        PlayerPrefs.DeleteKey("PuntuacionAcumulada");
        PlayerPrefs.DeleteKey("VidasActuales");
        SceneManager.LoadScene(escenaJuego);
    }

    public void IrAlMenu()
    {
        AudioManager.Instance?.SonarBoton();
        SceneManager.LoadScene(escenaMenu);
    }

    public void Salir()
    {
        AudioManager.Instance?.SonarBoton();
        Application.Quit();
    }

    public void VerPuntuaciones()
    {
        AudioManager.Instance?.SonarBoton();
        // Por ahora no hace nada, lo conectamos cuando hagas la tabla
        Debug.Log("Ver puntuaciones - proximamente");
    }
}
