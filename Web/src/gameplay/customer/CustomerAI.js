/**
 * CustomerAI.js — Müşteri davranışı, sabrı ve sipariş balonu (GDD 2.5)
 * Durum akışı: Spawning → WalkingToQueue → Waiting → Leaving/Angry
 */
import * as THREE from 'three';
import { makeCharacter, makeTextSprite } from '../KitchenWorld.js';
import { CUSTOMER_TYPES, PATIENCE, CHANNELS } from '../../core/Config.js';
import { UpgradeSystem } from '../../economy/UpgradeSystem.js';

export const CustomerState = {
  WalkingIn: 'walkingIn',
  Waiting: 'waiting',
  Leaving: 'leaving',
  Angry: 'angry',
  Done: 'done',
};

export class CustomerAI {
  /**
   * @param {object} opts {world, type, order, channel, slotIndex, target}
   */
  constructor(opts) {
    const { world, type, order, channel = CHANNELS.COUNTER, slotIndex = 0 } = opts;
    this.world = world;
    this.type = type;
    this.order = order;
    this.channel = channel;
    this.slotIndex = slotIndex;
    this.state = CustomerState.WalkingIn;
    this.isCar = channel === CHANNELS.DRIVE_THRU;

    // Sabır: tip -> hız, temizlikçi -> yavaşlatma
    let seconds = PATIENCE.baseDuration / type.patience;
    seconds *= UpgradeSystem.patienceMultiplier;
    if (channel === CHANNELS.DRIVE_THRU) seconds /= PATIENCE.driveThruFactor;
    this.totalPatience = seconds;
    this.patience = seconds;
    this.patienceDrainScale = 1;

    this.lastDrawnRatio = -1;
    this.bob = Math.random() * Math.PI * 2;
    this.leaveTimer = 0;
    this.result = null;     // 'served' | 'angry'

    this._buildMesh(opts.target);
  }

  _buildMesh(target) {
    this.group = new THREE.Group();
    if (this.isCar) {
      this._buildCar();
    } else {
      this.avatar = makeCharacter({ body: this.type.color, head: 0xffd9b3, hat: this.type.id === 'VIP' ? 0xFFD700 : null });
      this.group.add(this.avatar);
    }
    this.target = target.clone();
    // Sahneye giriş noktası
    if (this.isCar) this.group.position.set(target.x, 0, target.z + 16);
    else this.group.position.set(-15, 0, target.z - 0.4);

    this.bubble = makeTextSprite('', { w: 320, h: 190, size: 44 });
    this.bubble.scale.set(1.7, 1.0, 1);
    this.bubble.position.set(0, this.isCar ? 2.6 : 2.35, 0);
    this.group.add(this.bubble);
    this.world.scene.add(this.group);
    this.drawBubble();
  }

  _buildCar() {
    const car = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.BoxGeometry(1.7, 0.7, 3.6),
      new THREE.MeshStandardMaterial({ color: this.type.color, roughness: 0.5, metalness: 0.25 }),
    );
    body.position.y = 0.75; body.castShadow = true;
    car.add(body);

    const cabin = new THREE.Mesh(
      new THREE.BoxGeometry(1.5, 0.6, 1.8),
      new THREE.MeshStandardMaterial({ color: 0x2b3550, roughness: 0.25, metalness: 0.4 }),
    );
    cabin.position.set(0, 1.36, -0.2); cabin.castShadow = true;
    car.add(cabin);

    const wheelGeo = new THREE.CylinderGeometry(0.34, 0.34, 0.24, 12);
    const wheelMat = new THREE.MeshStandardMaterial({ color: 0x1b1b26, roughness: 0.9 });
    [[-0.85, 1.15], [0.85, 1.15], [-0.85, -1.15], [0.85, -1.15]].forEach(([x, z]) => {
      const w = new THREE.Mesh(wheelGeo, wheelMat);
      w.rotation.z = Math.PI / 2;
      w.position.set(x, 0.34, z);
      car.add(w);
    });

    const light = new THREE.Mesh(
      new THREE.BoxGeometry(0.3, 0.16, 0.1),
      new THREE.MeshStandardMaterial({ color: 0xfff3b0, emissive: 0xffdd66, emissiveIntensity: 0.8 }),
    );
    light.position.set(0, 0.85, 1.85);
    car.add(light);

