import React, {useEffect, useState} from"react"
import { Outlet, useLocation } from "react-router-dom";
import { CookTimeBanner, NavigationBar, SignUp } from "src/components";

export default function DefaultLayout() {
  const location = useLocation();
  return (
    <>
    <NavigationBar />
    {/* TODO don't render this always  */}
    {/* How to render the banner only at the home page */}
    {location.pathname === "/" && <CookTimeBanner />}
    <Outlet />
    </>
  );
}