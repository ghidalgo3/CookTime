import React, {useEffect, useState} from "react"
import { Alert, Col, Container, Row } from "react-bootstrap";
import { Helmet } from "react-helmet-async";
import { ActionFunction, ActionFunctionArgs, Form, useActionData } from "react-router-dom";
import SignUpForm from "src/components/Authentication/SignUpForm";
import { IAuthenticationProvider, SignUpResult } from "src/shared/AuthenticationProvider";
import { useTitle } from "src/shared/useTitle";

export const SIGN_UP_PAGE_PATH = "signup";

export function action(
  { signUp }: IAuthenticationProvider) : ActionFunction {
  return async (args: ActionFunctionArgs) => {
    const { request } = args;
    const formData = await request.formData()
    const result = await signUp(
      formData.get("username")!.toString(),
      formData.get("email")!.toString(),
      formData.get("password")!.toString(),
      formData.get("confirmPassword")!.toString());
    return result;
  }
}

export default function SignUp() {
  const actionData = useActionData() as SignUpResult;
  useEffect(() => {
    setShowAlert(!!actionData)
  }, [actionData]);
  const [showAlert, setShowAlert] = useState(false);
  const dismissAlert = () => setShowAlert(false);
  const successAlert = 
      <Alert dismissible variant="success" onClose={dismissAlert}>
        <Alert.Heading>Success!</Alert.Heading>
        <p className="margin-bottom-0rem">Look for a confirmation link in your email to verify your email.</p>
      </Alert>;
  const errorAlert = 
      <Alert dismissible variant="danger" onClose={dismissAlert}>
        <Alert.Heading>Uh-oh!</Alert.Heading>
        <p className="margin-bottom-0rem">{actionData?.message}</p>
    </Alert>;
  let alert
  switch (actionData?.success) {
    case true:
      alert = successAlert
      break;

    case false:
      alert = errorAlert
      break;

    default:
      alert = null
      break;
  }

  useTitle("Sign Up")
  return (
    <>

      <Helmet>
        <link rel="canonical" href={`${origin}/${SIGN_UP_PAGE_PATH}`} />
      </Helmet>

      <Container className="margin-top-625rem">
        {showAlert && alert}
        <Row className="justify-content-md-center">
          <Col lg className="max-width-34rem">
            <SignUpForm />
          </Col>
        </Row>
      </Container>
    </>
  );
}