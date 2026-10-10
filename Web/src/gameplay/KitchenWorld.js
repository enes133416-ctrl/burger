/**
 * KitchenWorld.js — 3D sahne kurulumu: restoran, tezgah, ızgara, malzeme kutuları, şef.
 * Low-poly + soft shadow + warm lighting (GDD 9.1). Tüm mesh'ler prosedürel üretilir,
 * böylece depoda binary asset taşınmaz.
 */
import * as THREE from 'three';
import { INGREDIENTS } from '../core/Config.js';
import { RESTAURANT_THEMES } from '../data/CosmeticData.js';

const COUNTER_TOP = 1.1;

export class KitchenWorld {
  constructor(container) {
    this.container = container;
    this.pickables = [];      // raycast için etkileşimli mesh'ler
    this.animated = [];       // update() çağrılacak objeler
    this.time = 0;

    this.renderer = new THREE.WebGLRenderer({ antialias: true, powerPreference: 'high-performance' });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    this.renderer.outputColorSpace = THREE.SRGBColorSpace;
    container.appendChild(this.renderer.domElement);

    this.scene = new THREE.Scene();
    this.scene.fog = new THREE.Fog(0x1a1a2e, 18, 42);

    this.camera = new THREE.PerspectiveCamera(48, 1, 0.1, 120);
    this.camera.position.set(0, 9, 10.6);
    this.camera.lookAt(0.6, 1.1, -0.4);

    this._buildLights();
    this._buildRoom();
    this._buildCounter();
    this._buildGrill();
    this._buildBins();
    this._buildPlate();
    this._buildTray();
    this._buildTrash();
    this._buildChef();
    this._buildDriveThru();
    this.setTheme('default');

    this._onResize = () => this.resize();
    window.addEventListener('resize', this._onResize);
    this.resize();
  }

  // ══════════════════ Sahne kurulumu ══════════════════
  _buildLights() {
    this.hemi = new THREE.HemisphereLight(0xffffff, 0x444466, 0.75);
    this.scene.add(this.hemi);

    this.sun = new THREE.DirectionalLight(0xfff0dd, 1.35);
    this.sun.position.set(6, 12, 6);
    this.sun.castShadow = true;
    this.sun.shadow.mapSize.set(1024, 1024);
    this.sun.shadow.camera.left = -14;
    this.sun.shadow.camera.right = 14;
    this.sun.shadow.camera.top = 14;
    this.sun.shadow.camera.bottom = -14;
    this.sun.shadow.camera.far = 40;
    this.sun.shadow.bias = -0.0012;
    this.scene.add(this.sun);

    this.warmLight = new THREE.PointLight(0xffb066, 0.9, 18, 2);
    this.warmLight.position.set(0, 3.4, 1.2);
    this.scene.add(this.warmLight);
  }

