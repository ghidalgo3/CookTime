import React, { useEffect } from "react"
import { Button, Form } from "react-bootstrap";
import { Form as RouterForm, Link, useLocation, useNavigate } from "react-router";
import { useAuthentication } from "./AuthenticationContext";

function GoogleIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" style={{ marginRight: '8px' }}>
      <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
      <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
      <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" />
      <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" />
    </svg>
  );
}

export function SignInForm() {
  const { user, signIn } = useAuthentication();
  const navigate = useNavigate();
  const { state } = useLocation();
  useEffect(() => {
    if (user) {
      if (state?.redirectTo) {
        navigate(state.redirectTo, { replace: true });
      } else if (window.history.length === 2) {
        // when you initiate a password reset flow, the flow is:
        // 1. Reset
        // 2. Sign In
        // Which is a special case!
        navigate("/", { replace: true });
      } else {
        navigate(-1);
      }
    }
  }, [user, navigate, state?.redirectTo]);

  const handleGoogleSignIn = () => {
    // Redirect to backend OAuth endpoint
    window.location.href = "/api/auth/google";
  };

  return (
    <>
      <h1>Sign in to CookTime</h1>

      <Button
        variant="outline-dark"
        className="width-100 margin-bottom-8 d-flex align-items-center justify-content-center"
        onClick={handleGoogleSignIn}
      >
        <GoogleIcon />
        Sign in with Google
      </Button>

      <div className="d-flex align-items-center my-3">
        <hr className="flex-grow-1" />
        <span className="mx-3 text-muted">or</span>
        <hr className="flex-grow-1" />
      </div>

      <RouterForm method="post" action="/signin">
        <Form.Group className="mb-3" controlId="formBasicEmail">
          {/* <Form.Label>Email address</Form.Label> */}
          <Form.Control type="text" placeholder="Username or email" name="usernameOrEmail" />
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

