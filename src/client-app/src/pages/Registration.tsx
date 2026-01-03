import React, { useEffect, useState } from "react"
import { Button, Card, Col, Container, Form, Row } from "react-bootstrap";

export default function Registration() {
  return (

    <Container>
      {/* {showAlert && alert} */}
      <Row className="justify-content-md-center">
        <Col lg className="max-width-34rem">
          <Card>
            <Card.Body style={{ padding: "30px" }}>
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
                    Submit
                  </Button>
                </div>
              </RouterForm>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
}