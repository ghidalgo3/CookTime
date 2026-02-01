import React from 'react';
import { Button, Modal } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';

export function DeleteConfirmModal() {
  const { recipe, showDeleteConfirm, setShowDeleteConfirm, onConfirmDelete } =
    useRecipeContext();

  return (
    <Modal show={showDeleteConfirm} onHide={() => setShowDeleteConfirm(false)}>
      <Modal.Header closeButton>
        <Modal.Title>Delete Recipe</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        Are you sure you want to delete "{recipe.name}"? This action cannot be undone!
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={() => setShowDeleteConfirm(false)}>
          Cancel
        </Button>
        <Button variant="danger" onClick={() => onConfirmDelete()}>
          Delete
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
