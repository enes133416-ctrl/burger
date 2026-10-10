/**
 * AdManager.js — Reklam yönetimi (GDD 4.2)
 * NOT: Web sürümünde SDK yoktur; reklamlar 3 saniyelik simülasyondur.
 * Unity sürümünde bu sınıf MAX/IronSource/AdMob çağrılarıyla değiştirilir.
 */
import { eventBus, EVENTS } from '../core/EventBus.js';
import { AD_FREQUENCY, REWARD_TYPES } from '../core/Config.js';
import { AnalyticsManager } from '../analytics/AnalyticsManager.js';
import { SaveManager } from '../core/SaveManager.js';

class AdManagerCore {
  constructor() {
    this.levelsSinceLastAd = 0;
    this.isInitialized = false;
    this.overlay = null;
    this.countdown = 0;
    this.pendingType = null;
    this.onComplete = null;
  }

  init() {
    if (this.overlay) return;
    this.overlay = document.getElementById('ad-overlay');
    this.adTitle = document.getElementById('ad-title');
    this.adTimer = document.getElementById('ad-timer');
    this.closeBtn = document.getElementById('btn-ad-close');
    this.closeBtn.addEventListener('click', () => this._finish(true));
    this.isInitialized = true;
  }

  /** Chef Pass: reklam sıklığı yarıya iner */
  get adFrequency() {
    const hasPass = SaveManager.data.hasChefPass && Date.now() < SaveManager.data.chefPassExpiry;
    return hasPass ? AD_FREQUENCY * 2 : AD_FREQUENCY;
  }

  // ─────────── Interstitial ───────────
  tryShowInterstitial() {
    this.levelsSinceLastAd++;
    if (this.levelsSinceLastAd < this.adFrequency) return false;
    this.levelsSinceLastAd = 0;
    this.showInterstitial();
    return true;
  }

  showInterstitial() {
    AnalyticsManager.trackAdShown('interstitial');
    this._show('Geçiş Reklamı', 3, () => {
      AnalyticsManager.track('ad_interstitial_closed');
    });
  }

  // ─────────── Rewarded ───────────
  showRewarded(type, onComplete) {
    AnalyticsManager.trackAdShown(`rewarded:${type}`);
    this.pendingType = type;
    this.onComplete = onComplete;
    this._show(REWARD_LABELS[type] || 'Ödüllü Reklam', 3, (finished) => {
      if (!finished) return;
      eventBus.publish(EVENTS.AD_REWARDED, { type });
      AnalyticsManager.trackAdWatched(type);
      if (this.onComplete) this.onComplete();
      this.onComplete = null;
    });
  }

  isRewardedReady() { return this.isInitialized; }

  _show(title, seconds, onClose) {
    if (!this.isInitialized) { onClose && onClose(false); return; }
    this.adTitle.textContent = title;
    this.countdown = seconds;
    this.overlay.classList.remove('hidden');
    this.adTimer.textContent = String(seconds);
    this.closeBtn.classList.add('hidden');

    clearInterval(this._timer);
    this._timer = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(this._timer);
        this.closeBtn.classList.remove('hidden');
        this._finish(true, onClose);
      } else {
        this.adTimer.textContent = String(this.countdown);
      }
    }, 1000);

    this._onCloseCb = onClose;
  }

  _finish(finished, cb) {
    clearInterval(this._timer);
    this.overlay.classList.add('hidden');
    const fn = cb || this._onCloseCb;
    this._onCloseCb = null;
    if (fn) fn(finished);
  }
}

const REWARD_LABELS = {
  [REWARD_TYPES.DoubleIncome]: '2x Gelir (3 dk)',
  [REWARD_TYPES.DoubleOfflineEarnings]: 'Offline Geliri 2x',
  [REWARD_TYPES.SlowPatience]: 'Sabır Barı Yavaşlatma',
  [REWARD_TYPES.RefillOven]: 'Fırınları Anında Doldur',
  [REWARD_TYPES.ReviveLevel]: 'Bölüme Devam Et',
  [REWARD_TYPES.SpeedBoost]: 'Hız Bonusu',
};

export const AdManager = new AdManagerCore();
export default AdManager;