  _buildRoom() {
    this.room = new THREE.Group();
    this.scene.add(this.room);

    this.floor = new THREE.Mesh(
      new THREE.PlaneGeometry(40, 30),
      new THREE.MeshStandardMaterial({ color: 0x4a4a63, roughness: 0.95 }),
    );
    this.floor.rotation.x = -Math.PI / 2;
    this.floor.receiveShadow = true;
    this.room.add(this.floor);

    this.backWall = new THREE.Mesh(
      new THREE.BoxGeometry(30, 7, 0.4),
      new THREE.MeshStandardMaterial({ color: 0x22223a, roughness: 0.9 }),
    );
    this.backWall.position.set(0, 3.5, -7);
    this.backWall.receiveShadow = true;
    this.room.add(this.backWall);

    this.sideWall = new THREE.Mesh(
      new THREE.BoxGeometry(0.4, 7, 18),
      new THREE.MeshStandardMaterial({ color: 0x22223a, roughness: 0.9 }),
    );
    this.sideWall.position.set(-14, 3.5, 0);
    this.room.add(this.sideWall);

    // Dekor: tabela + bitki + menü panosu
    this.sign = new THREE.Group();
    const signBoard = new THREE.Mesh(
      new THREE.BoxGeometry(5.2, 1.7, 0.25),
      new THREE.MeshStandardMaterial({ color: 0xff6b35, roughness: 0.6 }),
    );
    signBoard.castShadow = true;
    this.sign.add(signBoard);
    this.signLabel = makeTextSprite('🍔 BURGER RUSH', { w: 512, h: 168, size: 74, color: '#FFFFFF', bg: 'rgba(0,0,0,0)' });
    this.signLabel.position.set(0, 0, 0.2);
    this.signLabel.scale.set(4.6, 1.5, 1);
    this.sign.add(this.signLabel);
    this.sign.position.set(-1.4, 4.4, -6.6);
    this.room.add(this.sign);

    const menuPanel = new THREE.Mesh(
      new THREE.BoxGeometry(3.4, 2.2, 0.18),
      new THREE.MeshStandardMaterial({ color: 0x1f1f33, roughness: 0.8 }),
    );
    menuPanel.position.set(6.2, 3.4, -6.6);
    this.room.add(menuPanel);
    const menuLabel = makeTextSprite('MENU\nBurger  🪙25\nMenü  🪙40\nPatates 🪙15', { w: 512, h: 340, size: 54, color: '#FFD700', bg: 'rgba(0,0,0,0)' });
    menuLabel.position.set(6.2, 3.4, -6.45);
    menuLabel.scale.set(3, 2, 1);
    this.room.add(menuLabel);

    // Zemin çizgileri (mutfak hissi)
    for (let i = -3; i <= 3; i++) {
      const stripe = new THREE.Mesh(
        new THREE.PlaneGeometry(0.08, 14),
        new THREE.MeshBasicMaterial({ color: 0xffffff, transparent: true, opacity: 0.05 }),
      );
      stripe.rotation.x = -Math.PI / 2;
      stripe.position.set(i * 3.4, 0.01, 2);
      this.room.add(stripe);
    }
  }

  _buildCounter() {
    this.counter = new THREE.Group();
    this.scene.add(this.counter);

    const body = new THREE.Mesh(
      new THREE.BoxGeometry(13.4, COUNTER_TOP, 1.7),
      new THREE.MeshStandardMaterial({ color: 0x9c6b4a, roughness: 0.8 }),
    );
    body.position.set(0, COUNTER_TOP / 2, 0);
    body.castShadow = true; body.receiveShadow = true;
    this.counter.add(body);

    this.counterTop = new THREE.Mesh(
      new THREE.BoxGeometry(13.8, 0.14, 2.0),
      new THREE.MeshStandardMaterial({ color: 0xE8E2D6, roughness: 0.35, metalness: 0.1 }),
    );
    this.counterTop.position.set(0, COUNTER_TOP + 0.05, 0);
    this.counterTop.castShadow = true; this.counterTop.receiveShadow = true;
    this.counter.add(this.counterTop);

    // Ön panel ışık şeridi
    const strip = new THREE.Mesh(
      new THREE.BoxGeometry(13.4, 0.16, 0.06),
      new THREE.MeshStandardMaterial({ color: 0xff6b35, emissive: 0xff6b35, emissiveIntensity: 0.7 }),
    );
    strip.position.set(0, 0.82, -0.88);
    this.counter.add(strip);
    this.accentStrip = strip;
  }

