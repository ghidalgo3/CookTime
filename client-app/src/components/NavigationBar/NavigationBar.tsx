import React, { useEffect, useState } from "react"
import { Container, Form, Nav, Navbar, NavDropdown } from "react-bootstrap";
import { isMainThread } from "worker_threads";
import { Form as RouterForm, Link, NavLink } from "react-router-dom";
import imgs from "../../assets";
import "./NavigationBar.css"
import { useContext } from "react";
import { RequireAuth, useAuthentication } from "../Authentication/AuthenticationContext";
import { UserDetails } from "src/shared/AuthenticationProvider";
import { Category } from "src/shared/CookTime/CookTime.types";

export function action() {
}

export function NavigationBar({ categories }: { categories: string[] }) {
  const { user, signOut } = useAuthentication();
  function UserDropdown(user?: UserDetails | null) {
    if (user) {
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
      <NavDropdown title="Admin" id="basic-nav-dropdown">
        <NavDropdown.Item as={Link} to="#action/3.1">All ingredients</NavDropdown.Item>
        <NavDropdown.Item as={Link} to="#action/3.2">Ingredient normalizer</NavDropdown.Item>
      </NavDropdown>
    );
  }

  return (
    <header>
      <Navbar expand="lg">
        <Container>
          <Link to="/">
            <Navbar.Brand id="my-navbar-brand">
              <img
                alt=""
                src={imgs.logo}
                height="30"
                className="d-inline-block align-middle"
              />{' '}
              CookTime
            </Navbar.Brand>
          </Link>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link as={Link} to="/">Recipes</Nav.Link>
              <NavDropdown title="Categories" id="basic-nav-dropdown">
                {
                  categories?.map((category, idx) =>
                    <NavDropdown.Item
                      key={idx}
                      as={Link}
                      to={`/?search=${category}`}>
                      {`${category}`}
                    </NavDropdown.Item>
                  )
                }
              </NavDropdown>
              <Nav.Link
                className={!user ? "disabled" : ""}
                disabled={!user}
                as={Link}
                to="/cart">
                  Groceries List
              </Nav.Link>
              <Nav.Link href="#home">Blog</Nav.Link>
              <RequireAuth roles={["Administrator"]}>
                <AdminNavBarSection />
              </RequireAuth>
              {UserDropdown(user)}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container fluid>
        <RouterForm>
          <Form.Control
            name="search"
            id="search-bar"
            className=""
            type="search"
            placeholder="Search by recipe name or ingredients" />
        </RouterForm>
      </Container>
    </header>);
}

