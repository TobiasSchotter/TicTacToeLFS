using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;

    private int _type = -1;
    public int Type => _type;

    private bool _markerPlaced;
    private readonly List<Marker> _markers = new List<Marker>();
    private Marker CurrentMarker => _markers.Count > 0 ? _markers.Last() : null;
    private GameManager _gameManager;

    private MarkerSelection _xSelection;
    private MarkerSelection _oSelection;

    private Marker _lastMarker;

    private void Start()
    {
        _renderer.enabled = false;
    }

    public void Initialize(GameManager gameManager, MarkerSelection xSelection, MarkerSelection oSelection)
    {
        _gameManager = gameManager;
        _xSelection = xSelection;
        _oSelection = oSelection;
    }


    private bool CheckAvailableToTrigger()
    {
        if (GameManager.Instance.GameEnd || GameManager.Instance.DevMode ||
            GameManager.Instance.GetSelectedMarker == null)
        {
            return false;
        }

        if (_markerPlaced && GameManager.Instance.GetSelectedMarker != null && CurrentMarker != null)
        {
            return GameManager.Instance.GetSelectedMarker.Size > CurrentMarker.Size;
        }

        return true;
    }

    private void OnMouseOver()
    {
        if (!CheckAvailableToTrigger())
        {
            return;
        }

        _renderer.enabled = true;
    }

    private void OnMouseExit()
    {
        _renderer.enabled = false;
    }


    private void MakeMove(Marker marker, Vector3 position)
    {
        if (CurrentMarker != null)
        {
            CurrentMarker.OverRuled(true);

            CurrentMarker.Remove();
        }

        marker.SetPosition(position, transform, this);
        _markers.Add(marker);

        _renderer.enabled = false;
        _markerPlaced = true;
        marker.SetIsPlaced(true);
        _type = GameManager.Instance.Turn;
        _lastMarker = marker;


        if (_type == 0)
        {
            _xSelection.MarkersPoolX.Remove(marker);
        }
        else if (_type == 1)
        {
            _oSelection.MarkersPoolO.Remove(marker);
        }
        GameManager.Instance.MoveMade();

    }

    public void UndoLastMove()
    {
        Debug.Log("undo this");
        if (_lastMarker != null)
        {

            // Add the marker back to the pool
            if (_type == 0)
            {
                _xSelection.MarkersPoolX.Add(_lastMarker);
            }
            else if (_type == 1)
            {
                _oSelection.MarkersPoolO.Add(_lastMarker);
            }

            _lastMarker.Remove();
            _markers.Remove(_lastMarker);
            _lastMarker.SetIsPlaced(false);
            _lastMarker = null;
            _type = -1; // Reset the type
            _renderer.enabled = false;
            _oSelection.UpdateMarkers();
            _oSelection.Reset();
        }
    }

    private void OnMouseUpAsButton()
    {
        if (!CheckAvailableToTrigger())
        {
            return;
        }

        var marker = GameManager.Instance.GetSelectedMarker;
        if (marker == null)
        {
            return;
        }

        MakeMove(marker, transform.position);
    }

    public void MakeMoveFromJson(string jsonMove)
    {
        try
        {
            // Parse JSON and create a Marker instance
            Marker marker = CreateMarkerFromJson(jsonMove);

            if (marker == null)
            {
                Debug.LogError($"Error: No marker of that size and type left");
                return;
            }

            // Parse JSON data
            JObject jsonData = JObject.Parse(jsonMove);

            // Ensure the required fields are present in the JSON
            if (!jsonData.TryGetValue("row", out var rowToken) || !jsonData.TryGetValue("column", out var columnToken) ||
                !jsonData.TryGetValue("type", out var typeToken))
            {
                Debug.Log($"Error: Missing required fields in JSON");
                return;
            }

            // Convert tokens to appropriate types
            int row = rowToken.Value<int>();
            int column = columnToken.Value<int>();
            int type = typeToken.Value<int>();

            // Calculate the desired position for the marker based on row and column
            Vector3 desiredPosition = CalculateGridPosition(row, column);

            if (GameManager.Instance.Turn != type)
            {
                Debug.Log($"Error: Other player's turn to make a move");
                return;
            }

            if (_markerPlaced && CurrentMarker != null && ((marker.Size <= CurrentMarker.Size) || (CurrentMarker.Type == marker.Type)))
            {
                Debug.Log($"Error: Not a valid move!");
                return;
            }

            MakeMove(marker, desiredPosition);
        }
        catch (JsonException jsonEx)
        {
            Debug.LogError($"Error parsing JSON move: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"An unexpected error occurred: {ex.Message}");
        }
    }

    private Marker CreateMarkerFromJson(string jsonMove)
    {
        try
        {
            // Parse JSON data
            JObject jsonData = JObject.Parse(jsonMove);

            List<Marker> currentMarkerPool = new List<Marker>();

            // Extract relevant information
            int size = (int)jsonData["size"];  // Example: 0, 1, 2 for marker sizes
            int type = (int)jsonData["type"];  // Example: 0 for X, 1 for O

            // 0 == X and 1 == O
            if (type == 0)
            {
                currentMarkerPool = _xSelection.MarkersPoolX;
            }else if (type == 1)
            {
                currentMarkerPool = _oSelection.MarkersPoolO;
            }
            else
            {
                return null;
            }

            // Find the corresponding marker in existing pool
            Marker selectedMarker = FindMarkerInPool(type, size, currentMarkerPool);


            return selectedMarker;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing JSON move: {ex.Message}");
            return null;
        }
    }

    private Marker FindMarkerInPool(int type, int size, List<Marker> markerPool)
    {
        // Iterate through your existing pool of markers and find the matching one
        foreach (var marker in markerPool)
        {
            if (marker.Type == type && marker.Size == size && !marker.IsPlaced)
            {
                // If found, return the marker
                return marker;
            }
        }

        return null;
    }

    private Vector3 CalculateGridPosition(int row, int column)
    {
        // Assuming rows and columns are numbered from 0 to 2

        float cellSize = 3.4f; // Adjust this value based on actual cell size
        float offsetX = (column - 1) * cellSize;
        float offsetY = 4.58f; // Adjust this value based on the height offset
        float offsetZ = (-1*(row - 1)) * cellSize;

        return new Vector3(offsetX, offsetY, offsetZ);
    }

    public void RemoveMarker(Marker marker)
    {
        _markers.Remove(marker);

        if (CurrentMarker == null)
        {
            _type = -1;
            return;
        }

        _type = CurrentMarker.Type;
    }

    public int GetMarkerSize()
    {
        if (_markerPlaced)
        {
            return CurrentMarker.Size;
        }
        return -1;
    }
}