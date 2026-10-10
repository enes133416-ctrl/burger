/**
 * UpgradeSystem.js — Ekipman & personel yükseltmeleri (GDD 2.6)
 * Tüm pasif/aktif bonusların tek hesap merkezi.
 */
import { eventBus, EVENTS } from '../core/EventBus.js';
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from './EconomyManager.js';
import { OVEN, UNLOCKS } from '../core/Config.js';
import { STAFF, EQUIPMENT, staffById, equipmentById, priceFor } from '../data/UpgradeData.js';

const MAX_LEVEL = 5;

class UpgradeSystemCore {
  // ─────────── Ekipman ───────────
  equipmentLevel(id) { return SaveManager.data.equipment[id] || 1; }

  equipmentNextCost(id) {
    const def = equipmentById(id);
    const lvl = this.equipmentLevel(id);
    if (lvl >= MAX_LEVEL) return null;
    // costs[0] Lv1 içindir; Lv2 maliyeti costs[1]
    return def.costs[lvl] ?? priceFor(def.costs[1] || 400, lvl);
  }

  buyEquipment(id) {
    const cost = this.equipmentNextCost(id);
    if (cost == null) return { ok: false, reason: 'max' };
    if (!EconomyManager.deductCoins(cost)) return { ok: false, reason: 'coins' };
    SaveManager.data.equipment[id] = this.equipmentLevel(id) + 1;
    SaveManager.save();
    eventBus.publish(EVENTS.UPGRADE_PURCHASED, { id, kind: 'equipment', level: SaveManager.data.equipment[id] });
    return { ok: true, level: SaveManager.data.equipment[id] };
  }

  // ─────────── Personel ───────────
  staffState(id) {
    const s = SaveManager.data.staff[id];
    return s || { hired: false, level: 0 };
  }

  isHired(id) { return !!this.staffState(id).hired; }
  staffLevel(id) { return this.staffState(id).level; }

  hireCost(id) { return staffById(id).hireCost; }

  hire(id) {
    if (this.isHired(id)) return { ok: false, reason: 'owned' };
    const cost = this.hireCost(id);
    if (!EconomyManager.deductCoins(cost)) return { ok: false, reason: 'coins' };
    SaveManager.data.staff[id] = { hired: true, level: 1 };
    SaveManager.save();
    eventBus.publish(EVENTS.STAFF_HIRED, { id });
    return { ok: true };
  }

  staffNextCost(id) {
    const def = staffById(id);
    const lvl = this.staffLevel(id);
    if (lvl <= 0 || lvl >= MAX_LEVEL) return null;
    return def.levels[lvl].cost; // levels[lvl] = bir sonraki seviye (0-index)
  }

  upgradeStaff(id) {
    const cost = this.staffNextCost(id);
    if (cost == null) return { ok: false, reason: 'max' };
    if (!EconomyManager.deductCoins(cost)) return { ok: false, reason: 'coins' };
    const st = SaveManager.data.staff[id];
    st.level = Math.min(MAX_LEVEL, st.level + 1);
    SaveManager.save();
    eventBus.publish(EVENTS.STAFF_UPGRADED, { id, level: st.level });
    return { ok: true, level: st.level };
  }

  get activeStaffCount() { return STAFF.filter(s => this.isHired(s.id)).length; }

  get averageUpgradeLevel() {
    const eq = Object.values(SaveManager.data.equipment);
    const st = STAFF.filter(s => this.isHired(s.id)).map(s => this.staffLevel(s.id));
    const all = [...eq, ...st];
    return all.length ? all.reduce((a, b) => a + b, 0) / all.length : 0;
  }

  // ─────────── Türetilmiş değerler ───────────
  /** Fırın ayarları: slot / süre / yanma eşiği */
  get grillConfig() {
    const lvl = this.equipmentLevel('grill');
    const cfg = OVEN.levels[Math.min(lvl, OVEN.levels.length) - 1];
    return { ...cfg, level: lvl };
  }

  /** Kuyruk kapasitesi bonusu (tezgah seviyesi) */
  get queueBonus() { return this.equipmentLevel('counter') - 1; }

  /** Hazırlık hızı: tezgah Lv4+ → malzeme taşıma hızlanır (görsel/ipucu) */
  get prepSpeedMultiplier() { return 1 + (this.equipmentLevel('counter') - 1) * 0.06; }

  /** Toplam gelir çarpanı (kasa + kasiyer) */
  get incomeMultiplier() {
    let m = 1;
    m += (this.equipmentLevel('cashier_desk') - 1) * 0.08;          // kasa: her seviye +%8
    if (this.isHired('cashier')) {
      const lvl = this.staffLevel('cashier');                        // 1..5
      m += staffById('cashier').incomeBonus[lvl - 1];
    }
    return m;
  }

  /** Müşteri sabır çarpanı (temizlikçi) */
  get patienceMultiplier() {
    if (!this.isHired('cleaner')) return 1;
    return 1 + staffById('cleaner').patienceBonus[this.staffLevel('cleaner') - 1];
  }

  /** Aşçı: otomatik pişirme gecikmesi (saniye). null → kapalı */
  get autoCookDelay() {
    if (!this.isHired('cook')) return null;
    const arr = staffById('cook').autoCookDelay;
    return arr[this.staffLevel('cook') - 1];
  }

  /** Garson: otomatik servis gecikmesi (saniye). null → kapalı */
  get autoServeDelay() {
    if (!this.isHired('waiter')) return null;
    const arr = staffById('waiter').autoServeDelay;
    return arr[this.staffLevel('waiter') - 1];
  }

  /** Garson Lv4+ Drive-Thru kanalına da servis eder */
  get autoServeDriveThru() { return this.isHired('waiter') && this.staffLevel('waiter') >= 4; }

  /** Drive-Thru gelir bonusu */
  get driveThruBonus() {
    if (!this.isHired('driveThruStaff')) return 0;
    return staffById('driveThruStaff').driveThruBonus[this.staffLevel('driveThruStaff') - 1];
  }

  /** Garson bahşiş bonusu (Lv3+) */
  get tipBonus() {
    if (!this.isHired('waiter')) return 0;
    return this.staffLevel('waiter') >= 3 ? 0.15 : 0;
  }

  // ─────────── Offline verim katsayısı (GDD 2.7) ───────────
  get offlineEfficiency() {
    if (this.activeStaffCount === 0) return 0.15;
    if (this.averageUpgradeLevel >= 4) return 0.70;
    return 0.45;
  }

  isUnlocked(kind) {
    const lvl = SaveManager.data.currentLevel;
    return lvl >= (UNLOCKS[kind] || 1);
  }
}

export const UpgradeSystem = new UpgradeSystemCore();
export default UpgradeSystem;
