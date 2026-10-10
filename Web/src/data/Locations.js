/**
 * Locations.js — Dünya haritası lokasyonları (GDD 3.3)
 * Renkler 3D sahnenin zemin/duvar paletini belirler.
 */
export const LOCATIONS = [
  {
    id: 'downtown', name: '🏙️ Downtown Burger Stand', from: 1, to: 10,
    floor: 0x4A4A63, wall: 0x22223A, accent: 0xFF6B35, sky: 0x1A1A2E, weather: 'city',
  },
  {
    id: 'beach', name: '🏖️ Beach Burger Bar', from: 11, to: 20,
    floor: 0xE9D3A0, wall: 0x7FD8E8, accent: 0x2ECC71, sky: 0x8FD8F2, weather: 'sunny',
  },
  {
    id: 'rooftop', name: '🌆 Rooftop Lounge', from: 21, to: 30,
    floor: 0x3B3B55, wall: 0x15152A, accent: 0xBB6BD9, sky: 0x2B1B46, weather: 'night',
  },
  {
    id: 'mountain', name: '🏔️ Mountain Rest Stop', from: 31, to: 40,
    floor: 0x8C8C9E, wall: 0xDDE5EF, accent: 0x7FD8E8, sky: 0xBFD7EA, weather: 'snow',
  },
  {
    id: 'tokyo', name: '🗼 Tokyo Fusion', from: 41, to: 50,
    floor: 0x2F2F44, wall: 0x8E2B4A, accent: 0xFF5C8A, sky: 0x171024, weather: 'neon',
  },
  {
    id: 'foodtruck', name: '🎪 Festival Food Truck', from: 51, to: 55, seasonal: true,
    floor: 0x5C4B3A, wall: 0x8E5B2B, accent: 0xFFD700, sky: 0x2A1E12, weather: 'festival',
  },
  {
    id: 'space', name: '🚀 Space Burger', from: 56, to: 60, endgame: true,
    floor: 0x232338, wall: 0x12122A, accent: 0x6FD3FF, sky: 0x05050F, weather: 'space',
  },
];

export function locationById(id) {
  return LOCATIONS.find(l => l.id === id) || LOCATIONS[0];
}

export function locationForLevel(level) {
  return LOCATIONS.find(l => level >= l.from && level <= l.to) || LOCATIONS[0];
}