  _buildGrill() {
    this.grillGroup = new THREE.Group();
    this.grillGroup.position.set(-5.0, COUNTER_TOP + 0.14, 0.45);
    this.scene.add(this.grillGroup);

    const base = new THREE.Mesh(
      new THREE.BoxGeometry(2.6, 0.18, 1.15),
      new THREE.MeshStandardMaterial({ color: 0x3b3b4a, roughness: 0.5, metalness: 0.6 }),
    );
    base.castShadow = true; base.receiveShadow = true;
    this.grillGroup.add(base);

    const plate = new THREE.Mesh(
      new THREE.BoxGeometry(2.4, 0.06, 1.0),
      new THREE.MeshStandardMaterial({ color: 0x1b1b26, roughness: 0.35, emissive: 0x220a00, emissiveIntensity: 0.5 }),
    );
    plate.position.y = 0.12;
    this.grillGroup.add(plate);
    this.grillPlate = plate;

    this.grillSlotsGroup = new THREE.Group();
    this.grillSlotsGroup.position.y = 0.16;
    this.grillGroup.add(this.grillSlotsGroup);

    const label = makeTextSprite('IZGARA', { w: 256, h: 96, size: 56, color: '#FFB066', bg: 'rgba(0,0,0,0)' });
    label.position.set(0, 0.55, -0.75);
    label.scale.set(1.6, 0.6, 1);
    this.grillGroup.add(label);

    // Alev efekti (yanan ürünlerde görünür)
    this.flame = new THREE.Mesh(
      new THREE.PlaneGeometry(2.4, 1.0),
      new THREE.MeshBasicMaterial({ color: 0xff7b2e, transparent: true, opacity: 0, depthWrite: false }),
    );
    this.flame.rotation.x = -Math.PI / 2.6;
    this.flame.position.set(0, 0.25, 0);
    this.grillGroup.add(this.flame);
  }

  _buildBins() {
    this.bins = {};
    const order = ['bun', 'patty', 'cheese', 'lettuce', 'tomato', 'sauce'];
    order.forEach((id, i) => {
      const def = INGREDIENTS[id];
      const g = new THREE.Group();
      g.position.set(-3.2 + i * 1.2, COUNTER_TOP + 0.14, 0.45);

      const tray = new THREE.Mesh(
        new THREE.BoxGeometry(0.95, 0.22, 0.75),
        new THREE.MeshStandardMaterial({ color: 0xcfd3e0, roughness: 0.6 }),
      );
      tray.castShadow = true; tray.receiveShadow = true;
      g.add(tray);

      const sample = new THREE.Mesh(
        id === 'sauce' ? new THREE.CylinderGeometry(0.11, 0.13, 0.3, 10)
          : id === 'patty' ? new THREE.CylinderGeometry(0.26, 0.26, 0.12, 14)
            : id === 'bun' ? new THREE.SphereGeometry(0.26, 14, 10, 0, Math.PI * 2, 0, Math.PI / 2)
              : new THREE.BoxGeometry(0.44, 0.08, 0.44),
        new THREE.MeshStandardMaterial({ color: def.color, roughness: 0.65 }),
      );
      sample.position.y = 0.22;
      if (id === 'patty') sample.position.y = 0.19;
      sample.castShadow = true;
      g.add(sample);

      const label = makeTextSprite(`${def.emoji}`, { w: 128, h: 128, size: 84, bg: 'rgba(0,0,0,0)' });
      label.position.set(0, 0.78, 0);
      label.scale.set(0.42, 0.42, 1);
      g.add(label);

      // Tepsi mesh'i tıklanabilir olsun (büyük hedef)
      tray.userData.interactable = { kind: 'bin', ingredient: id };
      this.pickables.push(tray);
      this.scene.add(g);
      this.bins[id] = { group: g, tray, sample, def };
    });
  }

