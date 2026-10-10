/**
 * EventBus.js — Bağımlılıksız merkezi event sistemi (GDD 8.2)
 * Sistemler birbirini doğrudan referans etmek yerine event publish/subscribe eder.
 */
class EventBus {
  constructor() { this.handlers = new Map(); }

  subscribe(type, handler) {
    if (!this.handlers.has(type)) this.handlers.set(type, new Set());
    this.handlers.get(type).add(handler);
    return () => this.unsubscribe(type, handler);
  }

  unsubscribe(type, handler) {
    const set = this.handlers.get(type);
    if (set) set.delete(handler);
  }

  publish(type, payload) {
    const set = this.handlers.get(type);
    if (!set) return;
    for (const h of [...set]) {
      try { h(payload); } catch (err) { console.error(`[EventBus] ${type} handler hatası:`, err); }
    }
  }

  clear() { this.handlers.clear(); }
}

export const eventBus = new EventBus();

/** Event adları — merkezi sabitler */
export const EVENTS = {
  COIN_CHANGED: 'coin:changed',
  GEM_CHANGED: 'gem:changed',
  CUSTOMER_SERVED: 'customer:served',
  CUSTOMER_LEFT: 'customer:left',
  FOOD_BURNT: 'food:burnt',
  LEVEL_START: 'level:start',
  LEVEL_COMPLETE: 'level:complete',
  LEVEL_FAIL: 'level:fail',
  UPGRADE_PURCHASED: 'upgrade:purchased',
  STAFF_HIRED: 'staff:hired',
  STAFF_UPGRADED: 'staff:upgraded',
  STATE_CHANGED: 'game:state',
  TOAST: 'ui:toast',
  AD_REWARDED: 'ad:rewarded',
  COSMETIC_EQUIPPED: 'cosmetic:equipped',
};
