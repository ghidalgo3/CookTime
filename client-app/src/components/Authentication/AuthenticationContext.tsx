import React, {useEffect, useState} from "react"
import { useContext } from "react";
import { useLocation, Navigate } from "react-router-dom";
import { AuthenticationProvider, IAuthenticationProvider, Role } from "src/shared/AuthenticationProvider";

export let AuthContext = React.createContext<IAuthenticationProvider>(AuthenticationProvider);

export function useAuthentication() {
  return useContext(AuthContext);
}

export function AuthenticationContext({ children } : { children : React.ReactNode }) {
  const auth = useAuthentication();
  return (
    <AuthContext.Provider value={auth}>
      {children}
    </AuthContext.Provider>
  );
}

/**
 * This component selectively renders UI depending on the authentication state
 * of the user and their authorized roles.
 */
export function RequireAuth({ children, roles }: { roles: Role[], children: JSX.Element , redirect?: boolean | undefined}) {
  let auth = useAuthentication();
  // let location = useLocation();

  if (
    !auth.user
    // if the user doesn't have the right role, navigate them away
    || auth.user && auth.user.roles.find(role => roles.find(r => r === role))) {

    // Redirect them to the /login page, but save the current location they were
    // trying to go to when they were redirected. This allows us to send them
    // along to that page after they login, which is a nicer user experience
    // than dropping them off on the home page.
    return <></>
    // return <Navigate to="/signin" state={{ from: location }} replace />;
  }

  return children;
}