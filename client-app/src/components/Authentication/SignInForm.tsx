import React, {useEffect } from"react"
import { Button, Form } from "react-bootstrap";
import { Form as RouterForm, Link, useNavigate} from "react-router-dom";
import { useAuthentication } from "./AuthenticationContext";


export function SignInForm() {
  const {user, signIn } = useAuthentication();
  const navigate = useNavigate();
  useEffect(() => {
    if (user) {
      if (window.history.length <= 2) {
        navigate("/", {replace: true});
      } else {
        navigate(-1);
      }
    }
  }, [user]);
  return (
    <>
      <h1>Sign in to CookTime</h1>
      <RouterForm method="post" action="/signin">
        <Form.Group className="mb-3" controlId="formBasicEmail">
          {/* <Form.Label>Email address</Form.Label> */}
          <Form.Control type="email" placeholder="Username or email" name="usernameOrEmail" />
        </Form.Group>

        <Form.Group className="mb-3" controlId="formBasicPassword">
          {/* <Form.Label>Password</Form.Label> */}
          <Form.Control type="password" placeholder="Password" name="password" />
        </Form.Group>

        <Button
          disabled={user !== null}
          className="width-100 margin-bottom-8"
          variant="primary"
          type="submit">
          Sign in
        </Button>
      </RouterForm>
      <div className="createAccount">
        New to CookTime? <Link to="/SignUp">Create an account.</Link>
      </div>
      <div className="createAccount">
        <Link to="/ResetPassword">Forgot your password?</Link>
      </div>
    </>
  );
}

