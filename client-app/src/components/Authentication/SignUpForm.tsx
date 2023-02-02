import React, {useEffect, useState} from "react"
import { Button, Card, Form } from "react-bootstrap";
import { Form as RouterForm } from "react-router-dom";

export default function SignUpForm() {
  return (
    <>
      <Card>
        <Card.Body>
          <Card.Title>Sign up</Card.Title>
          <RouterForm method="post">
            <Form.Group controlId="formUsername">
              <Form.Label>This is your public username</Form.Label>
              <Form.Control
                required
                type="text"
                placeholder="User name"
                name="username" />
            </Form.Group>
            <Form.Group>
              <Form.Label>You will receive a confirmaton link at this email</Form.Label>
              <Form.Control
                required
                type="email"
                placeholder="Email"
                name="email" />
            </Form.Group>
            <Form.Group>
              <Form.Label>Choose a password</Form.Label>
              <Form.Control
                required
                type="password"
                placeholder="Password"
                name="password" />
            </Form.Group>
            <Form.Group>
              <Form.Label>Confirm your password</Form.Label>
              <Form.Control
                required
                type="password"
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