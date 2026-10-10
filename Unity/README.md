# 🍔 Burger Rush — Unity Kaynak Projesi

GDD Bölüm 7-8'de tanımlanan **Unity 2022.3 LTS + URP** projesinin kaynak ağacı.
95 C# script, GDD'deki klasör hiyerarşisiyle birebir aynı yapıda.

## 🚀 Projeyi açma

1. **Unity Hub → New Project → Universal 3D (URP)** → isim: `BurgerRush`
2. Proje oluştuktan sonra bu klasördeki `Assets/` ve `Packages/` içeriğini
   yeni projenin üzerine kopyala (var olan `Packages/manifest.json` dosyasını değiştir).
3. Unity'i aç: `Edit → Project Settings → Graphics` → URP asset'ini ata.
4. Sahneleri oluştur: `Assets/Scenes/README.md` dosyasındaki tabloyu takip et.
5. `00_Boot` sahnesini aç ve **Play**'e bas.

> Not: `.meta` dosyaları Unity tarafından ilk açılışta otomatik üretilir;
> repo'da tutulmaz (`.gitignore`).

## 📁 Klasör yapısı

```
Assets/
├── Scenes/            → 19 sahne (liste: Scenes/README.md)
└── Scripts/
    ├── Core/          → GameManager, LevelManager, SceneLoader, EventBus, Bootstrapper
    ├── Gameplay/
    │   ├── Kitchen/   → KitchenManager, OvenController, GrillController, AssemblyStation,
    │   │                IngredientDraggable, BurgerBuilder, FoodItem
    │   ├── Customer/  → CustomerManager, CustomerAI, CustomerSpawner, PatienceBar,
    │   │                OrderData, CustomerQueue
    │   ├── Channel/   → ChannelManager, BaseChannel, CounterChannel, DriveThruChannel,
    │   │                DeliveryChannel
    │   ├── Staff/     → StaffManager, StaffBase, Cashier, Cook, Waiter,
    │   │                DriveThruStaff, Cleaner
    │   └── Progression/ → LevelProgressionData, StarRatingSystem, LevelObjective,
    │                       MilestoneReward
    ├── Economy/       → EconomyManager, OfflineEarningsCalculator, UpgradeSystem,
    │                     HROfficeManager, UpgradeOfficeManager, PriceScaler
    ├── UI/            → HUD/, Menus/, Panels/, UIAnimator
    ├── Monetization/  → AdManager, IAPManager, RewardedAdHandler,
    │                     InterstitialAdController, ChefPassManager
    ├── Input/         → InputManager, TouchDragHandler, SwipeDetector, HapticManager
    ├── AI/            → CustomerBehaviourTree, NavMeshMover, AIDecisionMaker, CarController
    ├── Multiplayer/   → NetworkManager, CoopSessionManager, PlayerSync, LobbyUI
    ├── Data/          → SaveManager, PlayerData, LevelData, UpgradeData, CosmeticData,
    │                     OrderTicket
    ├── Audio/         → AudioManager, MusicController, SFXController, AudioData
    ├── Analytics/     → AnalyticsManager, EventTracker, ABTestManager
    ├── Seasonal/      → SeasonalEventManager, EventRewardSystem, ThemeManager
    └── Utils/         → ObjectPooler, ExtensionMethods, Singleton, Constants, DebugConsole
```

## 🔌 Entegrasyon noktaları

| Sistem | Durum | Yapılacaklar |
|---|---|---|
| Reklam (MAX/IronSource/AdMob) | İskelet | `AdManager.Initialize()` içindeki SDK çağrılarını aktifleştir |
| IAP (Unity Purchasing) | İskelet | `IAPManager.Initialize()` içinde `ConfigurationBuilder` kur |
| Multiplayer (Netcode/Photon) | Soyutlama | `NetworkManager` içindeki yorumlu satırları aç |
| Analitik (Firebase/GameAnalytics) | Log | `AnalyticsManager.Track()` içine SDK çağrısı ekle |
| Ses | Hazır | `AudioData` ScriptableObject'ine clip'leri sürükle |

## ⌨️ Debug konsolu

Development build'de **`` ` ``** tuşu ile açılır:

```
addcoins 10000
addgems 500
level 12
wipe
```
