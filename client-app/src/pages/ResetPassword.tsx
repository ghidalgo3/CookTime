import React, {useEffect, useState} from "react"
import { Form, Card, Col, Container, Row, Button, Alert } from "react-bootstrap"
import { ActionFunction, ActionFunctionArgs, Form as RouterForm, useActionData } from "react-router-dom";
import { IAuthenticationProvider } from "src/shared/AuthenticationProvider";

export function action(
  { sendPasswordResetEmail }: IAuthenticationProvider): ActionFunction {
  return async (args: ActionFunctionArgs) => {
    const { request } = args;
    const formData = await request.formData()
    const result = await sendPasswordResetEmail(
      formData.get("email")!.toString());
    return { response: result, statusCode: result.status };
  }
}

export default function ResetPassword() {
  const [showAlert, setShowAlert] = useState(false);
  const actionData = useActionData() as {response: Response, statusCode: number};
  useEffect(() => {
    setShowAlert(!!actionData)
  }, [actionData]);
  const dismissAlert = () => setShowAlert(false);
  const successAlert =
    <Alert dismissible variant="success" onClose={dismissAlert}>
      <Alert.Heading>Success!</Alert.Heading>
      <p className="margin-bottom-0rem">Look for a confirmation link in your email to verify your email.</p>
    </Alert>;
  const errorAlert =
    <Alert dismissible variant="danger" onClose={dismissAlert}>
      <Alert.Heading>Uh-oh!</Alert.Heading>
      <p className="margin-bottom-0rem">CookTime does not know about that email.</p>
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
  return (
    <Container>
      {showAlert && alert}
      <Row className="justify-content-md-center">
        <Col lg className="max-width-34rem">
          <Card>
            <Card.Body style={{padding: "30px"}}>
              <Card.Title>Forgot your password?</Card.Title>
              <RouterForm method="post">
                <Form.Group className="margin-top-15">
                  <Form.Label>Confirm your password</Form.Label>
                  <Form.Control
                    required
                    type="Email"
                    className="bg-light"
                    placeholder="your@email"
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
          </Card>
        </Col>
      </Row>
    </Container>
  )
}