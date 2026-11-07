import { useEffect, useRef, useState } from "react";
import {
  draggable,
  dropTargetForElements,
} from "@atlaskit/pragmatic-drag-and-drop/element/adapter";
import { GripVertical, Trash2 } from "lucide-react";
import type { DynamicFormField } from "../../types";
import { FIELD_TYPE_META } from "../../utils";

export type FormCanvasProps = {
  title?: string;
  emptyMessage?: string;
  fields: DynamicFormField[];
  selectedId?: string | null;
  onSelect: (id: string | null) => void;
  onRemove: (id: string) => void;
  onReorder: (fromIndex: number, toIndex: number) => void;
  onDropType: (type: DynamicFormField["type"], index?: number) => void;
};

function CanvasFieldRow({
  field,
  index,
  selected,
  onSelect,
  onRemove,
  animDelay,
}: {
  field: DynamicFormField;
  index: number;
  selected: boolean;
  onSelect: () => void;
  onRemove: () => void;
  animDelay: number;
}) {
  const ref = useRef<HTMLDivElement>(null);
  const handleRef = useRef<HTMLSpanElement>(null);
  useEffect(() => {
    const el = ref.current;
    const handle = handleRef.current;
    if (!el || !handle) return;
    return draggable({
      element: el,
      dragHandle: handle,
      getInitialData: () => ({
        source: "canvas",
        fieldId: field.id,
        index,
      }),
    });
  }, [field.id, index]);
  return (
    <div
      ref={ref}
      role="button"
      tabIndex={0}
      className={[
        "kit-df-canvas-row flex cursor-pointer items-center gap-2 rounded-xl border bg-white px-2.5 py-2.5 transition",
        selected
          ? "is-selected border-teal-400 bg-teal-50/70 ring-2 ring-teal-400/20"
          : "border-slate-200 hover:border-slate-300 hover:bg-slate-50",
      ].join(" ")}
      style={{ animationDelay: `${animDelay}ms` }}
      onClick={onSelect}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onSelect();
        }
      }}
    >
      <span
        ref={handleRef}
        className="inline-flex cursor-grab text-slate-400 transition-colors hover:text-slate-600 active:cursor-grabbing"
        aria-hidden
      >
        <GripVertical className="h-4 w-4" />
      </span>
      <div className="min-w-0 flex-1">
        <div
          className={[
            "kit-df-canvas-row-label truncate text-sm font-medium",
            field.required ? "is-required text-red-600" : "text-slate-800",
          ].join(" ")}
        >
          {field.label}
          {field.required ? " *" : ""}
        </div>
        <div className="kit-df-canvas-row-meta truncate text-xs text-slate-400">
          {FIELD_TYPE_META[field.type].label} · {field.key}
        </div>
      </div>
      <button
        type="button"
        className="inline-flex rounded-md p-1.5 text-slate-400 transition-colors hover:bg-red-50 hover:text-red-600"
        aria-label="Remove field"
        onClick={(e) => {
          e.stopPropagation();
          onRemove();
        }}
      >
        <Trash2 className="h-4 w-4" />
      </button>
    </div>
  );
}

export function FormCanvas({
  title = "Form",
  emptyMessage,
  fields,
  selectedId,
  onSelect,
  onRemove,
  onReorder,
  onDropType,
}: FormCanvasProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const [isOver, setIsOver] = useState(false);
  useEffect(() => {
    const el = rootRef.current;
    if (!el) return;
  }, [fields.length, onDropType, onReorder]);
  useEffect(() => {
    const cleanups: Array<() => void> = [];
    const nodes = rootRef.current?.querySelectorAll<HTMLElement>(
      "[data-canvas-index]",
    );
    nodes?.forEach((node) => {
      const toIndex = Number(node.dataset.canvasIndex);
      cleanups.push(
        dropTargetForElements({
          element: node,
          getData: () => ({ target: "row", index: toIndex }),
          onDrop: ({ source }) => {
            const data = source.data as {
              source?: string;
              fieldType?: DynamicFormField["type"];
              index?: number;
            };
            if (data.source === "palette" && data.fieldType) {
              onDropType(data.fieldType, toIndex);
              return;
            }
            if (data.source === "canvas" && typeof data.index === "number") {
              if (data.index !== toIndex) onReorder(data.index, toIndex);
            }
          },
        }),
      );
    });
    return () => {
      for (const stop of cleanups) stop();
    };
  }, [fields, onDropType, onReorder]);
  return (
    <div
      ref={rootRef}
      className={[
        "kit-df-canvas kit-dynamic-form-canvas flex min-h-[20rem] flex-col gap-2.5 rounded-[0.875rem] border border-slate-200 bg-white p-3.5 shadow-sm",
        isOver ? "is-over border-teal-300 ring-2 ring-teal-400/30" : "",
      ]
        .filter(Boolean)
        .join(" ")}
      onClick={(e) => {
        if (e.target === e.currentTarget) onSelect(null);
      }}
    >
      <p className="kit-df-section-title m-0 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
        {title}
      </p>
      {fields.length === 0 ? (
        <p className="m-0 flex flex-1 items-center justify-center rounded-lg border border-dashed border-slate-200 bg-slate-50/80 px-4 py-10 text-center text-sm text-slate-500">
          {emptyMessage}
        </p>
      ) : (
        <div className="flex flex-col gap-1.5">
          {fields.map((field, index) => (
            <div key={field.id} data-canvas-index={index}>
              <CanvasFieldRow
                field={field}
                index={index}
                selected={field.id === selectedId}
                onSelect={() =>
                  onSelect(field.id === selectedId ? null : field.id)
                }
                onRemove={() => onRemove(field.id)}
                animDelay={Math.min(index, 8) * 35}
              />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
