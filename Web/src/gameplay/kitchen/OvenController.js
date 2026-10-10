/**
 * OvenController.js — Izgara/fırın timer yönetimi (GDD 2.4)
 * %100 → hazır (yeşil glow), %115 → yanmaya başlar (kırmızı + duman)
 */
import * as THREE from 'three';
import { makeLayerMesh } from '../KitchenWorld.js';
import { eventBus, EVENTS } from '../../core/EventBus.js';
import { EconomyManager } from '../../economy/EconomyManager.js';
import { AudioManager } from '../../audio/AudioManager.js';
import { UpgradeSystem } from '../../economy/UpgradeSystem.js';
import { ECONOMY, INGREDIENTS } from '../../core/Config.js';
import { Haptic } from '../../input/HapticManager.js';

const RAW_COLOR = new THREE.Color(0x9C4A2F);
const COOKED_COLOR = new THREE.Color(0x5E3119);

export const SlotState = { Empty: 'empty', Cooking: 'cooking', Ready: 'ready', Burnt: 'burnt' };

const INGREDIENT_VALUE = INGREDIENTS.patty.value;

export class OvenController {
  constructor(world) {
    this.world = world;
    this.slots = [];
    this.config = UpgradeSystem.grillConfig;
    this.readyTray = [];          // aşçı Lv4+ buraya pişmiş köfte koyar
    this.onSlotChanged = null;
    this.applyLevel(UpgradeSystem.grillConfig.level, true);
  }

  applyLevel(level, silent = false) {
    this.config = UpgradeSystem.grillConfig;
    // Ekipman yükseltmesi oyun ortasında da gelebilir: önce slotları temizle,
    // aksi halde pişen köftelerin mesh'i sahneden silinir ama state'i kalır.
    if (!silent) this.slots.forEach(slot => this.clearSlot(slot));
    const nodes = this.world.layoutGrillSlots(this.config.slots);
    const old = this.slots;
    this.slots = nodes.map((n, i) => old[i] || {
      index: i, state: SlotState.Empty, progress: 0, mesh: null, node: n,
    });
    this.slots.forEach((s, i) => { s.node = nodes[i]; s.index = i; });
    if (!silent) this.refreshVisuals();
  }

  get freeSlot() { return this.slots.find(s => s.state === SlotState.Empty); }
  get readyCount() { return this.slots.filter(s => s.state === SlotState.Ready).length; }
  get cookingCount() { return this.slots.filter(s => s.state === SlotState.Cooking).length; }
  get burntCount() { return this.slots.filter(s => s.state === SlotState.Burnt).length; }

  placeRaw() {
    const slot = this.freeSlot;
    if (!slot) return false;
    slot.state = SlotState.Cooking;
    slot.progress = 0;
    slot.mesh = makeLayerMesh('patty', 'raw');
    slot.mesh.position.y = 0.06;
    slot.node.group.add(slot.mesh);
    slot.mesh.userData.interactable = { kind: 'grillSlot', index: slot.index };
    this.world.pickables.push(slot.mesh);
    AudioManager.play('sizzle');
    this.refreshVisuals();
    return true;
  }

  /** Hazır köfteyi al */
  collect(index) {
    const slot = this.slots[index];
    if (!slot) return null;
    if (slot.state !== SlotState.Ready && slot.state !== SlotState.Burnt) return null;
    const state = slot.state === SlotState.Burnt ? 'burnt' : 'cooked';
    this.clearSlot(slot);
    return { id: 'patty', state };
  }

  /** Yanan ürünü çöpe at (para kaybı) */
  discard(index) {
    const slot = this.slots[index];
    if (!slot) return;
    this.clearSlot(slot);
  }

  clearSlot(slot) {
    if (slot.mesh) {
      slot.node.group.remove(slot.mesh);
      this.world.pickables = this.world.pickables.filter(p => p !== slot.mesh);
      slot.mesh.geometry.dispose();
      slot.mesh.material.dispose();
      slot.mesh = null;
    }
    slot.state = SlotState.Empty;
    slot.progress = 0;
    this.refreshVisuals();
  }

  /** Gem ile anında pişirme */
  instantFinishAll() {
    let n = 0;
    for (const slot of this.slots) {
      if (slot.state === SlotState.Cooking) { slot.progress = 1; slot.state = SlotState.Ready; n++; this.refreshSlot(slot); }
    }
    if (n) { AudioManager.play('ding'); this.refreshVisuals(); }
    return n;
  }

  update(dt) {
    for (const slot of this.slots) {
      if (slot.state !== SlotState.Cooking) continue;
      slot.progress += dt / this.config.duration;

      if (slot.progress >= 1 && slot.state !== SlotState.Ready) {
        slot.state = SlotState.Ready;
        AudioManager.play('ding');
        Haptic.light();
      }
      if (slot.progress >= this.config.burn) {
        slot.state = SlotState.Burnt;
        AudioManager.play('burn');
        EconomyManager.deductCoins(Math.round(INGREDIENT_VALUE * ECONOMY.burntLossFactor));
        eventBus.publish(EVENTS.FOOD_BURNT, { name: 'Köfte', lostValue: INGREDIENT_VALUE });
      }
      this.refreshSlot(slot);
    }
  }

  refreshSlot(slot) {
    if (!slot.mesh) return;
    const mat = slot.mesh.material;
    if (slot.state === SlotState.Burnt) {
      mat.color.setHex(0x251C17);
      mat.emissive.setHex(0x220000);
      mat.emissiveIntensity = 0.6;
      slot.mesh.userData.smoke = true;
    } else if (slot.state === SlotState.Ready) {
      mat.color.setHex(0x5E3119);
      mat.emissive.setHex(0x2ecc71);
      mat.emissiveIntensity = 0.35 + Math.sin(performance.now() / 200) * 0.12;
    } else {
      const t = Math.min(1, slot.progress);
      mat.color.copy(RAW_COLOR).lerp(COOKED_COLOR, t);
      mat.emissive.setHex(0xff5a00);
      mat.emissiveIntensity = t * 0.25;
    }
  }

  refreshVisuals() {
    this.slots.forEach(s => this.refreshSlot(s));
    if (this.onSlotChanged) this.onSlotChanged();
  }

  reset() {
    this.slots.forEach(s => this.clearSlot(s));
    this.readyTray.length = 0;
    this.world.trayGroup.visible = false;
    this.world.trayStack.clear();
  }

  /** Aşçı personeli: pişen köfteyi tepsiye aktar */
  pushToTray(state = 'cooked') {
    if (this.readyTray.length >= 4) return false;
    this.readyTray.push(state);
    this.world.trayGroup.visible = true;
    this.renderTray();
    return true;
  }

  renderTray() {
    this.world.trayStack.clear();
    this.readyTray.forEach((state, i) => {
      const m = makeLayerMesh('patty', state);
      m.scale.setScalar(0.8);
      m.position.y = i * 0.09;
      m.userData.interactable = { kind: 'tray' };
      this.world.trayStack.add(m);
    });
  }

  takeFromTray() {
    const state = this.readyTray.pop();
    this.renderTray();
    if (!this.readyTray.length) this.world.trayGroup.visible = false;
    return state ? { id: 'patty', state } : null;
  }
}
