using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("Textos de valores (solo el numero/valor cambia)")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoOleada;
    [SerializeField] private TextMeshProUGUI textoRecord;

    [Header("Escenas")]
    [SerializeField] private string escenaJuego = "Level_Sala";
    [SerializeField] private string escenaMenu  = "MainMenu";

    void Start()
    {
        int puntos        = PlayerPrefs.GetInt("PuntuacionFinal", 0);
        int oleada        = PlayerPrefs.GetInt("OleadaFinal", 1);
        int record        = PlayerPrefs.GetInt("Record", 0);
        bool esNuevoRecord = puntos > record;

        if (esNuevoRecord)
        {
            PlayerPrefs.SetInt("Record", puntos);
            PlayerPrefs.Save();
        }

        if (textoPuntuacion != null) textoPuntuacion.text = $"RD$ {puntos}";
        if (textoOleada     != null) textoOleada.text     = $"{oleada}";
        if (textoRecord     != null) textoRecord.gameObject.SetActive(esNuevoRecord);
    }

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
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void VerPuntuaciones()
    {
        AudioManager.Instance?.SonarBoton();
        Debug.Log("Puntuaciones - proximamente");
    }
}
