
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [Header("Configuración del jugador")]
    [SerializeField] private string nombreEscenaMuseo = "Museo Si Si Colombia";
    [SerializeField] private string nombrePrefabJugador = "Jugador";
    [SerializeField] private int maximoJugadores = 10;

    private static PhotonLauncher instancia;

    // Guarda la referencia al jugador local creado mediante Photon.
    private GameObject jugadorLocal;

    // Indica si el jugador local terminó de inicializar su controlador y cámaras.
    public bool JugadorLocalListo
    {
        get
        {
            if (jugadorLocal == null)
                return false;

            MovimientoPersonaje movimiento =
                jugadorLocal.GetComponent<MovimientoPersonaje>();

            return movimiento != null && movimiento.EstaListo;
        }
    }

    private void Awake()
    {
        // Evita que existan dos launchers al cambiar entre escenas.
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;

        // Conserva el launcher entre escenas.
        DontDestroyOnLoad(gameObject);
    }

    public override void OnEnable()
    {
        // Activa los callbacks de Photon.
        base.OnEnable();

        // Evita registrar dos veces el evento de carga de escenas.
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    public override void OnDisable()
    {
        // Libera los callbacks heredados de Photon.
        base.OnDisable();

        // Deja de escuchar las cargas de escenas.
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void Start()
    {
        // Recupera el nombre guardado durante el inicio de sesión.
        string playerName = PlayerPrefs.GetString("PlayerName", "");

        // No conecta si todavía no hay un nombre válido.
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.Log("PhotonLauncher: esperando el nombre del usuario.");
            return;
        }

        PhotonNetwork.NickName = playerName;

        // Inicia la conexión si Photon aún no está conectado.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Conectando a Photon con nombre: " + playerName);
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.InRoom)
        {
            IntentarCrearJugador();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnConnectedToMaster()
    {
        // Al conectarse al servidor, entra al lobby.
        Debug.Log("Conectado al servidor Photon.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // Entra o crea la sala compartida del museo.
        Debug.Log("Unido al Lobby.");

        PhotonNetwork.JoinOrCreateRoom(
            "SalaGlobal",
            new RoomOptions
            {
                MaxPlayers = maximoJugadores,
                IsVisible = true,
                IsOpen = true
            },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        // Confirma que Photon permite crear jugadores en la sala.
        Debug.Log("Entró a la sala: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Jugadores en la sala: " +
                  PhotonNetwork.CurrentRoom.PlayerCount);

        // Solo crea el jugador si la escena del museo está activa.
        IntentarCrearJugador();
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Registra el nombre exacto que Unity comunica al cargar la escena.
        Debug.Log("PhotonLauncher: escena cargada = [" + escena.name + "]");

        // Comprueba si la escena corresponde al museo.
        if (!EsEscenaMuseo(escena.name))
        {
            Debug.Log("PhotonLauncher: esta no es la escena del museo.");
            return;
        }

        Debug.Log("PhotonLauncher: escena del museo detectada.");

        // Intenta crear al jugador cuando la escena ya está cargada.
        IntentarCrearJugador();
    }

    private bool EsEscenaMuseo(string nombreEscena)
    {
        // Normaliza el nombre para ignorar espacios y diferencias de mayúsculas.
        string nombreRecibido = NormalizarNombre(nombreEscena);
        string nombreConfigurado = NormalizarNombre(nombreEscenaMuseo);

        // También acepta las variantes de nombre que aparecieron en los registros.
        return nombreRecibido == nombreConfigurado
            || nombreRecibido == "museosisicolombia"
            || nombreRecibido == "museosísicolombia";
    }

    private string NormalizarNombre(string nombre)
    {
        // Elimina espacios y compara sin distinguir mayúsculas.
        return nombre.Replace(" ", "").Trim().ToLowerInvariant();
    }

    private void IntentarCrearJugador()
    {
        // No crea jugadores mientras esté activo el menú principal.
        string escenaActiva = SceneManager.GetActiveScene().name;

        if (!EsEscenaMuseo(escenaActiva))
        {
            Debug.Log(
                "PhotonLauncher: no se crea el jugador. Escena activa: [" +
                escenaActiva + "]"
            );
            return;
        }

        // Photon debe estar conectado y dentro de una sala.
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("PhotonLauncher: esperando entrar a una sala.");
            return;
        }

        // Evita crear dos jugadores locales.
        if (jugadorLocal != null)
        {
            Debug.Log("PhotonLauncher: el jugador local ya existe.");
            return;
        }

        // Comprueba que el prefab exista en una carpeta Resources.
        if (Resources.Load<GameObject>(nombrePrefabJugador) == null)
        {
            Debug.LogError(
                "No se encuentra el prefab '" + nombrePrefabJugador +
                "'. Debe estar en Assets/Resources/" +
                nombrePrefabJugador + ".prefab"
            );
            return;
        }

        // Genera una posición inicial dentro del museo.
        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(-5f, 5f),
            1f,
            UnityEngine.Random.Range(-5f, 5f)
        );

        Debug.Log(
            "PhotonLauncher: intentando instanciar '" +
            nombrePrefabJugador + "'."
        );

        // Crea el jugador mediante Photon para sincronizarlo en la sala.
        jugadorLocal = PhotonNetwork.Instantiate(
            nombrePrefabJugador,
            spawnPosition,
            Quaternion.identity
        );

        // Comprueba que Photon haya devuelto un objeto válido.
        if (jugadorLocal == null)
        {
            Debug.LogError("Photon no devolvió una referencia válida al jugador.");
            return;
        }

        Debug.Log("Jugador instanciado: " + jugadorLocal.name);

        // Comprueba que el prefab tenga PhotonView en su objeto raíz.
        PhotonView vistaPhoton = jugadorLocal.GetComponent<PhotonView>();

        if (vistaPhoton == null)
        {
            Debug.LogError(
                "El prefab Jugador no tiene PhotonView en su objeto raíz.",
                jugadorLocal
            );
        }
        else
        {
            Debug.Log(
                "PhotonView encontrado. ¿Es el jugador local?: " +
                vistaPhoton.IsMine,
                jugadorLocal
            );
        }

        // Comprueba que exista el controlador de movimiento y cámaras.
        MovimientoPersonaje movimiento =
            jugadorLocal.GetComponent<MovimientoPersonaje>();

        if (movimiento == null)
        {
            Debug.LogError(
                "El prefab Jugador no tiene MovimientoPersonaje en su objeto raíz.",
                jugadorLocal
            );
        }
        else
        {
            // Start() del controlador inicializará las cámaras.
            Debug.Log(
                "MovimientoPersonaje encontrado. Esperando inicialización de cámaras.",
                jugadorLocal
            );
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // Informa si Photon no pudo entrar a la sala.
        Debug.LogError(
            "Error al unirse a la sala. Código: " +
            returnCode + " | Detalle: " + message
        );
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Informa si se pierde la conexión con Photon.
        Debug.LogWarning("Desconectado de Photon: " + cause);

        // Limpia la referencia para permitir una nueva instancia.
        jugadorLocal = null;
    }

    public void ConectarConNombre(string nombre)
    {
        // Impide iniciar la conexión con un nombre vacío.
        if (string.IsNullOrWhiteSpace(nombre))
        {
            Debug.LogWarning("No se puede conectar a Photon sin un nombre.");
            return;
        }

        // Guarda el nombre en Photon y en las preferencias locales.
        PhotonNetwork.NickName = nombre;
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.Save();

        // Conecta o continúa el flujo de conexión existente.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Conectando a Photon con nombre: " + nombre);
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.InRoom)
        {
            IntentarCrearJugador();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
        }
    }
}
