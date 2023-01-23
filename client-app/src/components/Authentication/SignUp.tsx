import React, {useEffect, useState} from"react"
import { Button, Form } from "react-bootstrap";
import { Form as RouterForm, ActionFunctionArgs, redirect} from "react-router-dom";
import { AuthenticationProvider } from "src/shared/AuthenticationProvider";
import { resolveTypeReferenceDirective } from "typescript";


export function SignUp() {
  return (
    <RouterForm method="post" action="/signin">
      <Form.Group className="mb-3" controlId="formBasicEmail">
        <Form.Label>Email address</Form.Label>
        <Form.Control type="email" placeholder="Enter email" name="usernameOrEmail" />
        <Form.Text className="text-muted">
          We'll never share your email with anyone else.
        </Form.Text>
      </Form.Group>

      <Form.Group className="mb-3" controlId="formBasicPassword">
        <Form.Label>Password</Form.Label>
        <Form.Control type="password" placeholder="Password" name="password"/>
      </Form.Group>

      <Form.Group className="mb-3" controlId="formBasicCheckbox">
        <Form.Check type="checkbox" label="Check me out" />
      </Form.Group>

      <Button variant="primary" type="submit">
        Submit
      </Button>
    </RouterForm>
  );
}

export async function action({ request } : ActionFunctionArgs) {
  const formData = await request.formData()
  const result = await AuthenticationProvider.signIn(
    formData.get("usernameOrEmail")!.toString(),
    formData.get("password")!.toString(),
    true); 

  if (result !== "Failure") {
    return redirect("/");
  } else {
    return { errors: "Fail" };
  }
}
