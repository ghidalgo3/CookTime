import React, { useCallback, useEffect, useState } from "react";
import { Alert, Button, Col, Form, Modal, Row, Spinner, Table, Toast, ToastContainer } from "react-bootstrap";
import { Link, useLocation } from "react-router";
import { Helmet } from "react-helmet-async";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import {
  RecipeList,
  getLists,
  createList,
  deleteList,
  updateListMetadata
} from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

export const LIST_MANAGEMENT_PAGE_PATH = "lists";

// Inline editable list row component
function EditableListRow({ 
  list, 
  onTogglePublic, 
  onDelete,
  onSave,
  onError
}: { 
  list: RecipeList; 
  onTogglePublic: (list: RecipeList) => void;
  onDelete: (list: RecipeList) => void;
  onSave: (listId: string, name: string, description: string | null) => void;
  onError: (message: string) => void;
}) {
  const [name, setName] = useState(list.name);
  const [description, setDescription] = useState(list.description || "");
  const [saving, setSaving] = useState(false);
  
  const hasChanges = name !== list.name || description !== (list.description || "");

  const handleSave = async () => {
    if (!name.trim()) return;
    
    setSaving(true);
    try {
      await updateListMetadata(list.id, { 
        name: name.trim(),
        description: description.trim() || null
      });
      onSave(list.id, name.trim(), description.trim() || null);
    } catch (err) {
      onError("Failed to update list");
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="border rounded p-3 mb-3">
      {/* Name input - full width on mobile */}
      <Row className="mb-2">
        <Col xs={12}>
          <Form.Group>
            <Form.Label className="small text-muted mb-1">Name</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={saving}
              size="sm"
            />
          </Form.Group>
        </Col>
      </Row>
      
      {/* Description input - full width */}
      <Row className="mb-2">
        <Col xs={12}>
          <Form.Group>
            <Form.Label className="small text-muted mb-1">Description</Form.Label>
            <Form.Control
              type="text"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Add a description..."
              disabled={saving}
              size="sm"
            />
          </Form.Group>
        </Col>
      </Row>
      
      {/* Stats and actions row - responsive layout */}
      <Row className="align-items-center g-2">
        <Col xs={4} sm={3} md={2}>
          <div className="text-center">
            <Form.Label className="small text-muted mb-1 d-block">Recipes</Form.Label>
            <span>{list.recipeCount}</span>
          </div>
        </Col>
        <Col xs={4} sm={3} md={2}>
          <div className="text-center">
            <Form.Label className="small text-muted mb-1 d-block">Public</Form.Label>
            <Form.Check
              type="switch"
              checked={list.isPublic}
              onChange={() => onTogglePublic(list)}
              disabled={saving}
              className="d-flex justify-content-center"
            />
          </div>
        </Col>
        <Col xs={4} sm={6} md={8} className="text-end">
          <div className="d-flex justify-content-end gap-1 flex-wrap">
            <Link to={`/lists/${list.slug}`}>
              <Button variant="outline-primary" size="sm">View</Button>
            </Link>
            <Button
              variant="outline-danger"
              size="sm"
              onClick={() => onDelete(list)}
              disabled={saving}
            >
              Delete
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={handleSave}
              disabled={saving || !hasChanges || !name.trim()}
            >
              {saving ? (
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
              ) : (
                "Save"
              )}
            </Button>
          </div>
        </Col>
      </Row>
    </div>
  );
}

export default function ListManagement() {
  useTitle("Manage Lists");

  const { user } = useAuthentication();
  const location = useLocation();

  const [lists, setLists] = useState<RecipeList[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Create list state
  const [newListName, setNewListName] = useState("");
  const [creating, setCreating] = useState(false);

  // Delete confirmation modal state
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [listToDelete, setListToDelete] = useState<RecipeList | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Toast state
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchLists = useCallback(async () => {
    try {
      const allLists = await getLists();
      setLists(allLists);
      setError(null);
    } catch (err) {
      setError("Failed to load lists");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (user) {
      fetchLists();
    } else {
      setLoading(false);
    }
  }, [user, fetchLists]);

  const handleCreateList = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newListName.trim()) return;

    setCreating(true);
    try {
      await createList(newListName.trim());
      setNewListName("");
      await fetchLists();
    } catch (err) {
      setError("Failed to create list");
      console.error(err);
    } finally {
      setCreating(false);
    }
  }, [newListName, fetchLists]);

  const handleTogglePublic = useCallback(async (list: RecipeList) => {
    try {
      await updateListMetadata(list.id, { isPublic: !list.isPublic });
      // Update local state
      setLists(prev => prev.map(l =>
        l.id === list.id ? { ...l, isPublic: !l.isPublic } : l
      ));
    } catch (err) {
      setError("Failed to update list visibility");
      console.error(err);
    }
  }, []);

  const handleDeleteClick = useCallback((list: RecipeList) => {
    setListToDelete(list);
    setShowDeleteModal(true);
  }, []);

  const handleConfirmDelete = useCallback(async () => {
    if (!listToDelete) return;

    setDeleting(true);
    try {
      await deleteList(listToDelete.id);
      setShowDeleteModal(false);
      setListToDelete(null);
      await fetchLists();
    } catch (err) {
      setError("Failed to delete list");
      console.error(err);
    } finally {
      setDeleting(false);
    }
  }, [listToDelete, fetchLists]);

  const handleCancelDelete = useCallback(() => {
    setShowDeleteModal(false);
    setListToDelete(null);
  }, []);

  const handleListSaved = useCallback((listId: string, name: string, description: string | null) => {
    setLists(prev => prev.map(l =>
      l.id === listId ? { ...l, name, description } : l
    ));
    setToastMessage(`List "${name}" updated successfully!`);
  }, []);

  if (!user) {
    return (
      <div>
        <Helmet>
          <link rel="canonical" href={`${origin}/${LIST_MANAGEMENT_PAGE_PATH}`} />
        </Helmet>
        <h1 className="margin-bottom-20">Manage Lists</h1>
        <p className="text-muted">
          <Link to="/signin" state={{ redirectTo: location.pathname }}>Sign in</Link> to manage your lists.
        </p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="text-center padding-top-20">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  // Separate system lists (Groceries, Favorites) from user-created lists
  const systemListNames = ["Groceries", "Favorites"];
  const systemLists = lists.filter(l => systemListNames.includes(l.name));
  const userLists = lists.filter(l => !systemListNames.includes(l.name));

  return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/${LIST_MANAGEMENT_PAGE_PATH}`} />
      </Helmet>

      {/* Toast notification */}
      <ToastContainer position="top-end" className="p-3" style={{ zIndex: 1050 }}>
        <Toast 
          show={!!toastMessage} 
          onClose={() => setToastMessage(null)} 
          delay={3000} 
          autohide
          bg="success"
        >
          <Toast.Header>
            <strong className="me-auto">Success</strong>
          </Toast.Header>
          <Toast.Body className="text-white">{toastMessage}</Toast.Body>
        </Toast>
      </ToastContainer>

      <h1 className="margin-bottom-20">Manage Lists</h1>

      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Create New List Form */}
      <Form onSubmit={handleCreateList} className="margin-bottom-20">
        <Row className="align-items-end">
          <Col md={8}>
            <Form.Group>
              <Form.Label>Create a New List</Form.Label>
              <Form.Control
                type="text"
                placeholder="Enter list name..."
                value={newListName}
                onChange={(e) => setNewListName(e.target.value)}
                disabled={creating}
              />
            </Form.Group>
          </Col>
          <Col md={4}>
            <Button
              type="submit"
              variant="primary"
              disabled={creating || !newListName.trim()}
              className="w-100"
            >
              {creating ? (
                <>
                  <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                  {" "}Creating...
                </>
              ) : (
                "Create List"
              )}
            </Button>
          </Col>
        </Row>
      </Form>

      {/* System Lists */}
      {systemLists.length > 0 && (
        <>
          <h4 className="margin-top-20">System Lists</h4>
          <Table striped bordered hover>
            <thead>
              <tr>
                <th>Name</th>
                <th>Recipes</th>
              </tr>
            </thead>
            <tbody>
              {systemLists.map(list => {
                const listPath = list.name === "Groceries" ? "/Groceries" 
                  : list.name === "Favorites" ? "/recipes/favorites"
                  : `/lists/${list.slug}`;
                return (
                  <tr key={list.id}>
                    <td>
                      <Link to={listPath}>
                        {list.name}
                      </Link>
                    </td>
                    <td>{list.recipeCount}</td>
                  </tr>
                );
              })}
            </tbody>
          </Table>
        </>
      )}

      {/* User Created Lists */}
      <h4 className="margin-top-20">My Lists</h4>
      {userLists.length === 0 ? (
        <p className="text-muted">You haven't created any lists yet. Use the form above to create one!</p>
      ) : (
        userLists.map(list => (
          <EditableListRow
            key={list.id}
            list={list}
            onTogglePublic={handleTogglePublic}
            onDelete={handleDeleteClick}
            onSave={handleListSaved}
            onError={setError}
          />
        ))
      )}

      {/* Delete Confirmation Modal */}
      <Modal show={showDeleteModal} onHide={handleCancelDelete} centered>
        <Modal.Header closeButton>
          <Modal.Title>Delete List</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete the list "{listToDelete?.name}"?
          This will remove all recipes from the list. This action cannot be undone.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCancelDelete} disabled={deleting}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                {" "}Deleting...
              </>
            ) : (
              "Delete"
            )}
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}
