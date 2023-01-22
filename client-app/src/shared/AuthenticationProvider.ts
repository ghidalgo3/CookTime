export type Role = "User" | "Administrator";

export interface UserData {
  name: string,
  roles: Role[],
  csrfToken: string
}

export type AuthResult = "Success" | "Failure"

export interface IAuthenticationProvider {
  user : UserData | null,

  signUp(
    userName : string,
    email: string,
    password: string,
    confirmPassword: string) : Promise<AuthResult>,
  
  signIn(
    usernameOrEmail : string,
    password : string,
    rememberMe: boolean) : Promise<AuthResult>,

  signOut() : Promise<void>,
}

export const AuthenticationProvider : IAuthenticationProvider = {
  user: null,

  signIn: async function (usernameOrEmail: string, password: string, rememberMe: boolean): Promise<AuthResult> {
    let formData = new FormData();
    formData.append('userNameOrEmail', usernameOrEmail);
    formData.append('password', password);
    const response = await fetch("/api/account/signin", {
      method: "post",
      body: formData
    });
    if (response.ok) {
      this.user = await response.json();
      return "Success";
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

  signUp: function (userName: string, email: string, password: string, confirmPassword: string): Promise<AuthResult> {
    throw new Error("Function not implemented.");
  }
}