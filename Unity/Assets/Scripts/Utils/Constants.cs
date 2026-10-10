namespace BurgerRush.Utils
{
    /// <summary>
    /// Proje genelinde kullanılan sabitler: etiketler, sahne adları, katmanlar, fiyatlar.
    /// </summary>
    public static class Constants
    {
        public static class Scenes
        {
            public const string Boot = "00_Boot";
            public const string MainMenu = "01_MainMenu";
            public const string WorldMap = "02_WorldMap";
            public const string HROffice = "10_HROffice";
            public const string UpgradeOffice = "11_UpgradeOffice";
            public const string Shop = "12_Shop";
            public const string Wardrobe = "13_Wardrobe";
            public const string Settings = "14_Settings";
            public const string LevelComplete = "15_LevelComplete";
            public const string LevelFail = "16_LevelFail";
            public const string CoopLobby = "17_CoopLobby";
            public const string GameplayCoop = "18_Gameplay_Coop";
            public const string SeasonalEvent = "19_SeasonalEvent";

            /// <summary>Lokasyon index'ine göre gameplay sahnesi (03 → Downtown …).</summary>
            public static string GameplayFor(int locationIndex)
            {
                string[] gameplay = {
                    "03_Gameplay_Downtown", "04_Gameplay_Beach", "05_Gameplay_Rooftop",
                    "06_Gameplay_Mountain", "07_Gameplay_Tokyo", "08_Gameplay_FoodTruck", "09_Gameplay_Space"
                };
                return gameplay[Mathf.Clamp(locationIndex, 0, gameplay.Length - 1)];
            }
        }

        public static class Tags
        {
            public const string Player = "Player";
            public const string Customer = "Customer";
            public const string Ingredient = "Ingredient";
            public const string Grill = "Grill";
            public const string AssemblyStation = "AssemblyStation";
            public const string Trash = "Trash";
            public const string ServicePoint = "ServicePoint";
            public const string DriveThruWindow = "DriveThruWindow";
        }

        public static class Pool
        {
            public const string Customer = "Customer";
            public const string Food = "Food";
            public const string CoinFx = "CoinFx";
            public const string SmokeFx = "SmokeFx";
        }

        public static class Economy
        {
            public const int StartCoins = 250;
            public const int StartGems = 30;
            public const int MaxOfflineHours = 8;
            public const int MaxOfflineHoursWithAd = 16;
            public const float OfflineNoStaff = 0.15f;
            public const float OfflineWithStaff = 0.45f;
            public const float OfflineFullUpgrade = 0.70f;
        }

        public static class Ads
        {
            public const int InterstitialFrequency = 4;   // her 4 levelda 1 (GDD 4.2)
            public const float DoubleIncomeSeconds = 180f;
        }

        public static class Gems
        {
            public const int ResetPatience = 5;
            public const int InstantCook = 10;
            public const int SkipLevel = 50;
            public const int ExtraStaffSlot = 30;
        }
    }
}
