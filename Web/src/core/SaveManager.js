/**
 * SaveManager.js — JSON tabanlı kayıt sistemi (GDD 6.3)
 * Web sürümünde localStorage kullanılır; yapı Unity/PlayerData.cs ile birebir aynıdır.
 */
import { ECONOMY } from './Config.js';

const SAVE_KEY = 'burger_rush_save_v1';

function defaultData() {
  return {
    version: 1,
    currentLevel: 1,
    maxLevelReached: 1,
    totalCoins: ECONOMY.startCoins,
    totalGems: ECONOMY.startGems,
    totalStars: 0,
    levelStars: {},              // { [levelIndex]: 0..3 }
    staff: {},                   // { cashier: { hired: bool, level: 1..5 } }
    equipment: { grill: 1, counter: 1, cashier_desk: 1 },
    unlockedCosmetics: ['default', 'classic'],
    equippedTheme: 'default',
    equippedSkin: 'classic',
    lastOfflineTime: Date.now(),
    offlineEarnings: 0,
    hasChefPass: false,
    chefPassExpiry: 0,
    settings: { music: true, sfx: true, haptic: true, shadows: true },
    stats: { served: 0, burnt: 0, lost: 0, playSeconds: 0 },
    flags: { starterPackBought: false, tutorialDone: false },
  };
}

class SaveManagerCore {
  constructor() {
    this.data = this.load();
    this._saveTimer = null;
  }

  load() {
    try {
      const raw = localStorage.getItem(SAVE_KEY);
      if (!raw) return defaultData();
      const parsed = JSON.parse(raw);
      // Eksik alanları varsayılanla tamamla (geriye dönük uyumluluk)
      return { ...defaultData(), ...parsed, settings: { ...defaultData().settings, ...(parsed.settings || {}) } };
    } catch (err) {
      console.warn('[SaveManager] Kayıt okunamadı, sıfırdan başlanıyor:', err);
      return defaultData();
    }
  }

  save(immediate = false) {
    if (immediate) { this._flush(); return; }
    clearTimeout(this._saveTimer);
    this._saveTimer = setTimeout(() => this._flush(), 400);
  }

  _flush() {
    try {
      localStorage.setItem(SAVE_KEY, JSON.stringify(this.data));
    } catch (err) {
      console.warn('[SaveManager] Kayıt yazılamadı:', err);
    }
  }

  reset() {
    localStorage.removeItem(SAVE_KEY);
    this.data = defaultData();
    this._flush();
  }

  /** Oyuncu çıkarken offline gelir için zaman damgası */
  stampExit() {
    this.data.lastOfflineTime = Date.now();
    this._flush();
  }
}

export const SaveManager = new SaveManagerCore();
export default SaveManager;
