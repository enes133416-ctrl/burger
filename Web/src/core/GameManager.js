/**
 * GameManager.js — Oyun durum makinesi & ana döngü (GDD 8.2)
 * Boot → MainMenu → WorldMap → Gameplay → Paused / LevelComplete / LevelFail
 */
import * as THREE from 'three';
import { KitchenWorld } from '../gameplay/KitchenWorld.js';
import { OvenController, SlotState } from '../gameplay/kitchen/OvenController.js';
import { AssemblyStation } from '../gameplay/kitchen/AssemblyStation.js';
import { BurgerBuilder } from '../gameplay/kitchen/BurgerBuilder.js';
import { CustomerManager } from '../gameplay/customer/CustomerManager.js';
import { StaffManager } from '../gameplay/staff/StaffManager.js';
import { InputController } from '../input/InputController.js';
import { EconomyManager } from '../economy/EconomyManager.js';
import { UpgradeSystem } from '../economy/UpgradeSystem.js';
import { OfflineEarningsCalculator } from '../economy/OfflineEarnings.js';
import { AudioManager } from '../audio/AudioManager.js';
import { AdManager } from '../monetization/AdManager.js';
import { AnalyticsManager } from '../analytics/AnalyticsManager.js';
import { eventBus, EVENTS } from './EventBus.js';
import { SaveManager } from './SaveManager.js';
import { getLevel, computeStars } from '../data/LevelData.js';
import { locationForLevel, locationById } from '../data/Locations.js';
import { ECONOMY, GEM_COSTS, REWARD_TYPES, INGREDIENTS, UNLOCKS, PATIENCE } from './Config.js';
import { Haptic } from '../input/HapticManager.js';

export const GameState = {
  Boot: 'Boot',
  MainMenu: 'MainMenu',
  WorldMap: 'WorldMap',
  Gameplay: 'Gameplay',
  Paused: 'Paused',
  LevelComplete: 'LevelComplete',
  LevelFail: 'LevelFail',
};

export class GameManager {
  constructor(container, ui) {
    this.ui = ui;
    this.state = GameState.Boot;
    this.world = new KitchenWorld(container);
    this.oven = new OvenController(this.world);
    this.assembly = new AssemblyStation(this.world);
    this.customers = new CustomerManager(this.world);
    this.staff = new StaffManager(this.world);
    this.input = new InputController(this.world, this);

    this.level = null;
    this.timeLeft = 0;
    this.stats = null;
    this.slowPatienceTimer = 0;
    this.pendingOffline = null;

    this.customers.onCustomerLeft = (c) => this.onCustomerLost(c);
    eventBus.subscribe(EVENTS.FOOD_BURNT, () => {
      SaveManager.data.stats.burnt++;
      if (this.stats) this.stats.burnt++;
    });
    eventBus.subscribe(EVENTS.AD_REWARDED, ({ type }) => this.applyReward(type));

    this.staff.refreshAvatars();
    this.world.layoutGrillSlots(this.oven.config.slots);
  }

  // ══════════════════ Durum makinesi ══════════════════
  changeState(next) {
    this.state = next;
    eventBus.publish(EVENTS.STATE_CHANGED, { state: next });
    this.ui.onStateChanged(next);
  }

  // ══════════════════ Bölüm yaşam döngüsü ══════════════════
  startLevel(index) {
    this.level = getLevel(index);
    SaveManager.data.currentLevel = index;
    SaveManager.save();

    this.timeLeft = this.level.duration;
    this.stats = { served: 0, lost: 0, complaints: 0, earned: 0, burnt: 0, total: this.level.orders };

    this.assembly.clear();
    this.oven.applyLevel(UpgradeSystem.grillConfig.level, true);
    this.oven.reset();
    this.customers.start(this.level);
    this.staff.refreshAvatars();
    this.staff.cookTimer = 0;
    this.staff.serveTimer = 0;

    // Lokasyon paleti + tema
    const loc = locationForLevel(index);
    const theme = SaveManager.data.equippedTheme;
    if (theme && theme !== 'default') this.world.setTheme(theme);
    else this.world.applyLocationPalette(loc);

    this.world.chefTargetX = 1.5;
    AudioManager.playMusic(this.level.isBoss ? 'rush' : 'gameplay');
    AnalyticsManager.trackLevelStart(index);
    this.changeState(GameState.Gameplay);
    this.ui.showWorldTip(this.tutorialTip());
  }

