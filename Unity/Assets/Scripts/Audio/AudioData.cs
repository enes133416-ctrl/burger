using System;
using UnityEngine;

namespace BurgerRush.Audio
{
    public enum MusicTrack { MainMenu, Gameplay, RushHour, LevelComplete, Seasonal }

    /// <summary>Ses varlıklarının tek yerde toplandığı ScriptableObject (GDD 9.3).</summary>
    [CreateAssetMenu(menuName = "Burger Rush/Audio Data", fileName = "AudioData")]
    public class AudioData : ScriptableObject
    {
        [Header("Music")]
        public AudioClip menuMusic;
        public AudioClip gameplayMusic;
        public AudioClip rushMusic;
        public AudioClip victoryMusic;

        [Header("SFX")]
        public AudioClip cashRegister;   // Ching!
        public AudioClip ovenDone;       // Ding!
        public AudioClip burgerStack;    // Thwack!
        public AudioClip customerHappy;  // Mmm~
        public AudioClip customerAngry;  // Hmph!
        public AudioClip burn;           // Sizzle + Alarm
        public AudioClip driveThruBell;  // Beep!
        public AudioClip upgrade;        // Power up!
        public AudioClip starWin;        // Sparkle!
        public AudioClip almostReady;

        public AudioClip MusicFor(MusicTrack track) => track switch
        {
            MusicTrack.MainMenu => menuMusic,
            MusicTrack.Gameplay => gameplayMusic,
            MusicTrack.RushHour => rushMusic,
            MusicTrack.LevelComplete => victoryMusic,
            MusicTrack.Seasonal => menuMusic,
            _ => menuMusic,
        };

        public AudioClip SfxFor(string key) => key switch
        {
            "Coin" => cashRegister,
            "OvenDone" => ovenDone,
            "AlmostReady" => almostReady,
            "Thwack" => burgerStack,
            "CustomerHappy" => customerHappy,
            "CustomerAngry" => customerAngry,
            "Angry" => customerAngry,
            "Burn" => burn,
            "DriveThruBell" => driveThruBell,
            "PowerUp" => upgrade,
            "StarWin" => starWin,
            "Sizzle" => burn,
            "GrillPlace" => burgerStack,
            _ => null,
        };
    }
}
