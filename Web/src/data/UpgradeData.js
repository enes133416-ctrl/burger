/**
 * UpgradeData.js — Ekipman & personel ağaçları (GDD 2.6)
 * Her kalem 5 seviyeye sahiptir; fiyat PriceScaler ile büyür.
 */

/** Fiyat ölçekleyici: base * growth^(level-1) */
export function priceFor(base, level, growth = 2.2) {
  return Math.round(base * Math.pow(growth, level - 1));
}

export const EQUIPMENT = [
  {
    id: 'grill', name: 'Izgara / Fırın', emoji: '🔥',
    desc: 'Pişirme süresi, slot sayısı ve yanma toleransı',
    costs: [0, 400, 1200, 3200, 8000],       // costs[0] = Lv1 (başlangıç)
    perks: ['2 slot / 30sn', '3 slot / 24sn', '4 slot / 18sn', '4 slot / 14sn / geç yanma', '6 slot / 10sn / akıllı fırın'],
  },
  {
    id: 'counter', name: 'Tezgah', emoji: '🍽️',
    desc: 'Kuyruk kapasitesi ve hazırlık hızı',
    costs: [0, 350, 1000, 2600, 6500],
    perks: ['Temel tezgah', '+1 kuyruk', '+1 kuyruk / hız', 'Oto-malzeme', 'Robot kol'],
  },
  {
    id: 'cashier_desk', name: 'Kasa', emoji: '💰',
    desc: 'Ödeme hızı ve gelir bonusu',
    costs: [0, 300, 900, 2400, 6000],
    perks: ['Manuel kasa', 'Hızlı kasa', 'Temassız', 'AI Kasa', 'Hologram kasa'],
  },
];

/** Personel (GDD 2.6 tablosu) */
export const STAFF = [
  {
    id: 'cashier', name: 'Kasiyer', emoji: '🧑‍💼', hireCost: 500,
    desc: 'Sipariş alır, ödeme tahsil eder → gelir bonusu',
    levels: [
      { cost: 0,    perk: 'Gelir +%10' },
      { cost: 200,  perk: 'Gelir +%20' },
      { cost: 500,  perk: 'Gelir +%30 / Multitask' },
      { cost: 1200, perk: 'Gelir +%45 / Sabır buff' },
      { cost: 3000, perk: 'GOLD — Gelir +%70' },
    ],
    incomeBonus: [0.10, 0.20, 0.30, 0.45, 0.70],
  },
  {
    id: 'cook', name: 'Aşçı', emoji: '👨‍🍳', hireCost: 800,
    desc: 'Köfteleri otomatik ızgaraya atar, pişeni tepsiye alır',
    levels: [
      { cost: 0,    perk: 'Oto-pişirme (yavaş)' },
      { cost: 300,  perk: 'Oto-pişirme hızlı' },
      { cost: 800,  perk: '2 slot eşzamanlı' },
      { cost: 2000, perk: 'Pişeni tepsiye alır' },
      { cost: 5000, perk: 'GOLD — Tüm slotlar' },
    ],
    autoCookDelay: [6.0, 4.5, 3.2, 2.4, 1.6],
  },
  {
    id: 'waiter', name: 'Garson', emoji: '🤵', hireCost: 600,
    desc: 'Hazır burgeri eşleşen müşteriye otomatik servis eder',
    levels: [
      { cost: 0,    perk: 'Oto-servis (yavaş)' },
      { cost: 250,  perk: 'Oto-servis hızlı' },
      { cost: 700,  perk: 'Bahşiş +%15' },
      { cost: 1800, perk: 'Drive-Thru\'ya da servis' },
      { cost: 4500, perk: 'GOLD — Anında servis' },
    ],
    autoServeDelay: [3.5, 2.6, 1.9, 1.4, 0.8],
  },
  {
    id: 'driveThruStaff', name: 'Drive-Thru Görevlisi', emoji: '🚗', hireCost: 1200,
    desc: 'Araç kanalında gelir bonusu ve hız',
    levels: [
      { cost: 0,    perk: 'Araç geliri +%20' },
      { cost: 400,  perk: 'Araç geliri +%40' },
      { cost: 1000, perk: 'Araç geliri +%60' },
      { cost: 2500, perk: 'Araç geliri +%85' },
      { cost: 6000, perk: 'GOLD — Araç geliri +%120' },
    ],
    driveThruBonus: [0.20, 0.40, 0.60, 0.85, 1.20],
  },
  {
    id: 'cleaner', name: 'Temizlikçi', emoji: '🧹', hireCost: 400,
    desc: 'Müşteri sabrını yavaşlatır',
    levels: [
      { cost: 0,    perk: 'Sabır +%8' },
      { cost: 200,  perk: 'Sabır +%16' },
      { cost: 600,  perk: 'Sabır +%25' },
      { cost: 1500, perk: 'Sabır +%35' },
      { cost: 3800, perk: 'GOLD — Sabır +%50' },
    ],
    patienceBonus: [0.08, 0.16, 0.25, 0.35, 0.50],
  },
];

export function staffById(id) { return STAFF.find(s => s.id === id); }
export function equipmentById(id) { return EQUIPMENT.find(e => e.id === id); }
