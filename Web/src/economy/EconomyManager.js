/**
 * EconomyManager.js — Para, gem, gelir çarpanları (GDD Bölüm 4 & 2.7)
 */
import { eventBus, EVENTS } from '../core/EventBus.js';
import { SaveManager } from '../core/SaveManager.js';
import { ECONOMY } from '../core/Config.js';
import { UpgradeSystem } from './UpgradeSystem.js';

class EconomyManagerCore {
  constructor() {
    this.doubleIncomeTimer = 0;
    this.incomeMultiplier = 1;
  }

  // ─────────── Bakiye ───────────
  get coins() { return SaveManager.data.totalCoins; }
  get gems() { return SaveManager.data.totalGems; }

  addCoins(amount, reason = '') {
    if (amount <= 0) return;
    SaveManager.data.totalCoins += Math.round(amount);
    eventBus.publish(EVENTS.COIN_CHANGED, { amount: Math.round(amount), total: this.coins, reason });
    SaveManager.save();
  }

  deductCoins(amount) {
    if (amount <= 0) return true;
    if (SaveManager.data.totalCoins < amount) return false;
    SaveManager.data.totalCoins -= Math.round(amount);
    eventBus.publish(EVENTS.COIN_CHANGED, { amount: -Math.round(amount), total: this.coins });
    SaveManager.save();
    return true;
  }

  addGems(amount) {
    SaveManager.data.totalGems += amount;
    eventBus.publish(EVENTS.GEM_CHANGED, { total: this.gems });
    SaveManager.save();
  }

  spendGems(amount) {
    if (SaveManager.data.totalGems < amount) return false;
    SaveManager.data.totalGems -= amount;
    eventBus.publish(EVENTS.GEM_CHANGED, { total: this.gems });
    SaveManager.save();
    return true;
  }

  canAffordCoins(amount) { return this.coins >= amount; }
  canAffordGems(amount) { return this.gems >= amount; }

  // ─────────── Çarpanlar ───────────
  activateDoubleIncome(duration = ECONOMY.doubleIncomeDuration) {
    this.doubleIncomeTimer = Math.max(this.doubleIncomeTimer, duration);
  }

  get isDoubleIncomeActive() { return this.doubleIncomeTimer > 0; }

  /** Toplam gelir çarpanı (ekipman + personel + boost + chef pass) */
  get totalIncomeMultiplier() {
    let m = UpgradeSystem.incomeMultiplier;
    if (this.doubleIncomeTimer > 0) m *= 2;
    if (SaveManager.data.hasChefPass && Date.now() < SaveManager.data.chefPassExpiry) m *= 1.1;
    return m;
  }

  update(dt) {
    if (this.doubleIncomeTimer > 0) {
      this.doubleIncomeTimer = Math.max(0, this.doubleIncomeTimer - dt);
    }
  }

  // ─────────── Saatlik gelir tahmini (offline hesap için) ───────────
  calculateHourlyEarning() {
    const level = SaveManager.data.currentLevel;
    const base = 40 * Math.pow(1.18, level - 1);          // seviye başına büyüyen temel gelir
    return base * UpgradeSystem.incomeMultiplier;
  }

  /** Günlük Chef Pass ödülü */
  claimChefPassDaily() {
    if (!SaveManager.data.hasChefPass) return 0;
    if (Date.now() > SaveManager.data.chefPassExpiry) {
      SaveManager.data.hasChefPass = false;
      return 0;
    }
    const reward = 2000;
    this.addCoins(reward, 'chefpass');
    return reward;
  }

  grantChefPass(days = 7, coins = 2000) {
    SaveManager.data.hasChefPass = true;
    const base = Date.now() > SaveManager.data.chefPassExpiry ? Date.now() : SaveManager.data.chefPassExpiry;
    SaveManager.data.chefPassExpiry = base + days * 86400000;
    this.addCoins(coins, 'chefpass');
    SaveManager.save();
  }
}

export const EconomyManager = new EconomyManagerCore();
export default EconomyManager;
