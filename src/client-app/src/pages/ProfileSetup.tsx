import React, { useState } from "react"
import { Button, Card, Col, Container, Form, Row, Spinner, Alert } from "react-bootstrap";
import { useNavigate } from "react-router";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import { useTitle } from "src/shared/useTitle";

export const PROFILE_SETUP_PATH = "profile/setup";

export default function ProfileSetup() {
    const navigate = useNavigate();
    const { user, updateDisplayName } = useAuthentication();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string>();

    useTitle("Set Up Your Profile");

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setIsSubmitting(true);
        setError(undefined);

        const formData = new FormData(e.currentTarget);
        const displayName = formData.get("displayName")?.toString().trim();

        if (!displayName) {
            setError("Username is required");
            setIsSubmitting(false);
            return;
        }

        if (displayName.length < 2 || displayName.length > 50) {
            setError("Username must be between 2 and 50 characters");
            setIsSubmitting(false);
            return;
        }

        const result = await updateDisplayName(displayName);

        if (result.ok) {
            navigate("/", { replace: true });
        } else {
            setError(result.message);
            setIsSubmitting(false);
        }
    };

    return (
        <Container>
            <Row className="justify-content-md-center">
                <Col style={{ maxWidth: "540px", marginTop: "2rem" }}>
                    <Card>
                        <Card.Body style={{ padding: "30px" }}>
                            <Card.Title as="h1">Welcome to CookTime! 🍳</Card.Title>
                            <p className="text-muted">Choose a username that other users will see when you share recipes.</p>

                            {error && (
                                <Alert variant="danger" dismissible onClose={() => setError(undefined)}>
                                    {error}
                                </Alert>
                            )}

                            <Form onSubmit={handleSubmit}>
                                <Form.Group className="mb-3">
                                    <Form.Label>Username</Form.Label>
                                    <Form.Control
                                        required
                                        type="text"
                                        name="displayName"
                                        placeholder="Enter your username"
                                        minLength={2}
                                        maxLength={50}
                                        autoFocus
                                    />
                                    <Form.Text className="text-muted">
                                        This must be unique across CookTime.
                                    </Form.Text>
                                </Form.Group>

                                <Button
                                    className="width-100"
                                    type="submit"
                                    disabled={isSubmitting}
                                >
                                    {isSubmitting ? <Spinner size="sm" /> : "Continue"}
                                </Button>
                            </Form>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}
