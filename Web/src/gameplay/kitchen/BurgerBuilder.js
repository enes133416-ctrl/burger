/**
 * BurgerBuilder.js — Katmanlama & sipariş doğrulama (GDD 2.3)
 * Katman sırası: Alt ekmek → köfte → (peynir/marul/domates/sos) → üst ekmek
 */
import { INGREDIENTS } from '../../core/Config.js';

export class BurgerBuilder {
  constructor() { this.layers = []; }   // [{id, state:'raw'|'cooked'|'burnt'}]

  get isEmpty() { return this.layers.length === 0; }

  get layerIds() { return this.layers.map(l => l.id); }

  /** Buna eklenen ilk ekmek alt, son ekmek üst ekmektir */
  get fillings() {
    const ids = this.layerIds;
    const firstBun = ids.indexOf('bun');
    const lastBun = ids.lastIndexOf('bun');
    if (firstBun === -1) return ids;
    return ids.filter((_, i) => i !== firstBun && i !== lastBun);
  }

  get hasBottomBun() { return this.layerIds[0] === 'bun'; }
  get hasTopBun() { return this.layerIds.length > 1 && this.layerIds[this.layerIds.length - 1] === 'bun'; }
  get hasPatty() { return this.fillings.includes('patty'); }
  get isComplete() { return this.hasBottomBun && this.hasTopBun && this.hasPatty; }

  get isBurnt() { return this.layers.some(l => l.state === 'burnt'); }
  get hasRawPatty() { return this.layers.some(l => l.id === 'patty' && l.state === 'raw'); }

  add(id, state = 'raw') {
    this.layers.push({ id, state });
    return this;
  }

  clear() { this.layers = []; }

  toJSON() { return this.layers.map(l => ({ ...l })); }

  /** Görsel değer (yanmış ürünün parası) */
  get value() {
    return this.layers.reduce((sum, l) => sum + (INGREDIENTS[l.id]?.value || 0), 0);
  }

  /**
   * Siparişle karşılaştır.
   * @returns {{ok:boolean, reason?:string, missing:string[], extra:string[], tipFactor:number}}
   */
  evaluate(order) {
    const provided = this.fillings;
    const required = order.fillings;

    if (!this.hasBottomBun || !this.hasTopBun) return { ok: false, reason: 'Ekmek eksik!', missing: required, extra: [], tipFactor: 0 };
    if (!this.hasPatty) return { ok: false, reason: 'Köfte yok!', missing: required, extra: [], tipFactor: 0 };
    if (this.hasRawPatty) return { ok: false, reason: 'Köfte çiğ!', missing: required, extra: [], tipFactor: 0 };
    if (this.isBurnt) return { ok: false, reason: 'Yanmış köfte!', missing: required, extra: [], tipFactor: 0 };

    const pool = [...required];
    const extra = [];
    for (const f of provided) {
      const idx = pool.indexOf(f);
      if (idx >= 0) pool.splice(idx, 1);
      else extra.push(f);
    }
    const missing = pool;
    return {
      ok: missing.length === 0,
      reason: missing.length ? 'Yanlış sipariş!' : null,
      missing,
      extra,
      tipFactor: extra.length * 0.1,
    };
  }

  /** Otomatik servis (garson) için: sipariş birebir eşleşiyor mu */
  matches(order) {
    const res = this.evaluate(order);
    return res.ok;
  }
}

/** Sipariş üretimi (GDD 2.5) */
export function generateOrder(level, customerType) {
  const pool = level.ingredients.filter(i => i !== 'bun');
  const maxFilling = Math.min(level.maxFilling, pool.length);
  const count = 1 + Math.floor(Math.random() * maxFilling);
  const shuffled = [...pool].sort(() => Math.random() - 0.5);
  const fillings = shuffled.slice(0, count);
  if (!fillings.includes('patty')) fillings.unshift('patty');        // köfte zorunlu

  const isCombo = Math.random() < (level.comboChance || 0);
  const baseValue = fillings.reduce((s, f) => s + (INGREDIENTS[f]?.value || 1), 0);
  const basePrice = Math.round(
    (8 + baseValue * 3.2) * level.priceScale * customerType.pay * (isCombo ? 1.5 : 1),
  );

  return {
    fillings,
    isCombo,
    basePrice,
    label: fillings.map(f => INGREDIENTS[f].emoji).join(''),
  };
}
