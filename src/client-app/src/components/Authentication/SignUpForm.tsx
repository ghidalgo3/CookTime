import React, {useEffect, useState} from "react"
import { Button, Card, Form } from "react-bootstrap";
import { Form as RouterForm } from "react-router-dom";

export default function SignUpForm() {
  return (
    <>
      <Card>
        <Card.Body style={{padding: "30px"}}>
          <Card.Title>Sign up</Card.Title>
          <RouterForm method="post">
            <Form.Group className="margin-top-15" controlId="formUsername">
              <Form.Label>This is your public username</Form.Label>
              <Form.Control
                required
                className="bg-light"
                type="text"
                placeholder="User name"
                name="username" />
            </Form.Group>
            <Form.Group className="margin-top-15">
              <Form.Label>You will receive a confirmaton link at this email</Form.Label>
              <Form.Control
                required
                autoComplete="email"
                className="bg-light"
                type="email"
                placeholder="Email"
                name="email" />
            </Form.Group>
            <Form.Group className="margin-top-15">
              <Form.Label>Choose a password</Form.Label>
              <Form.Control
                required
                autoComplete="new-password"
                className="bg-light"
                type="password"
                placeholder="Password"
                name="password" />
            </Form.Group>
            <Form.Group className="margin-top-15">
              <Form.Label>Confirm your password</Form.Label>
              <Form.Control
                required
                type="password"
                autoComplete="new-password"
                className="bg-light"
                placeholder="Password"
                name="confirmPassword" />
            </Form.Group>
            <div className="mx-auto">
              <Button type="submit" className="pl-button btn-success btn-large btn-block mx-auto">Sign up</Button>
            </div>
          </RouterForm>
        </Card.Body>

      </Card>
    </>);
}