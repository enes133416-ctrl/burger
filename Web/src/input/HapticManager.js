/**
 * HapticManager.js — Dokunsal geri bildirim (GDD 2.3)
 * Web'de navigator.vibrate, mobilde aynı API Cordova/Capacitor ile çalışır.
 */
import { SaveManager } from '../core/SaveManager.js';

class HapticManager {
  get enabled() { return SaveManager.data.settings.haptic; }

  _vibrate(pattern) {
    if (!this.enabled) return;
    try { navigator.vibrate && navigator.vibrate(pattern); } catch (e) { /* sessiz */ }
  }

  light() { this._vibrate(12); }
  medium() { this._vibrate(25); }
  heavy() { this._vibrate([0, 30, 50, 30]); }
  success() { this._vibrate([12, 40, 12]); }
  error() { this._vibrate([60, 40, 60]); }
}

export const Haptic = new HapticManager();
export default Haptic;
