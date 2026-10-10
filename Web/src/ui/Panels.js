/**
 * Panels.js — HR Office, Upgrade Office, Shop, Gardırop, Ayarlar, Offline gelir (GDD 5.2)
 */
import { STAFF, EQUIPMENT, staffById, equipmentById } from '../data/UpgradeData.js';
import { RESTAURANT_THEMES, CHEF_SKINS, IAP_PACKS, GEM_PACKS } from '../data/CosmeticData.js';
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from '../economy/EconomyManager.js';
import { UpgradeSystem } from '../economy/UpgradeSystem.js';
import { AudioManager } from '../audio/AudioManager.js';
import { AnalyticsManager } from '../analytics/AnalyticsManager.js';
import { eventBus, EVENTS } from '../core/EventBus.js';
import { formatNumber } from './HUD.js';
import { Haptic } from '../input/HapticManager.js';

const PANEL_IDS = ['panel-offline', 'panel-hr', 'panel-upgrade', 'panel-shop', 'panel-wardrobe', 'panel-settings'];

export class Panels {
  constructor(game, ui) {
    this.game = game;
    this.ui = ui;
    this.layer = document.getElementById('panel-layer');
    this.shopTab = 'packs';
    this.wardrobeTab = 'themes';
    this.bind();

    eventBus.subscribe(EVENTS.COIN_CHANGED, () => this.refreshOpenPanel());
    eventBus.subscribe(EVENTS.GEM_CHANGED, () => this.refreshOpenPanel());
  }

  bind() {
    this.layer.addEventListener('click', e => {
      if (e.target === this.layer) this.close();
    });
    document.querySelectorAll('[data-close-panel]').forEach(b => b.addEventListener('click', () => {
      AudioManager.play('click');
      this.close();
    }));

    document.getElementById('btn-offline-claim').addEventListener('click', () => {
      AudioManager.init(); AudioManager.play('coin');
      this.ui.claimOffline(false);
      this.close();
    });
    document.getElementById('btn-offline-ad').addEventListener('click', () => {
      this.ui.showRewardedAd('DoubleOfflineEarnings');
    });

    // Ayarlar
    const s = SaveManager.data.settings;
    const bindToggle = (id, key, onChange) => {
      const el = document.getElementById(id);
      el.checked = s[key];
      el.addEventListener('change', () => {
        SaveManager.data.settings[key] = el.checked;
        SaveManager.save();
        if (onChange) onChange(el.checked);
      });
    };
    bindToggle('set-music', 'music', v => AudioManager.setMusicEnabled(v));
    bindToggle('set-sfx', 'sfx');
    bindToggle('set-haptic', 'haptic');
    bindToggle('set-shadows', 'shadows', v => this.game.world.setShadowsEnabled(v));

    document.getElementById('btn-reset-save').addEventListener('click', () => {
      if (!confirm('Tüm ilerleme silinecek. Emin misin?')) return;
      SaveManager.reset();
      location.reload();
    });
  }

  open(id) {
    PANEL_IDS.forEach(p => document.getElementById(p).classList.add('hidden'));
    const panel = document.getElementById(`panel-${id}`);
    if (!panel) return;
    panel.classList.remove('hidden');
    this.layer.classList.remove('hidden');
    this.openId = id;
    if (id === 'hr') this.renderHR();
    if (id === 'upgrade') this.renderUpgrade();
    if (id === 'shop') this.renderShop();
    if (id === 'wardrobe') this.renderWardrobe();
    if (id === 'settings') this.renderSettings();
  }

  close() {
    this.layer.classList.add('hidden');
    PANEL_IDS.forEach(p => document.getElementById(p).classList.add('hidden'));
    this.openId = null;
  }

  refreshOpenPanel() {
    if (this.openId === 'hr') this.renderHR();
    else if (this.openId === 'upgrade') this.renderUpgrade();
    else if (this.openId === 'shop') this.renderShop();
    else if (this.openId === 'wardrobe') this.renderWardrobe();
  }