  _buildPlate() {
    this.plateGroup = new THREE.Group();
    this.plateGroup.position.set(4.3, COUNTER_TOP + 0.14, 0.45);
    this.scene.add(this.plateGroup);

    const plate = new THREE.Mesh(
      new THREE.CylinderGeometry(0.72, 0.62, 0.09, 24),
      new THREE.MeshStandardMaterial({ color: 0xf7f7fb, roughness: 0.25, metalness: 0.15 }),
    );
    plate.castShadow = true; plate.receiveShadow = true;
    plate.userData.interactable = { kind: 'plate' };
    this.pickables.push(plate);
    this.plateGroup.add(plate);
    this.plateMesh = plate;

    this.burgerGroup = new THREE.Group();
    this.burgerGroup.position.y = 0.05;
    this.plateGroup.add(this.burgerGroup);

    const label = makeTextSprite('🍔 MONTAJ', { w: 256, h: 96, size: 44, color: '#A6A6C8', bg: 'rgba(0,0,0,0)' });
    label.position.set(0, 0.62, 0);
    label.scale.set(1.1, 0.42, 1);
    this.plateGroup.add(label);
  }

  _buildTray() {
    this.trayGroup = new THREE.Group();
    this.trayGroup.position.set(-3.55, COUNTER_TOP + 0.14, 0.45);
    this.trayGroup.visible = false;
    this.scene.add(this.trayGroup);

    const tray = new THREE.Mesh(
      new THREE.BoxGeometry(0.9, 0.08, 0.7),
      new THREE.MeshStandardMaterial({ color: 0xbfc6d6, roughness: 0.4, metalness: 0.4 }),
    );
    tray.castShadow = true;
    tray.userData.interactable = { kind: 'tray' };
    this.pickables.push(tray);
    this.trayGroup.add(tray);
    this.trayMesh = tray;

    this.trayStack = new THREE.Group();
    this.trayStack.position.y = 0.06;
    this.trayGroup.add(this.trayStack);
  }

  _buildTrash() {
    const g = new THREE.Group();
    g.position.set(5.7, COUNTER_TOP + 0.14, 0.45);
    const bin = new THREE.Mesh(
      new THREE.CylinderGeometry(0.36, 0.3, 0.62, 14),
      new THREE.MeshStandardMaterial({ color: 0x4b4b5e, roughness: 0.7 }),
    );
    bin.position.y = 0.31;
    bin.castShadow = true;
    bin.userData.interactable = { kind: 'trash' };
    this.pickables.push(bin);
    g.add(bin);

    const lid = new THREE.Mesh(
      new THREE.CylinderGeometry(0.38, 0.38, 0.06, 14),
      new THREE.MeshStandardMaterial({ color: 0x2e2e3d, roughness: 0.7 }),
    );
    lid.position.y = 0.64;
    g.add(lid);

    const label = makeTextSprite('🗑️', { w: 128, h: 128, size: 80, bg: 'rgba(0,0,0,0)' });
    label.position.set(0, 1.0, 0);
    label.scale.set(0.4, 0.4, 1);
    g.add(label);
    this.scene.add(g);
  }

  _buildChef() {
    this.chef = makeCharacter({ body: 0xf2f2f7, head: 0xffd9b3, hat: 0xffffff, apron: 0xff6b35, scale: 1.0 });
    this.chef.position.set(1.5, 0, 2.2);
    this.scene.add(this.chef);
    this.chefTargetX = 1.5;
    this.animated.push({ update: (t) => {
      this.chef.position.x += (this.chefTargetX - this.chef.position.x) * Math.min(1, t * 6);
      this.chef.children.forEach((c, i) => { c.position.y = (c.userData.baseY || 0) + Math.sin(this.time * 3 + i) * 0.02; });
    } });
  }

