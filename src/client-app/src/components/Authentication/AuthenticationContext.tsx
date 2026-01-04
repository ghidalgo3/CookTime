import React, { useEffect, useState } from "react"
import { useContext } from "react";
import { AuthenticationProvider, IAuthenticationProvider, Role, UserDetails } from "src/shared/AuthenticationProvider";

export const AuthContext = React.createContext<IAuthenticationProvider>(AuthenticationProvider);

export function useAuthentication() {
  return useContext(AuthContext);
}

export function AuthenticationContext({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDetails | null>(null);

  useEffect(() => {
    if (user == null) {
      AuthenticationProvider.getUserDetails().then(value => {
        setUser(value)
      })
    }
  }, [user])

  return (
    <AuthContext.Provider value={{
      user,
      signIn:
        async (usernameOrEmail, password, rememberMe) => {
          const userDetails = await AuthenticationProvider.signIn(usernameOrEmail, password, rememberMe);
          if (userDetails != "Failure") {
            setUser(userDetails);
          }
          return userDetails;
        },
      signOut: async () => {
        const didSignOut = await AuthenticationProvider.signOut();
        if (didSignOut) {
          setUser(null);
        }
        return didSignOut;
      },
      signUp: AuthenticationProvider.signUp,
      getUserDetails: AuthenticationProvider.getUserDetails,
      sendPasswordResetEmail: AuthenticationProvider.sendPasswordResetEmail,
      changePassword: AuthenticationProvider.changePassword
    }}>
      {children}
    </AuthContext.Provider>
  );
}

/**
 * This component selectively renders UI depending on the authentication state
 * of the user and their authorized roles.
 */
export function RequireAuth({ children, roles }: { roles: Role[], children: JSX.Element, redirect?: boolean | undefined }) {
  let auth = useAuthentication();
  // let location = useLocation();

  if (
    !auth.user
    // if the user doesn't have the right role, don't render anything
    || (auth.user && !auth.user.roles.find(role => roles.find(r => r === role)))) {
    return <></>
  }

  return children;
}