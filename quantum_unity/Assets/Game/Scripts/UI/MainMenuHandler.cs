using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Quantum;
using Quantum.Demo;

public class MainMenuHandler : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IOnEventCallback
{
    [Header("Connection Handler")]
    [SerializeField] private ConnectionHandler _connectionHandler;


    [Header("Panels")]

    [SerializeField] private Canvas _mainMenuCanvas;

    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private CanvasGroup _playerPanel;
    [SerializeField] private CanvasGroup _roomPanel;

    [Header("Players details")]

    [SerializeField] private TMP_InputField _playerNameInputField;

    [Header("Room details")]

    [SerializeField] private VerticalLayoutGroup _roomMemberListLayoutGroup;

    [SerializeField] private VerticalLayoutGroup _sessionListLayoutGroup;

    [SerializeField] private GameObject _roomNemberNamePrefab;

    private QuantumLoadBalancingClient _client;

    enum PhotonEventCode : byte
    {
        StartGame = 110
    }

    private string _playerName = "";

    private long _mapGuild = 0L;

    private bool _isQuickPlayAutoJoinInEnable = false;

    private void Start()
    {
        
    }


    private void Update()
    {
        _client?.Service();
    }

    #region Multiplayer connet and join

    private bool ConnetToMaster()
    {
        var appSettings = PhotonServerSettings.CloneAppSettings(PhotonServerSettings.Instance.AppSettings);

        _client = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol);

        _client.AddCallbackTarget(this);

        if (string.IsNullOrEmpty(appSettings.AppIdRealtime.Trim()))
        {
            Untils.DebugLogError("Missing Quantum AppID");
            return false;
        }

        if (_playerName == "")
            _playerName = Untils.GetRandomName();
        
        if (!_client.ConnectUsingSettings(appSettings, _playerName))
        {
            Untils.DebugLogError("Unable to issue Connect to Master command");
            return false;
        }

        Untils.DebugLog($"Attapting to connet to region {appSettings.FixedRegion}");

