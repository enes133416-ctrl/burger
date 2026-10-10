/**
 * logic.test.mjs — Saf mantık modülleri için duman testi (three.js gerektirmez)
 * Çalıştır: node tests/logic.test.mjs
 */
// ── Tarayıcı API'leri için最小 shim ──
const store = new Map();
globalThis.localStorage = {
  getItem: k => (store.has(k) ? store.get(k) : null),
  setItem: (k, v) => store.set(k, String(v)),
  removeItem: k => store.delete(k),
  clear: () => store.clear(),
};
globalThis.performance = globalThis.performance || { now: () => Date.now() };

const { SaveManager } = await import('../src/core/SaveManager.js');
const { EconomyManager } = await import('../src/economy/EconomyManager.js');
const { UpgradeSystem } = await import('../src/economy/UpgradeSystem.js');
const { OfflineEarningsCalculator } = await import('../src/economy/OfflineEarnings.js');
const { buildLevel, allLevels, computeStars, unlockedIngredients } = await import('../src/data/LevelData.js');
const { LOCATIONS } = await import('../src/data/Locations.js');
const { BurgerBuilder, generateOrder } = await import('../src/gameplay/kitchen/BurgerBuilder.js');
const { CUSTOMER_TYPES, OVEN } = await import('../src/core/Config.js');

let failures = 0;
function check(name, cond, extra = '') {
  if (cond) console.log(`  ✅ ${name}`);
  else { console.error(`  ❌ ${name} ${extra}`); failures++; }
}

console.log('\n[1] Save / Economy');
check('varsayılan bakiye', SaveManager.data.totalCoins === 250, SaveManager.data.totalCoins);
EconomyManager.addCoins(1000);
check('para eklendi', EconomyManager.coins === 1250, EconomyManager.coins);
check('para harcama', EconomyManager.deductCoins(500) && EconomyManager.coins === 750);
EconomyManager.addGems(10);
check('gem eklendi', EconomyManager.gems === 40, EconomyManager.gems);

console.log('\n[2] Level verisi');
const levels = allLevels();
check('50 bölüm üretildi', levels.length === 50);
check('bölüm 1 öğretici', levels[0].ingredients.length === 2, JSON.stringify(levels[0].ingredients));
check('boss bölümleri işaretli', [5, 15, 25, 35, 45].every(i => levels[i - 1].isBoss));
check('milestone bölümleri', [10, 20, 30, 40, 50].every(i => levels[i - 1].isMilestone));
check('drive-thru bölüm 8', levels[7].driveThru === true && levels[6].driveThru === false);
check('her bölümün lokasyonu var', levels.every(l => LOCATIONS.some(loc => l.index >= loc.from && l.index <= loc.to)));
check('malzeme kilitleri', unlockedIngredients(1).length === 2 && unlockedIngredients(7).length === 6);

console.log('\n[3] Yıldız hesabı');
const l1 = buildLevel(1);
check('3 yıldız', computeStars(l1, 8, 8, 0, 0) === 3);
check('2 yıldız (şikayet yok)', computeStars(l1, 7, 8, 0, 1) === 2);
check('1 yıldız', computeStars(l1, 6, 8, 2, 1) === 1);
check('0 yıldız', computeStars(l1, 2, 8, 3, 4) === 0);

console.log('\n[4] Burger doğrulama');
const order = { fillings: ['patty', 'cheese'], basePrice: 50, isCombo: false };
const good = new BurgerBuilder();
good.add('bun'); good.add('patty', 'cooked'); good.add('cheese'); good.add('bun');
check('doğru sipariş', good.evaluate(order).ok);
check('tam burger', good.isComplete && good.hasTopBun);

const extra = new BurgerBuilder();
extra.add('bun'); extra.add('patty', 'cooked'); extra.add('cheese'); extra.add('lettuce'); extra.add('bun');
const rExtra = extra.evaluate(order);
check('ekstra katman → bahşiş', rExtra.ok && rExtra.tipFactor === 0.1);

const raw = new BurgerBuilder();
raw.add('bun'); raw.add('patty', 'raw'); raw.add('cheese'); raw.add('bun');
check('çiğ köfte reddedilir', raw.evaluate(order).reason === 'Köfte çiğ!');

const burnt = new BurgerBuilder();
burnt.add('bun'); burnt.add('patty', 'burnt'); burnt.add('cheese'); burnt.add('bun');
check('yanmış köfte reddedilir', burnt.evaluate(order).reason === 'Yanmış köfte!');

const missing = new BurgerBuilder();
missing.add('bun'); missing.add('patty', 'cooked'); missing.add('bun');
check('eksik malzeme', missing.evaluate(order).ok === false && missing.evaluate(order).missing[0] === 'cheese');

console.log('\n[5] Sipariş üretimi');
for (let i = 0; i < 200; i++) {
  const lv = buildLevel(Math.floor(Math.random() * 50) + 1);
  const t = CUSTOMER_TYPES[lv.customerTypes[0]];
  const o = generateOrder(lv, t);
  if (!o.fillings.includes('patty') || o.basePrice <= 0) { check('sipariş geçerli', false, JSON.stringify(o)); break; }
}
check('200 sipariş geçerli', true);

console.log('\n[6] Upgrade sistemi');
check('başlangıç fırın Lv1', UpgradeSystem.grillConfig.slots === OVEN.levels[0].slots);
EconomyManager.addCoins(100000);
const hire = UpgradeSystem.hire('cashier');
check('kasiyer kiralandı', hire.ok && UpgradeSystem.isHired('cashier'));
const inc1 = UpgradeSystem.incomeMultiplier;
const up = UpgradeSystem.upgradeStaff('cashier');
check('kasiyer yükseltildi', up.ok && UpgradeSystem.staffLevel('cashier') === 2);
check('gelir arttı', UpgradeSystem.incomeMultiplier > inc1);
const eq = UpgradeSystem.buyEquipment('grill');
check('fırın yükseltildi', eq.ok && UpgradeSystem.grillConfig.slots === 3);
check('kuyruk bonusu', UpgradeSystem.queueBonus === 0);
check('offline verim (personel var)', UpgradeSystem.offlineEfficiency === 0.45, UpgradeSystem.offlineEfficiency);

console.log('\n[7] Offline gelir');
SaveManager.data.lastOfflineTime = Date.now() - 4 * 3600 * 1000;
const res = OfflineEarningsCalculator.calculate();
check('4 saat hesaplandı', Math.abs(res.hours - 4) < 0.01, res.hours);
check('kazanç pozitif', res.earnings > 0, res.earnings);
SaveManager.data.lastOfflineTime = Date.now() - 40 * 3600 * 1000;
const capped = OfflineEarningsCalculator.calculate();
check('tavan 8 saat', capped.hours === 8, capped.hours);
const claimed = OfflineEarningsCalculator.claim(false);
check('tahsil edildi', claimed === capped.earnings && SaveManager.data.offlineEarnings === 0);

console.log(`\n${failures === 0 ? '✅ TÜM TESTLER GEÇTİ' : `❌ ${failures} TEST BAŞARISIZ`}\n`);
process.exit(failures === 0 ? 0 : 1);
