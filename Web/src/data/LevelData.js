/**
 * LevelData.js — 50 bölümlük ilerleme haritası (GDD Bölüm 3)
 * Değerler prosedürel üretilir, ancak boss/milestone bölümleri elle işaretlenir.
 */
import { LOCATIONS } from './Locations.js';

export const BOSS_LEVELS = [5, 15, 25, 35, 45];
export const MILESTONE_LEVELS = [10, 20, 30, 40, 50];

/** Hangi malzeme hangi bölümde açılır (GDD 3.1) */
export function unlockedIngredients(level) {
  const list = ['bun', 'patty'];
  if (level >= 2) list.push('cheese');
  if (level >= 4) list.push('lettuce', 'tomato');
  if (level >= 7) list.push('sauce');
  return list;
}

/** Hangi müşteri tipi hangi bölümde görünür */
export function unlockedCustomerTypes(level) {
  const list = ['Casual'];
  if (level >= 2) list.push('BusinessPerson');
  if (level >= 5) list.push('Child');
  if (level >= 13) list.push('VIP');
  if (level >= 17) list.push('Influencer');
  return list;
}

const BOSS_NAMES = {
  5: 'Rush Hour',
  15: 'Celebrity VIP Visit',
  25: 'Food Critic',
  35: 'Franchise Denetimi',
  45: 'Dünya Turu Finali',
};

/**
 * Bir bölümün tanımını üretir.
 * @param {number} index 1..50
 */
export function buildLevel(index) {
  const location = LOCATIONS.find(l => index >= l.from && index <= l.to) || LOCATIONS[LOCATIONS.length - 1];
  const isBoss = BOSS_LEVELS.includes(index);
  const isMilestone = MILESTONE_LEVELS.includes(index);

  const orders = Math.min(30, 6 + Math.round(index * 1.1));
  const duration = Math.min(210, 95 + index * 4);
  const ingredients = unlockedIngredients(index);
  const maxFilling = Math.min(4, 1 + Math.floor(index / 4));

  return {
    index,
    location: location.id,
    locationName: location.name,
    name: isBoss ? BOSS_NAMES[index] : isMilestone ? 'Dönüm Noktası' : `Bölüm ${index}`,
    isBoss,
    isMilestone,
    orders,                                  // toplam müşteri sayısı
    minServe: Math.ceil(orders * (isBoss ? 0.75 : 0.6)), // başarı için gereken servis
    duration,                                // saniye
    spawnInterval: Math.max(2.2, 6.5 - index * 0.07),
    queueSize: Math.min(8, 3 + Math.floor(index / 2)),
    ingredients,
    customerTypes: unlockedCustomerTypes(index),
    maxFilling,
    comboChance: index >= 12 ? Math.min(0.35, 0.08 + index * 0.006) : 0,
    driveThru: index >= 8,
    priceScale: 1 + (index - 1) * 0.08,
    stars: { one: 0.6, two: 0.8, three: 0.95 },
  };
}

/** Tüm bölümler (cache'li) */
let cache = null;
export function allLevels() {
  if (!cache) {
    cache = [];
    for (let i = 1; i <= 50; i++) cache.push(buildLevel(i));
  }
  return cache;
}

export function getLevel(index) {
  return allLevels()[Math.min(index, 50) - 1];
}

/** 3 yıldız hesabı (GDD 3.2) */
export function computeStars(level, served, total, complaints, lost) {
  const ratio = total > 0 ? served / total : 0;
  if (ratio >= level.stars.three && complaints === 0 && lost === 0) return 3;
  if (ratio >= level.stars.two && complaints === 0) return 2;
  if (ratio >= level.stars.one) return 1;
  return 0;
}
