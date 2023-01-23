export type Role = "User" | "Administrator";

export interface UserDetails {
  name: string,
  id: string,
  csrfToken: string
  roles: Role[],
}

export type AuthResult = "Success" | "Failure"

export interface IAuthenticationProvider {
  user : UserDetails | null,

  signUp(
    userName : string,
    email: string,
    password: string,
    confirmPassword: string) : Promise<UserDetails | "Failure">,
  
  signIn(
    usernameOrEmail : string,
    password : string,
    rememberMe: boolean) : Promise<UserDetails | "Failure">,

  signOut() : Promise<void>,

  getUserDetails() : Promise<UserDetails | null>,
}

export const AuthenticationProvider : IAuthenticationProvider = {
  user: null,

  signIn: async function (usernameOrEmail: string, password: string, rememberMe: boolean): Promise<UserDetails | "Failure"> {
    let formData = new FormData();
    formData.append('userNameOrEmail', usernameOrEmail);
    formData.append('password', password);
    const response = await fetch("/api/account/signin", {
      method: "post",
      body: formData
    });
    if (response.ok) {
      return await response.json();
    } else {
      return "Failure";
    }
  },


  signOut: function (): Promise<void> {
    // TODO call server signout
    // TODO set current user to null
    // TODO notify subscribers auth state has changed.
    throw new Error("Function not implemented.");
  },

  signUp: async function (userName: string, email: string, password: string, confirmPassword: string): Promise<UserDetails | "Failure"> {
    return "Failure";
    // throw new Error("Function not implemented.");
  },

  getUserDetails: async function (): Promise<UserDetails | null> {
    const response = await fetch("/api/account/profile")
    if (response.ok) {
      return await response.json();
    } else {
      return null;
    }
  }
}