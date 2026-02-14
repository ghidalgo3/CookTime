import React, { useEffect, useState } from "react"
import { Alert, Col, Container, Row } from "react-bootstrap";
import { Helmet } from "react-helmet-async";
import { SignInForm } from "src/components";
import { useTitle } from "src/shared/useTitle";

export const SIGN_IN_PAGE_PATH = "signin"

export default function SignIn() {
  const [actionData, setActionData] = useState<any>();
  const [showAlert, setShowAlert] = useState(false);
  useEffect(() => {
    setShowAlert(!!actionData)
  }, [actionData]);
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