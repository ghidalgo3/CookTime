import React, { useEffect, useState } from "react"
import { Navbar } from "react-bootstrap";
import { Link, Outlet } from "react-router";
import imgs from "src/assets";

export default function PlainLayout() {
  let theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  return (
    <>
      <div data-bs-theme={theme}>
        <Navbar>
          <Navbar.Brand className="mx-auto">
            <Link
              id="pl-navbar-brand"
              to="/" >
              <img
                alt={""}
                height={30}
                style={{ marginRight: 8 }}
                className="d-inline-block align-top"
                src={imgs.logo}></img>
              CookTime
            </Link>
          </Navbar.Brand>
        </Navbar>
        <Outlet />
      </div>
    </>
  );
}