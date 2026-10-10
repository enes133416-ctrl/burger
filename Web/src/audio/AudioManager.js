/**
 * AudioManager.js — Ses efektleri & müzik (GDD 9.3)
 * WebAudio ile prosedürel üretilir; harici ses dosyası gerekmez.
 */
import { SaveManager } from '../core/SaveManager.js';

class AudioManagerCore {
  constructor() {
    this.ctx = null;
    this.masterGain = null;
    this.musicGain = null;
    this.sfxGain = null;
    this.musicTimer = null;
    this.currentTrack = null;
    this.started = false;
  }

  /** Kullanıcı etkileşimi sonrası çağrılmalı (tarayıcı politikası) */
  init() {
    if (this.ctx) return;
    const Ctx = window.AudioContext || window.webkitAudioContext;
    if (!Ctx) return;
    this.ctx = new Ctx();
    this.masterGain = this.ctx.createGain();
    this.masterGain.gain.value = 0.9;
    this.masterGain.connect(this.ctx.destination);

    this.sfxGain = this.ctx.createGain();
    this.sfxGain.gain.value = 0.55;
    this.sfxGain.connect(this.masterGain);

    this.musicGain = this.ctx.createGain();
    this.musicGain.gain.value = 0.22;
    this.musicGain.connect(this.masterGain);
    this.started = true;
  }

  resume() { if (this.ctx && this.ctx.state === 'suspended') this.ctx.resume(); }

  get sfxEnabled() { return SaveManager.data.settings.sfx; }
  get musicEnabled() { return SaveManager.data.settings.music; }

  _tone({ freq = 440, type = 'sine', dur = 0.18, gain = 0.5, sweep = null, delay = 0, target = null }) {
    if (!this.ctx || !this.sfxEnabled) return;
    const t0 = this.ctx.currentTime + delay;
    const osc = this.ctx.createOscillator();
    const g = this.ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(freq, t0);
    if (sweep) osc.frequency.exponentialRampToValueAtTime(Math.max(20, sweep), t0 + dur);
    g.gain.setValueAtTime(0.0001, t0);
    g.gain.exponentialRampToValueAtTime(gain, t0 + 0.012);
    g.gain.exponentialRampToValueAtTime(0.0001, t0 + dur);
    osc.connect(g);
    g.connect(target || this.sfxGain);
    osc.start(t0);
    osc.stop(t0 + dur + 0.03);
  }

  _noise({ dur = 0.25, gain = 0.25, delay = 0 }) {
    if (!this.ctx || !this.sfxEnabled) return;
    const len = Math.floor(this.ctx.sampleRate * dur);
    const buffer = this.ctx.createBuffer(1, len, this.ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < len; i++) data[i] = (Math.random() * 2 - 1) * (1 - i / len);
    const src = this.ctx.createBufferSource();
    src.buffer = buffer;
    const g = this.ctx.createGain();
    g.gain.value = gain;
    const filter = this.ctx.createBiquadFilter();
    filter.type = 'bandpass';
    filter.frequency.value = 1200;
    src.connect(filter); filter.connect(g); g.connect(this.sfxGain);
    src.start(this.ctx.currentTime + delay);
  }

  /** GDD 9.3 ses listesi */
  play(name) {
    if (!this.ctx) return;
    switch (name) {
      case 'coin':      // "Ching!"
        this._tone({ freq: 1180, type: 'triangle', dur: 0.12, gain: 0.35 });
        this._tone({ freq: 1760, type: 'triangle', dur: 0.16, gain: 0.28, delay: 0.06 });
        break;
      case 'ding':      // Fırın hazır
        this._tone({ freq: 1320, type: 'sine', dur: 0.35, gain: 0.35 });
        this._tone({ freq: 1980, type: 'sine', dur: 0.3, gain: 0.18, delay: 0.03 });
        break;
      case 'thwack':    // Burger katlama
        this._tone({ freq: 260, type: 'square', dur: 0.09, gain: 0.3, sweep: 90 });
        this._noise({ dur: 0.08, gain: 0.15 });
        break;
      case 'sizzle':
        this._noise({ dur: 0.5, gain: 0.12 });
        break;
      case 'burn':
        this._noise({ dur: 0.6, gain: 0.28 });
        this._tone({ freq: 320, type: 'sawtooth', dur: 0.5, gain: 0.2, sweep: 110 });
        break;
      case 'happy':
        this._tone({ freq: 660, type: 'sine', dur: 0.14, gain: 0.3 });
        this._tone({ freq: 880, type: 'sine', dur: 0.18, gain: 0.28, delay: 0.1 });
        break;
      case 'angry':
        this._tone({ freq: 300, type: 'sawtooth', dur: 0.28, gain: 0.25, sweep: 140 });
        break;
      case 'sparkle':   // Yıldız kazanımı
        [1046, 1318, 1568, 2093].forEach((f, i) => this._tone({ freq: f, type: 'sine', dur: 0.22, gain: 0.22, delay: i * 0.07 }));
        break;
      case 'beep':      // Drive-thru zili
        this._tone({ freq: 880, type: 'square', dur: 0.12, gain: 0.25 });
        break;
      case 'powerup':   // Upgrade
        [523, 659, 784, 1046].forEach((f, i) => this._tone({ freq: f, type: 'triangle', dur: 0.16, gain: 0.25, delay: i * 0.05 }));
        break;
      case 'click':
        this._tone({ freq: 520, type: 'square', dur: 0.05, gain: 0.18 });
        break;
      case 'fail':
        [440, 392, 330, 262].forEach((f, i) => this._tone({ freq: f, type: 'triangle', dur: 0.28, gain: 0.25, delay: i * 0.14 }));
        break;
      case 'victory':
        [523, 659, 784, 1046, 1318].forEach((f, i) => this._tone({ freq: f, type: 'triangle', dur: 0.3, gain: 0.26, delay: i * 0.1 }));
        break;
      default: break;
    }
  }

  // ─────────── Müzik (basit prosedürel loop) ───────────
  playMusic(track = 'menu') {
    if (!this.ctx) return;
    if (this.currentTrack === track) return;
    this.stopMusic();
    this.currentTrack = track;
    if (!this.musicEnabled) return;

    const patterns = {
      menu: [261.6, 329.6, 392.0, 329.6, 349.2, 440.0, 392.0, 293.7],
      gameplay: [392.0, 392.0, 523.3, 587.3, 523.3, 392.0, 440.0, 392.0],
      rush: [440.0, 523.3, 659.3, 880.0, 659.3, 880.0, 987.8, 1046.5],
    };
    const notes = patterns[track] || patterns.menu;
    const step = track === 'rush' ? 0.26 : 0.4;
    let i = 0;

    const tick = () => {
      if (!this.musicEnabled || !this.ctx) return;
      const f = notes[i % notes.length];
      this._tone({ freq: f, type: 'triangle', dur: step * 1.4, gain: 0.35, target: this.musicGain });
      if (i % 4 === 0) this._tone({ freq: f / 2, type: 'sine', dur: step * 2, gain: 0.4, target: this.musicGain });
      i++;
    };
    tick();
    this.musicTimer = setInterval(tick, step * 1000);
  }

  stopMusic() {
    if (this.musicTimer) clearInterval(this.musicTimer);
    this.musicTimer = null;
    this.currentTrack = null;
  }

  setMusicEnabled(v) { if (!v) this.stopMusic(); else this.playMusic('menu'); }
}

export const AudioManager = new AudioManagerCore();
export default AudioManager;
