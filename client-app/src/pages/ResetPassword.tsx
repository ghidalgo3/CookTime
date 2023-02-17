import React, {useEffect, useState} from "react"
import { Form, Card, Col, Container, Row, Button, Alert } from "react-bootstrap"
import { ActionFunction, ActionFunctionArgs, Form as RouterForm, Link, useActionData, useSearchParams } from "react-router-dom";
import { IAuthenticationProvider } from "src/shared/AuthenticationProvider";
import { useTitle } from "src/shared/useTitle";

export function action(
  { sendPasswordResetEmail, changePassword }: IAuthenticationProvider): ActionFunction {
  return async (args: ActionFunctionArgs) => {
    const { request, params } = args;
    console.log(request);
    console.log(params);
    const formData = await request.formData()
    if (formData.get("email")) {
      const result = await sendPasswordResetEmail(
        formData.get("email")!.toString());
      return { response: result, statusCode: result.status };
    } else if (formData.get("password")) {
      const result = await changePassword(
        formData.get("userId")!.toString(),
        formData.get("token")!.toString(),
        formData.get("password")!.toString(),
        formData.get("confirmPassword")!.toString(),
      );
      return { response: result, statusCode: result.status };
    }
  }
}

export default function ResetPassword() {
  const [showAlert, setShowAlert] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const emailReceived = searchParams.get("token") && searchParams.get("userId");
  const actionData = useActionData() as {response: Response, statusCode: number};
  useEffect(() => {
    setShowAlert(!!actionData)
  }, [actionData]);
  const dismissAlert = () => setShowAlert(false);

  const successAlert = emailReceived ?
    <Alert dismissible variant="success" onClose={dismissAlert}>
      <Alert.Heading>Success!</Alert.Heading>
      <p className="margin-bottom-0rem">Your password has been changed! <Link state={{redirectTo: "/"}} to="/signin">Sign in.</Link></p>
    </Alert>
  :
    <Alert dismissible variant="success" onClose={dismissAlert}>
      <Alert.Heading>Success!</Alert.Heading>
      <p className="margin-bottom-0rem">Look for a password change email.</p>
    </Alert>;

  const errorAlert =
    <Alert dismissible variant="danger" onClose={dismissAlert}>
      <Alert.Heading>Uh-oh!</Alert.Heading>
      <p className="margin-bottom-0rem">Something is wrong.</p>
    </Alert>;

  let alert
  console.log(actionData);
  switch (actionData?.response.ok) {
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
  useTitle("Reset Password")
  return (
    <Container>
      {showAlert && alert}
      <Row className="justify-content-md-center">
        <Col lg className="max-width-34rem">
          {emailReceived ? <PasswordReset /> : <EmailRequest />}
        </Col>
      </Row>
    </Container>
  )

  function PasswordReset() {
    return (
      <Card>
        <Card.Body style={{ padding: "30px" }}>
          <Card.Title>Forgot your password?</Card.Title>
          <RouterForm method="post">
            <Form.Group className="margin-top-15">
              <Form.Label>New password.</Form.Label>
              <Form.Control
                required
                autoComplete="new-password"
                type="Password"
                className="bg-light"
                placeholder="Password"
                name="password" />
            </Form.Group>
            <Form.Group className="margin-top-15">
              <Form.Label>Confirm your password.</Form.Label>
              <Form.Control
                required
                type="password"
                autoComplete="new-password"
                className="bg-light"
                placeholder="Confirm your password"
                name="confirmPassword" />
            </Form.Group>
            <input type="hidden" name="token" value={searchParams.get("token") ?? ""} />
            <input type="hidden" name="userId" value={searchParams.get("userId") ?? ""} />
            <div className="mx-auto">
              <Button
                type="submit"
                className="pl-button btn-success btn-large btn-block mx-auto">
                  Change password
              </Button>
            </div>
          </RouterForm>
        </Card.Body>
      </Card>
    )
  }

  function EmailRequest() {
    return (
      <Card>
        <Card.Body style={{ padding: "30px" }}>
          <Card.Title>Forgot your password?</Card.Title>
          <RouterForm method="post">
            <Form.Group className="margin-top-15">
              <Form.Label>Please enter your email to reset your password.</Form.Label>
              <Form.Control
                required
                autoComplete="email"
                type="email"
                className="bg-light"
                placeholder="your@email.com"
                name="email" />
            </Form.Group>
            <div className="mx-auto">
              <Button
                type="submit"
                className="pl-button btn-success btn-large btn-block mx-auto">
                Reset password
              </Button>
            </div>
          </RouterForm>
        </Card.Body>
      </Card>);
  }
}