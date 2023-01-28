import React, {useEffect, useState} from "react"
import { Navbar } from "react-bootstrap";
import { Link, Outlet } from "react-router-dom";
import imgs from "src/assets";

export default function PlainLayout() {
  return (
    <>
      <Navbar>
        <Navbar.Brand className="mx-auto">
          <Link
            id="pl-navbar-brand"
            to="/" >
            <img
              height={30}
              className="d-inline-block align-top"
              src={imgs.logo}></img>
              CookTime
          </Link>
        </Navbar.Brand>
      </Navbar>
      <Outlet />
    </>
  );
}