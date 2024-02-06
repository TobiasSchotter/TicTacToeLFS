using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CommandInput : MonoBehaviour
{
    private string input;

    public TMP_InputField inputField;

    public void ReadStringInput(string s)
    {
        input = s;
        HandleCommandInput(input);
        ClearInputField();
    }

    private void ClearInputField()
    {
        if (inputField != null)
        {
            inputField.text = string.Empty;
        }
    }

    private void HandleCommandInput(string input)
    {
        if (input.StartsWith("Command:"))
        {
            // Extract and parse JSON data
            string jsonData = input.Substring("Command:".Length);
            ExecuteCommand(jsonData);
        }
    }

    private void ExecuteCommand(string command)
    {

        JObject jsonData = JObject.Parse(command);
        int row = (int)jsonData["row"];
        int column = (int)jsonData["column"];
        HitBox hitBox = GameManager.Instance.GetHitBoxAt(row, column);

        if (hitBox != null)
        {
            hitBox.MakeMoveFromJson(command);

        }
        else
        {
            Debug.LogError($"HitBox at row {row} and column {column} not found.");
        }
    }
}