  _buildDriveThru() {
    this.driveThruGroup = new THREE.Group();
    this.driveThruGroup.visible = false;
    this.scene.add(this.driveThruGroup);

    const lane = new THREE.Mesh(
      new THREE.PlaneGeometry(3.4, 16),
      new THREE.MeshStandardMaterial({ color: 0x33333f, roughness: 1 }),
    );
    lane.rotation.x = -Math.PI / 2;
    lane.position.set(8.4, 0.02, 1.5);
    this.driveThruGroup.add(lane);

    const kiosk = new THREE.Mesh(
      new THREE.BoxGeometry(0.9, 2.2, 0.9),
      new THREE.MeshStandardMaterial({ color: 0x2f6fb0, roughness: 0.7 }),
    );
    kiosk.position.set(6.9, 1.1, -1.2);
    kiosk.castShadow = true;
    this.driveThruGroup.add(kiosk);

    const label = makeTextSprite('DRIVE-THRU', { w: 512, h: 128, size: 62, color: '#7FD8E8', bg: 'rgba(0,0,0,0)' });
    label.position.set(8.4, 3.2, -3.4);
    label.scale.set(3.4, 0.85, 1);
    this.driveThruGroup.add(label);

    // Servis penceresi
    const window = new THREE.Mesh(
      new THREE.BoxGeometry(1.6, 1.2, 0.25),
      new THREE.MeshStandardMaterial({ color: 0x7fd8e8, emissive: 0x1d4a55, emissiveIntensity: 0.5 }),
    );
    window.position.set(6.4, 2.0, -1.2);
    this.driveThruGroup.add(window);
  }

  setDriveThruVisible(v) { this.driveThruGroup.visible = v; }

  /** Şef kostümünü uygula (GDD 4.3) */
  setChefSkin(skin) {
    if (!skin) return;
    this.chef.traverse(o => {
      const role = o.userData && o.userData.role;
      if (!role || !o.material) return;
      if (role === 'body' || role === 'arm') o.material.color.setHex(skin.body);
      else if (role === 'hat') o.material.color.setHex(skin.hat);
      else if (role === 'apron') o.material.color.setHex(skin.apron);
    });
  }

  // ══════════════════ Tema / lokasyon ══════════════════
  setTheme(themeId) {
    const theme = RESTAURANT_THEMES.find(t => t.id === themeId) || RESTAURANT_THEMES[0];
    this.theme = theme;
    this.floor.material.color.setHex(theme.floor);
    this.backWall.material.color.setHex(theme.wall);
    this.sideWall.material.color.setHex(theme.wall);
    this.accentStrip.material.color.setHex(theme.accent);
    this.accentStrip.material.emissive.setHex(theme.accent);
    this.scene.background = new THREE.Color(theme.sky);
    this.scene.fog.color.setHex(theme.sky);
    this.warmLight.color.setHex(theme.accent);
  }

  applyLocationPalette(loc) {
    this.floor.material.color.setHex(loc.floor);
    this.backWall.material.color.setHex(loc.wall);
    this.sideWall.material.color.setHex(loc.wall);
    this.accentStrip.material.color.setHex(loc.accent);
    this.accentStrip.material.emissive.setHex(loc.accent);
    this.scene.background = new THREE.Color(loc.sky);
    this.scene.fog.color.setHex(loc.sky);
  }

  setShadowsEnabled(v) {
    this.renderer.shadowMap.enabled = v;
    this.sun.castShadow = v;
    this.scene.traverse(o => { if (o.isMesh) o.material.needsUpdate = true; });
  }

  // ══════════════════ Slot / görsel yardımcıları ══════════════════
  /** Izgara slotlarını seviyeye göre yeniden kurar */
  layoutGrillSlots(count) {
    this.grillSlotsGroup.clear();
    const slots = [];
    const cols = Math.min(3, count);
    const rows = Math.ceil(count / 3);
    for (let i = 0; i < count; i++) {
      const col = i % cols;
      const row = Math.floor(i / cols);
      const g = new THREE.Group();
      const spacingX = 0.72;
      const spacingZ = 0.42;
      const offsetX = (cols - 1) * spacingX * 0.5;
      const offsetZ = (rows - 1) * spacingZ * 0.5;
      g.position.set(col * spacingX - offsetX, 0, row * spacingZ - offsetZ);

      const ring = new THREE.Mesh(
        new THREE.CylinderGeometry(0.3, 0.3, 0.05, 16),
        new THREE.MeshStandardMaterial({ color: 0x55555f, roughness: 0.6 }),
      );
      ring.userData.interactable = { kind: 'grillSlot', index: i };
      g.add(ring);

      this.grillSlotsGroup.add(g);
      slots.push({ group: g, ring, index: i });
    }
    // Eski slot mesh'lerini pickables'tan temizle
    this.pickables = this.pickables.filter(p => !p.userData.interactable
      || p.userData.interactable.kind !== 'grillSlot');
    slots.forEach(s => this.pickables.push(s.ring));
    this.grillSlotNodes = slots;
    return slots;
  }

