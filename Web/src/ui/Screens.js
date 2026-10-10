/**
 * Screens.js — Ekran akışı: Boot → MainMenu → WorldMap → LevelSelect → Sonuç (GDD 5.2)
 */
import { LOCATIONS } from '../data/Locations.js';
import { getLevel } from '../data/LevelData.js';
import { SaveManager } from '../core/SaveManager.js';
import { EconomyManager } from '../economy/EconomyManager.js';
import { UpgradeSystem } from '../economy/UpgradeSystem.js';
import { AudioManager } from '../audio/AudioManager.js';
import { formatNumber } from './HUD.js';

const SCREEN_IDS = ['screen-boot', 'screen-menu', 'screen-worldmap', 'screen-levelselect',
  'screen-result', 'screen-fail'];

export class Screens {
  constructor(game, ui) {
    this.game = game;
    this.ui = ui;
    this.current = 'screen-boot';
    this.selectedLocation = 'downtown';
    this.lastResult = null;
    this.bind();
  }

  show(id) {
    SCREEN_IDS.forEach(sid => document.getElementById(sid).classList.toggle('active', sid === id));
    this.current = id;
  }

  hideAll() {
    SCREEN_IDS.forEach(sid => document.getElementById(sid).classList.remove('active'));
    this.current = null;
  }

