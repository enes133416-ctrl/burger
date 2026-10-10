/**
 * CosmeticData.js — Kozmetik içerikler (GDD 4.3 / 9.1)
 * Fiyatlar ya gem ya da gerçek para (demo) cinsindendir.
 */

export const RESTAURANT_THEMES = [
  { id: 'default',   name: 'Klasik Burger', emoji: '🍔', price: 0,    iap: null,   floor: 0x4A4A63, wall: 0x22223A, accent: 0xFF6B35, sky: 0x1A1A2E },
  { id: 'cyberpunk', name: 'Neon Cyberpunk Diner', emoji: '🌃', price: 60,  iap: '$2.99', floor: 0x1B1030, wall: 0x2A0F4A, accent: 0x00E5FF, sky: 0x0A0618 },
  { id: 'vintage',   name: 'Vintage 50s American', emoji: '🚙', price: 40,  iap: '$1.99', floor: 0xD9534F, wall: 0xF5E6C8, accent: 0x2ECC71, sky: 0xF7D9A0 },
  { id: 'tokyo',     name: 'Tokyo Street Food', emoji: '🏮', price: 75,  iap: '$3.99', floor: 0x2F2F44, wall: 0x8E2B4A, accent: 0xFF5C8A, sky: 0x171024 },
  { id: 'tropical',  name: 'Tropical Paradise', emoji: '🌴', price: 50,  iap: '$2.99', floor: 0x2E8B57, wall: 0x7FD8E8, accent: 0xFFD700, sky: 0x8FD8F2 },
  { id: 'space',     name: 'Space Station Cafeteria', emoji: '🚀', price: 100, iap: '$5.99', floor: 0x232338, wall: 0x12122A, accent: 0x6FD3FF, sky: 0x05050F },
];

export const CHEF_SKINS = [
  { id: 'classic',  name: 'Klasik Şef', emoji: '👨‍🍳', price: 0,   iap: null,   body: 0xF2F2F7, hat: 0xFFFFFF, apron: 0xFF6B35 },
  { id: 'ninja',    name: 'Ninja Chef', emoji: '🥷', price: 25,  iap: '$0.99', body: 0x1C1C28, hat: 0x2E2E40, apron: 0xBB6BD9 },
  { id: 'robot',    name: 'Robot Cook', emoji: '🤖', price: 35,  iap: '$1.99', body: 0xB8C4D6, hat: 0x8FA3BF, apron: 0x00E5FF },
  { id: 'viking',   name: 'Viking Burger Master', emoji: '🪓', price: 45, iap: '$2.99', body: 0xC89B6A, hat: 0x8B6F47, apron: 0x8B4A2B },
  { id: 'astronaut',name: 'Astronaut Grill Master', emoji: '🧑‍🚀', price: 80, iap: '$2.99', body: 0xE8E8F0, hat: 0xCFE8FF, apron: 0x6FD3FF },
  { id: 'gold',     name: 'Gold Chef (Starter Pack)', emoji: '✨', price: 0, iap: 'pack', body: 0xFFD700, hat: 0xFFF3B0, apron: 0xD9531F },
];

/** IAP paketleri (GDD 4.3) — web demo sürümünde simülasyondur */
export const IAP_PACKS = [
  {
    id: 'starter', name: '🍔 Starter Pack', iap: '$1.99', oneTime: true,
    contents: { coins: 10000, gems: 50, skin: 'gold', doubleOfflineHours: 24 },
    desc: '10.000 🪙 · 50 💎 · Gold Chef önlüğü · 24 saat 2x offline gelir',
  },
  {
    id: 'chefpass', name: '👨‍🍳 Chef Pass', iap: '$4.99', weekly: true,
    contents: { chefPassDays: 7 },
    desc: 'Günlük 2.000 🪙 · Reklam sayısı -%50 · VIP spawn +%20 · Özel rozet',
  },
];

export const GEM_PACKS = [
  { id: 'gems_50',   gems: 50,   iap: '$0.99' },
  { id: 'gems_150',  gems: 150,  iap: '$2.99' },
  { id: 'gems_500',  gems: 500,  iap: '$7.99', popular: true },
  { id: 'gems_1200', gems: 1200, iap: '$14.99' },
  { id: 'gems_3500', gems: 3500, iap: '$34.99' },
];

/** Sezonsal etkinlikler (GDD 10) */
export const SEASONAL_EVENTS = [
  { id: 'halloween', name: '🎃 Halloween Haunted Burger', month: 10, accent: 0xE67E22, sky: 0x120A1E },
  { id: 'christmas', name: '🎄 Christmas Cozy Café',      month: 12, accent: 0x2ECC71, sky: 0x0E1B2A },
  { id: 'valentine', name: "💝 Valentine's Sweetheart",   month: 2,  accent: 0xFF5C8A, sky: 0x2A0E1A },
  { id: 'sakura',    name: '🌸 Cherry Blossom Fusion',    month: 4,  accent: 0xFF9EC4, sky: 0x2A1A2E },
  { id: 'summer',    name: '☀️ Summer Beach Grill',       month: 7,  accent: 0x7FD8E8, sky: 0x8FD8F2 },
];

export function currentSeasonalEvent(date = new Date()) {
  return SEASONAL_EVENTS.find(e => e.month === date.getMonth() + 1) || null;
}
