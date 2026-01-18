export type Role = "User" | "Administrator";

export interface UserDetails {
  name: string,
  id: string,
  csrfToken: string
  roles: Role[],
  needsProfileSetup: boolean,
}

export interface IAuthenticationProvider {
  user: UserDetails | null,

  signOut(): Promise<boolean>,

  getUserDetails(): Promise<UserDetails | null>,

  updateDisplayName(displayName: string): Promise<{ ok: true } | { ok: false, message: string }>,
}

export const AuthenticationProvider: IAuthenticationProvider = {
  user: null,

  signOut: async function (): Promise<boolean> {
    const response = await fetch("/api/auth/signout");
    return response.ok;
  },

  getUserDetails: async function (): Promise<UserDetails | null> {
    const response = await fetch("/api/account/profile");
    if (response.ok) {
      return await response.json();
    } else {
      return null;
    }
  },

  updateDisplayName: async function (displayName: string): Promise<{ ok: true } | { ok: false, message: string }> {
    const response = await fetch("/api/account/profile", {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ displayName }),
    });
    if (response.ok) {
      return { ok: true };
    } else {
      const error = await response.json().catch(() => ({ message: "Something went wrong" }));
      return { ok: false, message: error.message || "Something went wrong" };
    }
  },
}