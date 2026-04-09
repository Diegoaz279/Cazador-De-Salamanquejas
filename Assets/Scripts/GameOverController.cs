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
        int puntos  = PlayerPrefs.GetInt("", 0);
        int oleada  = PlayerPrefs.GetInt("", 1);
        int record  = PlayerPrefs.GetInt("", 0);
        bool nuevo  = puntos > record;

        if (nuevo) { PlayerPrefs.SetInt("", puntos); PlayerPrefs.Save(); record = puntos; }

        if (textoPuntuacion != null) textoPuntuacion.text = $"{puntos}";
        if (textoOleada     != null) textoOleada.text     = $"{oleada}";
        if (textoRecord     != null)
        {
            textoRecord.text  = nuevo ? "¡NUEVO RECORD! ¡Qué bruto!" : $"Record: RD$ {record}";
            textoRecord.color = nuevo ? Color.yellow : Color.white;
        }
    }

    public void Reiniciar() => SceneManager.LoadScene(escenaJuego);
    public void IrAlMenu()  => SceneManager.LoadScene(escenaMenu);
    public void Salir()     => Application.Quit();
}
