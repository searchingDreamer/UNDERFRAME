using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevConsole : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject consolePanel;       // Панель консоли (UI)
    public TMP_InputField inputField;     // Поле ввода команд (TextMeshPro)
    public TMP_Text outputText;            // Текст вывода лога (TextMeshPro)

    readonly Queue<string> logQueue = new();
    readonly int maxLogLines = 30;

    private bool consoleActive = false;

    // Здесь хранить ссылки на игровые объекты/параметры, которыми хотим управлять
    public Transform playerTransform;
    public GameObject prefabToSpawn;
    public GameObject upgradesPrefabToSpawn;
    private PlayerStats playerStats;
    private bool flyMode = false;

    void Start()
    {
        HideConsole();
        playerStats = playerTransform.GetComponent<PlayerStats>();
    }

    void Update()
    {
        // Toggle консоли по кнопке ~ (тильда)
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (consoleActive)
                HideConsole();
            else
                ShowConsole();
        }

        // Если консоль активна, слушаем Enter для отправки команды
        if (consoleActive && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            if (!string.IsNullOrEmpty(command))
            {
                AddLog("> " + command);
                ProcessCommand(command);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }

        // Пример: если полёт включен — даём возможность летать
        if (flyMode && playerTransform != null)
        {
            HandleFlyMode();
        }
    }

    void ShowConsole()
    {
        consolePanel.SetActive(true);
        inputField.ActivateInputField();
        consoleActive = true;
    }

    void HideConsole()
    {
        consolePanel.SetActive(false);
        consoleActive = false;
    }

    void AddLog(string message)
    {
        if (logQueue.Count >= maxLogLines)
            logQueue.Dequeue();

        logQueue.Enqueue(message);

        outputText.text = string.Join("\n", logQueue);
    }
    void ClearLog()
    {
        logQueue.Clear();
        outputText.text = "";
    }

    void ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "upgrades":
                if (upgradesPrefabToSpawn == null)
                {
                    AddLog("Error: upgradesPrefabToSpawn not assigned!");
                    break;
                }
                Instantiate(upgradesPrefabToSpawn, new Vector3(3, 2, 12), Quaternion.identity);
                AddLog("Spawned upgrades");
                break;

            case "spawn":
                // spawn [x] [y] [z]
                if (prefabToSpawn == null)
                {
                    AddLog("Error: prefabToSpawn not assigned!");
                    break;
                }
                Vector3 pos = Vector3.zero;
                if (parts.Length == 4)
                {
                    if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y) && float.TryParse(parts[3], out float z))
                        pos = new Vector3(x, y, z);
                    else
                    {
                        AddLog("Error: Invalid position parameters.");
                        break;
                    }
                }
                else if (playerTransform != null)
                {
                    pos = playerTransform.position + playerTransform.forward * 3f;
                }
                Instantiate(prefabToSpawn, pos, Quaternion.identity);
                AddLog("Spawned prefab at " + pos);
                break;

            case "fly":
                flyMode = !flyMode;
                AddLog("Fly mode " + (flyMode ? "enabled" : "disabled"));
                break;

            case "change":
                if (parts.Length != 3)
                {
                    AddLog("Usage: change [parameter] [value]\nList of supported variables: strength, pickuprange, holddistance, maxhealth, maxstamina, runspeed");
                    break;
                }

                string statName = parts[1].ToLower();
                if (!float.TryParse(parts[2], out float newValue))
                {
                    AddLog("Error: value must be a number");
                    break;
                }

                if (playerStats == null)
                {
                    AddLog("Error: PlayerStats not assigned.");
                    break;
                }

                bool success = playerStats.ApplyStatChange(statName, newValue);
                if (success)
                    AddLog($"Changed {statName} to {newValue}");
                else
                    AddLog($"Unknown parameter: {statName}");
                break;

            case "help":
                AddLog("Commands:\nspawn [x y z] - Spawn prefab\nfly - Toggle fly mode\nhelp - Show commands\nchange [parameter] [value] - Change player stats\nupgrades - Spawn upgrades");
                break;

            case "clear":
                ClearLog();
                break;
            // Добавь свои команды ниже
            default:
                AddLog("Unknown command: " + cmd);
                break;
        }
    }

    void HandleFlyMode()
    {
        float speed = 10f;
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += playerTransform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= playerTransform.forward;
        if (Input.GetKey(KeyCode.A)) dir -= playerTransform.right;
        if (Input.GetKey(KeyCode.D)) dir += playerTransform.right;
        if (Input.GetKey(KeyCode.Space)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) dir -= Vector3.up;

        playerTransform.position += speed * Time.deltaTime * dir.normalized;
    }
}