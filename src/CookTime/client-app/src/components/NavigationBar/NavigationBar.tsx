import React, { useEffect, useState } from "react"
import { Button, Container, Form, Nav, Navbar, NavDropdown } from "react-bootstrap";
import { Form as RouterForm, Link } from "react-router";
import imgs from "../../assets";
import "./NavigationBar.css"
import { GitHubIcon } from "../SVG";
import { RequireAuth, useAuthentication } from "../Authentication/AuthenticationContext";
import { UserDetails } from "src/shared/AuthenticationProvider";
import { RECIPE_CREATE_PAGE_PATH } from "src/pages/RecipeCreation";
import { RecipeList, getLists } from "src/shared/CookTime";

export function action() {
}

export function NavigationBar({ categories }: { categories: string[] }) {
  const { user, signOut } = useAuthentication();
  const [lists, setLists] = useState<RecipeList[]>([]);

  useEffect(() => {
    if (user) {
      getLists().then(setLists).catch(console.error);
    } else {
      setLists([]);
    }
  }, [user]);

  function UserDropdown(user?: UserDetails | null) {
    if (user) {
      return (
        <NavDropdown title={user.name} id="basic-nav-dropdown">
          <NavDropdown.Item as={Link} to="/recipes/mine">
            My recipes
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
        <NavDropdown.Item as={Link} to="/Admin/IngredientsView">Ingredients</NavDropdown.Item>
      </NavDropdown>
    );
  }

  return (
    <header>
      <Navbar expand="md">
        <Container fluid style={{ lineHeight: "30px", justifyContent: "space-between", display: "flex" }}>
          <Link to="/">
            <Navbar.Brand id="my-navbar-brand">
              <img
                alt=""
                src={imgs.logo}
                height="30"
                className="d-inline-block align-middle"
              />{' '}
              <div className="d-inline-block align-middle padding-bottom-2 padding-left-10">CookTime</div>
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
                as={Link}
                to="/about">
                About
              </Nav.Link>
              {user ? (
                <NavDropdown title="Lists" id="lists-nav-dropdown">
                  <NavDropdown.Item as={Link} to="/Groceries">
                    Groceries
                  </NavDropdown.Item>
                  <NavDropdown.Item as={Link} to="/recipes/favorites">
                    Favorites
                  </NavDropdown.Item>
                  {lists.filter(l => l.name !== "Groceries" && l.name !== "Favorites").length > 0 && (
                    <NavDropdown.Divider />
                  )}
                  {lists
                    .filter(l => l.name !== "Groceries" && l.name !== "Favorites")
                    .map(list => (
                      <NavDropdown.Item key={list.id} as={Link} to={`/lists/${list.slug}`}>
                        {list.name}
                      </NavDropdown.Item>
                    ))}
                  <NavDropdown.Divider />
                  <NavDropdown.Item as={Link} to="/lists">
                    Manage Lists
                  </NavDropdown.Item>
                </NavDropdown>
              ) : (
                <Nav.Link
                  id="my-nav-link-disabled"
                  disabled={true}
                  as={Link}
                  to="/Groceries">
                  Lists
                </Nav.Link>
              )}
              <Nav.Link href="/Blog/index.html">Blog</Nav.Link>
              <RequireAuth roles={["Administrator"]}>
                <AdminNavBarSection />
              </RequireAuth>
              {UserDropdown(user)}
              <RequireAuth roles={["User"]}>
                <Nav.Link
                  id={!user ? "my-nav-link-disabled" : ""}
                  // className={!user ? "my-nav-link-disabled" : ""}
                  disabled={!user}
                  as={Link}
                  to={`/${RECIPE_CREATE_PAGE_PATH}`}>
                  <Button>
                    Create Recipe
                  </Button>
                </Nav.Link>
              </RequireAuth>
            </Nav>
            <Nav.Link
              className="github-link"
              href="https://github.com/ghidalgo3/CookTime"
              target="_blank"
              rel="noopener noreferrer"
              title="View on GitHub">
              {GitHubIcon}
            </Nav.Link>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container fluid>
        <RouterForm>
          <Form.Control
            name="search"
            id="search-bar"
            className="me-2 input-field-style"
            type="search"
            placeholder="Search by recipe name or ingredients" />
        </RouterForm>
      </Container>
    </header>);
}

