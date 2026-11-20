using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Quantum;
using ExitGames.Client.Photon.StructWrapping;
using Quantum.Physics2D;
using UnityEngine.UI;

public class InGameUIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] TextMeshProUGUI playCdText;
    [SerializeField] TextMeshProUGUI gameStateText;
    [SerializeField] TextMeshProUGUI PlayerTxt;
    [SerializeField] TextMeshProUGUI _playerLevel;
    [SerializeField] TextMeshProUGUI _playerGold;
    [SerializeField] TextMeshProUGUI _playerExp;
    [SerializeField] Slider _playerExpBar;
    [SerializeField] Camera _mainCamera;
    [SerializeField] UnityEngine.LayerMask _GroundlayerMask;
    [SerializeField] UnityEngine.LayerMask _CharlayerMask;
    [SerializeField] Transform _cam;
    [SerializeField] Transform _camPos;
    private GameSessionHandler _BoardGameObject;
    private Transform _selectedCharacter;

    Vector3 _mousePosition;
    bool _isCharActive;
    int _selectedBoxIndex;
    int playerIndex;
    bool _isUnitSelected;

    bool _isMouseUp;
    bool _isMouseDown;

    QuantumGame _game;
    Frame _frame;

    GameSession _gameSession;

    IAction _selectedChar;
    GameObject _selectedUnit;

    private void OnEnable()
    {
        EventManager.CamPosAc += CamAc;
        EventManager.MousePosition3D += UpdateMousePosiition;
        EventManager.IsCharActive += IsCharActive;
        EventManager.BoardLoad += BoardLoad;
        EventManager.BoxIndex += IsBoxIndex;
    }

    private void OnDisable()
    {
        EventManager.CamPosAc = CamAc;
        EventManager.MousePosition3D -= UpdateMousePosiition;
        EventManager.IsCharActive -= IsCharActive;
        EventManager.BoardLoad -= BoardLoad;
        EventManager.BoxIndex -= IsBoxIndex;

        
    }

    // Start is called before the first frame update
    void Start()
    {
        _BoardGameObject = GameObject.FindObjectOfType<GameSessionHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        _game = QuantumRunner.Default.Game;
        _frame = _game.Frames.Verified;


        if (Untils.TryGetQuantumFrame(out Frame frame))
        {
            foreach (var player in frame.GetComponentIterator<playerLink>())
            {

                if (_game.PlayerIsLocal(player.Component.Player))
                {
                    _playerGold.text = player.Component.Gold.ToString();
                    _playerLevel.text = $"lvl {player.Component.Level}";
                    _playerExp.text = $"{player.Component.Exp}/{player.Component.MaxExp}";
                    _playerExpBar.maxValue = player.Component.MaxExp;
                    _playerExpBar.value = player.Component.Exp;
                    break;
                }
            }
            if (frame.TryGetSingletonEntityRef<GameSession>(out var entity) == false)
            {
                countDownText.text = "GameSession singleton Not Found";
                return;
            }

            var gameSession = frame.GetSingleton<GameSession>();

            int countDown = (int)gameSession.TimeUntilStat;
            int preprationCD = (int)gameSession.PrepartionCooldown;
            int playTime = (int)gameSession.GameTimeCooldown;

            _gameSession = gameSession;

            switch (gameSession.State)
            {
                case GameState.Countdown:
                    countDownText.text = $"{countDown}";
                    break;

                case GameState.Preparation:
                    if (countDown == 0)
                        countDownText.text = "GO!";
                    if (countDown < 0)
                    {
                        countDownText.text = "";
                    }

                    if(preprationCD > 0)
                    {
                        gameStateText.text = "Pre";
                        playCdText.text = preprationCD.ToString();
                    }

                    break;

                case GameState.Playing:

                    if (playTime > 0)
                    {
                        gameStateText.text = "Playing";
                        playCdText.text = playTime.ToString();
                    }

                    break;
                case GameState.GameOver:
                    countDownText.text = "GAME OVER";
                    break;
            }

        }

        BoardHandler board = null;

        if(_isUnitSelected)
        {

            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, _GroundlayerMask))
            {
                    Debug.LogError(hit.transform.name);

                if (hit.transform.TryGetComponent<BoardHandler>(out board))
                {
                    if (_isUnitSelected && !board.IsFull)
                    {

                        if(_gameSession.State == GameState.Playing)
                        {
                            if(!board.IsActiveChar)
                            {
                                _mousePosition = hit.transform.position;
                                //_isCharActive = board.IsActiveChar;
                                //_selectedBoxIndex = board.Index;
                            }
                        }

                        else if(_gameSession.State == GameState.Preparation)
                        {
                            _mousePosition = hit.transform.position;
                            //_isCharActive = board.IsActiveChar;
                            //_selectedBoxIndex = board.Index;
                        }
                    }
                }
            }
        }

        if(UnityEngine.Input.GetMouseButtonDown(0))
        {
            RaycastHit hit2;
            Ray ray2 = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray2, out hit2, 100, _CharlayerMask))
            {
               if(hit2.transform.parent.TryGetComponent<IAction>(out IAction action))
                {
                    if (!action.IsCharacterHasPlayer())
                        return;

                    Debug.LogError(hit2.transform.parent.name);

                    _isUnitSelected = true;
                    _selectedChar = action;
                    if(_gameSession.State == GameState.Preparation)
                    {
                        if(_selectedChar != null)
                        {
                            _selectedCharacter = hit2.transform;
                            _selectedUnit = hit2.transform.parent.gameObject;
                            CharacterSelectFunc();
                            _isMouseDown = true;
                        }

                    }

                    else if(_gameSession.State == GameState.Playing)
                    {
                        if(!_selectedChar.IsCharacterActive())
                        {
                            if (_selectedChar != null)
                            {
                                _selectedCharacter = hit2.transform;
                                _selectedUnit = hit2.transform.parent.gameObject;
                                CharacterSelectFunc();
                                _isMouseDown=true;
                            }
                        }
                    }


                }
                
            }
        }

        if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            _isMouseUp = true;
        }

        if(_isMouseUp)
        {
            _isMouseUp = false;
            if(_isMouseDown)
            {

            _isMouseDown = false;
            Up(board);
            }
        }


    }

    private void Up(BoardHandler board)
    {
        if (_selectedCharacter != null && board != null)
        {
            if (_gameSession.State == GameState.Playing)
            {
                if (!board.IsActiveChar)
                {
                    //_mousePosition = hit.transform.position;
                    _isCharActive = board.IsActiveChar;
                    _selectedBoxIndex = board.Index;

                    board.IsFull = true;

                    var sc = _selectedUnit.GetComponent<UnitViewHandler>();
                    sc.Squat.IsFull = false;
                    sc.Squat = board;
                }
            }
            else if (_gameSession.State == GameState.Preparation)
            {

                //_mousePosition = hit.transform.position;
                _isCharActive = board.IsActiveChar;
                _selectedBoxIndex = board.Index;

                board.IsFull = true;

                var sc = _selectedUnit.GetComponent<UnitViewHandler>();
                sc.Squat.IsFull = false;
                sc.Squat = board;
            }

        }
        _selectedCharacter.GetChild(1).gameObject.SetActive(false);
        _selectedCharacter.GetChild(0).gameObject.SetActive(true);
        Debug.LogError(111111);
        _selectedCharacter = null;
        _selectedUnit = null;
        _BoardGameObject.GetBoardObject.gameObject.SetActive(false);
        _selectedChar?.DeActive();
    }
    
    private void Down()
    {
        
    }

    private void CharacterSelectFunc()
    {
        _selectedCharacter.transform.GetChild(1).gameObject.SetActive(true);
        _selectedCharacter.transform.GetChild(0).gameObject.SetActive(false);

        _BoardGameObject.GetBoardObject.gameObject.SetActive(true);
        _selectedChar.Action();
    }
    

    private void CamAc()
    {
        _cam.position = _camPos.position;
        _cam.rotation = _camPos.rotation;
    }

    public void CharacterCreateUIButton(int index)
    {
        var game = QuantumRunner.Default.Game;
        var frame = _game.Frames.Verified;
        
        foreach (var player in frame.GetComponentIterator<playerLink>())
        {

            if (game.PlayerIsLocal(player.Component.Player))
            {
                playerIndex = player.Component.Player;
                var script = frame.Get<playerLink>(player.Entity);
                script.CharacterCreate = true;
                script.CharacterPrefabIndex = index;
                frame.Set(player.Entity, script);
                break;
            }
        }
        PlayerTxt.text = playerIndex.ToString();
       
        
        
    }

    private Vector3 UpdateMousePosiition()
    {
        return _mousePosition;
    }

    private bool IsCharActive()
    {
        return _isCharActive;
    }

    private int IsBoxIndex()
    {
        return _selectedBoxIndex;
    }

    private void BoardLoad(GameObject board)
    {
        //_BoardGameObject = board;
    }




    public void Exp()
    {
        var game = QuantumRunner.Default.Game;
        var frame = _game.Frames.Verified;

        var player = new EntityRef();
        foreach (var temp in frame.GetComponentIterator<playerLink>())
        {
            if (game.PlayerIsLocal(temp.Component.Player))
            {
                player = temp.Entity;
                break;
            }
        }
        var script = frame.Get<playerLink>(player);
        
        if(script.Gold >= 4)
        {
            var comand = new PlayerExpUpdate()
            {
                PlayerRef = player,
                Exp = 4
            };

            game.SendCommand(comand);
        }

    }




}
