/**
 * Config.js — Oyunun tüm sabitleri (GDD Bölüm 2, 4, 5)
 * Buradaki değerler hem oyun dengesi hem de UI için tek kaynaktır.
 */

export const PALETTE = {
  primary: '#FF6B35',
  gold: '#FFD700',
  accent: '#2ECC71',
  danger: '#E74C3C',
  background: '#1A1A2E',
};

/** Malzemeler — GDD 2.3 katmanlama sırası */
export const INGREDIENTS = {
  bun:     { id: 'bun',     name: 'Ekmek',   emoji: '🍞', color: 0xE8B26A, value: 1,  needsCooking: false },
  patty:   { id: 'patty',   name: 'Köfte',   emoji: '🥩', color: 0x9C4A2F, cookedColor: 0x5E3119, burntColor: 0x251C17, value: 4, needsCooking: true },
  cheese:  { id: 'cheese',  name: 'Peynir',  emoji: '🧈', color: 0xFFC93C, value: 2,  needsCooking: false },
  lettuce: { id: 'lettuce', name: 'Marul',   emoji: '🥬', color: 0x5FBF5F, value: 1,  needsCooking: false },
  tomato:  { id: 'tomato',  name: 'Domates', emoji: '🍅', color: 0xE74C3C, value: 2,  needsCooking: false },
  sauce:   { id: 'sauce',   name: 'Sos',     emoji: '🫗', color: 0xFF6B35, value: 1,  needsCooking: false },
};

export const LAYER_ORDER = ['bun', 'patty', 'cheese', 'lettuce', 'tomato', 'sauce', 'bun'];

/** Müşteri tipleri — GDD 2.5 */
export const CUSTOMER_TYPES = {
  Casual:         { id: 'Casual',         name: 'Sıradan',        emoji: '🙂', patience: 1.0, pay: 1.00, color: 0x6FA8DC, unlockLevel: 1  },
  BusinessPerson: { id: 'BusinessPerson', name: 'İş İnsanı',      emoji: '💼', patience: 2.5, pay: 1.30, color: 0x34495E, unlockLevel: 2  },
  Child:          { id: 'Child',          name: 'Çocuk',          emoji: '🧒', patience: 3.0, pay: 0.70, color: 0xF4A6C0, unlockLevel: 5  },
  VIP:            { id: 'VIP',            name: 'VIP',            emoji: '🌟', patience: 0.7, pay: 2.00, color: 0xFFD700, unlockLevel: 13 },
  Influencer:     { id: 'Influencer',     name: 'Influencer',     emoji: '📸', patience: 1.5, pay: 1.15, color: 0xBB6BD9, unlockLevel: 17 },
};

export const PATIENCE = {
  baseDuration: 60,          // saniye (1x hızda)
  driveThruFactor: 1.3,      // arabalar %30 daha hızlı sıkılır
  warnRatio: 0.35,           // bar kırmızıya döner
  happyTipAt: 0.8,           // >%80 kalan sabır → %50 bahşiş
  okTipAt: 0.5,              // >%50 kalan sabır → %20 bahşiş
};

export const OVEN = {
  levels: [
    { level: 1, slots: 2, duration: 30, burn: 1.15 },
    { level: 2, slots: 3, duration: 24, burn: 1.15 },
    { level: 3, slots: 4, duration: 18, burn: 1.15 },
    { level: 4, slots: 4, duration: 14, burn: 1.25 },
    { level: 5, slots: 6, duration: 10, burn: 1.50 },
  ],
};

export const ECONOMY = {
  startCoins: 250,
  startGems: 30,
  comboOrderBonus: 0.5,       // menü seti siparişleri +%50
  extraLayerTip: 0.1,         // ekstra katman başına +%10 bahşiş
  wrongOrderPenalty: 0.5,     // hatalı siparişte ödeme %50
  burntLossFactor: 1.0,       // yanmış ürün = değeri kadar para kaybı
  doubleIncomeDuration: 180,  // 3 dk
  slowPatienceDuration: 30,
  offlineMaxHours: 8,
  offlineAdHours: 16,
  offlineNoStaff: 0.15,
  offlineWithStaff: 0.45,
  offlineFullUpgrade: 0.70,
};

export const GEM_COSTS = {
  resetPatience: 5,
  instantCook: 10,
  skipLevel: 50,
  extraStaffSlot: 30,
};

/** Ödüllü reklam türleri (GDD 4.2) */
export const REWARD_TYPES = {
  DoubleIncome: 'DoubleIncome',
  DoubleOfflineEarnings: 'DoubleOfflineEarnings',
  SlowPatience: 'SlowPatience',
  RefillOven: 'RefillOven',
  ReviveLevel: 'ReviveLevel',
  SpeedBoost: 'SpeedBoost',
};

/** Kanal türleri (GDD 2.2) */
export const CHANNELS = { COUNTER: 'counter', DRIVE_THRU: 'driveThru' };

export const UNLOCKS = {
  driveThru: 8,
  hrOffice: 9,
  upgradeOffice: 14,
  delivery: 27,
  coop: 16,
};

export const AD_FREQUENCY = 4; // her 4 levelde 1 interstitial (GDD 4.2)
