using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameManagerObject = new GameObject("Game Manager");
                _instance = gameManagerObject.AddComponent<GameManager>();
                DontDestroyOnLoad(gameManagerObject);
            }
            return _instance;
        }
    }

    [SerializeField] private MarkerSelection _xSelection;
    [SerializeField] private MarkerSelection _oSelection;

    private int _rows;
    private int _turn;
    private int _match = 3;
    private bool _gameEnd = false;
    private bool _devMode = true;
    private Board _board;
    private Dictionary<string, HitBox> _fields = new Dictionary<string, HitBox>();
    private List<HitBox> _matchedPattern = new List<HitBox>();
    private TicTacToeAI _ticTacToeAIPlayer1;
    private TicTacToeAI _ticTacToeAIPlayer2;
    private bool _isAiEnabled = false;
    private bool _isAivsAiEnabled = false;
    private bool canAIMakeMove = true;

    public int Turn => _turn % 2;
    public int Rows => _rows;
    public int Match => _match;
    public bool GameEnd => _gameEnd;

    public bool DevMode => _devMode;
    public Board Board => _board;
    public List<HitBox> Pattern => _matchedPattern;

    // Add a 2D array to store HitBoxes
    private HitBox[,] _hitBoxes;
    
    
    /*public Marker GetSelectedMarker => Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;*/
    public Marker GetSelectedMarker
    {
        get
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == "2dGame")
            {
                return Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;
            }
            else if (currentScene.name == "3dGame")
            {
                return Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;
            }

            // Standardfall, sollte nicht erreicht werden
            return null;
        }
    }

    private int _maxMoves => _rows * _rows;

    public event Action<bool, int> OnGameEnd;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _devMode = false;
        InitializeGame();
    }

    private void InitializeGame()
    {
        InitializeAI();

        _xSelection.SetTurn(0);
        _oSelection.SetTurn(1);
    }

    void Update()
    {

        // Check if AI vs AI is enabled
        if (_isAivsAiEnabled && !_gameEnd && !_isAiEnabled)
        {
            // Check if it's the AI Player 1's turn
            if (Turn == 0)
            {
                if (canAIMakeMove)
                {
                    StartCoroutine(DelayedAIMove(_ticTacToeAIPlayer1));
                    canAIMakeMove = false;
                }
            }
            // Check if it's the AI Player 2's turn
            else if (Turn == 1)
            {
                if (canAIMakeMove)
                {
                    StartCoroutine(DelayedAIMove(_ticTacToeAIPlayer2));
                    canAIMakeMove = false;
                }
            }
        }
        // Check if single AI is enabled
        else if (_isAiEnabled && Turn == 1 && !_gameEnd && !_isAivsAiEnabled)
        {
            if (canAIMakeMove)
            {
                StartCoroutine(DelayedAIMove(_ticTacToeAIPlayer2));
                canAIMakeMove = false;
            }
        }
    }

    private IEnumerator DelayedAIMove(TicTacToeAI aiPlayer)
    {
        // Wait for 1 seconds
        yield return new WaitForSeconds(1.0f);

        string boardState = GetBoardStateAsJson();
        aiPlayer.MakeAIMove(boardState);

        canAIMakeMove = true;
    }

    public void Set(int rows, int match = 3)
    {
        _rows = rows;
        _match = match;
    }

    public void AddHitBox(HitBox hitBox, int x, int y)
    {
        // store hit box with x y row as key
        _fields.Add($"{x},{y}", hitBox);

        if (x >= 0 && x < _hitBoxes.GetLength(0) && y >= 0 && y < _hitBoxes.GetLength(1))
        {
            _hitBoxes[x, y] = hitBox;
        }
        else
        {
            Debug.LogError($"Attempted to add HitBox outside the bounds of _hitBoxes. x: {x}, y: {y}");
        }

        hitBox.Initialize(this, _xSelection, _oSelection);
    }

    public HitBox GetHitBoxAt(int x, int y)
    {
        if (x >= 0 && x < _rows && y >= 0 && y < _rows)
        {
            return _hitBoxes[x, y];
        }
        else
        {
            Debug.LogError($"Invalid position: ({x}, {y})");
            return null;
        }
    }

    public void Clear()
    {
        _fields.Clear();
        _matchedPattern?.Clear();
        _gameEnd = false;
        _turn = 0;
        OnGameEnd?.Invoke(_gameEnd, -1);
    }

    public void MoveMade()
    {
        _turn++;
        Scene currentScene = SceneManager.GetActiveScene();

        _matchedPattern = CheckWin();
        if (_matchedPattern != null)
        {
            // WINNER
            _gameEnd = true;
            OnGameEnd?.Invoke(_gameEnd, _matchedPattern[0].Type);
        }
        else if (_turn >= _maxMoves && currentScene.name == "2dGame")
        {
            // TIE
            _gameEnd = true;
            OnGameEnd?.Invoke(_gameEnd, -1);
        }

        _xSelection.UpdateMarkers();
        _oSelection.UpdateMarkers();

        GetBoardStateAsJson();
    }

    public List<HitBox> CheckWin()
    {
        return PatternFinder.CheckWin(_fields);
    }
    public bool ToggleDevMode()
    {
        _devMode = !_devMode;
        return _devMode;
    }
    public void SetBoard(Board board)
    {
        _board = board;

        // 3 rows and columns
        _hitBoxes = new HitBox[3, 3];
    }

    public void Reset()
    {
        _board.Reset();
        _xSelection.Reset();
        _oSelection.Reset();
        InitializeAI();

    }

    private void InitializeAI()
    {
        // Initialize marker pool based on the game type
        List<int> markerPool = new List<int>();

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "2dGame")
        {
            markerPool = new List<int> { 0, 0, 0, 0, 0, 0 };
        }
        else if (currentScene.name == "3dGame")
        {
            markerPool = new List<int> { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        }

        // Reinitialize TicTacToeAI instances with the correct marker pool
        _ticTacToeAIPlayer1 = new TicTacToeAI(markerPool, 0);
        _ticTacToeAIPlayer2 = new TicTacToeAI(markerPool, 1);

        canAIMakeMove = true;
    }

    public string GetBoardStateAsJson()
    {
        if (_board != null)
        {
            List<List<Dictionary<string, object>>> boardState = new List<List<Dictionary<string, object>>>();

            for (int i = 0; i < 3; i++) // Assuming 3 rows
            {
                List<Dictionary<string, object>> row = new List<Dictionary<string, object>>();

                for (int j = 0; j < 3; j++) // Assuming 3 columns
                {
                    HitBox hitBox;
                    if (_fields.TryGetValue($"{i},{j}", out hitBox))
                    {
                        Dictionary<string, object> cell = new Dictionary<string, object>
                    {
                        { "type", hitBox.Type == 0 ? 0 : hitBox.Type == 1 ? 1 : "-1" },
                        { "size", hitBox.GetMarkerSize()}
                    };
                        row.Add(cell);
                    }
                    else
                    {
                        row.Add(new Dictionary<string, object> { { "type", " " }, { "size", 0 } });
                    }
                }
                boardState.Add(row);
            }

            // Convert the list to JSON using Newtonsoft.Json
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { board = boardState });

            //Debug.Log(json);

            return json;
        }

        return null;
    }

    public void ToggleAI(bool enableAI)
    {
        _isAiEnabled = enableAI;
    }

    public void ToggleAIvsAi(bool enableAI)
    {
        _isAivsAiEnabled = enableAI;
    }
}