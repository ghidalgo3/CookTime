import React, {useEffect, useState} from"react"
import { Container, Nav, Navbar, NavDropdown } from "react-bootstrap";
import { isMainThread } from "worker_threads";
import { Link, NavLink } from "react-router-dom";
import imgs from "../../assets";
import "./NavigationBar.css"
import { useContext } from "react";
import { RequireAuth, useAuthentication } from "../Authentication/AuthenticationContext";
import { UserDetails } from "src/shared/AuthenticationProvider";

export interface NavigationBarProps {
}

export function NavigationBar(props: NavigationBarProps | null) {
  const { user, signOut } = useAuthentication();

  function UserDropdown(user? : UserDetails | null) {
    if(user) {
      return (
        <NavDropdown title={user.name} id="basic-nav-dropdown">
          <NavDropdown.Item as={Link} to="/recipes/mine">
            My recipes
          </NavDropdown.Item>
          <NavDropdown.Item as={Link} to="/recipes/favorites">
            Favorites
          </NavDropdown.Item>
          <NavDropdown.Divider />
          <NavDropdown.Item onClick={signOut}>
            Sign out
          </NavDropdown.Item>
        </NavDropdown>
      )
    } else {
      return <Nav.Link as={Link} to="/signin">Sign in</Nav.Link>
    }
  }

  function AdminNavBarSection() {
    return (
      <RequireAuth roles={["Administrator"]}>
        <NavDropdown title="Admin" id="basic-nav-dropdown">
          <NavDropdown.Item as={Link} to="#action/3.1">All ingredients</NavDropdown.Item>
          <NavDropdown.Item as={Link} to="#action/3.2">Ingredient normalizer</NavDropdown.Item>
        </NavDropdown>
      </RequireAuth>
    );
  }

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
              {/* <NavDropdown.Item href="#action/3.1">Action</NavDropdown.Item>
              <NavDropdown.Item href="#action/3.2">
                Another action
              </NavDropdown.Item>
              <NavDropdown.Item href="#action/3.3">Something</NavDropdown.Item>
              <NavDropdown.Divider />
              <NavDropdown.Item href="#action/3.4">
                Separated link
              </NavDropdown.Item> */}
            </NavDropdown>
            <Nav.Link href="/cart">Groceries List</Nav.Link>
            <Nav.Link href="/Blog">Blog</Nav.Link>
            <AdminNavBarSection />
            {UserDropdown(user)}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>);
}