  // ══════════════════ Offline gelir ══════════════════
  showOffline(result) {
    PANEL_IDS.forEach(p => document.getElementById(p).classList.add('hidden'));
    document.getElementById('panel-offline').classList.remove('hidden');
    this.layer.classList.remove('hidden');
    this.openId = 'offline';
    document.getElementById('offline-desc').textContent =
      `${result.hours.toFixed(1)} saat kapalıydın · verim %${Math.round(result.efficiency * 100)} · saatlik 🪙${formatNumber(result.hourly)}`;
    document.getElementById('offline-amount').textContent = `🪙 ${formatNumber(result.earnings)}`;
    document.getElementById('btn-offline-ad').classList.toggle('hidden', !result.canDoubleWithAd);
  }

  // ══════════════════ HR Office ══════════════════
  renderHR() {
    const list = document.getElementById('hr-list');
    list.innerHTML = '';
    STAFF.forEach(def => {
      const hired = UpgradeSystem.isHired(def.id);
      const level = UpgradeSystem.staffLevel(def.id);
      const nextCost = UpgradeSystem.staffNextCost(def.id);
      const card = document.createElement('div');
      card.className = 'card';
      const pips = Array.from({ length: 5 }, (_, i) => `<div class="pip ${i < level ? 'on' : ''}"></div>`).join('');

      let actionHtml;
      if (!hired) {
        actionHtml = `<button class="btn btn-buy" data-hire="${def.id}" ${EconomyManager.canAffordCoins(def.hireCost) ? '' : 'disabled'}>🪙${formatNumber(def.hireCost)}</button>`;
      } else if (nextCost != null) {
        actionHtml = `<button class="btn btn-buy" data-up="${def.id}" ${EconomyManager.canAffordCoins(nextCost) ? '' : 'disabled'}>🪙${formatNumber(nextCost)}</button>`;
      } else {
        actionHtml = `<span class="tag owned">MAX</span>`;
      }

      card.innerHTML = `
        <div class="card-row">
          <div class="card-emoji">${def.emoji}</div>
          <div class="card-body">
            <div class="card-title">${def.name} ${hired ? `<span class="tag">Lv ${level}</span>` : '<span class="tag">İşe alınmadı</span>'}</div>
            <div class="card-sub">${def.desc}</div>
            <div class="pips">${pips}</div>
            ${hired ? `<div class="card-sub">Sıradaki: ${nextCost != null ? def.levels[level].perk : '—'}</div>` : ''}
          </div>
          ${actionHtml}
        </div>`;
      list.appendChild(card);
    });

    list.querySelectorAll('[data-hire]').forEach(b => b.addEventListener('click', () => {
      const res = UpgradeSystem.hire(b.dataset.hire);
      if (!res.ok) { this.ui.toast('Yetersiz bakiye 🪙', 'bad'); return; }
      AudioManager.play('powerup');
      Haptic.success();
      this.game.staff.refreshAvatars();
      this.ui.toast('Personel işe alındı! 👨‍🍳', 'good');
      this.renderHR();
    }));

    list.querySelectorAll('[data-up]').forEach(b => b.addEventListener('click', () => {
      const res = UpgradeSystem.upgradeStaff(b.dataset.up);
      if (!res.ok) { this.ui.toast('Yetersiz bakiye 🪙', 'bad'); return; }
      AudioManager.play('powerup');
      Haptic.success();
      this.game.staff.refreshAvatars();
      this.ui.toast(`${staffById(b.dataset.up).name} → Lv ${res.level}`, 'good');
      this.renderHR();
    }));
  }

