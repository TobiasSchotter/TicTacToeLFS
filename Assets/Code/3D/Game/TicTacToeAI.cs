using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class TicTacToeAI : MonoBehaviour
{
    private const int PLAYER_1 = 0;
    private const int PLAYER_2 = 1;
    private const int EMPTY = -1;
    private int playerType;

    private List<int> markerPool;

    public TicTacToeAI(List<int> markerPool, int type)
    {
        InitializeMarkerPool(markerPool);
        playerType = type;
    }

    private void InitializeMarkerPool(List<int> markerPoolToSet)
    {
        markerPool = markerPoolToSet;
    }

    // Function to convert JSON data to a 2D array
    private int[,] ConvertJsonToBoard(string jsonData)
    {
        JObject json = JObject.Parse(jsonData);
        JArray boardArray = (JArray)json["board"];

        int[,] board = new int[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                board[i, j] = (int)boardArray[i][j]["type"];
            }
        }

        return board;
    }

    // Function to check if the board is full
    private bool IsBoardFull(int[,] board)
    {
        foreach (int cell in board)
        {
            if (cell == EMPTY)
            {
                return false;
            }
        }
        return true;
    }

    // Function to check if the current player wins
    private bool CheckWinner(int[,] board, int player)
    {
        // Check rows and columns
        for (int i = 0; i < 3; i++)
        {
            if ((board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) ||
                (board[0, i] == player && board[1, i] == player && board[2, i] == player))
            {
                return true;
            }
        }

        // Check diagonals
        if ((board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) ||
            (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player))
        {
            return true;
        }

        return false;
    }

    // Minimax algorithm to find the best move
    private int MiniMax(int[,] board, int depth, bool isMaximizing)
    {
        int result = Evaluate(board);

        if (result != 0)
        {
            return result;
        }

        if (IsBoardFull(board))
        {
            return 0;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == EMPTY)
                    {
                        board[i, j] = PLAYER_2;
                        int score = MiniMax(board, depth + 1, false);
                        board[i, j] = EMPTY;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == EMPTY)
                    {
                        board[i, j] = PLAYER_1;
                        int score = MiniMax(board, depth + 1, true);
                        board[i, j] = EMPTY;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
    }

    // Evaluate the board state
    private int Evaluate(int[,] board)
    {
        if (CheckWinner(board, PLAYER_1))
        {
            return -1;
        }
        else if (CheckWinner(board, PLAYER_2))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    // Function to find the best move using the Minimax algorithm
    public Vector2Int FindBestMove(string jsonData)
    {
        int[,] board = ConvertJsonToBoard(jsonData);

        int bestMoveScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == EMPTY)
                {
                    board[i, j] = PLAYER_2;
                    int moveScore = MiniMax(board, 0, false);
                    board[i, j] = EMPTY;

                    if (moveScore > bestMoveScore)
                    {
                        bestMoveScore = moveScore;
                        bestMove = new Vector2Int(i, j);
                    }
                }
            }
        }

        return bestMove;
    }

    // CALL ON AI TURN
    public void MakeAIMove(string jsonBoard)
    {
        // Find the best move using the Minimax algorithm
        Vector2Int bestMove = FindBestMove(jsonBoard);

        // Make the move
        int row = bestMove.x;
        int column = bestMove.y;

        int randomIndex = new System.Random().Next(0, markerPool.Count);
        int size = markerPool[randomIndex];


        int type = playerType;

        var commandObject = new
        {
            row,
            column,
            type,
            size
        };

        // Convert the object to a JSON string
        string command = JsonConvert.SerializeObject(commandObject);

        HitBox hitBox = GameManager.Instance.GetHitBoxAt(row, column);

        if (hitBox != null)
        {
            hitBox.MakeMoveFromJson(command);
            markerPool.RemoveAt(randomIndex);

        }
        else
        {
            Debug.LogError($"HitBox at row {row} and column {column} not found.");
        }
    }
}