  tutorialTip() {
    const lvl = this.level.index;
    if (lvl === 1) return 'Malzeme kutusuna dokun → sonra tabağa dokun. Köfteyi önce ızgaraya koy!';
    if (lvl === 2) return 'Yeni malzeme: 🧈 Peynir. Sipariş balonuna bak!';
    if (lvl === 3) return 'HR Office\'ten kasiyer kiralayarak gelirini artır.';
    if (lvl === 4) return '🥬 Marul ve 🍅 Domates açıldı.';
    if (lvl === 5) return '⭐ BOSS: Rush Hour! Hızlı servis = yüksek bahşiş.';
    if (lvl === 8) return '🚗 Drive-Thru açıldı! Arabalar daha çabuk sıkılır.';
    if (lvl === 9) return '👨‍🍳 HR Office açıldı: personel kiralayıp yükselt.';
    if (lvl === 14) return '🛠️ Upgrade Office açıldı: ekipmanlarını güçlendir.';
    return null;
  }

  restartLevel() { this.startLevel(this.level ? this.level.index : 1); }

  pause() {
    if (this.state !== GameState.Gameplay) return;
    this.changeState(GameState.Paused);
  }

  resume() {
    if (this.state !== GameState.Paused) return;
    this.changeState(GameState.Gameplay);
  }

  quitToMenu() {
    this.customers.reset();
    this.assembly.clear();
    this.oven.reset();
    AudioManager.playMusic('menu');
    this.changeState(GameState.MainMenu);
  }

  completeLevel() {
    if (this.state !== GameState.Gameplay) return;
    const stars = computeStars(this.level, this.stats.served, this.stats.total, this.stats.complaints, this.stats.lost);
    const prevStars = SaveManager.data.levelStars[this.level.index] || 0;
    if (stars > prevStars) SaveManager.data.totalStars += stars - prevStars;
    SaveManager.data.levelStars[this.level.index] = Math.max(prevStars, stars);
    SaveManager.data.maxLevelReached = Math.max(SaveManager.data.maxLevelReached, Math.min(50, this.level.index + 1));

    // Milestone ödülleri (GDD 3.1)
    let bonus = 0;
    if (this.level.isMilestone) {
      bonus = this.level.index === 10 ? 5000 : this.level.index === 20 ? 15000 : this.level.index === 30 ? 40000 : 100000;
      EconomyManager.addCoins(bonus, 'milestone');
      this.unlockCosmeticForMilestone(this.level.index);
    }

    SaveManager.save();
    AudioManager.play(stars > 0 ? 'victory' : 'fail');
    AudioManager.playMusic('menu');
    AnalyticsManager.trackLevelComplete(this.level.index, stars, this.stats.earned);
    this.changeState(GameState.LevelComplete);

    const showAd = AdManager.tryShowInterstitial();
    this.ui.showResult({
      success: true, level: this.level, stars, bonus,
      served: this.stats.served, total: this.stats.total,
      lost: this.stats.lost, complaints: this.stats.complaints,
      earned: this.stats.earned, showAd,
    });
  }

  failLevel(reason = 'time') {
    if (this.state !== GameState.Gameplay) return;
    AudioManager.play('fail');
    AudioManager.playMusic('menu');
    AnalyticsManager.trackLevelFail(this.level.index, reason);
    this.changeState(GameState.LevelFail);
    this.ui.showFail({ level: this.level, served: this.stats.served, total: this.stats.total, minServe: this.level.minServe });
  }

  unlockCosmeticForMilestone(index) {
    const unlocks = { 10: 'vintage', 20: 'ninja', 30: 'tropical', 40: 'robot', 50: 'space' };
    const id = unlocks[index];
    if (!id) return;
    if (!SaveManager.data.unlockedCosmetics.includes(id)) {
      SaveManager.data.unlockedCosmetics.push(id);
      this.ui.toast(`🎁 Kozmetik açıldı: ${id}`, 'good');
    }
  }

  // ══════════════════ Ana döngü ══════════════════
  update(dt) {
    EconomyManager.update(dt);
    this.input.update(dt);

    if (this.state === GameState.Gameplay) {
      this.timeLeft -= dt;
      SaveManager.data.stats.playSeconds += dt;

      if (this.slowPatienceTimer > 0) {
        this.slowPatienceTimer -= dt;
        if (this.slowPatienceTimer <= 0) this.customers.clearSlowPatience();
      }

      this.customers.update(dt);
      this.oven.update(dt);
      this.staff.update(dt, {
        oven: this.oven,
        assembly: this.assembly,
        customers: this.customers,
        onAutoServe: (c) => this.autoServe(c),
      });

      // Bölüm bitiş koşulları
      if (this.customers.allResolved || (this.stats.served + this.stats.lost) >= this.level.orders) {
        if (this.stats.served >= this.level.minServe) this.completeLevel();
        else this.failLevel('orders');
      } else if (this.timeLeft <= 0) {
        if (this.stats.served >= this.level.minServe) this.completeLevel();
        else this.failLevel('time');
      }
    } else if (this.state === GameState.Paused) {
      // duraklatıldı: hiçbir oyun sistemi güncellenmez
    }

    this.world.update(dt);
  }