  // ══════════════════ Upgrade Office ══════════════════
  renderUpgrade() {
    const list = document.getElementById('upgrade-list');
    list.innerHTML = '';
    EQUIPMENT.forEach(def => {
      const level = UpgradeSystem.equipmentLevel(def.id);
      const cost = UpgradeSystem.equipmentNextCost(def.id);
      const card = document.createElement('div');
      card.className = 'card';
      const pips = Array.from({ length: 5 }, (_, i) => `<div class="pip ${i < level ? 'on' : ''}"></div>`).join('');
      card.innerHTML = `
        <div class="card-row">
          <div class="card-emoji">${def.emoji}</div>
          <div class="card-body">
            <div class="card-title">${def.name} <span class="tag">Lv ${level}</span></div>
            <div class="card-sub">${def.desc}</div>
            <div class="pips">${pips}</div>
            <div class="card-sub">Mevcut: ${def.perks[level - 1]}</div>
            ${cost != null ? `<div class="card-sub">Sıradaki: ${def.perks[level]}</div>` : ''}
          </div>
          ${cost != null
            ? `<button class="btn btn-buy" data-eq="${def.id}" ${EconomyManager.canAffordCoins(cost) ? '' : 'disabled'}>🪙${formatNumber(cost)}</button>`
            : '<span class="tag owned">MAX</span>'}
        </div>`;
      list.appendChild(card);
    });

    list.querySelectorAll('[data-eq]').forEach(b => b.addEventListener('click', () => {
      const res = UpgradeSystem.buyEquipment(b.dataset.eq);
      if (!res.ok) { this.ui.toast('Yetersiz bakiye 🪙', 'bad'); return; }
      AudioManager.play('powerup');
      Haptic.success();
      if (b.dataset.eq === 'grill') {
        this.game.oven.applyLevel(res.level);
        this.ui.toast(`Fırın Lv ${res.level}: ${equipmentById('grill').perks[res.level - 1]}`, 'good');
      } else {
        this.ui.toast(`${equipmentById(b.dataset.eq).name} → Lv ${res.level}`, 'good');
      }
      this.renderUpgrade();
    }));
  }

  // ══════════════════ Mağaza (IAP simülasyonu) ══════════════════
  renderShop() {
    const tabs = document.getElementById('shop-tabs');
    tabs.innerHTML = '';
    [['packs', '📦 Paketler'], ['gems', '💎 Gem']].forEach(([id, label]) => {
      const t = document.createElement('div');
      t.className = `tab ${this.shopTab === id ? 'active' : ''}`;
      t.textContent = label;
      t.addEventListener('click', () => { this.shopTab = id; this.renderShop(); });
      tabs.appendChild(t);
    });

    const list = document.getElementById('shop-list');
    list.innerHTML = '';

    if (this.shopTab === 'packs') {
      IAP_PACKS.forEach(pack => {
        const owned = pack.oneTime && SaveManager.data.flags.starterPackBought;
        const card = document.createElement('div');
        card.className = 'card';
        card.innerHTML = `
          <div class="card-row">
            <div class="card-emoji">${pack.name.split(' ')[0]}</div>
            <div class="card-body">
              <div class="card-title">${pack.name.split(' ').slice(1).join(' ')} <span class="tag">${pack.iap}</span></div>
              <div class="card-sub">${pack.desc}</div>
            </div>
            <button class="btn btn-gem" data-pack="${pack.id}" ${owned ? 'disabled' : ''}>${owned ? 'Alındı' : 'Satın al'}</button>
          </div>`;
        list.appendChild(card);
      });
      list.querySelectorAll('[data-pack]').forEach(b => b.addEventListener('click', () => this.buyPack(b.dataset.pack)));
    } else {
      GEM_PACKS.forEach(p => {
        const card = document.createElement('div');
        card.className = 'card';
        card.innerHTML = `
          <div class="card-row">
            <div class="card-emoji">💎</div>
            <div class="card-body">
              <div class="card-title">${p.gems} Gem ${p.popular ? '<span class="tag milestone">EN POPÜLER</span>' : ''}</div>
              <div class="card-sub">${p.iap} · demo satın alma</div>
            </div>
            <button class="btn btn-gem" data-gems="${p.gems}" data-id="${p.id}">${p.iap}</button>
          </div>`;
        list.appendChild(card);
      });
      list.querySelectorAll('[data-gems]').forEach(b => b.addEventListener('click', () => {
        EconomyManager.addGems(Number(b.dataset.gems));
        AnalyticsManager.trackIAP(b.dataset.id, b.textContent);
        AudioManager.play('sparkle');
        this.ui.toast(`+${b.dataset.gems} 💎 eklendi (demo)`, 'good');
      }));
    }
  }

