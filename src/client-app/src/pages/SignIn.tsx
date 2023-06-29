import React, {useEffect, useState} from"react"
import { Alert, Col, Container, Row } from "react-bootstrap";
import { Helmet } from "react-helmet-async";
import { ActionFunction, ActionFunctionArgs, redirect, useActionData } from "react-router-dom";
import { SignInForm } from "src/components";
import { IAuthenticationProvider } from "src/shared/AuthenticationProvider";
import { useTitle } from "src/shared/useTitle";

export const SIGN_IN_PAGE_PATH = "signin"

export function action(
  { signIn }: IAuthenticationProvider) : ActionFunction {
  return async (args: ActionFunctionArgs) => {
    const { request } = args;
    const formData = await request.formData()
    const result = await signIn(
      formData.get("usernameOrEmail")!.toString(),
      formData.get("password")!.toString(),
      true);

    if (result !== "Failure") {
      return new Response(null, {
        status: 302,
        headers: { Location: "/" },
      });
    } else {
      return { errors: "Fail" };
    }
  }
}

export function SignIn() {
  const actionData = useActionData();
  useEffect(() => {
    setShowAlert(!!actionData)
  }, [actionData]);
  const [showAlert, setShowAlert] = useState(false);
  const dismissAlert = () => setShowAlert(false);
  const errorAlert =
    <Alert dismissible variant="danger" onClose={dismissAlert}>
      <Alert.Heading>Uh-oh!</Alert.Heading>
      <p className="margin-bottom-0rem">User name or password is wrong</p>
    </Alert>;
  
  useTitle("Sign In")
  return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/${SIGN_IN_PAGE_PATH}`} />
      </Helmet>
      <Container>
        {showAlert && errorAlert}
        <Row className="justify-content-md-center">
          <Col style={{ maxWidth: "540px", marginTop: "1rem" }}>
            <SignInForm />
          </Col>
        </Row>
      </Container>
    </>
  );
}