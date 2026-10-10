# 🎬 Sahne Listesi (GDD Bölüm 7)

Bu klasördeki `.unity` dosyaları Unity Hub ile oluşturulur (repo'da yalnızca iskelet tutulur).
Projeyi ilk kez açtığında aşağıdaki sahneleri **File → Build Settings → Add Open Scenes**
sırasıyla ekle.

| # | Sahne | İçerik | Additive |
|---|---|---|---|
| 00 | `00_Boot.unity` | Uygulama başlatma, SDK init, splash | Hayır |
| 01 | `01_MainMenu.unity` | Ana menü, animasyonlu arka plan, müzik | Hayır |
| 02 | `02_WorldMap.unity` | 3D dünya haritası, kilit sistemi | Hayır |
| 03 | `03_Gameplay_Downtown.unity` | Lokasyon 1 (Level 1-10) | Hayır |
| 04 | `04_Gameplay_Beach.unity` | Lokasyon 2 (Level 11-20) | Hayır |
| 05 | `05_Gameplay_Rooftop.unity` | Lokasyon 3 (Level 21-30) | Hayır |
| 06 | `06_Gameplay_Mountain.unity` | Lokasyon 4 (Level 31-40) | Hayır |
| 07 | `07_Gameplay_Tokyo.unity` | Lokasyon 5 (Level 41-50) | Hayır |
| 08 | `08_Gameplay_FoodTruck.unity` | Sezonsal lokasyon | Hayır |
| 09 | `09_Gameplay_Space.unity` | Endgame lokasyon | Hayır |
| 10 | `10_HROffice.unity` | 3D ofis, personel UI (`HROfficeUI`) | **Additive** |
| 11 | `11_UpgradeOffice.unity` | 3D ofis, ekipman UI (`UpgradeOfficeUI`) | **Additive** |
| 12 | `12_Shop.unity` | IAP mağazası (`ShopUI`) | **Additive** |
| 13 | `13_Wardrobe.unity` | Kozmetik deneme/satın alma (`WardrobeUI`) | **Additive** |
| 14 | `14_Settings.unity` | Ayarlar paneli (`SettingsUI`) | **Additive** |
| 15 | `15_LevelComplete.unity` | Sonuç ekranı (`LevelCompleteUI`) | **Additive** |
| 16 | `16_LevelFail.unity` | Başarısızlık ekranı (`LevelFailUI`) | **Additive** |
| 17 | `17_CoopLobby.unity` | Netcode/Photon eşleşme (`LobbyUI`) | Hayır |
| 18 | `18_Gameplay_Coop.unity` | Senkronize 2 oyuncu mutfak | Hayır |
| 19 | `19_SeasonalEvent.unity` | Özel etkinlik sahnesi | **Additive** |

## Sahne kurulum adımları

1. **00_Boot**: Boş sahne → `Bootstrapper` component'ini bir GameObject'e ekle.
2. **01_MainMenu**: Canvas + `MainMenuUI` (play/shop/wardrobe/settings butonlarını bağla).
3. **02_WorldMap**: Canvas + `WorldMapUI`, her lokasyon için bir `LocationCard`.
4. **03-09_Gameplay**: Ortak prefab'ler:
   - `KitchenManager` (Singleton) → `OvenController`, `GrillController`, `AssemblyStation`
   - `CustomerManager` + `CustomerSpawner` + `ChannelManager`
   - `StaffManager` (personel prefab'lerini `staffPrefabs` dizisine ekle)
   - `HUDManager` + `CoinDisplay` + `LevelProgressBar` + `TimerDisplay`
   - NavMesh: `Window → AI → Navigation` → **Bake**
5. **10-16 Paneller**: İlgili UI script'ini Canvas'a ekle, referansları sürükle-bırak.
6. **17-18 Co-op**: `NetworkManager` + `CoopSessionManager` + `PlayerSync` prefab'i.

## URP ayarları

- `Edit → Project Settings → Graphics` → **Scriptable Render Pipeline Settings** alanına
  URP asset'ini sürükle (`Assets/Settings/UniversalRP-HighFidelity`).
- Performans hedefleri (GDD 6.2): gölge mesafesi 40, shadow cascade 2, anti-aliasing SMAA.
