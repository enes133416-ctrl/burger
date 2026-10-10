/**
 * UIManager.js — HUD + Screens + Panels'i bir araya toplayan köprü katmanı.
 * GameManager yalnızca bu sınıf üzerinden UI ile konuşur.
 */
import { HUD } from './HUD.js';
import { Screens } from './Screens.js';
import { Panels } from './Panels.js';
import { AdManager } from '../monetization/AdManager.js';
import { AudioManager } from '../audio/AudioManager.js';
import { OfflineEarningsCalculator } from '../economy/OfflineEarnings.js';
import { GameState } from '../core/GameManager.js';

export class UIManager {
  constructor() {
    this.game = null;
    this.adPause = false;
  }

  bind(game) {
    this.game = game;
    this.hud = new HUD(game, this);
    this.screens = new Screens(game, this);
    this.panels = new Panels(game, this);
    return this;
  }

  // ══════════════════ GameManager arayüzü ══════════════════
  onStateChanged(state) {
    this.hud.setVisible(state === GameState.Gameplay);
    if (state === GameState.Paused && !this.adPause) this.screens.showPause(true);
    if (state !== GameState.Paused) this.screens.showPause(false);
    if (state === GameState.Gameplay) this.refreshHUD();
  }

  toast(msg, kind = '') { this.hud.toast(msg, kind); }
  refreshHUD() { this.hud.refreshCurrency(); }
  showWorldTip(text) { this.hud.showTip(text); }
  showResult(data) { this.screens.showResult(data); }
  showFail(data) { this.screens.showFail(data); }
  hideScreens() { this.screens.hideAll(); }
  tick() { this.hud.tick(); }

  // ══════════════════ Reklam ══════════════════
  showRewardedAd(type, onDone) {
    const wasPlaying = this.game.state === GameState.Gameplay;
    if (wasPlaying) { this.adPause = true; this.game.pause(); }
    AudioManager.init();
    AdManager.showRewarded(type, () => {
      if (wasPlaying) {
        this.adPause = false;
        this.screens.showPause(false);
        this.game.resume();
      }
      if (onDone) onDone();
    });
  }

  // ══════════════════ Offline gelir ══════════════════
  showOffline() {
    const result = OfflineEarningsCalculator.calculate();
    if (result.earnings <= 0) return false;
    this.panels.showOffline(result);
    return true;
  }

  claimOffline(double) {
    const amount = OfflineEarningsCalculator.claim(double);
    if (amount > 0) {
      AudioManager.play('coin');
      this.toast(`+🪙 ${Math.round(amount)} offline gelir ${double ? '(2x)' : ''}`, 'good');
    }
    this.panels.close();
    this.refreshHUD();
  }

  // ══════════════════ Başlatma ══════════════════
  start() {
    this.hud.refreshCurrency();
    this.screens.runBoot(() => {
      this.screens.openMenu();
      this.showOffline();
    });
  }
}
