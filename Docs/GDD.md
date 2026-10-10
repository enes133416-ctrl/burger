# 🍔 GAME DESIGN DOCUMENT (GDD)
### Proje Adı: **"BURGER RUSH"** — 3D Idle Arcade Simulation
> Version 1.0  | Hybrid-Casual Mobile

---

# 📋 İÇİNDEKİLER

1. [Oyun Genel Bakışı](#1)
2. [Core Loop & Mekanikler](#2)
3. [Progression Sistemi](#3)
4. [Monetizasyon](#4)
5. [UI/UX Tasarımı](#5)
6. [Teknik Mimari](#6)
7. [Unity Scene Listesi](#7)
8. [Script Yapısı](#8)
9. [Sanat & Ses Yönü](#9)
10. [Sezonsal Etkinlikler](#10)

---

# 1. 🎮 OYUN GENEL BAKIŞI

## 1.1 Temel Bilgiler

| Parametre | Değer |
|---|---|
| **Oyun Adı** | Burger Rush |
| **Tür** | 3D Idle Arcade Simulation / Hybrid-Casual |
| **Platform** | iOS & Android |
| **Engine** | Unity 2022.3 LTS (web prototip: Three.js) |
| **Render Pipeline** | URP (Universal Render Pipeline) |
| **Hedef Kitle** | 16-35 yaş, casual-midcore oyuncular |
| **Oturum Süresi** | 5-12 dakika |
| **Referans** | Pizza Ready (Supercent), Cooking Craze |
| **Farklılaştırıcı** | Burger teması + Co-op mod + Az agresif reklam + Dinamik mutfak teknolojisi |

## 1.2 Vizyon Cümlesi
> *"Küçük bir burger standından küresel bir fast-food imparatorluğuna yüksel — gerçek zamanlı mutfak fiziği ve dinamik müşteri yapay zekasıyla."*

## 1.3 Farklılaştırıcılar (USP)

```
✅ Fizik tabanlı malzeme sürükleme (Rigidbody + PhysX)
✅ Dinamik müşteri AI (NavMesh + Behaviour Tree)
✅ Çift kanal: Counter + Drive-Thru
✅ Co-op mod: 2 oyuncu aynı restoranda
✅ Gerçekçi burger katmanlama animasyonları
✅ Sezonsal tema değişimi (Halloween Burger, Noel Kafe vb.)
✅ Reklam frekansı rakiplerin %40 altında
```

---

# 2. 🔄 CORE LOOP & MEKANİKLER

## 2.1 Ana Döngü (Core Loop)

```
┌─────────────────────────────────────────────────────┐
│                    CORE LOOP                        │
│                                                     │
│  MÜŞTERİ GELİR                                     │
│       ↓                                             │
│  SİPARİŞ ALIR (Patience Bar başlar)                │
│       ↓                                             │
│  OYUNCU MALZEMELERİ SÜRÜKLER                       │
│       ↓                                             │
│  FIRINDA PİŞİRİR (Timer)                           │
│       ↓                                             │
│  HAMBURGER KATMANLAR                               │
│       ↓                                             │
│  TESLİM EDER                                       │
│       ↓                                             │
│  PARA KAZANIR (Yıldız x Bahşiş)                   │
│       ↓                                             │
│  UPGRADE SATIN ALIR                                │
│       ↓                                             │
│  (Sonraki level / Yeni lokasyon)                   │
│       ↑___________________________|                 │
└─────────────────────────────────────────────────────┘
```

## 2.2 Dual-Channel Sistemi

### 🏠 İç Mekan (Counter Channel)
```
[Müşteri Kuyruğu] → [Sipariş Ekranı] → [Hazırlık Tezgahı]
        ↓
[Fırın/Izgara] → [Montaj Tezgahı] → [Servis Noktası]
```

| Özellik | Detay |
|---|---|
| Max Kuyruk | Level 1: 3 kişi → Level 10: 8 kişi |
| Servis Süresi | 45-90 saniye (upgradeable) |
| Patience Bar | 100 → 0 (müşteri türüne göre hız değişir) |
| Bahşiş Bonusu | Hızlı servis = +%20-50 bahşiş |

### 🚗 Drive-Thru Channel
```
[Araba Kuyruğu] → [Hoparlör Sipariş] → [Öncelikli Hazırlık]
        ↓
[Pencere Teslim] → [Araba Çıkışı]
```

| Özellik | Detay |
|---|---|
| Açılış | Level 8'de unlock |
| Max Kuyruk | 4 araba |
| Patience | Counter'dan %30 daha hızlı biter (arabalar acelecidir) |
| Bonus | Her tamamlanan araba = +Drive-Thru multiplier |

## 2.3 Malzeme Sürükleme Sistemi

```csharp
// Fizik Tabanlı Sürükleme — Temel Mantık
[Swipe] → Raycast Hit Detection
       → Rigidbody.AddForce (yön vektörü)
       → Snap Zone Magnetic Pull (2 birim mesafede)
       → Haptic Feedback (Light Impact)
```

**Katmanlama Sırası (Burger Assembly):**
```
1. Alt Ekmek (Bun Bottom)     ← Zorunlu
2. Köfte (Patty)              ← Zorunlu (pişmiş olmalı)
3. Peynir (Cheese)            ← Opsiyonel
4. Marul (Lettuce)            ← Opsiyonel
5. Domates (Tomato)           ← Opsiyonel
6. Sos (Sauce)                ← Swipe ile sürükleme
7. Üst Ekmek (Bun Top)       ← Zorunlu
```

> ❌ Yanlış sıra → Müşteri şikayeti → Yıldız kaybı
> ✅ Ekstra katman → Bahşiş bonusu

## 2.4 Fırın Timer Yönetimi

```
[Ham Köfte Yerleştir]
       ↓
[Timer Bar görünür: 0% → 100%]
       ├── %75'te "Hazır" ses efekti
       ├── %100'de yeşil glow
       └── %115'te yanmaya başlar (kırmızı + duman)

Yanmış ürün = Para kaybı + Müşteri şikayeti
```

| Fırın Seviyesi | Pişirme Süresi | Slot Sayısı |
|---|---|---|
| Lv 1 | 30 saniye | 2 slot |
| Lv 2 | 24 saniye | 3 slot |
| Lv 3 | 18 saniye | 4 slot |
| Lv 4 | 14 saniye | 4 slot |
| Lv 5 | 10 saniye | 6 slot |

## 2.5 Müşteri Patience Bar

```
Müşteri Tipleri:
┌──────────────┬───────────────┬──────────────┬──────────────┐
│ Tip          │ Patience Hızı │ Ödeme        │ Görünüm      │
├──────────────┼───────────────┼──────────────┼──────────────┤
│ Casual       │ Yavaş (1x)    │ Normal       │ Rahat kıyafet│
│ Businessperson│ Hızlı (2.5x) │ +%30 fazla  │ Takım elbise │
│ Çocuk        │ Çok Hızlı(3x)│ Az           │ Okul çantası │
│ VIP          │ Yavaş (0.7x) │ +%100 fazla │ Altın rozet  │
│ Influencer   │ Orta (1.5x)  │ +Reklam ödülü│ Telefon çeker│
└──────────────┴───────────────┴──────────────┴──────────────┘
```

## 2.6 Personel Sistemi

### 👨‍🍳 Personel Tipleri

| Personel | Görevi | Başlangıç Fiyatı | 
|---|---|---|
| Kasiyer | Sipariş alır, para teslim | 500 🪙 |
| Aşçı | Köfte pişirir | 800 🪙 |
| Garson | Servis yapar | 600 🪙 |
| Drive-Thru Görevlisi | Araçlara hizmet | 1200 🪙 |
| Temizlikçi | Masaları temizler | 400 🪙 |

### 🏢 HR Office — Personel Upgrade Ağacı

```
HER PERSONEL İÇİN 5 SEVİYE:

Lv1 [Speed +10%]        → 200 🪙
Lv2 [Accuracy +15%]     → 500 🪙
Lv3 [Multitask x2]      → 1200 🪙
Lv4 [Patience Buff +20%]→ 3000 🪙
Lv5 [GOLD TIER - Özel animasyon + +50% verim] → 8000 🪙
```

### 🏢 Upgrade Office — Ekipman Upgrade Ağacı

```
IZGARA/FIRIN:
Lv1 → Lv2 → Lv3 → Lv4 → Lv5
 [Hız] [Slot] [Anti-yanma] [Turbo] [Smart Oven AI]

TEZGAH:
Lv1 → Lv2 → Lv3 → Lv4 → Lv5
 [1 alan] [2 alan] [Oto-malzeme] [Hız +] [Robot Arm]

KASA:
Lv1 → Lv2 → Lv3 → Lv4 → Lv5
 [Manuel] [Hızlı] [Contactless] [AI Kasa] [Hologram]
```

## 2.7 Idle Gelir Mekaniği

```
Offline Gelir Hesaplama:
─────────────────────────────────────
Saatlik Gelir = (Ortalama müşteri/saat) × (Ortalama sipariş değeri) × (Personel verimi)

Max Offline Süre: 8 saat
Rewarded Ad izle → 8 saat → 16 saate çıkar

Offline Gelir Katsayısı:
• Personel yok:  %15 verim
• Personel var:  %45 verim  
• Full upgrade:  %70 verim
─────────────────────────────────────
```

---

# 3. 📈 PROGRESSION SİSTEMİ

## 3.1 Level Haritası

### 🟢 Erken Oyun (Level 1-10): Tutorial & Temel Mekanikler

```
Level 01 → Tutorial: İlk burger yap (Sadece ekmek + köfte)
Level 02 → Peynir eklendi, 2. müşteri tipi
Level 03 → İlk personel kiralama (Kasiyer)
Level 04 → Malzeme çeşidi arttı (marul, domates)
Level 05 → ⭐ BOSS LEVEL: Rush Hour (3 dakika yoğun)
Level 06 → Fırın upgrade tanıtımı
Level 07 → İkinci menü öğesi (Cheeseburger seti)
Level 08 → 🚗 Drive-Thru açılıyor!
Level 09 → HR Office açılıyor
Level 10 → ⭐ MILESTONE: İlk lokasyon tamamlandı!
           → Ödül: Özel kostüm + 5000 🪙
```

### 🟡 Orta Oyun (Level 11-25): Çoklu Sipariş & Strateji

```
Level 11 → Yeni lokasyon: Beach Burger Bar 🏖️
Level 12 → Combo sipariş sistemi (Menü seti)
Level 13 → VIP müşteri ilk kez gelir
Level 14 → Upgrade Office açılıyor
Level 15 → ⭐ BOSS LEVEL: Celebrity VIP Visit
Level 16 → Co-op mod tanıtımı (beta)
Level 17 → Çalışan çakışma yönetimi
Level 18 → Yeni malzeme: Özel soslar (3 çeşit)
Level 20 → ⭐ MILESTONE: 2. lokasyon!
           → Ödül: Drive-Thru skin + 15000 🪙
Level 25 → ⭐ BOSS LEVEL: Food Critic gelir (Hata yok!)
```

### 🔴 Geç Oyun (Level 26+): Hız & Strateji Dengesi

```
Level 26 → Yeni lokasyon: Rooftop Burger Lounge 🌆
Level 27 → Triple channel: Counter + Drive-Thru + Delivery
Level 28 → Smart Kitchen AI (oto-yönetim upgrade)
Level 30 → ⭐ MILESTONE: 3. lokasyon!
Level 35 → Franchise sistemi (Pasif gelir)
Level 40 → World Tour: Tokyo Sushi Burger 🍣
Level 50 → ENDGAME: Global Fast-Food Empire 🌍
```

## 3.2 Yıldız Sistemi

```
Her Level İçin 3 Yıldız:

⭐        = Tüm siparişlerin %60'ı tamamlandı
⭐⭐      = Tüm siparişlerin %80'i + sıfır yanlış
⭐⭐⭐    = %95+ + Hız bonusu + Sıfır müşteri kaybı

Yıldız Kullanımı:
├── Sezonsal etkinlik biletleri
├── Kozmetik kilit açma
└── Franchise puanı
```

## 3.3 Lokasyon Haritası

```
🗺️ DÜNYA HARİTASI:

[1] 🏙️ Downtown Burger Stand    → Level 1-10
[2] 🏖️ Beach Burger Bar         → Level 11-20
[3] 🌆 Rooftop Lounge           → Level 21-30
[4] 🏔️ Mountain Rest Stop       → Level 31-40
[5] 🗼 Tokyo Fusion              → Level 41-50
[6] 🎪 Festival Food Truck      → Sezonsal
[7] 🚀 Space Burger (Endgame)   → Level 50+
```

---

# 4. 💰 MONETİZASYON

## 4.1 Gelir Dağılımı

```
┌─────────────────────────────────────────┐
│         GELİR YAPISI                   │
│                                         │
│  Reklam Geliri        %70              │
│  ├── Interstitial     %30              │
│  ├── Rewarded Video   %35              │
│  └── Banner           %5              │
│                                         │
│  IAP Geliri           %30              │
│  ├── Chef Pass        %15              │
│  ├── Kozmetik         %8               │
│  └── Starter Pack     %7               │
└─────────────────────────────────────────┘
```

## 4.2 Reklam Stratejisi (Rakipten %40 Daha Az Agresif)

| Reklam Tipi | Frekans | Tetikleyici |
|---|---|---|
| Interstitial | Her 4 levelda 1 | Level başarı ekranında |
| Rewarded Video | İsteğe bağlı | Oyuncu kendisi seçer |
| Banner | Sadece menü ekranında | Oyun içinde yok |

### Rewarded Video Ödülleri:
```
🎬 Reklam İzle →
├── 2x Gelir (3 dakika)
├── Offline geliri 2x'e çıkar
├── Patience bar yavaşlatma (30 sn)
├── Tüm fırınları anında doldur
└── Kaybedilen seviyeyi devam ettir
```

## 4.3 IAP Paketleri

### 🍔 Starter Pack (1-time offer, 1.99$)
```
├── 10.000 🪙
├── 50 💎 Gem
├── Özel apron skin (Gold Chef)
└── 2x offline gelir (24 saat)
```

### 👨‍🍳 Chef Pass (Haftalık, 4.99$)
```
├── Günlük 2.000 🪙
├── Reklam sayısı -50%
├── VIP müşteri spawn +20%
├── Exclusive Chef Pass rozeti
└── Haftalık özel kostüm
```

### 💎 Gem Paketleri
```
50 Gem   → 0.99$
150 Gem  → 2.99$
500 Gem  → 7.99$
1200 Gem → 14.99$  ← En popüler
3500 Gem → 34.99$
```

### 🎨 Kozmetik Paketler
```
Restoran Temaları (2.99$ - 5.99$):
├── Neon Cyberpunk Diner
├── Vintage 50s American
├── Tokyo Street Food
├── Tropical Paradise
└── Space Station Cafeteria

Karakter Kostümleri (0.99$ - 2.99$):
├── Ninja Chef
├── Robot Cook
├── Viking Burger Master
└── Astronaut Grill Master
```

## 4.4 Gem Harcama Alanları

```
💎 Gem Kullanım:
├── Patience bar sıfırla (5 💎)
├── Fırın anında bitir (10 💎)
├── Level skip (50 💎)
├── Ekstra personel slotu (30 💎)
└── Kozmetik skin (20-100 💎)
```

---

# 5. 🖥️ UI/UX TASARIMI

## 5.1 HUD Elemanları

```
┌────────────────────────────────────────────────┐
│ [🪙 12,450]  BURGER RUSH    [⚙️] [👤] [🔔]   │
│ Level 12 ████████░░░░  60%                     │
├────────────────────────────────────────────────┤
│                                                │
│         [3D OYUN ALANI]                       │
│                                                │
│  [Müşteri 1 ████░ ]  [Müşteri 2 ██░░░ ]      │
│  [Müşteri 3 █████ ]                           │
│                                                │
│         [COUNTER]  [DRIVE-THRU]               │
│                                                │
├────────────────────────────────────────────────┤
│ [HR Office] [Upgrade] [Menu] [🎬 2x Gelir]   │
└────────────────────────────────────────────────┘
```

## 5.2 Ekran Akışı (Screen Flow)

```
SPLASH SCREEN
     ↓
MAIN MENU
     ├── [Play] → LEVEL SELECT → GAMEPLAY
     ├── [Shop] → IAP STORE
     ├── [Settings] → AUDIO/GRAPHICS/PRIVACY
     └── [Profile] → STATS/ACHIEVEMENTS

GAMEPLAY
     ├── [Pause] → PAUSE MENU
     │              ├── Resume
     │              ├── Restart
     │              └── Main Menu
     ├── [Level Complete] → RESULT SCREEN
     │              ├── ⭐⭐⭐ Rating
     │              ├── Para Kazancı
     │              ├── Reklam (2x teklif)
     │              └── Next Level
     └── [Level Fail] → FAIL SCREEN
                    ├── Retry (Gem veya Reklam)
                    └── Main Menu
```

## 5.3 Renk Paleti & Tipografi

```
ANA RENKLER:
├── Primary:    #FF6B35 (Turuncu - Burger sıcaklığı)
├── Secondary:  #FFD700 (Altın - Para/Başarı)
├── Accent:     #2ECC71 (Yeşil - Başarı/Onay)
├── Warning:    #E74C3C (Kırmızı - Yanma/Acele)
└── Background: #1A1A2E (Koyu lacivert)

TİPOGRAFİ:
├── Başlıklar:  Fredoka One (Bold, Rounded)
├── UI Yazılar: Nunito (SemiBold)
└── Sayılar:    Digital-7 (Para/Timer)
```

---

# 6. ⚙️ TEKNİK MİMARİ

## 6.1 Sistem Mimarisi

```
┌─────────────────────────────────────────────────────┐
│                 GAME ARCHITECTURE                   │
│                                                     │
│  ┌──────────┐    ┌──────────┐    ┌──────────────┐  │
│  │  Input   │───▶│  Game    │───▶│   Renderer   │  │
│  │  System  │    │  Manager │    │   (URP)      │  │
│  └──────────┘    └────┬─────┘    └──────────────┘  │
│                       │                             │
│         ┌─────────────┼──────────────┐             │
│         ▼             ▼              ▼             │
│  ┌─────────────┐ ┌─────────┐ ┌──────────────┐    │
│  │  Customer   │ │ Kitchen │ │   Economy    │    │
│  │  AI System  │ │ System  │ │   Manager    │    │
│  └─────────────┘ └─────────┘ └──────────────┘    │
│         │             │              │             │
│         └─────────────┼──────────────┘             │
│                       ▼                             │
│              ┌─────────────────┐                   │
│              │   Save System   │                   │
│              │  (JSON + Cloud) │                   │
│              └─────────────────┘                   │
└─────────────────────────────────────────────────────┘
```

## 6.2 Performans Hedefleri

| Platform | Min FPS | Hedef FPS | Draw Calls |
|---|---|---|---|
| Low-end Android | 30 FPS | 45 FPS | < 80 |
| Mid-range | 45 FPS | 60 FPS | < 120 |
| High-end iOS | 60 FPS | 60 FPS | < 150 |

## 6.3 Kayıt Sistemi

```csharp
// Save Data Yapısı
PlayerData {
    int currentLevel;
    float totalCoins;
    float totalGems;
    Dictionary<string, int> staffLevels;
    Dictionary<string, int> equipmentLevels;
    List<string> unlockedCosmetics;
    DateTime lastOfflineTime;
    float offlineEarnings;
    bool hasChefPass;
    DateTime chefPassExpiry;
}
```

---

# 7. 🎬 UNITY SCENE LİSTESİ

```
📁 Assets/
└── 📁 Scenes/
    ├── 🎬 00_Boot.unity
    ├── 🎬 01_MainMenu.unity
    ├── 🎬 02_WorldMap.unity
    ├── 🎬 03_Gameplay_Downtown.unity
    ├── 🎬 04_Gameplay_Beach.unity
    ├── 🎬 05_Gameplay_Rooftop.unity
    ├── 🎬 06_Gameplay_Mountain.unity
    ├── 🎬 07_Gameplay_Tokyo.unity
    ├── 🎬 08_Gameplay_FoodTruck.unity
    ├── 🎬 09_Gameplay_Space.unity
    ├── 🎬 10_HROffice.unity
    ├── 🎬 11_UpgradeOffice.unity
    ├── 🎬 12_Shop.unity
    ├── 🎬 13_Wardrobe.unity
    ├── 🎬 14_Settings.unity
    ├── 🎬 15_LevelComplete.unity
    ├── 🎬 16_LevelFail.unity
    ├── 🎬 17_CoopLobby.unity
    ├── 🎬 18_Gameplay_Coop.unity
    └── 🎬 19_SeasonalEvent.unity
```

### Scene Detayları

| Scene | İçerik | Additive? |
|---|---|---|
| 00_Boot | Uygulama başlatma, SDK init, ilk yükleme | Hayır |
| 01_MainMenu | Ana menü, animasyonlu arka plan, müzik | Hayır |
| 02_WorldMap | 3D dünya haritası, kilit sistemi | Hayır |
| 03-09_Gameplay | Her lokasyon için ayrı sahne, farklı asset seti | Hayır |
| 10_HROffice | 3D ofis, personel UI | Additive |
| 11_UpgradeOffice | 3D ofis, ekipman UI | Additive |
| 12_Shop | IAP mağazası | Additive |
| 13_Wardrobe | Kozmetik deneme/satın alma | Additive |
| 17_CoopLobby | Photon/Netcode ile eşleşme | Hayır |
| 18_Gameplay_Coop | Senkronize 2 oyuncu mutfak | Hayır |
| 19_SeasonalEvent | Özel etkinlik sahnesi | Additive |

---

# 8. 📜 SCRIPT YAPISI

## 8.1 Klasör Hiyerarşisi

```
📁 Assets/Scripts/
├── 📁 Core/          → GameManager, LevelManager, SceneLoader, EventBus, Bootstrapper
├── 📁 Gameplay/
│   ├── 📁 Kitchen/   → KitchenManager, OvenController, GrillController, AssemblyStation,
│   │                   IngredientDraggable, BurgerBuilder, FoodItem
│   ├── 📁 Customer/  → CustomerManager, CustomerAI, CustomerSpawner, PatienceBar,
│   │                   OrderData, CustomerQueue
│   ├── 📁 Channel/   → ChannelManager, BaseChannel, CounterChannel, DriveThruChannel,
│   │                   DeliveryChannel
│   ├── 📁 Staff/     → StaffManager, StaffBase, Cashier, Cook, Waiter,
│   │                   DriveThruStaff, Cleaner
│   └── 📁 Progression/ → LevelProgressionData, StarRatingSystem, LevelObjective,
│                          MilestoneReward
├── 📁 Economy/       → EconomyManager, OfflineEarningsCalculator, UpgradeSystem,
│                       HROfficeManager, UpgradeOfficeManager, PriceScaler
├── 📁 UI/
│   ├── 📁 HUD/       → HUDManager, CoinDisplay, LevelProgressBar, PatientBar, TimerDisplay
│   ├── 📁 Menus/     → MainMenuUI, WorldMapUI, PauseMenuUI, LevelCompleteUI,
│   │                   LevelFailUI, ShopUI
│   ├── 📁 Panels/    → HROfficeUI, UpgradeOfficeUI, WardrobeUI, OfflineEarningsUI,
│   │                   SettingsUI
│   └── UIAnimator.cs
├── 📁 Monetization/  → AdManager, IAPManager, RewardedAdHandler,
│                       InterstitialAdController, ChefPassManager
├── 📁 Input/         → InputManager, TouchDragHandler, SwipeDetector, HapticManager
├── 📁 AI/            → CustomerBehaviourTree, NavMeshMover, AIDecisionMaker, CarController
├── 📁 Multiplayer/   → NetworkManager, CoopSessionManager, PlayerSync, LobbyUI
├── 📁 Data/          → SaveManager, PlayerData, LevelData, UpgradeData, CosmeticData
├── 📁 Audio/         → AudioManager, MusicController, SFXController, AudioData
├── 📁 Analytics/     → AnalyticsManager, EventTracker, ABTestManager
├── 📁 Seasonal/      → SeasonalEventManager, EventRewardSystem, ThemeManager
└── 📁 Utils/         → ObjectPooler, ExtensionMethods, Singleton, Constants, DebugConsole
```

## 8.2 Temel Script İçerikleri

> Tam kaynak kod için `Unity/Assets/Scripts/` klasörüne bakın (95 dosya).
> Aşağıda GDD'de tanımlanan referans uygulamalar özetlenmiştir.

### GameManager.cs
```csharp
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    { Boot, MainMenu, WorldMap, Gameplay, Paused, LevelComplete, LevelFail, Shop }

    public GameState CurrentState { get; private set; }
    public static event Action<GameState> OnStateChanged;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeSystems();   // Save → Analytics → Ads → OfflineEarnings
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        HandleStateTransition(newState);   // timeScale, müzik, reklam
    }
}
```

### CustomerAI.cs
```csharp
public class CustomerAI : MonoBehaviour
{
    public enum CustomerType { Casual, BusinessPerson, Child, VIP, Influencer }
    public enum CustomerState
    { Spawning, WalkingToQueue, Waiting, Ordering, WaitingForFood, Leaving, Angry }

    // Sabır: basePatienceDuration / typeMultiplier * (temizlikçi buff'ı)
    // Bahşiş: kalan sabır > %80 → %50 · > %50 → %20
    // Yanlış sipariş → ödeme %50 + yıldız kaybı
}
```

### OvenController.cs
```csharp
public class OvenController : MonoBehaviour
{
    public enum SlotState { Empty, Cooking, Ready, Burnt }

    // cookProgress 0 → 1 (cookingDuration saniye)
    // %100 → Ready (yeşil glow + "Ding!" + haptic)
    // burnThreshold (%115) → Burnt (kırmızı + duman + para kaybı)
    // ApplyUpgrade(level) → slot sayısı / süre / yanma toleransı
}
```

### OfflineEarningsCalculator.cs
```csharp
public class OfflineEarningsCalculator : Singleton<OfflineEarningsCalculator>
{
    const int MAX_OFFLINE_HOURS = 8;
    const int AD_BONUS_HOURS = 16;
    const float NO_STAFF = 0.15f, WITH_STAFF = 0.45f, FULL_UPGRADE = 0.70f;

    // earnings = hourlyEarning * clampedHours * efficiency
}
```

### EventBus.cs
```csharp
public static class EventBus
{
    public static void Subscribe<T>(Action<T> handler);
    public static void Unsubscribe<T>(Action<T> handler);
    public static void Publish<T>(T eventData);
}

// Event verileri: CustomerServedEvent, FoodBurntEvent, LevelCompleteEvent,
// LevelFailEvent, UpgradePurchasedEvent, CoinChangedEvent, AdRewardedEvent …
```

### AdManager.cs
```csharp
public class AdManager : Singleton<AdManager>
{
    public enum RewardType
    { DoubleIncome, SpeedBoost, DoubleOfflineEarnings, ReviveLevel, ExtraIngredients }

    // TryShowInterstitial() → her 4 levelda 1 (Chef Pass ile 8)
    // ShowRewardedAd(type, onSuccess, onFail) → ApplyReward(type)
}
```

### ObjectPooler.cs
```csharp
public class ObjectPooler : Singleton<ObjectPooler>
{
    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation);
    public void ReturnToPool(string tag, GameObject obj);
    // Pool doluysa expandable=true ise dinamik genişlet
}
```

---

# 9. 🎨 SANAT & SES YÖNETİMİ

## 9.1 Görsel Stil

```
Stil: Low-Poly + Soft Shadows + Warm Lighting

Karakter Tasarımı:
├── Chibi-ish oranlar (Baş biraz büyük)
├── Ekspresif yüz animasyonları
├── 4 renk varyant per müşteri tipi
└── 15+ kostüm unlock

Mekan Tasarımı:
├── Her lokasyon farklı renk paleti
├── Çalışan ekipmanlar (buhar, ışık animasyonları)
├── Dinamik gün/gece döngüsü
└── Hava durumu efektleri (yağmur, güneş)
```

## 9.2 Animasyon Listesi

| Animasyon | Tetikleyici | Süre |
|---|---|---|
| Müşteri yürüyüş | NavMesh hareketi | Loop |
| Müşteri mutlu | Sipariş teslim | 1.5s |
| Müşteri sinirli | Patience = 0 | 1s |
| Burgeri katla | Assembly station | 0.8s |
| Fırından al | CollectFood() | 0.5s |
| Para uçuşu | EconomyManager | 0.6s |
| Yıldız patlaması | Level complete | 2s |
| Personel çalış | Staff active | Loop |

## 9.3 Ses Tasarımı

```
🎵 MÜZİK:
├── MainMenu: Upbeat jazzy lounge
├── Gameplay_Early: Cheerful pop
├── Gameplay_Rush: Fast-paced electronic
├── LevelComplete: Victory fanfare (3s)
└── Seasonal: Tema müzikleri

🔊 SES EFEKTLERİ:
├── Kasada para: "Ching!"
├── Fırın hazır: "Ding!"
├── Burger katlan: "Thwack!"
├── Müşteri memnun: "Mmm~"
├── Müşteri sinirli: "Hmph!"
├── Yanma: "Sizzle + Alarm"
├── Drive-thru zil: "Beep!"
├── Upgrade: "Power up!"
└── Yıldız kazan: "Sparkle!"
```

---

# 10. 🎃 SEZONSAL ETKİNLİKLER

## 10.1 Etkinlik Takvimi

| Etkinlik | Dönem | Tema | Özel Mekanik |
|---|---|---|---|
| 🎃 Halloween Haunted Burger | Ekim | Karanlık mutfak | Hayalet müşteriler |
| 🎄 Christmas Cozy Café | Aralık | Kar + Sıcak | Sıcak içecek eklendi |
| 💝 Valentine's Sweetheart Diner | Şubat | Pembe/Kırmızı | Çift siparişler |
| 🌸 Cherry Blossom Fusion | Nisan | Japon teması | Sushi-burger çaprazı |
| ☀️ Summer Beach Grill | Temmuz | Tropikal | Buzlu içecek hızlanması |

## 10.2 Etkinlik Yapısı

```
ETKİNLİK SÜRESİ: 14 gün

Günlük Görevler:
├── 50 burger sat → 500 🎟️ Token
├── 0 müşteri kaybetme → 300 🎟️ Token
└── 3 yıldızlı level → 200 🎟️ Token

Token Mağazası:
├── Özel kostüm (1500 🎟️)
├── Restoran teması (2500 🎟️)
├── 10.000 🪙 (1000 🎟️)
├── Özel personel skin (3000 🎟️)
└── Etkinlik rozeti (5000 🎟️)
```

---

# 📊 KPI & HEDEFLER

```
HEDEF METRİKLER (İlk 90 gün):

├── D1 Retention:  > %45
├── D7 Retention:  > %22
├── D30 Retention: > %10
├── Session Length: 7-10 dakika
├── Sessions/Day:   2.8
├── ARPU (Day 30): $0.35+
├── eCPM:          $8-15 (Rewarded)
└── Conversion (IAP): %2.5+

A/B TEST LİSTESİ:
├── Reklam frekansı (3 vs 4 vs 5 level)
├── Starter pack fiyatı ($0.99 vs $1.99)
├── Onboarding uzunluğu
└── Patience bar hızı
```
