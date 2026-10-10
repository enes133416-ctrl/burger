# 🍔 Burger Rush — Web / Mobil Build

GDD'nin tarayıcıda **oynanabilir** prototipi. Three.js + Vite ile yazılmıştır,
hiçbir binary asset içermez (tüm modeller, sesler ve ikonlar prosedürel üretilir).

## Çalıştırma

```bash
npm install
npm run dev        # http://localhost:5173  (0.0.0.0 — ağdan da erişilir)
npm run build      # dist/
npm run preview    # dist/ önizlemesi
```

## Test

```bash
node tests/logic.test.mjs
```

## Mobil uygulamaya dönüştürme (Capacitor)

```bash
npm run build
npx cap init "Burger Rush" com.studio.burgerrush --web-dir=dist
npx cap add android     # veya: npx cap add ios
npx cap sync
npx cap open android
```

> `vite.config.js` içinde `base: './'` ayarlıdır — `dist/` doğrudan file:// veya
> WebView içinden de açılabilir.

## Klasör yapısı

```
src/
├── main.js                    → Bootstrapper: sistem başlatma + ana döngü
├── core/
│   ├── Config.js              → Tüm sabitler (GDD 2, 4, 5)
│   ├── EventBus.js            → Merkezi event sistemi (GDD 8.2)
│   ├── SaveManager.js         → localStorage kayıt (PlayerData şeması, GDD 6.3)
│   └── GameManager.js         → Durum makinesi + ana döngü (GDD 8.2)
├── data/
│   ├── LevelData.js           → 50 bölüm, boss/milestone, yıldız hesabı (GDD 3)
│   ├── Locations.js           → 7 lokasyon paleti (GDD 3.3)
│   ├── UpgradeData.js         → Ekipman + personel ağaçları (GDD 2.6)
│   └── CosmeticData.js        → Temalar, kostümler, IAP, sezonlar (GDD 4.3, 10)
├── gameplay/
│   ├── KitchenWorld.js        → 3D sahne: tezgah, ızgara, kutular, şef, drive-thru
│   ├── kitchen/               → OvenController, AssemblyStation, BurgerBuilder
│   ├── customer/              → CustomerAI, CustomerManager
│   ├── channel/               → Channel, ChannelManager (Counter + Drive-Thru)
│   └── staff/                 → StaffManager (aşçı/garson otomasyonu)
├── economy/                   → EconomyManager, UpgradeSystem, OfflineEarnings
├── monetization/              → AdManager (simülasyon), ChefPassManager
├── input/                     → InputController (drag&drop), HapticManager
├── audio/                     → AudioManager (WebAudio prosedürel SFX/müzik)
├── seasonal/                  → SeasonalEventManager
├── analytics/                 → AnalyticsManager
└── ui/                        → UIManager → HUD + Screens + Panels
```

## Oynanış notları

| Sistem | Davranış |
|---|---|
| Pişirme | Izgaraya ham köfte koy → %100 hazır (yeşil) → %115 yanık (kırmızı + para kaybı) |
| Bahşiş | Kalan sabır > %80 → %50 · > %50 → %20 · ekstra katman → +%10 · garson Lv3+ → +%15 |
| Hatalı sipariş | Ödeme %50, şikayet sayacı +1 (2-3. yıldızı engeller) |
| Drive-Thru | Bölüm 8'de açılır, sabır %30 hızlı tükenir, her araç kalıcı +%8 çarpan |
| Otomasyon | Aşçı: ızgaraya otomatik köfte · Garson: hazır burgeri eşleşen müşteriye servis |
| Offline | 8 saat tavan (reklamla 16), verim: personel yok %15 / var %45 / full %70 |

## Bilinen sınırlamalar

- Reklamlar **simülasyondur** (3 saniyelik sayaç). Gerçek SDK için
  `src/monetization/AdManager.js` içindeki SDK çağrılarını aktifleştirin.
- IAP satın alımları **demo** modundadır, gerçek ödeme alınmaz.
- Sesler WebAudio ile sentezlenir; Unity sürümünde gerçek asset'ler kullanılır.
- Co-op multiplayer henüz yoktur (Unity tarafında `Multiplayer/` iskeleti hazır).
