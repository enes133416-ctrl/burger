/**
 * HUD.js — Oyun içi üst bar, sayaçlar, alt butonlar ve bildirimler (GDD 5.1)
 */
import { eventBus, EVENTS } from '../core/EventBus.js';
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from '../economy/EconomyManager.js';
import { UpgradeSystem } from '../economy/UpgradeSystem.js';
import { AudioManager } from '../audio/AudioManager.js';
import { UNLOCKS } from '../core/Config.js';

export class HUD {
  constructor(game, ui) {
    this.game = game;
    this.ui = ui;
    this.el = document.getElementById('hud');
    this.coinsEl = document.querySelector('#hud-coins span');
    this.gemsEl = document.querySelector('#hud-gems span');
    this.levelLabel = document.getElementById('hud-level-label');
    this.timerEl = document.getElementById('hud-timer');
    this.progressEl = document.getElementById('hud-progress');
    this.goalEl = document.getElementById('hud-goal');
    this.boost2x = document.getElementById('boost-2x');
    this.boostSlow = document.getElementById('boost-slow');
    this.toasts = document.getElementById('toasts');

    this.tip = document.createElement('div');
    this.tip.className = 'world-tip hidden';
    this.el.appendChild(this.tip);

    eventBus.subscribe(EVENTS.COIN_CHANGED, () => this.refreshCurrency());
    eventBus.subscribe(EVENTS.GEM_CHANGED, () => this.refreshCurrency());
    this.bindButtons();
  }

  bindButtons() {
    const click = (id, fn) => {
      const el = document.getElementById(id);
      if (el) el.addEventListener('click', () => { AudioManager.play('click'); fn(); });
    };

    click('btn-pause', () => this.game.pause());
    click('btn-hr', () => {
      if (SaveManager.data.currentLevel < UNLOCKS.hrOffice) return this.ui.toast(`HR Office bölüm ${UNLOCKS.hrOffice}'te açılır 🔒`, 'bad');
      this.ui.panels.open('hr');
    });
    click('btn-upgrade', () => {
      if (SaveManager.data.currentLevel < UNLOCKS.upgradeOffice) return this.ui.toast(`Upgrade Office bölüm ${UNLOCKS.upgradeOffice}'te açılır 🔒`, 'bad');
      this.ui.panels.open('upgrade');
    });
    click('btn-ad', () => this.ui.showRewardedAd('DoubleIncome'));
    click('btn-gem-cook', () => this.game.buyInstantCook());
    click('btn-gem-patience', () => this.game.buySlowPatience());
  }

  setVisible(v) { this.el.classList.toggle('hidden', !v); }

  refreshCurrency() {
    this.coinsEl.textContent = formatNumber(EconomyManager.coins);
    this.gemsEl.textContent = formatNumber(EconomyManager.gems);
  }

  /** Her kare çağrılır (timer & boost göstergeleri) */
  tick() {
    const game = this.game;
    if (!game.level) return;
    this.levelLabel.textContent = `Level ${game.level.index}`;
    const t = Math.max(0, game.timeLeft);
    this.timerEl.textContent = `${String(Math.floor(t / 60)).padStart(2, '0')}:${String(Math.floor(t % 60)).padStart(2, '0')}`;
    this.timerEl.style.color = t < 20 ? '#E74C3C' : '';
    this.progressEl.style.width = `${Math.round(game.progressRatio * 100)}%`;
    const s = game.stats;
    this.goalEl.textContent = `Sipariş ${s.served}/${game.level.orders} · Hedef ${game.level.minServe} · Kaçan ${s.lost}`;

    // Boost etiketleri
    const dbl = EconomyManager.doubleIncomeTimer;
    this.boost2x.classList.toggle('hidden', dbl <= 0);
    if (dbl > 0) this.boost2x.querySelector('span').textContent = `${Math.ceil(dbl)}s`;

    const slow = game.slowPatienceTimer;
    this.boostSlow.classList.toggle('hidden', slow <= 0);
    if (slow > 0) this.boostSlow.querySelector('span').textContent = `${Math.ceil(slow)}s`;

    // Kilitli butonlar
    const lvl = SaveManager.data.currentLevel;
    document.getElementById('btn-hr').classList.toggle('locked', lvl < UNLOCKS.hrOffice);
    document.getElementById('btn-upgrade').classList.toggle('locked', lvl < UNLOCKS.upgradeOffice);
  }

  showTip(text) {
    if (!text) { this.tip.classList.add('hidden'); return; }
    this.tip.textContent = text;
    this.tip.classList.remove('hidden');
    clearTimeout(this._tipTimer);
    this._tipTimer = setTimeout(() => this.tip.classList.add('hidden'), 7000);
  }

  toast(message, kind = '') {
    const el = document.createElement('div');
    el.className = `toast ${kind}`;
    el.textContent = message;
    this.toasts.appendChild(el);
    setTimeout(() => el.remove(), 1900);
    while (this.toasts.children.length > 4) this.toasts.firstChild.remove();
  }
}

export function formatNumber(n) {
  n = Math.round(n);
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 10_000) return `${(n / 1000).toFixed(1)}K`;
  return n.toLocaleString('tr-TR');
}
