import React, {useEffect, useState} from"react"
import { Col, Container, Row } from "react-bootstrap";
import { ActionFunction, ActionFunctionArgs, redirect, useActionData } from "react-router-dom";
import { SignInForm } from "src/components";
import { IAuthenticationProvider } from "src/shared/AuthenticationProvider";

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