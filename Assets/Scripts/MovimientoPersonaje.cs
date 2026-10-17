
using UnityEngine;
using Photon.Pun;

public class MovimientoPersonaje : MonoBehaviourPun
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 6f; // Velocidad con la que el personaje avanza y retrocede.
    [SerializeField] private CharacterController characterController; // Componente que permite mover al personaje respetando las colisiones.
    [SerializeField] private Transform transformPersonaje; // Transform que controla la orientación y dirección del personaje.

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 1f; // Altura aproximada que alcanza el personaje al saltar.
    [SerializeField] private float gravedad = -20f; // Fuerza que hace que el personaje vuelva a caer.
    private float velocidadVertical = 0f; // Guarda la velocidad vertical durante el salto y la caída.

    [Header("Giro del personaje")]
    [SerializeField] private float duracionGiro = 0.3f; // Tiempo que tarda el personaje en completar su giro.

    [Header("Cámaras")]
    [SerializeField] private Camera camaraTerceraPersona; // Cámara original que muestra al personaje desde atrás.
    [SerializeField] private Camera camaraPrimeraPersona; // Cámara original situada a la altura de los ojos del personaje.
    [SerializeField] private Camera camaraOrbital; // Tercera cámara que gira alrededor del personaje utilizando el mouse.
    [SerializeField] private KeyCode teclaCambiarCamara = KeyCode.C; // Tecla que permite alternar entre las tres cámaras.

    [Header("Configuración de cámara orbital")]
    [SerializeField] private float distanciaOrbital = 4f; // Distancia entre la cámara orbital y el punto que observa.
    [SerializeField] private float alturaObjetivoOrbital = 1.5f; // Altura del punto del personaje hacia el que mira la cámara.
    [SerializeField] private float sensibilidadMouse = 3f; // Sensibilidad del movimiento del mouse al girar la cámara.
    [SerializeField] private float anguloVerticalMinimo = -20f; // Inclinación mínima que puede alcanzar la cámara.
    [SerializeField] private float anguloVerticalMaximo = 70f; // Inclinación máxima que puede alcanzar la cámara.

    private bool primeraPersona = false; // Indica si está activa la cámara en primera persona.
    private bool girando = false; // Indica si el personaje está realizando un giro.
    private float anguloObjetivo; // Ángulo final al que debe girar el personaje.
    private float velocidadGiro; // Velocidad utilizada para completar el giro.
    private float anguloInicial; // Ángulo del personaje antes de iniciar el giro.

    private int modoCamara = 0; // Identifica la cámara activa: 0 = tercera persona, 1 = primera persona, 2 = orbital.
    private float anguloOrbitalHorizontal; // Controla el giro horizontal de la cámara alrededor del personaje.
    private float anguloOrbitalVertical = 8f; // Controla la inclinación vertical inicial de la cámara orbital.

    private void Awake()
    {
        // Busca automáticamente el CharacterController si no fue asignado en el Inspector.
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // Si no se asignó un objeto para controlar la orientación, utiliza el objeto actual.
        if (transformPersonaje == null)
            transformPersonaje = transform;
    }

    private void Start()
    {
        // Comprueba que el personaje tenga el componente necesario para moverse y saltar.
        if (characterController == null)
        {
            Debug.LogError(
                "MovimientoPersonaje: falta CharacterController.", this);
            enabled = false;
            return;
        }

        // Desactiva las cámaras de los personajes controlados por otros jugadores.
        // Así cada usuario controla únicamente su propio personaje y sus cámaras.
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

        // Evita una duración de giro igual a cero o demasiado pequeña.
        duracionGiro = Mathf.Max(0.05f, duracionGiro);

        // Calcula la velocidad para realizar el giro de 45 grados en el tiempo establecido.
        velocidadGiro = 45f / duracionGiro;

        // Inicializa el ángulo horizontal de la cámara orbital con la orientación actual del personaje.
        anguloOrbitalHorizontal = transformPersonaje.eulerAngles.y;

        // Inicia el juego utilizando la cámara original en tercera persona.
        CambiarCamara(0);
    }

    private void Update()
    {
        // Solo el propietario del personaje puede controlar sus movimientos y cámaras.
        if (!photonView.IsMine)
            return;

        // Detecta si el jugador presiona A o D para iniciar un giro.
        ProcesarEntradaGiro();

        // Actualiza progresivamente el giro hasta alcanzar el ángulo objetivo.
        ActualizarGiro();

        // Gestiona el desplazamiento horizontal, el salto y la gravedad.
        MovimientoDelPersonaje();

        // Al presionar C, pasa a la siguiente cámara.
        // El operador % hace que después de la cámara orbital vuelva a la primera cámara.
        if (Input.GetKeyDown(teclaCambiarCamara))
        {
            CambiarCamara((modoCamara + 1) % 3);
        }

        // Solo procesa el movimiento del mouse cuando está activa la cámara orbital.
        if (modoCamara == 2)
        {
            ActualizarCamaraOrbital();
        }
    }

    private void LateUpdate()
    {
        // Actualiza la posición de la cámara orbital después de mover al personaje.
        // Esto permite que la cámara siga al personaje sin modificar su orientación.
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

        // Calcula el ángulo final sumando el giro solicitado.
        anguloObjetivo = anguloInicial + angulo;

        // Indica que el personaje ha comenzado a girar.
        girando = true;
    }

    private void ActualizarGiro()
    {
        // Si no hay un giro activo, no necesita actualizar la orientación.
        if (!girando)
            return;

        // Obtiene el ángulo horizontal actual del personaje.
        float anguloActual = transformPersonaje.eulerAngles.y;

        // Se acerca progresivamente al ángulo objetivo usando el tiempo transcurrido.
        float nuevoAngulo = Mathf.MoveTowardsAngle(
            anguloActual,
            anguloObjetivo,
            velocidadGiro * Time.deltaTime
        );

        // Aplica la nueva orientación al personaje.
        // Si las cámaras son hijas de este objeto, también girarán con él.
        transformPersonaje.rotation = Quaternion.Euler(
            transformPersonaje.eulerAngles.x,
            nuevoAngulo,
            transformPersonaje.eulerAngles.z
        );

        // Comprueba si el personaje ya llegó prácticamente al ángulo objetivo.
        if (Mathf.Abs(
            Mathf.DeltaAngle(nuevoAngulo, anguloObjetivo)
        ) < 0.1f)
        {
            // Ajusta la orientación final para evitar pequeñas diferencias de ángulo.
            transformPersonaje.rotation = Quaternion.Euler(
                transformPersonaje.eulerAngles.x,
                anguloObjetivo,
                transformPersonaje.eulerAngles.z
            );

            // Finaliza el giro y permite iniciar uno nuevo.
            girando = false;
        }
    }

    private void MovimientoDelPersonaje()
    {
        // Variable que indica cuánto avanzar o retroceder.
        float adelante = 0f;

        // W aumenta el movimiento hacia delante.
        if (Input.GetKey(KeyCode.W))
            adelante += 1f;

        // S permite retroceder.
        if (Input.GetKey(KeyCode.S))
            adelante -= 1f;

        // Calcula la dirección horizontal según la orientación del personaje.
        Vector3 direccion = transformPersonaje.forward * adelante;

        // Evita que la magnitud de la dirección horizontal supere 1.
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        // Comprueba si el CharacterController está tocando el suelo.
        if (characterController.isGrounded)
        {
            // Mantiene una pequeña velocidad hacia abajo para ayudar a detectar el suelo.
            if (velocidadVertical < 0f)
                velocidadVertical = -2f;

            // Solo permite saltar cuando el personaje está en el suelo.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Calcula la velocidad inicial necesaria para alcanzar la altura de salto configurada.
                velocidadVertical = Mathf.Sqrt(
                    alturaSalto * -2f * gravedad
                );
            }
        }

        // Aplica la gravedad continuamente para que el personaje suba y después caiga.
        velocidadVertical += gravedad * Time.deltaTime;

        // Calcula el desplazamiento horizontal según la dirección y velocidad configuradas.
        Vector3 movimiento = direccion * velocidadMovimiento;

        // Añade la velocidad vertical para combinar el desplazamiento con el salto.
        movimiento.y = velocidadVertical;

        // Mueve al personaje teniendo en cuenta el tiempo transcurrido.
        // CharacterController.Move permite controlar conjuntamente el salto y la gravedad.
        characterController.Move(movimiento * Time.deltaTime);
    }

    private void ActualizarCamaraOrbital()
    {
        // Mantiene el cursor visible para permitir el uso de botones y otros elementos de la interfaz.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // La cámara orbital solo gira mientras se mantiene presionado el botón derecho del mouse.
        // Así se puede mover el cursor normalmente cuando no se está girando la cámara.
        if (Input.GetMouseButton(1))
        {
            // El movimiento horizontal del mouse cambia el ángulo alrededor del personaje.
            anguloOrbitalHorizontal +=
                Input.GetAxis("Mouse X") * sensibilidadMouse;

            // El movimiento vertical del mouse cambia la inclinación de la cámara.
            anguloOrbitalVertical -=
                Input.GetAxis("Mouse Y") * sensibilidadMouse;

            // Limita la inclinación para evitar que la cámara dé vueltas verticales completas.
            anguloOrbitalVertical = Mathf.Clamp(
                anguloOrbitalVertical,
                anguloVerticalMinimo,
                anguloVerticalMaximo
            );
        }
    }

    private void ColocarCamaraOrbital()
    {
        // Sitúa el punto de observación a la altura de los ojos
        // y un poco delante del personaje, para que el modelo no tape tanto la vista.
        Vector3 puntoObjetivo = transformPersonaje.position
            + Vector3.up * alturaObjetivoOrbital
            + transformPersonaje.forward * 0.5f;

        // Calcula la orientación de la cámara a partir de los ángulos controlados por el mouse.
        Quaternion rotacionOrbital = Quaternion.Euler(
            anguloOrbitalVertical,
            anguloOrbitalHorizontal,
            0f
        );

        // Calcula la posición de la cámara a la distancia configurada respecto al punto observado.
        Vector3 desplazamiento = rotacionOrbital * Vector3.back * distanciaOrbital;

        // Asigna la posición calculada a la cámara orbital.
        camaraOrbital.transform.position = puntoObjetivo + desplazamiento;

        // Hace que la cámara mire hacia el personaje sin modificar la rotación del personaje.
        camaraOrbital.transform.LookAt(puntoObjetivo);
    }

    private void CambiarCamara(int nuevoModo)
    {
        // Guarda el índice de la cámara seleccionada.
        modoCamara = nuevoModo;

        // Actualiza la variable que indica si se está usando la cámara en primera persona.
        primeraPersona = (modoCamara == 1);

        // Activa el objeto de la cámara seleccionada y desactiva las otras dos.
        camaraTerceraPersona.gameObject.SetActive(modoCamara == 0);
        camaraPrimeraPersona.gameObject.SetActive(modoCamara == 1);
        camaraOrbital.gameObject.SetActive(modoCamara == 2);

        // Habilita únicamente el componente Camera del modo seleccionado.
        camaraTerceraPersona.enabled = (modoCamara == 0);
        camaraPrimeraPersona.enabled = (modoCamara == 1);
        camaraOrbital.enabled = (modoCamara == 2);

        // Obtiene el AudioListener de cada cámara, si existe.
        AudioListener audioTercera =
            camaraTerceraPersona.GetComponent<AudioListener>();

        AudioListener audioPrimera =
            camaraPrimeraPersona.GetComponent<AudioListener>();

        AudioListener audioOrbital =
            camaraOrbital.GetComponent<AudioListener>();

        // Mantiene activo solo el AudioListener de la cámara seleccionada.
        if (audioTercera != null)
            audioTercera.enabled = (modoCamara == 0);

        if (audioPrimera != null)
            audioPrimera.enabled = (modoCamara == 1);

        if (audioOrbital != null)
            audioOrbital.enabled = (modoCamara == 2);

        // Mantiene el cursor visible y libre para interactuar con la interfaz.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Al activar la cámara orbital, inicializa su ángulo horizontal detrás del personaje.
        // La orientación del personaje no se modifica al mover la cámara con el mouse.
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

        // Desactiva la cámara de los personajes que no pertenecen al jugador local.
        camara.enabled = false;

        // Desactiva también su AudioListener para evitar conflictos de audio.
        AudioListener audio =
            camara.GetComponent<AudioListener>();

        if (audio != null)
            audio.enabled = false;

        // Desactiva el objeto de la cámara.
        camara.gameObject.SetActive(false);
    }
}
