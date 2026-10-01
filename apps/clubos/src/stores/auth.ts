import { defineStore } from 'pinia';
import type { User } from '@bkk/api';

// Own key: the player app (another origin in dev, but the same in some deployments) must never share or clobber this session.
const KEY = 'bkk.clubos.auth';

interface Saved { token: string; user: User; expiresAt: string }

function load(): Saved | null {
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return null;
    const s = JSON.parse(raw) as Saved;
    return Date.parse(s.expiresAt) > Date.now() ? s : null;
  } catch {
    return null;
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => {
    const s = load();
    return { token: s?.token ?? null as string | null, user: s?.user ?? null as User | null, expiresAt: s?.expiresAt ?? null as string | null };
  },
  getters: {
    isLoggedIn: (s) => !!s.token,
    isOwner: (s) => s.user?.role === 'partner_owner',
  },
  actions: {
    login(token: string, user: User, expiresAt: string) {
      Object.assign(this, { token, user, expiresAt });
      try { localStorage.setItem(KEY, JSON.stringify({ token, user, expiresAt })); } catch { /* private mode */ }
    },
    logout() {
      Object.assign(this, { token: null, user: null, expiresAt: null });
      try { localStorage.removeItem(KEY); } catch { /* ignore */ }
    },
  },
});