        return true;
    }

    private EnterRoomParams CreateEnterRoomParams(string roomName)
    {
        EnterRoomParams enterRoomParams = new EnterRoomParams();

        enterRoomParams.RoomOptions = new RoomOptions();
        enterRoomParams.RoomName = roomName;
        enterRoomParams.RoomOptions.IsVisible = true;
        enterRoomParams.RoomOptions.MaxPlayers = 0;

        enterRoomParams.RoomOptions.Plugins = new string[] { "QuantumPlugin" };
        enterRoomParams.RoomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            {"MAP-GUID", _mapGuild },
        };

        enterRoomParams.RoomOptions.PlayerTtl = PhotonServerSettings.Instance.PlayerTtlInSeconds * 1000;
        enterRoomParams.RoomOptions.EmptyRoomTtl = PhotonServerSettings.Instance.EmptyRoomTtlInSeconds * 1000;

        return enterRoomParams;
    }

    private void JoinRandomOrCreateRoom()
    {
        _connectionHandler.Client = _client;
        _connectionHandler.StartFallbackSendAckThread();

        var allMapInResources = UnityEngine.Resources.LoadAll<MapAsset>(QuantumEditorSettings.Instance.DatabasePathInResources);
        _mapGuild = allMapInResources[0].AssetObject.Guid.Value;

        Untils.DebugLog($"Using Map long GUID {allMapInResources[0].AssetObject.Guid}");

        EnterRoomParams enterRoomParams = CreateEnterRoomParams("");

        OpJoinRandomRoomParams joinRandomParams = new OpJoinRandomRoomParams();
        if(!_client.OpJoinRandomOrCreateRoom(joinRandomParams, enterRoomParams))
        {
            Untils.DebugLogError("Unable to join random room or Create room");
            return;
        }
        Untils.DebugLog("Join random room or Create room");

    }

    private void StartGame()
    {
        if(!_client.OpRaiseEvent((byte)PhotonEventCode.StartGame, null, new RaiseEventOptions{ Receivers = ReceiverGroup.All}, SendOptions.SendReliable))
        {
            Untils.DebugLogError("Unable to start game");
            return;
        }
        Untils.DebugLog("Start Game");
    }

    private void StartQuantumGame()
    {
        if(QuantumRunner.Default != null)
        {
            Untils.DebugLogWarning($"Another QuantumRunner '{QuantumRunner.Default.name}' has preveted starting the game");
            return;
        }

        RuntimeConfig runtimeConfig = new RuntimeConfig();

        runtimeConfig.Map.Id = _mapGuild;

        var param = new QuantumRunner.StartParameters
        {
            RuntimeConfig = runtimeConfig,
            DeterministicConfig = DeterministicSessionConfigAsset.Instance.Config,
            GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer,
            FrameData = null,
            InitialFrame = 0,
            PlayerCount = _client.CurrentRoom.MaxPlayers,
            LocalPlayerCount = 1,
            RecordingFlags = RecordingFlags.None,
            NetworkClient = _client,
            StartGameTimeoutInSeconds = 10.0f
        };

        var clientId = ClientIdProvider.CreateClientId(ClientIdProvider.Type.PhotonUserId, _client);

        Untils.DebugLog($"Starting QuantumRunner with clientID {clientId} and map guid {_mapGuild}. local player count {param.LocalPlayerCount}");

        _mainMenuCanvas.gameObject.SetActive( false );

        QuantumRunner.StartGame(clientId, param);
    }

    #endregion

    #region UI

    private void HideAllPanels()
    {
        _mainPanel.gameObject.SetActive(false);
        _roomPanel.gameObject.SetActive(false);
        _playerPanel.gameObject.SetActive(false);
    }

    private void UpdateRoomDatails()
    {
        if(!_client.InRoom)
        {
            Untils.DebugLogError("Client no longer in room, connet update room datails");
            ClearRoomNemberList();

            return;
        }
            
        ClearRoomNemberList();

        foreach(KeyValuePair<int, Player> player in _client.CurrentRoom.Players)
        {
            GameObject instantiatedObject = Instantiate(_roomNemberNamePrefab, _roomMemberListLayoutGroup.transform);
            instantiatedObject.GetComponent<TextMeshProUGUI>().text = player.Value.NickName;
        }
    }

    private void ClearRoomNemberList()
    {
        for(int i = _roomMemberListLayoutGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_roomMemberListLayoutGroup.transform.GetChild(i).gameObject);
        }
    }

    IEnumerator UpdateRoomDetailsCO()
    {
        while(_roomPanel.gameObject.activeInHierarchy)
        {
            UpdateRoomDatails();
            yield return new WaitForSeconds(1);
        }
    }

    #endregion

    #region UI Events

    public void OnPlayerDetailsSetClicked()
    {
        _playerName = _playerNameInputField.text;


        HideAllPanels();
        _mainPanel.gameObject.SetActive(true);
    }

    public void OnQuickPlayClicked()
    {
        HideAllPanels();
        _roomPanel.gameObject.SetActive(true);

        _isQuickPlayAutoJoinInEnable = true;

        if(_client != null && _client.IsConnected)
        {
            JoinRandomOrCreateRoom();
        }
        else
            ConnetToMaster();
    }

    public void OnStartGameClicked()
    {
        StartGame();
    }
    
    public void OnSessionBrowserClicked()
    {
        _isQuickPlayAutoJoinInEnable = false;
    }



    #endregion

    #region Unity Events
    #endregion

    #region Connection Events

    public void OnConnected()
    {
        Untils.DebugLog($"OnConnected UserID: {_client.UserId}");
    }

    public void OnConnectedToMaster()
    {
        Untils.DebugLog($"Connected to Master server in region: {_client.CloudRegion}");

        JoinRandomOrCreateRoom();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Untils.DebugLog($"OnDisConnected cause: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Untils.DebugLog($"OnRegionListReceived");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Untils.DebugLog($"OnCustomAuthhenticationResponde");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        Untils.DebugLog($"OnCustomAuthhenticationFailed");
    }



    #endregion

    #region Room Events

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Untils.DebugLog($"OnFriendListUpdate");
    }

    public void OnCreatedRoom()
    {
        Untils.DebugLog($"OnCreateRoom");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Untils.DebugLog($"OnCreateRoomFailed");
    }

    public void OnJoinedRoom()
    {
        Untils.DebugLog($"OnJoinRoom");

        StartCoroutine(UpdateRoomDetailsCO());
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Untils.DebugLog($"OmJoinRoomFailed return code {returnCode} message {message}");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Untils.DebugLog($"OmJoinRandomFailed return code {returnCode} message {message}");
    }

    public void OnLeftRoom()
    {
        Untils.DebugLog($"OnLeftRoom");
    }


    #endregion

    #region Photon Events

    public void OnEvent(EventData photonEvent)
    {
        Untils.DebugLog($"photonEvent received code {photonEvent.Code}. ");

        switch(photonEvent.Code)
        {
            case (byte)PhotonEventCode.StartGame:
                _client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out object mapGuidValue);

                if(mapGuidValue == null)
                {
                    Untils.DebugLogError("Failed to get map GUID, disconneting");
                    _client.Disconnect();

                    return;
                }

                if(_client.LocalPlayer.IsMasterClient)
                {
                    _client.CurrentRoom.IsVisible = false;
                    _client.CurrentRoom.IsOpen = false;
                }

                StartQuantumGame();

                break;
        }
    }

    #endregion

    #region Lobby Events
    #endregion


}
