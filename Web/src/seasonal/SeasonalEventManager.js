/**
 * SeasonalEventManager.js — Sezonsal tema değişimi (GDD 10)
 * Ay bazlı etkinlik: mekan paletini ve vurgu rengini değiştirir.
 */
import { currentSeasonalEvent } from '../data/CosmeticData.js';
import { SaveManager } from '../core/SaveManager.js';

class SeasonalEventManagerCore {
  constructor() { this.active = null; }

  /** @param {object} world KitchenWorld */
  apply(world) {
    const evt = currentSeasonalEvent();
    this.active = evt;
    if (!evt) return null;
    // Oyuncu özel tema seçmediyse sezonsal palet uygulanır
    if (SaveManager.data.equippedTheme === 'default') {
      world.scene.background.setHex(evt.sky);
      world.scene.fog.color.setHex(evt.sky);
      world.accentStrip.material.color.setHex(evt.accent);
      world.accentStrip.material.emissive.setHex(evt.accent);
    }
    return evt;
  }
}

export const SeasonalEventManager = new SeasonalEventManagerCore();
export default SeasonalEventManager;