  // ══════════════════ Etkileşim (Input → Game) ══════════════════
  /** @returns {object|null} elde tutulan öğe */
  handlePick(target) {
    const { info, customer } = target;
    switch (info.kind) {
      case 'bin': {
        if (!this.level || !this.level.ingredients.includes(info.ingredient)) {
          this.ui.toast('Bu malzeme henüz açılmadı', 'bad');
          return null;
        }
        this.moveChefTo(info.ingredient);
        return { kind: 'ingredient', id: info.ingredient, state: 'raw' };
      }
      case 'grillSlot': {
        const slot = this.oven.slots[info.index];
        if (!slot) return null;
        if (slot.state === SlotState.Ready || slot.state === SlotState.Burnt) {
          const item = this.oven.collect(info.index);
          if (item) return { kind: 'patty', state: item.state };
          return null;
        }
        if (slot.state === SlotState.Cooking) { this.ui.toast('Pişiyor… ⏳'); return null; }
        return null;
      }
      case 'tray': {
        const item = this.oven.takeFromTray();
        if (item) return { kind: 'patty', state: item.state };
        return null;
      }
      case 'plate': {
        if (this.assembly.isEmpty) return null;
        if (!this.assembly.builder.isComplete) { this.ui.toast('Üst ekmek eksik! 🍞', 'bad'); return null; }
        const layers = this.assembly.takeBurger();
        if (layers) return { kind: 'burger', layers };
        return null;
      }
      case 'customer': {
        if (customer) this.ui.toast(`Sipariş: ${customer.order.fillings.map(f => INGREDIENTS[f].emoji).join(' ')} — 🪙${customer.order.basePrice}`);
        return null;
      }
      default: return null;
    }
  }

  /** @returns {boolean} öğe tüketildi mi */
  handleDrop(item, target) {
    const { info, customer } = target;
    switch (info.kind) {
      case 'plate': {
        if (item.kind === 'burger') { this.ui.toast('Zaten tabakta'); return false; }
        if (item.kind === 'patty') {
          if (item.state === 'burnt') { this.ui.toast('Yanmış köfte! Çöpe at 🗑️', 'bad'); return false; }
          this.assembly.addLayer('patty', item.state);
          return true;
        }
        if (item.id === 'patty') { this.ui.toast('Köfte çiğ! Izgaraya koy 🔥', 'bad'); return false; }
        this.assembly.addLayer(item.id, 'raw');
        return true;
      }
      case 'grillSlot': {
        if (item.kind === 'ingredient' && item.id === 'patty') {
          const slot = this.oven.slots[info.index];
          if (!slot || slot.state !== SlotState.Empty) { this.ui.toast('Slot dolu'); return false; }
          this.oven.placeRaw();
          return true;
        }
        if (item.kind === 'patty') { this.ui.toast('Pişmiş köfteyi tabağa koy 🍽️'); return false; }
        return false;
      }
      case 'trash': {
        this.ui.toast('Çöpe atıldı 🗑️');
        return true;   // öğeyi yok et
      }
      case 'customer': {
        if (item.kind !== 'burger') { this.ui.toast('Önce burgeri tamamla!', 'bad'); return false; }
        if (!customer || customer.state !== 'waiting') return false;
        this.serveCustomer(customer, item.layers);
        return true;
      }
      default: return false;
    }
  }

  moveChefTo(ingredientId) {
    const bin = this.world.bins[ingredientId];
    if (bin) this.world.chefTargetX = bin.group.position.x;
  }

  // ══════════════════ Servis & ekonomi ══════════════════
  burgerBuilderFromLayers(layers) {
    const b = new BurgerBuilder();
    layers.forEach(l => b.add(l.id, l.state));
    return b;
  }

  autoServe(customer) {
    const layers = this.assembly.takeBurger();
    if (!layers) return;
    this.serveCustomer(customer, layers);
  }

