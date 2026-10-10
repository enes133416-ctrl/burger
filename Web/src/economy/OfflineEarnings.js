/**
 * OfflineEarnings.js — Offline gelir hesaplayıcı (GDD 2.7)
 * Max 8 saat, ödüllü reklamla 16 saate çıkar.
 */
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from './EconomyManager.js';
import { UpgradeSystem } from './UpgradeSystem.js';
import { ECONOMY } from '../core/Config.js';

class OfflineEarningsCalculatorCore {
  /** @returns {{earnings:number, hours:number, realHours:number, canDoubleWithAd:boolean}} */
  calculate() {
    const now = Date.now();
    const last = SaveManager.data.lastOfflineTime || now;
    const elapsedMs = Math.max(0, now - last);
    const realHours = elapsedMs / 3600000;
    const hours = Math.min(realHours, ECONOMY.offlineMaxHours);

    const hourly = EconomyManager.calculateHourlyEarning();
    const efficiency = UpgradeSystem.offlineEfficiency;
    const earnings = Math.floor(hourly * hours * efficiency);

    SaveManager.data.offlineEarnings = earnings;
    SaveManager.save();

    return {
      earnings,
      hours,
      realHours,
      hourly,
      efficiency,
      canDoubleWithAd: earnings > 0,
    };
  }

  claim(double = false) {
    const base = SaveManager.data.offlineEarnings || 0;
    if (base <= 0) return 0;
    const amount = double ? base * 2 : base;
    EconomyManager.addCoins(amount, 'offline');
    SaveManager.data.offlineEarnings = 0;
    SaveManager.data.lastOfflineTime = Date.now();
    SaveManager.save();
    return amount;
  }

  /** Ödüllü reklam sonrası 8 saatlik tavanı 16 saate çıkarır */
  recalculateWithAdBonus() {
    const now = Date.now();
    const last = SaveManager.data.lastOfflineTime || now;
    const realHours = (now - last) / 3600000;
    const hours = Math.min(realHours, ECONOMY.offlineAdHours);
    const hourly = EconomyManager.calculateHourlyEarning();
    const earnings = Math.floor(hourly * hours * UpgradeSystem.offlineEfficiency);
    SaveManager.data.offlineEarnings = earnings;
    SaveManager.save();
    return earnings;
  }
}

export const OfflineEarningsCalculator = new OfflineEarningsCalculatorCore();
export default OfflineEarningsCalculator;
