using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Utils
{
    /// <summary>
    /// Geliştirici konsolu: Editörde ve development build'de komut çalıştırır.
    /// Örnek: DebugConsole.Register("addcoins", args => EconomyManager.Instance.AddCoins(float.Parse(args[0])));
    /// </summary>
    public class DebugConsole : Singleton<DebugConsole>
    {
        private readonly Dictionary<string, Action<string[]>> commands = new();
        private readonly List<string> log = new();
        private bool visible;

        public static void Register(string name, Action<string[]> action)
            => Instance.commands[name.ToLowerInvariant()] = action;

        public void Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            var parts = input.Split(' ');
            var name = parts[0].ToLowerInvariant();
            var args = new string[Mathf.Max(0, parts.Length - 1)];
            if (parts.Length > 1) Array.Copy(parts, 1, args, 0, args.Length);

            if (commands.TryGetValue(name, out var action))
            {
                try { action(args); Log($"> {input}"); }
                catch (Exception e) { Log($"! {e.Message}"); }
            }
            else Log($"! Bilinmeyen komut: {name}");
        }

        public void Log(string message)
        {
            log.Add(message);
            if (log.Count > 200) log.RemoveAt(0);
            Debug.Log($"[Console] {message}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote)) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible || !Debug.isDebugBuild) return;
            GUI.Box(new Rect(10, 10, 460, 300), "Burger Rush — Debug Console");
            GUILayout.BeginArea(new Rect(20, 40, 440, 260));
            foreach (var line in log) GUILayout.Label(line);
            GUILayout.EndArea();
        }
    }
}