  bind() {
    const click = (id, fn) => {
      const el = document.getElementById(id);
      if (el) el.addEventListener('click', () => { AudioManager.init(); AudioManager.play('click'); fn(); });
    };

    click('btn-play', () => this.openWorldMap());
    click('btn-shop', () => this.ui.panels.open('shop'));
    click('btn-wardrobe', () => this.ui.panels.open('wardrobe'));
    click('btn-settings', () => this.ui.panels.open('settings'));

    document.querySelectorAll('[data-back]').forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.play('click');
        const target = btn.dataset.back;
        if (target === 'menu') this.openMenu();
        else if (target === 'worldmap') this.openWorldMap();
      });
    });

    // Sonuç ekranı
    click('btn-result-ad', () => {
      const btn = document.getElementById('btn-result-ad');
      this.ui.showRewardedAd('DoubleIncome', () => {
        if (!this.lastResult) return;
        EconomyManager.addCoins(this.lastResult.earned, 'result_ad');
        this.lastResult.earned *= 2;
        this.renderResultRows(this.lastResult);
        btn.disabled = true;
        btn.textContent = '✅ 2x Alındı';
      });
    });
    click('btn-result-next', () => {
      const next = Math.min(50, SaveManager.data.currentLevel + 1);
      this.hideAll();
      this.game.startLevel(next);
    });
    click('btn-result-menu', () => { this.game.quitToMenu(); this.openMenu(); });

    // Başarısızlık ekranı
    click('btn-revive-ad', () => this.ui.showRewardedAd('ReviveLevel'));
    click('btn-revive-gem', () => {
      this.hideAll();
      this.game.skipLevelWithGems();
    });
    click('btn-fail-menu', () => { this.game.quitToMenu(); this.openMenu(); });

    // Pause
    click('btn-resume', () => { this.showPause(false); this.game.resume(); });
    click('btn-restart', () => { this.showPause(false); this.game.restartLevel(); });
    click('btn-quit', () => { this.showPause(false); this.game.quitToMenu(); this.openMenu(); });
  }

  // ══════════════════ Boot ══════════════════
  runBoot(onDone) {
    const fill = document.getElementById('boot-fill');
    const text = document.getElementById('boot-text');
    const steps = [
      [25, 'SDK\'lar başlatılıyor…'],
      [55, 'Kayıt dosyası okunuyor…'],
      [80, 'Mutfak hazırlanıyor…'],
      [100, 'Hazır!'],
    ];
    let i = 0;
    const tick = () => {
      if (i < steps.length) {
        fill.style.width = `${steps[i][0]}%`;
        text.textContent = steps[i][1];
        i++;
        setTimeout(tick, 320);
      } else {
        setTimeout(onDone, 250);
      }
    };
    tick();
  }

  // ══════════════════ Ana menü ══════════════════
  openMenu() {
    this.show('screen-menu');
    const s = SaveManager.data;
    document.getElementById('menu-stats').innerHTML = `
      <span>📍 Bölüm ${s.currentLevel}</span>
      <span>⭐ ${s.totalStars} yıldız</span>
      <span>🍔 ${formatNumber(s.stats.served)} servis</span>
      <span>👨‍🍳 ${UpgradeSystem.activeStaffCount} personel</span>`;
    document.getElementById('chef-pass-badge').classList.toggle('hidden', !(s.hasChefPass && Date.now() < s.chefPassExpiry));
    document.getElementById('wm-coins').textContent = formatNumber(EconomyManager.coins);
    AudioManager.playMusic('menu');
  }

  // ══════════════════ Dünya haritası ══════════════════
  openWorldMap() {
    this.show('screen-worldmap');
    const list = document.getElementById('location-list');
    const maxLevel = SaveManager.data.maxLevelReached || 1;
    list.innerHTML = '';

    LOCATIONS.forEach(loc => {
      const locked = maxLevel < loc.from;
      const stars = this.starsInRange(loc.from, loc.to);
      const maxStars = this.maxStarsInRange(loc.from, loc.to);
      const card = document.createElement('div');
      card.className = `card ${locked ? 'locked' : ''}`;
      card.innerHTML = `
        <div class="card-row">
          <div class="card-emoji" style="font-size:34px">${loc.name.split(' ')[0]}</div>
          <div class="card-body">
            <div class="card-title">${loc.name.split(' ').slice(1).join(' ')}</div>
            <div class="card-sub">Bölüm ${loc.from}-${loc.to} · ⭐ ${stars}/${maxStars}</div>
          </div>
          <button class="btn ${locked ? 'btn-ghost' : 'btn-primary'}" ${locked ? 'disabled' : ''}>
            ${locked ? '🔒' : 'Seç'}
          </button>
        </div>`;
      if (!locked) {
        card.querySelector('button').addEventListener('click', () => {
          AudioManager.play('click');
          this.openLevelSelect(loc.id);
        });
      }
      list.appendChild(card);
    });
  }

  starsInRange(from, to) {
    let total = 0;
    for (let i = from; i <= Math.min(to, 50); i++) total += SaveManager.data.levelStars[i] || 0;
    return total;
  }

  maxStarsInRange(from, to) { return (Math.min(to, 50) - from + 1) * 3; }

  // ══════════════════ Bölüm seçimi ══════════════════
  openLevelSelect(locationId) {
    this.selectedLocation = locationId;
    const loc = LOCATIONS.find(l => l.id === locationId);
    this.show('screen-levelselect');
    document.getElementById('ls-title').textContent = loc.name;
    document.getElementById('ls-coins').textContent = formatNumber(EconomyManager.coins);

    const grid = document.getElementById('level-grid');
    grid.innerHTML = '';
    const maxLevel = SaveManager.data.maxLevelReached || 1;

    for (let i = loc.from; i <= Math.min(loc.to, 50); i++) {
      const level = getLevel(i);
      const locked = i > maxLevel;
      const stars = SaveManager.data.levelStars[i] || 0;
      const cell = document.createElement('div');
      cell.className = `level-cell ${locked ? 'locked' : ''} ${i === SaveManager.data.currentLevel ? 'current' : ''} ${stars > 0 ? 'done' : ''} ${level.isBoss ? 'boss' : ''}`;
      cell.innerHTML = `${i}<div class="mini-stars">${'★'.repeat(stars)}${'☆'.repeat(3 - stars)}</div>`;
      cell.title = level.isBoss ? `BOSS: ${level.name}` : level.name;
      if (!locked) {
        cell.addEventListener('click', () => {
          AudioManager.init();
          AudioManager.play('click');
          this.hideAll();
          this.game.startLevel(i);
        });
      }
      grid.appendChild(cell);
    }
  }

  // ══════════════════ Sonuç ══════════════════
  showResult(data) {
    this.lastResult = { ...data };
    this.show('screen-result');
    document.getElementById('result-title').textContent =
      data.stars > 0 ? `Bölüm ${data.level.index} Tamamlandı!` : 'Bölüm Bitti';
    document.getElementById('result-stars').textContent = '★'.repeat(data.stars) + '☆'.repeat(3 - data.stars);
    this.renderResultRows(this.lastResult);

    const nextBtn = document.getElementById('btn-result-next');
    nextBtn.textContent = data.level.index >= 50 ? '🏆 İmparatorluk Kuruldu!' : `Sonraki Bölüm ▶ (${data.level.index + 1})`;
    nextBtn.disabled = data.level.index >= 50;

    const adBtn = document.getElementById('btn-result-ad');
    adBtn.disabled = false;
    adBtn.textContent = '🎬 Ödülü 2x Al';
  }

  renderResultRows(data) {
    const rows = [
      ['💵 Kazanç', `🪙 ${formatNumber(data.earned)}`],
      ['🍔 Servis', `${data.served}/${data.total}`],
      ['😞 Kaçan müşteri', `${data.lost}`],
      ['😡 Şikayet', `${data.complaints}`],
    ];
    if (data.bonus > 0) rows.push(['🎁 Milestone bonusu', `🪙 ${formatNumber(data.bonus)}`]);
    rows.push(['💰 Toplam', `🪙 ${formatNumber(EconomyManager.coins)}`]);
    document.getElementById('result-rows').innerHTML =
      rows.map(([k, v]) => `<div class="result-row"><span>${k}</span><span>${v}</span></div>`).join('');
  }

  showFail(data) {
    this.show('screen-fail');
    document.getElementById('fail-rows').innerHTML = `
      <div class="result-row"><span>🍔 Servis</span><span>${data.served}/${data.total}</span></div>
      <div class="result-row"><span>🎯 Hedef</span><span>${data.minServe}</span></div>
      <div class="result-row"><span>💡 İpucu</span><span>Personel kirala, fırını yükselt</span></div>`;
  }

  showPause(v) {
    document.getElementById('pause-layer').classList.toggle('hidden', !v);
  }
}
