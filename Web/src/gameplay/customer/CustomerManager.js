/**
 * CustomerManager.js — Müşteri spawn, kuyruk ve istatistik yönetimi (GDD 2.2 / 2.5)
 */
import { CustomerAI, CustomerState } from './CustomerAI.js';
import { generateOrder } from '../kitchen/BurgerBuilder.js';
import { ChannelManager } from '../channel/ChannelManager.js';
import { CUSTOMER_TYPES, CHANNELS } from '../../core/Config.js';
import { SaveManager } from '../../core/SaveManager.js';

export class CustomerManager {
  constructor(world) {
    this.world = world;
    this.channels = new ChannelManager(world);
    this.customers = [];
    this.reset();
  }

  reset() {
    this.customers.forEach(c => c.dispose());
    this.customers = [];
    this.spawned = 0;
    this.served = 0;
    this.lost = 0;
    this.complaints = 0;
    this.spawnTimer = 1.0;
    this.level = null;
  }

  start(level) {
    this.reset();
    this.level = level;
    this.channels.setup(level);
    this.spawnTimer = 1.0;
  }

  update(dt) {
    if (!this.level) return;

    // Spawn
    if (this.spawned < this.level.orders) {
      this.spawnTimer -= dt;
      if (this.spawnTimer <= 0) {
        const spawned = this.spawnOne();
        this.spawnTimer = spawned ? this.level.spawnInterval : 1.0;
      }
    }

    // Müşteri güncelleme
    for (const c of [...this.customers]) {
      const before = c.state;
      c.update(dt);
      if (before === CustomerState.Waiting && c.state === CustomerState.Angry) {
        this.lost++;
        SaveManager.data.stats.lost++;
        if (this.onCustomerLeft) this.onCustomerLeft(c);
      }
      if (c.state === CustomerState.Done) {
        this.release(c);
      }
    }
  }

  spawnOne() {
    const channel = this.channels.pickChannel();
    if (!channel) return false;

    const types = this.level.customerTypes;
    let typeId = types[Math.floor(Math.random() * types.length)];
    // Chef Pass: VIP spawn şansı +%20
    if (SaveManager.data.hasChefPass && types.includes('VIP') && Math.random() < 0.2) typeId = 'VIP';
    const type = CUSTOMER_TYPES[typeId];
    const order = generateOrder(this.level, type);

    const slotIndex = channel.freeIndex;
    const target = channel.slotPosition(slotIndex);
    const customer = new CustomerAI({
      world: this.world, type, order,
      channel: channel.id, slotIndex, target,
    });
    channel.occupy(customer);
    this.customers.push(customer);
    this.spawned++;
    return true;
  }

  release(customer) {
    if (customer.channelRef) customer.channelRef.release(customer);
    customer.dispose();
    this.customers = this.customers.filter(c => c !== customer);
    if (customer.channelRef) customer.channelRef.compact();
  }

  /** Mesh'ten müşteri bul (drag & drop servis) */
  findByObject(object) {
    let o = object;
    while (o) {
      const found = this.customers.find(c => c.group === o || c.avatar === o);
      if (found) return found;
      o = o.parent;
    }
    return null;
  }

  get activeCount() { return this.customers.filter(c => c.state === CustomerState.Waiting).length; }
  get allResolved() { return this.spawned >= this.level.orders && this.customers.length === 0; }

  /** En uzun süredir bekleyen ve siparişi verilen burgerle eşleşen müşteri (garson) */
  findMatchFor(builder) {
    let best = null;
    for (const c of this.customers) {
      if (c.state !== CustomerState.Waiting) continue;
      if (!builder.matches(c.order)) continue;
      if (!best || c.patience < best.patience) best = c;
    }
    return best;
  }

  /** Tüm bekleyen müşterilerin sabrını yavaşlat (ödüllü reklam) */
  applySlowPatience(scale) {
    this.customers.forEach(c => { c.patienceDrainScale = scale; });
  }

  clearSlowPatience() {
    this.customers.forEach(c => { c.patienceDrainScale = 1; });
  }

  resetPatienceAll() {
    this.customers.forEach(c => c.resetPatience());
  }
}
