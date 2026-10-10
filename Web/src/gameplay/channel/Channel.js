/**
 * Channel.js — Servis kanalı tabanı (GDD 2.2)
 */
import * as THREE from 'three';
import { CHANNELS } from '../../core/Config.js';

export class Channel {
  constructor(id, world, maxQueue) {
    this.id = id;
    this.world = world;
    this.maxQueue = maxQueue;
    this.customers = new Array(maxQueue).fill(null);
  }

  get isDriveThru() { return this.id === CHANNELS.DRIVE_THRU; }
  get freeIndex() { return this.customers.findIndex(c => c === null); }
  get hasRoom() { return this.freeIndex >= 0; }
  get count() { return this.customers.filter(Boolean).length; }

  slotPosition(index) {
    if (this.isDriveThru) return new THREE.Vector3(8.6, 0, 3.0 - index * 2.4);
    return new THREE.Vector3(-5.6 + index * 1.7, 0, index % 2 === 0 ? -2.1 : -2.95);
  }

  occupy(customer) {
    const i = this.freeIndex;
    if (i < 0) return -1;
    this.customers[i] = customer;
    customer.channelRef = this;
    return i;
  }

  release(customer) {
    const i = this.customers.indexOf(customer);
    if (i >= 0) this.customers[i] = null;
  }

  clear() { this.customers.fill(null); }

  /** Kuyruktaki müşterileri öne kaydır */
  compact() {
    const list = this.customers.filter(Boolean);
    this.customers.fill(null);
    list.forEach((c, i) => {
      this.customers[i] = c;
      c.slotIndex = i;
      c.target.copy(this.slotPosition(i));
    });
  }
}
