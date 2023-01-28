import React, {useEffect, useState} from"react"
import { Col, Container, Row } from "react-bootstrap";
import { SignInForm } from "src/components";

export function SignIn() {
  return (
    <Container>
      <Row className="justify-content-md-center">
        <Col style={{maxWidth: "540px", marginTop: "1rem"}}>
          <SignInForm />
        </Col>
      </Row>
    </Container>
  );
}