/**
 * AssemblyStation.js — Montaj tezgahı: burger katmanlarının 3D gösterimi (GDD 2.3)
 */
import { BurgerBuilder } from './BurgerBuilder.js';
import { makeLayerMesh, makeBunTop } from '../KitchenWorld.js';
import { AudioManager } from '../../audio/AudioManager.js';
import { Haptic } from '../../input/HapticManager.js';

export class AssemblyStation {
  constructor(world) {
    this.world = world;
    this.builder = new BurgerBuilder();
    this.group = world.burgerGroup;
  }

  get isEmpty() { return this.builder.isEmpty; }
  get isComplete() { return this.builder.isComplete; }

  addLayer(id, state = 'raw') {
    this.builder.add(id, state);
    this.rebuild();
    AudioManager.play('thwack');
    Haptic.light();
  }

  /** Tabağı boşalt ve içeriği döndür */
  takeBurger() {
    if (this.builder.isEmpty) return null;
    const layers = this.builder.toJSON();
    this.builder.clear();
    this.rebuild();
    return layers;
  }

  clear() { this.builder.clear(); this.rebuild(); }

  /** Katman mesh'lerini sıfırdan kur */
  rebuild() {
    this.group.clear();
    let y = 0;
    const layers = this.builder.layers;
    layers.forEach((layer, i) => {
      const isTopBun = layer.id === 'bun' && i === layers.length - 1 && layers.length > 1;
      const mesh = isTopBun ? makeBunTop() : makeLayerMesh(layer.id, layer.state);
      const height = isTopBun ? 0.26 : (mesh.geometry.parameters.height || 0.1);
      mesh.position.y = y + (isTopBun ? 0.14 : height / 2);
      this.group.add(mesh);
      y += height;
    });

    // Tabağın üstüne "hazır" parıltısı
    this.world.plateMesh.material.emissive.setHex(this.builder.isComplete ? 0x2ecc71 : 0x000000);
    this.world.plateMesh.material.emissiveIntensity = this.builder.isComplete ? 0.45 : 0;
  }
}
