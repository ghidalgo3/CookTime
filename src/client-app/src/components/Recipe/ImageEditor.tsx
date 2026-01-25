import React from 'react';
import { Button, Form } from 'react-bootstrap';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  horizontalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Image } from 'src/shared/CookTime';
import { PendingImage, ImageOrderItem } from './RecipeContext';

// Sortable image item component for drag and drop
interface SortableImageItemProps {
  image: Image | PendingImage;
  index: number;
  totalImages: number;
  isPending: boolean;
  onRemove: () => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
}

function SortableImageItem({
  image,
  index,
  totalImages,
  isPending,
  onRemove,
  onMoveUp,
  onMoveDown,
}: SortableImageItemProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id: image.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  const imageUrl = isPending
    ? (image as PendingImage).previewUrl
    : (image as Image).url;

  return (
    <div ref={setNodeRef} style={style} className="sortable-image-item">
      <div className="image-preview-wrapper">
        <img src={imageUrl} alt={`Recipe image ${index + 1}`} />
        {isPending && <div className="pending-badge">Pending</div>}
        <div className="image-controls">
          <Button
            variant="light"
            size="sm"
            className="drag-handle"
            {...attributes}
            {...listeners}
            title="Drag to reorder"
          >
            <i className="bi bi-grip-vertical"></i>
          </Button>
          <div className="arrow-controls">
            <Button
              variant="light"
              size="sm"
              onClick={onMoveUp}
              disabled={index === 0}
              title="Move up"
            >
              <i className="bi bi-chevron-left"></i>
            </Button>
            <Button
              variant="light"
              size="sm"
              onClick={onMoveDown}
              disabled={index === totalImages - 1}
              title="Move down"
            >
              <i className="bi bi-chevron-right"></i>
            </Button>
          </div>
          <Button
            variant="danger"
            size="sm"
            className="remove-btn"
            onClick={onRemove}
            title="Remove image"
          >
            <i className="bi bi-x"></i>
          </Button>
        </div>
        <div className="image-position">{index + 1}</div>
      </div>
    </div>
  );
}

interface ImageEditorProps {
  images: Image[];
  pendingImages: PendingImage[];
  imageOrder: ImageOrderItem[];
  onReorder: (newOrder: ImageOrderItem[]) => void;
  onRemoveExisting: (imageId: string) => void;
  onRemovePending: (imageId: string) => void;
  onAddImages: (files: FileList) => void;
  disabled: boolean;
  maxImages: number;
}

export function ImageEditor({
  images,
  pendingImages,
  imageOrder,
  onReorder,
  onRemoveExisting,
  onRemovePending,
  onAddImages,
  disabled,
  maxImages,
}: ImageEditorProps) {
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Build display items based on the stored order
  const imagesMap = new Map(images.map((img) => [img.id, img]));
  const pendingMap = new Map(pendingImages.map((img) => [img.id, img]));

  type DisplayItem = (Image | PendingImage) & { isPending: boolean };

  const allItems: DisplayItem[] = imageOrder
    .map((orderItem) => {
      if (orderItem.isPending) {
        const pending = pendingMap.get(orderItem.id);
        return pending ? { ...pending, isPending: true } : null;
      } else {
        const existing = imagesMap.get(orderItem.id);
        return existing ? { ...existing, isPending: false } : null;
      }
    })
    .filter((item): item is DisplayItem => item !== null);

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (over && active.id !== over.id) {
      const oldIndex = imageOrder.findIndex((item) => item.id === active.id);
      const newIndex = imageOrder.findIndex((item) => item.id === over.id);
      const reorderedOrder = arrayMove(imageOrder, oldIndex, newIndex);
      onReorder(reorderedOrder);
    }
  };

  const moveItem = (index: number, direction: 'up' | 'down') => {
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= imageOrder.length) return;

    const reorderedOrder = arrayMove(imageOrder, index, newIndex);
    onReorder(reorderedOrder);
  };

  const totalImages = images.length + pendingImages.length;
  const canAddMore = totalImages < maxImages;

  return (
    <div className="image-editor">
      {allItems.length > 0 && (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={allItems.map((item) => item.id)}
            strategy={horizontalListSortingStrategy}
          >
            <div className="image-grid">
              {allItems.map((item, index) => (
                <SortableImageItem
                  key={item.id}
                  image={item as Image | PendingImage}
                  index={index}
                  totalImages={allItems.length}
                  isPending={item.isPending}
                  onRemove={() =>
                    item.isPending
                      ? onRemovePending(item.id)
                      : onRemoveExisting(item.id)
                  }
                  onMoveUp={() => moveItem(index, 'up')}
                  onMoveDown={() => moveItem(index, 'down')}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}

      <Form.Group controlId="formFileMultiple" className="mt-3">
        <Form.Control
          type="file"
          accept=".jpg,.jpeg,.png,.webp"
          multiple
          disabled={disabled || !canAddMore}
          onChange={(e) => {
            const input = e.target as HTMLInputElement;
            if (input.files && input.files.length > 0) {
              onAddImages(input.files);
              input.value = ''; // Reset to allow selecting same files again
            }
          }}
        />
        <Form.Text className="text-muted">
          {canAddMore
            ? `Add up to ${maxImages - totalImages} more image${
                maxImages - totalImages !== 1 ? 's' : ''
              } (max ${maxImages} total). Drag to reorder.`
            : `Maximum ${maxImages} images reached.`}
        </Form.Text>
      </Form.Group>
    </div>
  );
}
