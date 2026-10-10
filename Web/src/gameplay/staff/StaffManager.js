/**
 * StaffManager.js — Personel otomasyonu (GDD 2.6)
 * Aşçı: ızgaraya otomatik köfte atar, pişeni tepsiye alır.
 * Garson: hazır burgeri eşleşen müşteriye otomatik servis eder.
 * Kasiyer/Temizlikçi/Drive-Thru görevlisi: pasif bonuslar (UpgradeSystem).
 */
import * as THREE from 'three';
import { makeCharacter } from '../KitchenWorld.js';
import { STAFF } from '../../data/UpgradeData.js';
import { UpgradeSystem } from '../../economy/UpgradeSystem.js';

const STAFF_COLORS = {
  cashier: 0x2ECC71,
  cook: 0xFF6B35,
  waiter: 0x3498DB,
  driveThruStaff: 0x9B59B6,
  cleaner: 0x7FD8E8,
};

export class StaffManager {
  constructor(world) {
    this.world = world;
    this.group = new THREE.Group();
    this.world.scene.add(this.group);
    this.avatars = {};
    this.cookTimer = 0;
    this.serveTimer = 0;
    this.time = 0;
  }

  /** Kiralanan personel için mutfakta avatar oluştur */
  refreshAvatars() {
    STAFF.forEach((def, i) => {
      const hired = UpgradeSystem.isHired(def.id);
      if (hired && !this.avatars[def.id]) {
        const avatar = makeCharacter({
          body: STAFF_COLORS[def.id], head: 0xffd9b3,
          hat: def.id === 'cook' ? 0xFFFFFF : null,
          apron: def.id === 'waiter' ? 0x1B1B2E : null,
          scale: 0.92,
        });
        avatar.position.set(-6.5 + i * 1.5, 0, 3.0);
        avatar.rotation.y = Math.PI;
        this.group.add(avatar);
        this.avatars[def.id] = avatar;
      } else if (!hired && this.avatars[def.id]) {
        this.group.remove(this.avatars[def.id]);
        delete this.avatars[def.id];
      }
    });
  }

  /**
   * @param {number} dt
   * @param {object} ctx { oven, assembly, customers, onAutoServe }
   */
  update(dt, ctx) {
    this.time += dt;
    this._animateAvatars();

    // ─── Aşçı: otomatik pişirme ───
    const cookDelay = UpgradeSystem.autoCookDelay;
    if (cookDelay != null) {
      this.cookTimer += dt;
      if (this.cookTimer >= cookDelay) {
        this.cookTimer = 0;
        const lvl = UpgradeSystem.staffLevel('cook');
        const perTick = lvl >= 5 ? 6 : lvl >= 3 ? 2 : 1;
        for (let i = 0; i < perTick; i++) {
          if (!ctx.oven.placeRaw()) break;
        }
        // Lv4+: pişen köfteleri tepsiye al
        if (lvl >= 4) {
          ctx.oven.slots.forEach(slot => {
            if (slot.state === 'ready') {
              if (ctx.oven.pushToTray('cooked')) ctx.oven.collect(slot.index);
            }
          });
        }
      }
    }

    // ─── Garson: otomatik servis ───
    const serveDelay = UpgradeSystem.autoServeDelay;
    if (serveDelay != null) {
      this.serveTimer += dt;
      if (this.serveTimer >= serveDelay) {
        this.serveTimer = 0;
        if (ctx.assembly.builder.isComplete) {
          const allowDrive = UpgradeSystem.autoServeDriveThru;
          const candidates = ctx.customers.customers.filter(c =>
            c.state === 'waiting' && (allowDrive || c.channel !== 'driveThru'));
          const match = candidates
            .filter(c => ctx.assembly.builder.matches(c.order))
            .sort((a, b) => a.patience - b.patience)[0];
          if (match && ctx.onAutoServe) ctx.onAutoServe(match);
        }
      }
    }
  }

  _animateAvatars() {
    Object.values(this.avatars).forEach((a, i) => {
      a.position.y = Math.sin(this.time * 4 + i) * 0.05;
      a.rotation.z = Math.sin(this.time * 2 + i) * 0.03;
    });
  }

  get activeStaffCount() { return Object.keys(this.avatars).length; }

  clearAvatars() {
    Object.values(this.avatars).forEach(a => this.group.remove(a));
    this.avatars = {};
  }
}
