using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoOleada;
    [SerializeField] private TextMeshProUGUI textoRecord;

    [Header("Escenas")]
    [SerializeField] private string escenaJuego = "Level_Sala";
    [SerializeField] private string escenaMenu  = "MainMenu";

    void Start()
    {
        int puntos  = PlayerPrefs.GetInt("PuntuacionFinal", 0);
        int oleada  = PlayerPrefs.GetInt("OleadaFinal", 1);
        int record  = PlayerPrefs.GetInt("Record", 0);
        bool esRecord = puntos > record;

        if (esRecord)
        {
            PlayerPrefs.SetInt("Record", puntos);
            PlayerPrefs.Save();
            record = puntos;
        }

        if (textoPuntuacion != null) textoPuntuacion.text = $"Puntuacion: RD$ {puntos}";
        if (textoOleada     != null) textoOleada.text     = $"Oleada alcanzada: {oleada}";
        if (textoRecord     != null)
        {
            textoRecord.text  = esRecord ? "¡NUEVO RECORD!" : $"Record: RD$ {record}";
            textoRecord.color = esRecord ? Color.yellow : Color.white;
        }
    }

    public void Reiniciar()   => SceneManager.LoadScene(escenaJuego);
    public void IrAlMenu()    => SceneManager.LoadScene(escenaMenu);
    public void Salir()       => Application.Quit();
}
