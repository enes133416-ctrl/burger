/**
 * main.js — Bootstrapper: sistemleri başlatır, ana döngüyü kurar (GDD 6.1)
 */
import './styles.css';
import { GameManager } from './core/GameManager.js';
import { UIManager } from './ui/UIManager.js';
import { SaveManager } from './core/SaveManager.js';
import { EconomyManager } from './economy/EconomyManager.js';
import { AudioManager } from './audio/AudioManager.js';
import { AdManager } from './monetization/AdManager.js';
import { AnalyticsManager } from './analytics/AnalyticsManager.js';
import { UpgradeSystem } from './economy/UpgradeSystem.js';
import { ChefPassManager } from './monetization/ChefPassManager.js';
import { SeasonalEventManager } from './seasonal/SeasonalEventManager.js';
import { CHEF_SKINS } from './data/CosmeticData.js';

function bootstrap() {
  const container = document.getElementById('viewport');

  const ui = new UIManager();
  const game = new GameManager(container, ui);
  ui.bind(game);

  // Sistem başlatma sırası (GDD 8.2 Bootstrapper)
  AdManager.init();
  AnalyticsManager.init();
  ChefPassManager.check();
  SeasonalEventManager.apply(game.world);

  // Kayıtlı kozmetikleri uygula
  const skin = CHEF_SKINS.find(s => s.id === SaveManager.data.equippedSkin);
  if (skin) game.world.setChefSkin(skin);
  game.world.setShadowsEnabled(SaveManager.data.settings.shadows);

  // Ses yalnızca ilk kullanıcı etkileşiminden sonra başlatılabilir
  const unlockAudio = () => { AudioManager.init(); AudioManager.resume(); };
  window.addEventListener('pointerdown', unlockAudio, { once: true });
  window.addEventListener('keydown', unlockAudio, { once: true });

  ui.start();

  // ─────────── Ana döngü ───────────
  let last = performance.now();
  const loop = (now) => {
    const dt = Math.min(0.05, (now - last) / 1000);   // max 50ms (lag koruması)
    last = now;
    game.update(dt);
    ui.tick();
    requestAnimationFrame(loop);
  };
  requestAnimationFrame(loop);

  // ─────────── Uygulama yaşam döngüsü ───────────
  document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
      SaveManager.stampExit();
      if (game.state === 'Gameplay') game.pause();
    }
  });
  window.addEventListener('beforeunload', () => SaveManager.stampExit());

  // Otomatik kayıt
  setInterval(() => SaveManager.save(true), 15000);

  // Debug konsolu (geliştirme kolaylığı)
  window.BurgerRush = { game, ui, SaveManager, EconomyManager, UpgradeSystem, AnalyticsManager };
}

if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap);
else bootstrap();
