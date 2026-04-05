using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Nombre de la escena del juego")]
    [SerializeField] private string escenaJuego = "Level_Sala";

    [Header("Panel de opciones (opcional)")]
    [SerializeField] private GameObject panelOpciones;

    public void Jugar()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
