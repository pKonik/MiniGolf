using UnityEngine;
using UnityEngine.InputSystem;

public class GolfCameraFollow : MonoBehaviour
{
    public Transform pelota;

    public float sensibilidad = 0.2f;
    public float velocidadZoom = 0.01f;
    public float distanciaMinima = 1f;
    public float distanciaMaxima = 15f;

    private float distancia;
    private float giro;
    private float inclinacion;

    void Start()
    {
        if (pelota == null)
            return;

        Vector3 haciaPelota = pelota.position - transform.position;
        distancia = Mathf.Clamp(
            haciaPelota.magnitude,
            distanciaMinima,
            distanciaMaxima
        );

        if (haciaPelota.sqrMagnitude > 0.001f)
        {
            Vector3 angulos = Quaternion.LookRotation(
                haciaPelota,
                Vector3.up
            ).eulerAngles;

            giro = angulos.y;
            inclinacion = Mathf.DeltaAngle(0f, angulos.x);
        }

        inclinacion = Mathf.Clamp(inclinacion, 10f, 80f);
    }

    void LateUpdate()
    {
        if (pelota == null)
            return;

        var raton = Mouse.current;

        if (raton != null)
        {
            // No girar mientras se arrastra el golpe.
            if (raton.rightButton.isPressed &&
                !raton.leftButton.isPressed)
            {
                Vector2 movimiento = raton.delta.ReadValue();

                giro += movimiento.x * sensibilidad;
                inclinacion -= movimiento.y * sensibilidad;
                inclinacion = Mathf.Clamp(inclinacion, 10f, 80f);
            }

            if (!raton.leftButton.isPressed)
            {
                distancia -= raton.scroll.ReadValue().y * velocidadZoom;
                distancia = Mathf.Clamp(
                    distancia,
                    distanciaMinima,
                    distanciaMaxima
                );
            }
        }

        Quaternion rotacion = Quaternion.Euler(
            inclinacion,
            giro,
            0f
        );

        transform.position =
            pelota.position + rotacion * Vector3.back * distancia;

        transform.rotation = rotacion;
    }
}