  resize() {
    const w = this.container.clientWidth || window.innerWidth;
    const h = this.container.clientHeight || window.innerHeight;
    this.renderer.setSize(w, h, false);

    const aspect = w / h;
    this.camera.aspect = aspect;

    // Kamerayı sahneye göre konumlandır: yatayda en az ±10 birim görünür olsun.
    // Böylece dikey (portrait) ekranlarda ızgara ve drive-thru kadraj dışında kalmaz.
    const target = new THREE.Vector3(0.6, 1.1, -0.4);
    const dir = new THREE.Vector3(-0.015, 0.585, 0.815).normalize();
    const halfV = Math.tan((this.camera.fov * Math.PI) / 360);
    const dist = Math.min(42, Math.max(13.5, 10.2 / (halfV * aspect)));
    this.camera.position.copy(target).addScaledVector(dir, dist);
    this.camera.lookAt(target);
    this.camera.updateProjectionMatrix();
  }

  update(dt) {
    this.time += dt;
    for (const a of this.animated) a.update(dt);
    if (this.flame) {
      this.flame.material.opacity = 0.25 + Math.sin(this.time * 8) * 0.08;
    }
    this.renderer.render(this.scene, this.camera);
  }

  dispose() {
    window.removeEventListener('resize', this._onResize);
    this.renderer.dispose();
    if (this.renderer.domElement.parentNode) this.renderer.domElement.parentNode.removeChild(this.renderer.domElement);
  }
}

// ══════════════════ Yardımcı üreticiler ══════════════════

/** Emoji/metin sprite'ı (canvas tabanlı, harici asset yok) */
export function makeTextSprite(text, opts = {}) {
  const { w = 256, h = 128, size = 64, color = '#FFFFFF', bg = 'rgba(0,0,0,0.55)', font = 'bold' } = opts;
  const canvas = document.createElement('canvas');
  canvas.width = w; canvas.height = h;
  const ctx = canvas.getContext('2d');
  if (bg !== 'rgba(0,0,0,0)') { ctx.fillStyle = bg; ctx.fillRect(0, 0, w, h); }
  ctx.font = `${font} ${size}px "Nunito", "Segoe UI", system-ui, sans-serif`;
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillStyle = color;
  const lines = String(text).split('\n');
  const lh = size * 1.15;
  lines.forEach((line, i) => ctx.fillText(line, w / 2, h / 2 + (i - (lines.length - 1) / 2) * lh));
  const tex = new THREE.CanvasTexture(canvas);
  tex.colorSpace = THREE.SRGBColorSpace;
  const sprite = new THREE.Sprite(new THREE.SpriteMaterial({ map: tex, transparent: true, depthTest: true }));
  sprite.userData.canvas = canvas;
  sprite.userData.ctx = ctx;
  sprite.userData.tex = tex;
  return sprite;
}

