import React, {useEffect, useState} from"react"
import { Container, Nav, Navbar, NavDropdown } from "react-bootstrap";
import { isMainThread } from "worker_threads";
import { Link, NavLink } from "react-router-dom";
import imgs from "../../assets";
import "./NavigationBar.css"
import { useContext } from "react";
import { RequireAuth, useAuthentication } from "../Authentication/AuthenticationContext";

export interface NavigationBarProps {
}

export function NavigationBar(props: NavigationBarProps | null) {
  const { user } = useAuthentication();
  return (
    <Navbar expand="lg">
      <Container>
        <Navbar.Brand id="my-navbar-brand" href="#home">
          <img
            alt=""
            src={imgs.logo}
            height="30"
            className="d-inline-block align-middle"
          />{' '}
          CookTime
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link href="#home">Recipes</Nav.Link>
            <NavDropdown title="Categories" id="basic-nav-dropdown">
              <NavDropdown.Item href="#action/3.1">Action</NavDropdown.Item>
              <NavDropdown.Item href="#action/3.2">
                Another action
              </NavDropdown.Item>
              <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
              <NavDropdown.Divider />
              <NavDropdown.Item href="#action/3.4">
                Separated link
              </NavDropdown.Item>
            </NavDropdown>
            <Nav.Link href="#home">Groceries List</Nav.Link>
            {
              user ?
              <Nav.Link>
                Hello {user.name}
              </Nav.Link>
              : 
              <Nav.Link as={Link} to="/signin">
                Sign in
                {/* <Link to="/signin">
                  Sign in
                </Link> */}
              </Nav.Link>
            }
            <Nav.Link href="#home">Blog</Nav.Link>
            <AdminNavBarSection />
            <NavDropdown title="User Id" id="basic-nav-dropdown">
              <NavDropdown.Item as={Link} to="/signin">
                Another sign in button
              </NavDropdown.Item>
              <NavDropdown.Item href="#action/3.2">
                Another action
              </NavDropdown.Item>
              <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
              <NavDropdown.Divider />
              <NavDropdown.Item href="#action/3.4">
                Separated link
              </NavDropdown.Item>
            </NavDropdown>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>);
}

function AdminNavBarSection() {
  return (
    <RequireAuth roles={["Administrator"]}>
      <NavDropdown title="Admin" id="basic-nav-dropdown">
        <NavDropdown.Item href="#action/3.1">Action</NavDropdown.Item>
        <NavDropdown.Item href="#action/3.2">
          Another action
        </NavDropdown.Item>
        <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
        <NavDropdown.Divider />
        <NavDropdown.Item href="#action/3.4">
          Separated link
        </NavDropdown.Item>
      </NavDropdown>
    </RequireAuth>
  );
}