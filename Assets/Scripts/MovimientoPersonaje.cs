

using UnityEngine;
using Photon.Pun;
using TMPro; // NUEVO: permite controlar textos TextMeshPro de la interfaz.

public class MovimientoPersonaje : MonoBehaviourPun
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 6f; // Velocidad de avance y retroceso.
    [SerializeField] private CharacterController characterController; // Controla el movimiento y las colisiones.
    [SerializeField] private Transform transformPersonaje; // Controla la orientación del personaje.

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 1f; // Altura aproximada del salto.
    [SerializeField] private float gravedad = -20f; // Fuerza que hace caer al personaje.
    private float velocidadVertical = 0f; // Velocidad vertical durante el salto y la caída.

    [Header("Giro del personaje")]
    [SerializeField] private float duracionGiro = 0.3f; // Tiempo que tarda en girar.

    [Header("Cámaras")]
    [SerializeField] private Camera camaraTerceraPersona; // Cámara situada detrás del personaje.
    [SerializeField] private Camera camaraPrimeraPersona; // Cámara desde la perspectiva del personaje.
    [SerializeField] private Camera camaraOrbital; // Cámara que gira alrededor del personaje.
    [SerializeField] private KeyCode teclaCambiarCamara = KeyCode.C; // Tecla para cambiar de cámara.

    // NUEVO: referencia al texto que indica qué cámara está activa.
    [Header("Interfaz de cámara")]
    [SerializeField] private TMP_Text textoModoCamara;

    [Header("Configuración de cámara orbital")]
    [SerializeField] private float distanciaOrbital = 4f; // Distancia entre la cámara y el punto observado.
    [SerializeField] private float alturaObjetivoOrbital = 1.5f; // Altura del punto que observa la cámara.
    [SerializeField] private float sensibilidadMouse = 3f; // Sensibilidad al mover el ratón.
    [SerializeField] private float anguloVerticalMinimo = -20f; // Inclinación vertical mínima.
    [SerializeField] private float anguloVerticalMaximo = 70f; // Inclinación vertical máxima.

    private bool primeraPersona = false; // Indica si está activa la primera persona.
    private bool girando = false; // Indica si el personaje está girando.
    private float anguloObjetivo; // Ángulo final del giro.
    private float velocidadGiro; // Velocidad del giro.
    private float anguloInicial; // Orientación anterior al giro.

    private int modoCamara = 0; // 0 = tercera persona, 1 = primera persona, 2 = cámara orbital.
    private float anguloOrbitalHorizontal; // Giro horizontal de la cámara orbital.
    private float anguloOrbitalVertical = 8f; // Inclinación inicial de la cámara orbital.

    // Indica si el personaje local terminó de inicializar su controlador y sus cámaras.
    // PhotonLauncher consulta esta propiedad antes de ocultar la pantalla de carga.
    public bool EstaListo { get; private set; } = false;

    private void Awake()
    {
        // Busca el CharacterController si no fue asignado en el Inspector.
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // Utiliza el objeto actual si no se asignó un Transform.
        if (transformPersonaje == null)
            transformPersonaje = transform;
    }

    private void Start()
    {
        // Comprueba que el personaje tenga el componente necesario para moverse.
        if (characterController == null)
        {
            Debug.LogError("MovimientoPersonaje: falta CharacterController.", this);
            enabled = false;
            return;
        }

        // Desactiva las cámaras de los personajes controlados por otros jugadores.
        if (!photonView.IsMine)
        {
            DesactivarCamara(camaraTerceraPersona);
            DesactivarCamara(camaraPrimeraPersona);
            DesactivarCamara(camaraOrbital);
            return;
        }

        // Comprueba que las tres cámaras estén asignadas en el Inspector.
        if (camaraTerceraPersona == null ||
            camaraPrimeraPersona == null ||
            camaraOrbital == null)
        {
            Debug.LogError(
                "MovimientoPersonaje: asigna las tres cámaras en el Inspector.", this);
            enabled = false;
            return;
        }

        // NUEVO: busca el texto en el Canvas de la escena si no fue asignado.
        // El objeto de la interfaz debe llamarse exactamente TextoModoCamara.
        if (textoModoCamara == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoModoCamara");

            if (objetoTexto != null)
                textoModoCamara = objetoTexto.GetComponent<TMP_Text>();
        }

        // Informa si no se encontró el texto, sin impedir que el jugador se mueva.
        if (textoModoCamara == null)
        {
            Debug.LogWarning(
                "MovimientoPersonaje: no se encontró TextoModoCamara en la escena.");
        }

        // Evita una duración de giro igual a cero o demasiado pequeña.
        duracionGiro = Mathf.Max(0.05f, duracionGiro);

        // Calcula la velocidad necesaria para girar 45 grados en el tiempo establecido.
        velocidadGiro = 45f / duracionGiro;

        // Inicializa la cámara orbital con la orientación del personaje.
        anguloOrbitalHorizontal = transformPersonaje.eulerAngles.y;

        // Activa inicialmente la cámara de tercera persona.
        CambiarCamara(0);

        // Comprueba que la cámara inicial esté activa y habilitada.
        if (!camaraTerceraPersona.enabled ||
            !camaraTerceraPersona.gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "MovimientoPersonaje: la cámara de tercera persona no está activa.",
                this);

            EstaListo = false;
            return;
        }

        // Informa al sistema de carga que el jugador terminó de inicializarse.
        EstaListo = true;
    }

    private void Update()
    {
        // Solo el propietario del personaje puede controlar sus movimientos y cámaras.
        if (!photonView.IsMine)
            return;

        // Detecta si se presiona A o D para iniciar un giro.
        ProcesarEntradaGiro();

        // Actualiza progresivamente el giro del personaje.
        ActualizarGiro();

        // Gestiona el desplazamiento, el salto y la gravedad.
        MovimientoDelPersonaje();

        // Al pulsar C, cambia a la siguiente cámara.
        if (Input.GetKeyDown(teclaCambiarCamara))
        {
            CambiarCamara((modoCamara + 1) % 3);
        }

        // Solo permite girar la cámara con el ratón en el modo orbital.
        if (modoCamara == 2)
        {
            ActualizarCamaraOrbital();
        }
    }

    private void LateUpdate()
    {
        // Actualiza la posición de la cámara orbital después de mover al personaje.
        if (photonView.IsMine && modoCamara == 2 && camaraOrbital != null)
        {
            ColocarCamaraOrbital();
        }
    }

    private void ProcesarEntradaGiro()
    {
        // No permite iniciar otro giro mientras el actual no haya terminado.
        if (girando)
            return;

        // A inicia un giro hacia la izquierda de 45 grados.
        if (Input.GetKeyDown(KeyCode.A))
        {
            IniciarGiro(-45f);
        }
        // D inicia un giro hacia la derecha de 45 grados.
        else if (Input.GetKeyDown(KeyCode.D))
        {
            IniciarGiro(45f);
        }
    }

    private void IniciarGiro(float angulo)
    {
        // Guarda la orientación horizontal actual del personaje.
        anguloInicial = transformPersonaje.eulerAngles.y;

        // Calcula el ángulo final del giro.
        anguloObjetivo = anguloInicial + angulo;

        // Indica que el personaje comenzó a girar.
        girando = true;
    }

    private void ActualizarGiro()
    {
        // No hace nada si no hay un giro activo.
        if (!girando)
            return;

        // Obtiene el ángulo horizontal actual.
        float anguloActual = transformPersonaje.eulerAngles.y;

        // Se acerca progresivamente al ángulo objetivo.
        float nuevoAngulo = Mathf.MoveTowardsAngle(
            anguloActual,
            anguloObjetivo,
            velocidadGiro * Time.deltaTime
        );

        // Aplica la nueva orientación al personaje.
        transformPersonaje.rotation = Quaternion.Euler(
            transformPersonaje.eulerAngles.x,
            nuevoAngulo,
            transformPersonaje.eulerAngles.z
        );

        // Comprueba si el personaje llegó al ángulo objetivo.
        if (Mathf.Abs(Mathf.DeltaAngle(nuevoAngulo, anguloObjetivo)) < 0.1f)
        {
            // Ajusta la orientación final para evitar pequeñas diferencias.
            transformPersonaje.rotation = Quaternion.Euler(
                transformPersonaje.eulerAngles.x,
                anguloObjetivo,
                transformPersonaje.eulerAngles.z
            );

            // Finaliza el giro.
            girando = false;
        }
    }

    private void MovimientoDelPersonaje()
    {
        // Indica cuánto avanzar o retroceder.
        float adelante = 0f;

        // W permite avanzar.
        if (Input.GetKey(KeyCode.W))
            adelante += 1f;

        // S permite retroceder.
        if (Input.GetKey(KeyCode.S))
            adelante -= 1f;

        // Calcula la dirección según la orientación del personaje.
        Vector3 direccion = transformPersonaje.forward * adelante;

        // Limita la magnitud de la dirección horizontal.
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        // Comprueba si el personaje está tocando el suelo.
        if (characterController.isGrounded)
        {
            // Mantiene una pequeña velocidad hacia abajo para detectar el suelo.
            if (velocidadVertical < 0f)
                velocidadVertical = -2f;

            // Solo permite saltar cuando está en el suelo.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Calcula la velocidad inicial del salto.
                velocidadVertical = Mathf.Sqrt(alturaSalto * -2f * gravedad);
            }
        }

        // Aplica la gravedad durante el salto y la caída.
        velocidadVertical += gravedad * Time.deltaTime;

        // Calcula el desplazamiento horizontal.
        Vector3 movimiento = direccion * velocidadMovimiento;

        // Añade la velocidad vertical.
        movimiento.y = velocidadVertical;

        // Mueve al personaje respetando las colisiones.
        characterController.Move(movimiento * Time.deltaTime);
    }

    private void ActualizarCamaraOrbital()
    {
        // Mantiene el cursor visible y libre para interactuar con la interfaz.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // La cámara solo gira mientras se mantiene pulsado el botón derecho del ratón.
        if (Input.GetMouseButton(1))
        {
            // El movimiento horizontal cambia el ángulo alrededor del personaje.
            anguloOrbitalHorizontal += Input.GetAxis("Mouse X") * sensibilidadMouse;

            // El movimiento vertical cambia la inclinación de la cámara.
            anguloOrbitalVertical -= Input.GetAxis("Mouse Y") * sensibilidadMouse;

            // Limita la inclinación vertical.
            anguloOrbitalVertical = Mathf.Clamp(
                anguloOrbitalVertical,
                anguloVerticalMinimo,
                anguloVerticalMaximo
            );
        }
    }

    private void ColocarCamaraOrbital()
    {
        // Define el punto observado, situado a cierta altura y delante del personaje.
        Vector3 puntoObjetivo = transformPersonaje.position
            + Vector3.up * alturaObjetivoOrbital
            + transformPersonaje.forward * 0.5f;

        // Calcula la orientación orbital con los ángulos controlados por el ratón.
        Quaternion rotacionOrbital = Quaternion.Euler(
            anguloOrbitalVertical,
            anguloOrbitalHorizontal,
            0f
        );

        // Calcula la posición de la cámara respecto al punto observado.
        Vector3 desplazamiento = rotacionOrbital * Vector3.back * distanciaOrbital;

        // Coloca la cámara en la posición calculada.
        camaraOrbital.transform.position = puntoObjetivo + desplazamiento;

        // Hace que la cámara mire hacia el punto observado.
        camaraOrbital.transform.LookAt(puntoObjetivo);
    }

    private void CambiarCamara(int nuevoModo)
    {
        // Guarda el índice de la cámara seleccionada.
        modoCamara = nuevoModo;

        // Indica si está activa la cámara en primera persona.
        primeraPersona = (modoCamara == 1);

        // NUEVO: actualiza el texto de la interfaz según la cámara seleccionada.
        if (textoModoCamara != null)
        {
            switch (modoCamara)
            {
                case 0:
                    textoModoCamara.text = "Cámara Tercera Persona";
                    break;

                case 1:
                    textoModoCamara.text = "Cámara Primera Persona";
                    break;

                case 2:
                    textoModoCamara.text = "Cámara lectura + Click Derecho";
                    break;
            }
        }

        // Activa el objeto de la cámara seleccionada y desactiva las otras dos.
        camaraTerceraPersona.gameObject.SetActive(modoCamara == 0);
        camaraPrimeraPersona.gameObject.SetActive(modoCamara == 1);
        camaraOrbital.gameObject.SetActive(modoCamara == 2);

        // Habilita únicamente el componente Camera del modo seleccionado.
        camaraTerceraPersona.enabled = (modoCamara == 0);
        camaraPrimeraPersona.enabled = (modoCamara == 1);
        camaraOrbital.enabled = (modoCamara == 2);

        // Obtiene el AudioListener de cada cámara, si existe.
        AudioListener audioTercera = camaraTerceraPersona.GetComponent<AudioListener>();
        AudioListener audioPrimera = camaraPrimeraPersona.GetComponent<AudioListener>();
        AudioListener audioOrbital = camaraOrbital.GetComponent<AudioListener>();

        // Mantiene activo únicamente el AudioListener de la cámara seleccionada.
        if (audioTercera != null)
            audioTercera.enabled = (modoCamara == 0);

        if (audioPrimera != null)
            audioPrimera.enabled = (modoCamara == 1);

        if (audioOrbital != null)
            audioOrbital.enabled = (modoCamara == 2);

        // Mantiene el cursor visible y libre para interactuar con la interfaz.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Al activar la cámara orbital, inicializa su ángulo horizontal
        // con la orientación del personaje y coloca la cámara.
        if (modoCamara == 2)
        {
            anguloOrbitalHorizontal = transformPersonaje.eulerAngles.y;
            ColocarCamaraOrbital();
        }
    }

    private void DesactivarCamara(Camera camara)
    {
        // Evita errores si la cámara no está asignada.
        if (camara == null)
            return;

        // Desactiva la cámara de los personajes de otros jugadores.
        camara.enabled = false;

        // Desactiva también su AudioListener para evitar conflictos de audio.
        AudioListener audio = camara.GetComponent<AudioListener>();

        if (audio != null)
            audio.enabled = false;

        // Desactiva el objeto de la cámara.
        camara.gameObject.SetActive(false);
    }
}
