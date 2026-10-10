using System.Collections.Generic;
using UnityEngine;

namespace BurgerRush.Multiplayer
{
    /// <summary>
    /// Co-op oturum: iki oyuncu aynı mutfakta, görev paylaşımı (GDD 3.1 Level 16).
    /// </summary>
    public class CoopSessionManager : Utils.Singleton<CoopSessionManager>
    {
        [SerializeField] private int maxPlayers = 2;

        private readonly Dictionary<string, PlayerSync> players = new();

        public int PlayerCount => players.Count;
        public bool IsFull => PlayerCount >= maxPlayers;

        public void RegisterPlayer(PlayerSync player)
        {
            if (!player || IsFull) return;
            players[player.PlayerId] = player;
        }

        public void UnregisterPlayer(string playerId) => players.Remove(playerId);

        /// <summary>Boştaki oyuncuya iş atar (basit round-robin).</summary>
        public PlayerSync AssignTask()
        {
            foreach (var player in players.Values)
                if (player && !player.IsBusy) return player;
            return null;
        }

        public void EndSession() => players.Clear();
    }
}
