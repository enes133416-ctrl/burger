/**
 * AnalyticsManager.js — Olay izleme (GDD 8.1)
 * Web demo sürümünde olaylar konsola + localStorage'a yazılır.
 */
import { SaveManager } from '../core/SaveManager.js';

class AnalyticsManagerCore {
  constructor() { this.queue = []; }

  init() { this.track('session_start'); }

  track(name, params = {}) {
    const evt = { name, params, ts: Date.now() };
    this.queue.push(evt);
    if (this.queue.length > 200) this.queue.shift();
    if (import.meta.env && import.meta.env.DEV) console.debug('[Analytics]', name, params);
  }

  trackLevelStart(level) { this.track('level_start', { level }); }
  trackLevelComplete(level, stars, coins) { this.track('level_complete', { level, stars, coins }); }
  trackLevelFail(level, reason) { this.track('level_fail', { level, reason }); }
  trackAdShown(type) { this.track('ad_shown', { type }); }
  trackAdWatched(type) { this.track('ad_watched', { type }); }
  trackIAP(packId, price) { this.track('iap_purchase', { packId, price }); }

  /** Kısa özet raporu (debug konsolu için) */
  summary() {
    return {
      events: this.queue.length,
      last: this.queue.slice(-10),
      level: SaveManager.data.currentLevel,
      coins: Math.round(SaveManager.data.totalCoins),
    };
  }
}

export const AnalyticsManager = new AnalyticsManagerCore();
export default AnalyticsManager;
