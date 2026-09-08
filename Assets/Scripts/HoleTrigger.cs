using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class HoleTrigger : MonoBehaviour
{
    public GolfBallController pelota;
    public float segundosAntesDeReiniciar = 2f;

    private bool victoria;


    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider otro)
    {
        IntentarCompletar(otro);
    }

    void OnTriggerStay(Collider otro)
    {
        IntentarCompletar(otro);
    }

    void IntentarCompletar(Collider otro)
    {
        if (victoria)
            return;

        Rigidbody cuerpo = otro.attachedRigidbody;

        if (cuerpo == null)
            return;

        GolfBallController candidata = cuerpo.GetComponent<GolfBallController>();
        if (candidata == null || (pelota != null && candidata != pelota))
            return;

        pelota = candidata;
        StartCoroutine(CompletarHoyo(cuerpo));
    }

    IEnumerator CompletarHoyo(Rigidbody cuerpo)
    {
        victoria = true;


        // Bloquear nuevos golpes y detener la pelota.
        pelota.enabled = false;

        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
        cuerpo.isKinematic = true;

        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, segundosAntesDeReiniciar));

        // Recargar la escena completa restablece pelota, golpes y obstaculos.
        SceneManager.LoadScene(gameObject.scene.path);
    }
    void OnGUI()
    {
        if (!victoria)
            return;

        GUIStyle estilo = new GUIStyle(GUI.skin.box);
        estilo.fontSize = 24;
        estilo.alignment = TextAnchor.MiddleCenter;
        estilo.wordWrap = true;

        Rect cuadro = new Rect(
            Screen.width / 2f - 210f,
            Screen.height / 2f - 70f,
            420f,
            140f
        );

        GUI.Box(
            cuadro,
            "¡Victoria!\nHoyo completado\n" +

            "Reiniciando el nivel...",
            estilo
        );
    }
}
