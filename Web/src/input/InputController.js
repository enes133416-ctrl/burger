/**
 * InputController.js — Dokunmatik sürükleme & swipe (GDD 8.1 Input klasörü)
 * Raycast → pickup → snap zone → drop. Hem sürükleme hem "dokun-dokun" modu desteklenir.
 */
import * as THREE from 'three';
import { makeLayerMesh, makeBunTop } from '../gameplay/KitchenWorld.js';

const HOLD_PLANE_Y = 1.95;

export class InputController {
  constructor(world, game) {
    this.world = world;
    this.game = game;
    this.raycaster = new THREE.Raycaster();
    this.pointer = new THREE.Vector2();
    this.plane = new THREE.Plane(new THREE.Vector3(0, 1, 0), -HOLD_PLANE_Y);
    this.hitPoint = new THREE.Vector3();
    this.held = null;
    this.heldMesh = null;
    this.downInfo = null;
    this.enabled = true;

    const el = world.renderer.domElement;
    el.addEventListener('pointerdown', e => this.onDown(e));
    el.addEventListener('pointermove', e => this.onMove(e));
    window.addEventListener('pointerup', e => this.onUp(e));
    el.addEventListener('pointercancel', () => this.cancelHold());
    el.addEventListener('contextmenu', e => e.preventDefault());
  }

  _updatePointer(e) {
    const rect = this.world.renderer.domElement.getBoundingClientRect();
    this.pointer.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
    this.pointer.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
  }

  _pick() {
    this.raycaster.setFromCamera(this.pointer, this.world.camera);
    const hits = this.raycaster.intersectObjects(this.world.pickables, false);
    if (hits.length) return hits[0];
    // Müşteriler (avatar gövdesi) ikinci öncelik
    const customerHits = this.raycaster.intersectObjects(
      this.game.customers.customers.map(c => c.avatar), true,
    );
    return customerHits.length ? customerHits[0] : null;
  }

  /** Bir mesh'ten interactable verisini çıkar (parent'lara bakarak) */
  _interactableOf(object) {
    let o = object;
    while (o) {
      if (o.userData && o.userData.interactable) return o.userData.interactable;
      o = o.parent;
    }
    return null;
  }

  onDown(e) {
    if (!this.enabled) return;
    this._updatePointer(e);
    this.downInfo = { x: e.clientX, y: e.clientY, t: performance.now() };
    if (this.held) return;   // zaten elde bir şey var → bırakma aşaması

    const hit = this._pick();
    if (!hit) return;
    const info = this._interactableOf(hit.object);
    let target = null;
    if (info) target = { info, object: hit.object };
    else {
      const customer = this.game.customers.findByObject(hit.object);
      if (customer) target = { info: { kind: 'customer' }, customer, object: hit.object };
    }
    if (!target) return;

    const item = this.game.handlePick(target);
    if (item) this.setHeld(item);
  }

  onMove(e) {
    if (!this.held || !this.heldMesh) return;
    this._updatePointer(e);
    this.raycaster.setFromCamera(this.pointer, this.world.camera);
    if (this.raycaster.ray.intersectPlane(this.plane, this.hitPoint)) {
      this.heldMesh.position.copy(this.hitPoint);
    }
  }

  onUp(e) {
    if (!this.held) { this.downInfo = null; return; }
    this._updatePointer(e);
    const hit = this._pick();

    let target = null;
    if (hit) {
      const info = this._interactableOf(hit.object);
      if (info) target = { info, object: hit.object };
      else {
        const customer = this.game.customers.findByObject(hit.object);
        if (customer) target = { info: { kind: 'customer' }, customer, object: hit.object };
      }
    }

    if (target) {
      const consumed = this.game.handleDrop(this.held, target);
      if (consumed) this.clearHeld();
      return;   // geçersiz hedef → elde tutmaya devam
    }

    // Boşluğa bırakıldı
    const isTap = this.downInfo
      && Math.hypot(e.clientX - this.downInfo.x, e.clientY - this.downInfo.y) < 14
      && performance.now() - this.downInfo.t < 400;
    if (!isTap) this.cancelHold();
    this.downInfo = null;
  }

  setHeld(item) {
    this.held = item;
    this.heldMesh = this._meshFor(item);
    this.world.scene.add(this.heldMesh);
    this.raycaster.setFromCamera(this.pointer, this.world.camera);
    if (this.raycaster.ray.intersectPlane(this.plane, this.hitPoint)) this.heldMesh.position.copy(this.hitPoint);
  }

  clearHeld() {
    if (this.heldMesh) {
      this.world.scene.remove(this.heldMesh);
      this.heldMesh.traverse(o => { if (o.geometry) o.geometry.dispose(); if (o.material) o.material.dispose(); });
    }
    this.held = null;
    this.heldMesh = null;
    this.downInfo = null;
  }

  /** Geçersiz bırakma: malzemeyi yok et */
  cancelHold() {
    if (!this.held) return;
    this.clearHeld();
  }

  _meshFor(item) {
    const g = new THREE.Group();
    if (item.kind === 'burger') {
      let y = 0;
      item.layers.forEach((layer, i) => {
        const isTop = layer.id === 'bun' && i === item.layers.length - 1 && item.layers.length > 1;
        const m = isTop ? makeBunTop() : makeLayerMesh(layer.id, layer.state);
        const h = isTop ? 0.26 : (m.geometry.parameters.height || 0.1);
        m.position.y = y + (isTop ? 0.14 : h / 2);
        y += h;
        g.add(m);
      });
    } else {
      const id = item.kind === 'patty' ? 'patty' : item.id;
      const state = item.kind === 'patty' ? item.state : 'raw';
      const m = makeLayerMesh(id, state);
      g.add(m);
    }
    g.scale.setScalar(0.95);
    g.userData.spin = true;
    return g;
  }

  update(dt) {
    if (this.heldMesh) {
      this.heldMesh.rotation.y += dt * 1.2;
      this.heldMesh.position.y = HOLD_PLANE_Y + Math.sin(performance.now() / 220) * 0.06;
    }
  }
}