    this.car = car;
    this.avatar = car;
    this.group.add(car);
  }

  // ══════════════════ Durum ══════════════════
  update(dt) {
    switch (this.state) {
      case CustomerState.WalkingIn: {
        const p = this.group.position;
        const dir = new THREE.Vector3().subVectors(this.target, p);
        const dist = dir.length();
        const speed = this.isCar ? 9 : 3.4;
        if (dist < 0.12) {
          this.group.position.copy(this.target);
          this.state = CustomerState.Waiting;
          if (!this.isCar) this.group.rotation.y = 0;   // tezgaha (+z) dön
        } else {
          dir.normalize();
          p.addScaledVector(dir, Math.min(speed * dt, dist));
          if (this.isCar) this.group.rotation.y = Math.PI;     // -z yönüne bak
          else { this.group.rotation.y = Math.PI / 2; this.bob += dt * 12; this.avatar.position.y = Math.abs(Math.sin(this.bob)) * 0.08; }
        }
        break;
      }
      case CustomerState.Waiting: {
        this.patience -= dt * this.patienceDrainScale;
        this.bob += dt * 3;
        // Kuyruk ilerlediğinde yeni hedefe yumuşak geçiş
        this.group.position.lerp(this.target, Math.min(1, dt * 3));
        if (!this.isCar) {
          this.avatar.position.y = Math.sin(this.bob) * 0.03;
          this.avatar.rotation.y = Math.sin(this.bob * 0.6) * 0.05;
        }
        const ratio = this.patienceRatio;
        if (Math.abs(ratio - this.lastDrawnRatio) > 0.03) this.drawBubble();
        if (this.patience <= 0) this.leave('angry');
        break;
      }
      case CustomerState.Leaving:
      case CustomerState.Angry: {
        this.leaveTimer += dt;
        const p = this.group.position;
        const step = (this.isCar ? 10 : 3.6) * dt;
        if (this.isCar) p.z -= step; else p.x -= step;
        if (this.state === CustomerState.Angry) {
          this.avatar.rotation.z = Math.sin(this.leaveTimer * 30) * 0.12;
        } else if (!this.isCar) {
          this.avatar.position.y = Math.abs(Math.sin(this.leaveTimer * 12)) * 0.16;
        }
        if (this.leaveTimer > 3) this.state = CustomerState.Done;
        break;
      }
      default: break;
    }
  }

  get patienceRatio() { return Math.max(0, Math.min(1, this.patience / this.totalPatience)); }

  /** Sabır sıfırla (gem) */
  resetPatience() {
    this.patience = this.totalPatience;
    this.drawBubble();
  }

  serve(wasWrong = false) {
    this.result = wasWrong ? 'wrong' : 'served';
    this.leave(wasWrong ? 'angry' : 'happy');
  }

  leave(kind) {
    if (this.state === CustomerState.Leaving || this.state === CustomerState.Angry || this.state === CustomerState.Done) return;
    this.state = kind === 'angry' ? CustomerState.Angry : CustomerState.Leaving;
    this.leaveTimer = 0;
    this.bubble.visible = false;
  }

  dispose() {
    this.world.scene.remove(this.group);
    this.group.traverse(o => {
      if (o.geometry) o.geometry.dispose();
      if (o.material) { if (o.material.map) o.material.map.dispose(); o.material.dispose(); }
    });
  }

  // ══════════════════ Sipariş balonu ══════════════════
  drawBubble() {
    const { ctx, canvas, tex } = this.bubble.userData;
    const w = canvas.width, h = canvas.height;
    const ratio = this.patienceRatio;
    this.lastDrawnRatio = ratio;
    ctx.clearRect(0, 0, w, h);

    // Panel
    const r = 26;
    ctx.beginPath();
    ctx.moveTo(r, 6); ctx.lineTo(w - r, 6); ctx.quadraticCurveTo(w - 6, 6, w - 6, 6 + r);
    ctx.lineTo(w - 6, h - 30); ctx.lineTo(w / 2 + 14, h - 30); ctx.lineTo(w / 2, h - 6); ctx.lineTo(w / 2 - 14, h - 30);
    ctx.lineTo(6, h - 30); ctx.lineTo(6, 6 + r); ctx.quadraticCurveTo(6, 6, r, 6);
    ctx.closePath();
    ctx.fillStyle = 'rgba(18,18,34,0.90)';
    ctx.fill();
    ctx.strokeStyle = ratio < PATIENCE.warnRatio ? '#E74C3C' : 'rgba(255,255,255,0.25)';
    ctx.lineWidth = 4; ctx.stroke();

    // Sipariş ikonları
    ctx.font = '54px "Segoe UI Emoji", "Apple Color Emoji", "Noto Color Emoji", sans-serif';
    ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
    const icons = this.order.fillings.map(f => EMOJI[f] || '❓');
    if (this.order.isCombo) icons.push('🍟');
    const total = icons.length;
    icons.forEach((ic, i) => {
      const x = w / 2 + (i - (total - 1) / 2) * 56;
      ctx.fillText(ic, x, 58);
    });

    // Sabır barı
    const bx = 22, by = h - 78, bw = w - 44, bh = 22;
    ctx.fillStyle = 'rgba(0,0,0,0.55)';
    ctx.beginPath(); ctx.roundRect ? ctx.roundRect(bx, by, bw, bh, 11) : ctx.rect(bx, by, bw, bh); ctx.fill();
    const barColor = ratio > 0.6 ? '#2ECC71' : ratio > PATIENCE.warnRatio ? '#FFD700' : '#E74C3C';
    ctx.fillStyle = barColor;
    ctx.beginPath();
    if (ctx.roundRect) ctx.roundRect(bx, by, Math.max(4, bw * ratio), bh, 11); else ctx.rect(bx, by, Math.max(4, bw * ratio), bh);
    ctx.fill();

    // Fiyat
    ctx.font = 'bold 30px "Nunito", system-ui, sans-serif';
    ctx.fillStyle = '#FFD700';
    ctx.fillText(`🪙${this.order.basePrice}`, w / 2, h - 40);
    tex.needsUpdate = true;
  }
}

const EMOJI = { bun: '🍞', patty: '🥩', cheese: '🧈', lettuce: '🥬', tomato: '🍅', sauce: '🫗' };
