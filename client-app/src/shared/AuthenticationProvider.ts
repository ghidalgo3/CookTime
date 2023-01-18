type Role = "User" | "Administrator";

interface UserData {
  name: string,
  roles: Role[]
}

type SignInResult = "Success" | "Failure"

export interface IAuthenticationProvider {
  user : UserData | null,

  signIn(
    usernameOrEmail : string,
    password : string,
    rememberMe: boolean) : Promise<SignInResult>,

  signOut() : Promise<void>,
}

export const AuthenticationProvider : IAuthenticationProvider = {
  user: null,
  signIn: async function (usernameOrEmail: string, password: string, rememberMe: boolean): Promise<SignInResult> {
    let formData = new FormData();
  formData.append('userNameOrEmail', usernameOrEmail);
  formData.append('password', password);
    const response = await fetch("/api/account/signin", {
      method: "post",
      body: formData
    })
    if (response.ok) {
      this.user = await response.json()
      return "Success"
    } else {
      return "Failure"
    }
  },
  signOut: function (): Promise<void> {
    throw new Error("Function not implemented.");
  }
}