  buyPack(id) {
    const pack = IAP_PACKS.find(p => p.id === id);
    if (!pack) return;
    if (id === 'starter') {
      if (SaveManager.data.flags.starterPackBought) return;
      SaveManager.data.flags.starterPackBought = true;
      EconomyManager.addCoins(pack.contents.coins, 'iap');
      EconomyManager.addGems(pack.contents.gems);
      if (!SaveManager.data.unlockedCosmetics.includes('gold')) SaveManager.data.unlockedCosmetics.push('gold');
      SaveManager.data.equippedSkin = 'gold';
      this.game.world.setChefSkin(CHEF_SKINS.find(s => s.id === 'gold'));
    } else if (id === 'chefpass') {
      EconomyManager.grantChefPass(pack.contents.chefPassDays, 2000);
    }
    AnalyticsManager.trackIAP(id, pack.iap);
    AudioManager.play('sparkle');
    this.ui.toast(`${pack.name} alındı! (demo)`, 'good');
    this.renderShop();
  }

  // ══════════════════ Gardırop ══════════════════
  renderWardrobe() {
    const tabs = document.getElementById('wardrobe-tabs');
    tabs.innerHTML = '';
    [['themes', '🎨 Restoran Temaları'], ['skins', '👨‍🍳 Şef Kostümleri']].forEach(([id, label]) => {
      const t = document.createElement('div');
      t.className = `tab ${this.wardrobeTab === id ? 'active' : ''}`;
      t.textContent = label;
      t.addEventListener('click', () => { this.wardrobeTab = id; this.renderWardrobe(); });
      tabs.appendChild(t);
    });

    const list = document.getElementById('wardrobe-list');
    list.innerHTML = '';
    const items = this.wardrobeTab === 'themes' ? RESTAURANT_THEMES : CHEF_SKINS;
    const owned = SaveManager.data.unlockedCosmetics;

    items.forEach(item => {
      const isOwned = owned.includes(item.id);
      const equipped = this.wardrobeTab === 'themes'
        ? SaveManager.data.equippedTheme === item.id
        : SaveManager.data.equippedSkin === item.id;
      const card = document.createElement('div');
      card.className = `card ${isOwned ? '' : 'locked'}`;

      let action;
      if (equipped) action = '<span class="tag equipped">KUŞANILDI</span>';
      else if (isOwned) action = `<button class="btn btn-primary" data-equip="${item.id}">Kuşan</button>`;
      else action = `<button class="btn btn-gem" data-buy="${item.id}" ${EconomyManager.canAffordGems(item.price) ? '' : 'disabled'}>💎${item.price}</button>`;

      card.innerHTML = `
        <div class="card-row">
          <div class="card-emoji">${item.emoji}</div>
          <div class="card-body">
            <div class="card-title">${item.name} ${equipped ? '' : ''}</div>
            <div class="card-sub">${item.iap ? `Normalde ${item.iap}` : (isOwned ? 'Sahipsin' : `💎 ${item.price} gem`)}</div>
          </div>
          ${action}
        </div>`;
      list.appendChild(card);
    });

    list.querySelectorAll('[data-buy]').forEach(b => b.addEventListener('click', () => {
      const item = items.find(i => i.id === b.dataset.buy);
      if (!EconomyManager.spendGems(item.price)) { this.ui.toast('Yetersiz gem 💎', 'bad'); return; }
      SaveManager.data.unlockedCosmetics.push(item.id);
      SaveManager.save();
      AudioManager.play('sparkle');
      this.renderWardrobe();
    }));

    list.querySelectorAll('[data-equip]').forEach(b => b.addEventListener('click', () => {
      const id = b.dataset.equip;
      if (this.wardrobeTab === 'themes') {
        SaveManager.data.equippedTheme = id;
        this.game.world.setTheme(id);
      } else {
        SaveManager.data.equippedSkin = id;
        this.game.world.setChefSkin(CHEF_SKINS.find(s => s.id === id));
      }
      SaveManager.save();
      AudioManager.play('click');
      this.renderWardrobe();
    }));
  }

  renderSettings() {
    const s = SaveManager.data.stats;
    document.getElementById('set-stats').textContent =
      `🍔 ${formatNumber(s.served)} servis · 🔥 ${formatNumber(s.burnt)} yanık · 😞 ${formatNumber(s.lost)} kaçan · ⏱ ${Math.round(s.playSeconds / 60)} dk`;
  }
}
