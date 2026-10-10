# 🍔 BURGER RUSH — 3D Idle Arcade Simulation

> *"Küçük bir burger standından küresel bir fast-food imparatorluğuna yüksel."*

Hybrid-casual mobil oyun projesi. Bu repo iki paralel üretim hattı içerir:

| Klasör | Ne | Durum |
|---|---|---|
| **[`Web/`](Web)** | Tarayıcıda **oynanabilir** 3D prototip (Three.js + Vite). Mobilde de çalışır, Capacitor ile APK/IPA'ya dönüştürülebilir. | ✅ Oynanabilir |
| **[`Unity/`](Unity)** | GDD Bölüm 7-8'deki **Unity 2022.3 LTS + URP** kaynak projesi (95 C# script, aynı klasör hiyerarşisi) | 🧩 İskelet + sistemler |
| **[`Docs/GDD.md`](Docs/GDD.md)** | Oyun Tasarım Dokümanı (v1.0) | 📄 Tam metin |

---

## 🚀 Hızlı Başlangıç (Web)

```bash
cd Web
npm install
npm run dev          # http://localhost:5173
```

Üretim derlemesi:

```bash
npm run build        # çıktı: Web/dist/
npm run preview
```

### Nasıl oynanır?

1. **Malzeme kutusuna dokun** → eline malzeme gelir.
2. **Tabağa dokun** → burger yığınına katman eklenir.
3. **Köfteyi ızgaraya koy** → 30 sn pişer (yeşil yanınca al, kırmızı yanarsa para kaybedersin).
4. **Üst ekmeği koy ve burgeri müşteriye sürükle** → para + bahşiş.
5. Kazandığın parayla **HR Office**'ten personel al, **Upgrade Office**'ten fırını yükselt.

> Hem sürükle-bırak hem "dokun → dokun" modu desteklenir (mobil uyumlu).

### 🎮 Kontroller

| Girdi | İşlev |
|---|---|
| Dokun / tıkla | Malzeme al, tabağa koy, ızgaradan al |
| Sürükle & bırak | Malzemeyi doğrudan hedefe taşı |
| Müşteriye dokun | Siparişini gör |
| `⏸` | Duraklat |
| `` ` `` (Unity build) | Debug konsolu |

---

## 🧩 Unity Projesi

Detaylı kurulum: **[`Unity/README.md`](Unity/README.md)** · Sahne listesi: **[`Unity/Assets/Scenes/README.md`](Unity/Assets/Scenes/README.md)**

```text
Unity 2022.3 LTS → URP template → Assets/ + Packages/ kopyala → sahneleri oluştur → Play
```

Klasör yapısı GDD Bölüm 8.1 ile birebir aynıdır: `Core`, `Gameplay/{Kitchen,Customer,Channel,Staff,Progression}`,
`Economy`, `UI/{HUD,Menus,Panels}`, `Monetization`, `Input`, `AI`, `Multiplayer`, `Data`, `Audio`,
`Analytics`, `Seasonal`, `Utils`.

SDK entegrasyon noktaları (reklam, IAP, analitik, multiplayer) yorum satırlarıyla işaretlenmiştir.

---

## 🗺️ Yol Haritası

- [x] Core loop: sipariş → pişirme → katmanlama → servis → para
- [x] Dual-channel: Counter + Drive-Thru (Lv8)
- [x] Fırın timer'ı, yanma mekaniği, 5 seviye ekipman
- [x] 5 müşteri tipi + patience bar + bahşiş
- [x] 5 personel tipi + otomasyon (aşçı/garson)
- [x] 50 bölüm, 7 lokasyon, boss/milestone
- [x] Idle/offline gelir (8s → 16s reklam)
- [x] Reklam (simülasyon) + IAP paketleri + kozmetik
- [x] Sezonsal etkinlik iskeleti
- [ ] Gerçek reklam SDK (AdMob/MAX) + Unity IAP
- [ ] Co-op multiplayer (Netcode/Photon)
- [ ] Bulut kayıt + liderlik tablosu
- [ ] Ses/müzik asset'leri (şu an prosedürel WebAudio sentezi)

---

## 🧪 Testler

```bash
cd Web
node tests/logic.test.mjs
```

Saf mantık modülleri için duman testi: kayıt/ekonomi, bölüm verisi, yıldız hesabı,
burger doğrulama, sipariş üretimi, yükseltme sistemi ve offline gelir.

---

## 📄 Lisans

MIT — detaylar için [`LICENSE`](LICENSE).
