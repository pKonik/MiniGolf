using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class GolfBallController : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camara;

    [Header("Golpe")]
    public float potenciaMaxima = 12f;
    public float arrastreMaximo = 2f;
    public float arrastreMinimo = 0.05f;

    [Header("Preparar siguiente tiro")]
    public float velocidadPermitida = 0.3f;

    [Header("Flecha")]
    public Color colorFlecha = Color.yellow;
    public float grosorFlecha = 0.035f;

    private Rigidbody rb;
    private SphereCollider esfera;
    private LineRenderer flecha;
    private Material materialFlecha;

    private Vector3 inicio;
    private Quaternion rotacionInicial;
    private Vector3 posicionPreparacion;
    private Vector3 cursorInicial;
    private Vector3 vectorTiro;
    private Vector3 golpePendiente;

    private Plane planoArrastre;
    private bool preparando;
    private float ultimoContacto = -10f;
    private int golpes;

    private bool EnSuelo =>
        Time.time - ultimoContacto < 0.15f;

    private bool PuedeGolpear =>
        EnSuelo &&
        rb.linearVelocity.magnitude <= velocidadPermitida &&
        golpePendiente.sqrMagnitude == 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        esfera = GetComponent<SphereCollider>();

        rb.sleepThreshold = 0f;
        rb.WakeUp();

        inicio = rb.position;
        rotacionInicial = rb.rotation;

        if (camara == null)
            camara = Camera.main;

        CrearFlecha();
    }

    void CrearFlecha()
    {
        GameObject objeto = new GameObject("Flecha de apuntado");
        objeto.transform.SetParent(transform, false);

        flecha = objeto.AddComponent<LineRenderer>();
        flecha.useWorldSpace = true;
        flecha.positionCount = 5;
        flecha.startWidth = grosorFlecha;
        flecha.endWidth = grosorFlecha;
        flecha.numCapVertices = 4;
        flecha.enabled = false;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            materialFlecha = new Material(shader);

            if (materialFlecha.HasProperty("_BaseColor"))
                materialFlecha.SetColor("_BaseColor", colorFlecha);

            if (materialFlecha.HasProperty("_Color"))
                materialFlecha.SetColor("_Color", colorFlecha);

            flecha.sharedMaterial = materialFlecha;
        }

        flecha.startColor = colorFlecha;
        flecha.endColor = colorFlecha;
        flecha.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        flecha.receiveShadows = false;
    }

    void Update()
    {
        if (Mouse.current == null || camara == null)
            return;

        var raton = Mouse.current;
        var teclado = Keyboard.current;

        if (teclado != null && teclado.rKey.wasPressedThisFrame)
        {
            Reiniciar();
            return;
        }

        if (preparando)
        {
            // Clic derecho cancela el tiro.
            if (raton.rightButton.wasPressedThisFrame)
            {
                Cancelar();
                return;
            }

            ActualizarArrastre();

            if (raton.leftButton.wasReleasedThisFrame)
                SoltarGolpe();

            return;
        }

        if (!PuedeGolpear || !raton.leftButton.wasPressedThisFrame)
            return;

        Ray rayo = camara.ScreenPointToRay(
            raton.position.ReadValue()
        );

        // Solo se empieza al hacer clic sobre la pelota.
        if (!esfera.Raycast(rayo, out RaycastHit impacto, 1000f))
            return;

        posicionPreparacion = rb.position;
        planoArrastre = new Plane(
            Vector3.up,
            esfera.bounds.center
        );

        if (!planoArrastre.Raycast(rayo, out float distancia))
            return;

        cursorInicial = rayo.GetPoint(distancia);
        vectorTiro = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        preparando = true;
    }

    void ActualizarArrastre()
    {
        Ray rayo = camara.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!planoArrastre.Raycast(rayo, out float distancia))
            return;

        Vector3 cursorActual = rayo.GetPoint(distancia);

        // Arrastrar hacia atrás apunta hacia delante.
        vectorTiro = cursorInicial - cursorActual;
        vectorTiro.y = 0f;
        vectorTiro = Vector3.ClampMagnitude(
            vectorTiro,
            Mathf.Max(arrastreMaximo, 0.01f)
        );

        DibujarFlecha();
    }

    void DibujarFlecha()
    {
        float longitud = vectorTiro.magnitude;

        if (longitud < arrastreMinimo)
        {
            flecha.enabled = false;
            return;
        }

        Vector3 direccion = vectorTiro.normalized;
        Vector3 lateral = Vector3.Cross(
            Vector3.up,
            direccion
        );

        Vector3 origen = esfera.bounds.center;
        Vector3 punta = origen + vectorTiro;
        float cabeza = Mathf.Min(0.25f, longitud * 0.35f);

        Vector3 alaIzquierda =
            punta - direccion * cabeza + lateral * cabeza * 0.5f;

        Vector3 alaDerecha =
            punta - direccion * cabeza - lateral * cabeza * 0.5f;

        flecha.SetPosition(0, origen);
        flecha.SetPosition(1, punta);
        flecha.SetPosition(2, alaIzquierda);
        flecha.SetPosition(3, punta);
        flecha.SetPosition(4, alaDerecha);
        flecha.enabled = true;
    }

    void SoltarGolpe()
    {
        float distancia = vectorTiro.magnitude;

        if (distancia >= arrastreMinimo)
        {
            float porcentaje = Mathf.Clamp01(
                distancia / Mathf.Max(arrastreMaximo, 0.01f)
            );

            golpePendiente =
                vectorTiro.normalized * porcentaje * potenciaMaxima;
        }

        Cancelar();
    }

    void FixedUpdate()
    {
        // Mantener la pelota en su sitio mientras apuntas.
        if (preparando)
        {
            rb.position = posicionPreparacion;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        if (golpePendiente.sqrMagnitude == 0f)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        rb.AddForce(
            golpePendiente,
            ForceMode.VelocityChange
        );

        golpePendiente = Vector3.zero;
        golpes++;
    }

    void OnCollisionEnter(Collision colision)
    {
        ComprobarApoyo(colision);
    }

    void OnCollisionStay(Collision colision)
    {
        ComprobarApoyo(colision);
    }

    void ComprobarApoyo(Collision colision)
    {
        foreach (ContactPoint contacto in colision.contacts)
        {
            if (Vector3.Dot(contacto.normal, Vector3.up) > 0.5f)
            {
                ultimoContacto = Time.time;
                break;
            }
        }
    }

    void Cancelar()
    {
        preparando = false;
        vectorTiro = Vector3.zero;

        if (flecha != null)
            flecha.enabled = false;
    }

    public void Reiniciar()
    {
        Cancelar();
        golpePendiente = Vector3.zero;

        rb.position = inicio;
        rb.rotation = rotacionInicial;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        ultimoContacto = -10f;
        golpes = 0;
        rb.WakeUp();
    }

    void OnApplicationFocus(bool tieneFoco)
    {
        if (!tieneFoco)
            Cancelar();
    }

    void OnDisable()
    {
        Cancelar();
        golpePendiente = Vector3.zero;
    }

    void OnDestroy()
    {
        if (materialFlecha != null)
            Destroy(materialFlecha);
    }

    void OnGUI()
    {
        string estado;

        if (camara == null)
            estado = "Asigna Main Camera en el Inspector.";
        else if (preparando)
            estado = "Arrastra hacia atrás y suelta para golpear.";
        else if (PuedeGolpear)
            estado = "Haz clic sobre la pelota para preparar el tiro.";
        else
            estado = "Pelota en movimiento o sin apoyo.";

        float porcentaje = preparando
            ? Mathf.Clamp01(
                vectorTiro.magnitude /
                Mathf.Max(arrastreMaximo, 0.01f)
            ) * 100f
            : 0f;

        GUI.Box(new Rect(15, 15, 440, 110), "");
        GUI.Label(new Rect(25, 25, 420, 25), estado);

        GUI.Label(
            new Rect(25, 55, 420, 25),
            $"Golpes: {golpes} | Potencia: {porcentaje:F0}%"
        );

        GUI.Label(
            new Rect(25, 85, 420, 25),
            "R: reiniciar | Clic derecho: cancelar"
        );
    }
}