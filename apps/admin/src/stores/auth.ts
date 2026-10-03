import { defineStore } from 'pinia';
import type { User } from '@bkk/api';

// Own key: never share or clobber the player / ClubOS sessions.
const KEY = 'bkk.admin.auth';

interface Saved { token: string; user: User; expiresAt: string }

function load(): Saved | null {
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return null;
    const s = JSON.parse(raw) as Saved;
    return Date.parse(s.expiresAt) > Date.now() && s.user.role === 'platform_admin' ? s : null;
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
