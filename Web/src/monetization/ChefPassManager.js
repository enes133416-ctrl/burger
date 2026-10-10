/**
 * ChefPassManager.js — Chef Pass süre kontrolü ve günlük ödül (GDD 4.3)
 */
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from '../economy/EconomyManager.js';

class ChefPassManagerCore {
  /** Açılışta çağrılır: süre dolduysa pası kapatır, geçerliyse günlük ödülü verir. */
  check() {
    const data = SaveManager.data;
    if (!data.hasChefPass) return null;
    if (Date.now() > data.chefPassExpiry) {
      data.hasChefPass = false;
      SaveManager.save();
      return { expired: true };
    }
    const now = Date.now();
    const lastClaim = data.lastDailyClaim || 0;
    if (now - lastClaim > 20 * 3600 * 1000) {
      data.lastDailyClaim = now;
      SaveManager.save();
      const reward = EconomyManager.claimChefPassDaily();
      return { claimed: reward };
    }
    return { active: true };
  }

  get daysLeft() {
    if (!SaveManager.data.hasChefPass) return 0;
    return Math.max(0, Math.ceil((SaveManager.data.chefPassExpiry - Date.now()) / 86400000));
  }
}

export const ChefPassManager = new ChefPassManagerCore();
export default ChefPassManager;