  serveCustomer(customer, layers) {
    const builder = this.burgerBuilderFromLayers(layers);
    const result = builder.evaluate(customer.order);
    const wrong = !result.ok;

    let pay = customer.order.basePrice;
    let tip = 0;

    if (wrong) {
      pay *= ECONOMY.wrongOrderPenalty;
      this.stats.complaints++;
      this.ui.toast(`😡 ${result.reason}`, 'bad');
      Haptic.error();
    } else {
      const ratio = customer.patienceRatio;
      tip = ratio > PATIENCE.happyTipAt ? pay * 0.5 : ratio > PATIENCE.okTipAt ? pay * 0.2 : 0;
      tip += pay * (result.extra.length * ECONOMY.extraLayerTip);
      tip += pay * UpgradeSystem.tipBonus;
      this.ui.toast(`+🪙${Math.round(pay + tip)} ${tip > 0 ? `(bahşiş ${Math.round(tip)})` : ''}`, 'good');
      Haptic.success();
    }

    let mult = EconomyManager.totalIncomeMultiplier;
    if (customer.channel === 'driveThru') mult *= (1 + UpgradeSystem.driveThruBonus) * this.customers.channels.driveThruMultiplier;

    const total = Math.max(1, Math.round((pay + tip) * mult));
    EconomyManager.addCoins(total, 'order');
    this.stats.earned += total;
    this.stats.served++;
    SaveManager.data.stats.served++;

    if (customer.channel === 'driveThru') this.customers.channels.registerDriveThruComplete();

    AudioManager.play(wrong ? 'angry' : 'coin');
    if (!wrong) AudioManager.play('happy');

    customer.serve(wrong);
    this.moveChefTo('bun');
    this.ui.refreshHUD();
  }

  onCustomerLost(customer) {
    this.stats.lost++;
    this.ui.toast('😞 Müşteri terk etti!', 'bad');
    AudioManager.play('angry');
    this.ui.refreshHUD();
  }

  // ══════════════════ Ödüller / gem ══════════════════
  applyReward(type) {
    switch (type) {
      case REWARD_TYPES.DoubleIncome:
        EconomyManager.activateDoubleIncome(ECONOMY.doubleIncomeDuration);
        this.ui.toast('🎬 2x Gelir aktif (3 dk)', 'good');
        break;
      case REWARD_TYPES.DoubleOfflineEarnings:
        OfflineEarningsCalculator.recalculateWithAdBonus();
        this.ui.claimOffline(true);
        break;
      case REWARD_TYPES.SlowPatience:
        this.slowPatienceTimer = ECONOMY.slowPatienceDuration;
        this.customers.applySlowPatience(0.5);
        this.ui.toast('🐢 Sabır barı yavaşladı (30 sn)', 'good');
        break;
      case REWARD_TYPES.RefillOven: {
        let n = 0;
        while (this.oven.placeRaw() && n < 6) n++;
        this.oven.instantFinishAll();
        this.ui.toast(`🔥 ${n} köfte hazır!`, 'good');
        break;
      }
      case REWARD_TYPES.ReviveLevel:
        this.timeLeft = Math.max(this.timeLeft, 45);
        this.ui.hideScreens();
        this.changeState(GameState.Gameplay);
        this.ui.toast('💚 Bölüme devam!', 'good');
        break;
      default: break;
    }
    this.ui.refreshHUD();
  }

  buyInstantCook() {
    if (!EconomyManager.spendGems(GEM_COSTS.instantCook)) { this.ui.toast('Yetersiz gem 💎', 'bad'); return; }
    const n = this.oven.instantFinishAll();
    this.ui.toast(n ? `⚡ ${n} köfte pişti!` : 'Pişen köfte yok', n ? 'good' : '');
    AudioManager.play('powerup');
  }

  buySlowPatience() {
    if (!EconomyManager.spendGems(GEM_COSTS.resetPatience)) { this.ui.toast('Yetersiz gem 💎', 'bad'); return; }
    this.customers.resetPatienceAll();
    this.ui.toast('💎 Sabrınız tazelendi!', 'good');
    AudioManager.play('sparkle');
  }

  skipLevelWithGems() {
    if (!EconomyManager.spendGems(GEM_COSTS.skipLevel)) { this.ui.toast('Yetersiz gem 💎', 'bad'); return; }
    const idx = this.level.index;
    SaveManager.data.levelStars[idx] = Math.max(SaveManager.data.levelStars[idx] || 0, 1);
    SaveManager.data.maxLevelReached = Math.max(SaveManager.data.maxLevelReached, Math.min(50, idx + 1));
    SaveManager.save();
    this.startLevel(Math.min(50, idx + 1));
  }

  /** İlerleme yüzdesi (HUD) */
  get progressRatio() {
    if (!this.level) return 0;
    return Math.min(1, (this.stats.served + this.stats.lost) / this.level.orders);
  }
}
