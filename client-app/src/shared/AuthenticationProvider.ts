export type Role = "User" | "Administrator";

export interface UserDetails {
  name: string,
  id: string,
  csrfToken: string
  roles: Role[],
}

export interface SignUpResult {
  success: boolean,
  message: string
}

export type AuthResult = "Success" | "Failure"

export interface IAuthenticationProvider {
  user : UserDetails | null,

  signUp(
    userName : string,
    email: string,
    password: string,
    confirmPassword: string) : Promise<SignUpResult>,
  
  signIn(
    usernameOrEmail : string,
    password : string,
    rememberMe: boolean) : Promise<UserDetails | "Failure">,

  signOut() : Promise<boolean>,

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


  signOut: async function (): Promise<boolean> {
    const response = await fetch("/api/account/signout")
    return response.ok;
  },

  signUp: async function (userName: string, email: string, password: string, confirmPassword: string): Promise<SignUpResult> {
    let formData = new FormData();
    formData.append('username', userName);
    formData.append('email', email);
    formData.append('password', password);
    formData.append('confirmPassword', confirmPassword);
    const response = await fetch("/api/account/signup", {
      method: "post",
      body: formData
    });
    if (response.ok) {
      return await response.json() as SignUpResult;
    } else {
      const error = await response.json() 
      return {
        success: false,
        message: "Username and email must be unique in CookTime. Password must be between 6 and 100 characters long."
      }
    }
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