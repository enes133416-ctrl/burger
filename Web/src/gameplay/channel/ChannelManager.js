/**
 * ChannelManager.js — Counter + Drive-Thru kanallarının yönetimi (GDD 2.2)
 */
import { Channel } from './Channel.js';
import { CHANNELS, UNLOCKS } from '../../core/Config.js';
import { UpgradeSystem } from '../../economy/UpgradeSystem.js';
import { AudioManager } from '../../audio/AudioManager.js';

export class ChannelManager {
  constructor(world) {
    this.world = world;
    this.channels = {};
  }

  setup(level) {
    const queueSize = Math.min(8, level.queueSize + UpgradeSystem.queueBonus);
    this.channels[CHANNELS.COUNTER] = new Channel(CHANNELS.COUNTER, this.world, queueSize);
    this.channels[CHANNELS.DRIVE_THRU] = new Channel(CHANNELS.DRIVE_THRU, this.world, 4);

    const dtUnlocked = level.index >= UNLOCKS.driveThru;
    this.world.setDriveThruVisible(dtUnlocked);
    this.driveThruUnlocked = dtUnlocked;
    this.driveThruMultiplier = 1;
    this.driveThruCompleted = 0;
  }

  get counter() { return this.channels[CHANNELS.COUNTER]; }
  get driveThru() { return this.channels[CHANNELS.DRIVE_THRU]; }

  /** Yeni müşteri için kanal seç (Drive-Thru açıksa %35 şans) */
  pickChannel() {
    if (this.driveThruUnlocked && this.driveThru.hasRoom && Math.random() < 0.35) return this.driveThru;
    if (this.counter.hasRoom) return this.counter;
    if (this.driveThruUnlocked && this.driveThru.hasRoom) return this.driveThru;
    return null;
  }

  /** Drive-Thru tamamlanan her araç çarpanı artırır (GDD 2.2) */
  registerDriveThruComplete() {
    this.driveThruCompleted++;
    this.driveThruMultiplier = 1 + Math.min(1.0, this.driveThruCompleted * 0.08);
    AudioManager.play('beep');
  }

  releaseAll() {
    Object.values(this.channels).forEach(c => c.clear());
  }
}