/** Düşük poligonlu karakter (müşteri / personel / şef) */
export function makeCharacter({ body = 0x6fa8dc, head = 0xffd9b3, hat = null, apron = null, scale = 1 } = {}) {
  const g = new THREE.Group();
  const bodyMesh = new THREE.Mesh(
    new THREE.CylinderGeometry(0.3, 0.34, 0.75, 12),
    new THREE.MeshStandardMaterial({ color: body, roughness: 0.75 }),
  );
  bodyMesh.position.y = 0.62; bodyMesh.castShadow = true;
  bodyMesh.userData.baseY = 0.62; bodyMesh.userData.role = 'body';
  g.add(bodyMesh);

  const headMesh = new THREE.Mesh(
    new THREE.SphereGeometry(0.27, 14, 12),
    new THREE.MeshStandardMaterial({ color: head, roughness: 0.8 }),
  );
  headMesh.position.y = 1.22; headMesh.castShadow = true;
  headMesh.userData.baseY = 1.22; headMesh.userData.role = 'head';
  g.add(headMesh);

  const legs = new THREE.Mesh(
    new THREE.CylinderGeometry(0.14, 0.16, 0.3, 10),
    new THREE.MeshStandardMaterial({ color: 0x33334a, roughness: 0.9 }),
  );
  legs.position.y = 0.15;
  g.add(legs);

  // Kollar
  [-1, 1].forEach(s => {
    const arm = new THREE.Mesh(
      new THREE.CylinderGeometry(0.075, 0.075, 0.5, 8),
      new THREE.MeshStandardMaterial({ color: body, roughness: 0.75 }),
    );
    arm.position.set(s * 0.33, 0.72, 0);
    arm.rotation.z = s * 0.25;
    arm.userData.baseY = 0.72; arm.userData.role = 'arm';
    g.add(arm);
  });

  if (apron != null) {
    const ap = new THREE.Mesh(
      new THREE.BoxGeometry(0.5, 0.5, 0.1),
      new THREE.MeshStandardMaterial({ color: apron, roughness: 0.8 }),
    );
    ap.position.set(0, 0.55, 0.26);
    ap.userData.baseY = 0.55; ap.userData.role = 'apron';
    g.add(ap);
  }

  if (hat != null) {
    const h = new THREE.Mesh(
      new THREE.CylinderGeometry(0.24, 0.2, 0.3, 12),
      new THREE.MeshStandardMaterial({ color: hat, roughness: 0.7 }),
    );
    h.position.y = 1.6; h.castShadow = true;
    h.userData.baseY = 1.6; h.userData.role = 'hat';
    g.add(h);
  }

  g.scale.setScalar(scale);
  g.userData.parts = { bodyMesh, headMesh };
  return g;
}

/** Hamburger katmanı mesh'i */
export function makeLayerMesh(ingredientId, state = 'raw') {
  const def = INGREDIENTS[ingredientId];
  let color = def.color;
  if (ingredientId === 'patty') color = state === 'burnt' ? def.burntColor : state === 'cooked' ? def.cookedColor : def.color;

  let geo;
  switch (ingredientId) {
    case 'bun': geo = new THREE.CylinderGeometry(0.42, 0.4, 0.13, 18); break;
    case 'patty': geo = new THREE.CylinderGeometry(0.4, 0.4, 0.12, 18); break;
    case 'cheese': geo = new THREE.BoxGeometry(0.78, 0.05, 0.78); break;
    case 'lettuce': geo = new THREE.CylinderGeometry(0.46, 0.44, 0.07, 18); break;
    case 'tomato': geo = new THREE.CylinderGeometry(0.4, 0.4, 0.07, 18); break;
    case 'sauce': geo = new THREE.CylinderGeometry(0.32, 0.32, 0.04, 16); break;
    default: geo = new THREE.BoxGeometry(0.5, 0.1, 0.5);
  }
  const mesh = new THREE.Mesh(geo, new THREE.MeshStandardMaterial({ color, roughness: 0.7 }));
  mesh.castShadow = true;
  return mesh;
}

/** Üst ekmek (kubbe) */
export function makeBunTop() {
  const geo = new THREE.SphereGeometry(0.43, 18, 12, 0, Math.PI * 2, 0, Math.PI / 2);
  const mesh = new THREE.Mesh(geo, new THREE.MeshStandardMaterial({ color: 0xE8B26A, roughness: 0.7 }));
  mesh.scale.y = 0.62;
  mesh.castShadow = true;
  return mesh;
}

export { COUNTER_TOP };
