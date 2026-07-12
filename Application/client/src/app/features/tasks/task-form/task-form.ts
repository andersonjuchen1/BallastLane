import { Component, effect, inject, input, output } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import {
  TASK_STATUSES,
  TASK_STATUS_LABELS,
  TaskResponse,
  UpdateTaskRequest,
} from '../data/task.models';

/** Rejects a date before today (local). Allows today and future dates. */
function notPastDate(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  if (!value) {
    return null;
  }
  const selectedEndOfDay = new Date(`${value}T23:59:59`);
  const startOfToday = new Date();
  startOfToday.setHours(0, 0, 0, 0);
  return selectedEndOfDay.getTime() < startOfToday.getTime() ? { pastDate: true } : null;
}

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.html',
})
export class TaskForm {
  private readonly fb = inject(FormBuilder);

  /** The task being edited, or null when creating a new one. */
  readonly task = input<TaskResponse | null>(null);
  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  readonly save = output<UpdateTaskRequest>();
  readonly cancel = output<void>();

  readonly statuses = TASK_STATUSES;
  readonly statusLabels = TASK_STATUS_LABELS;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    dueDate: ['', [Validators.required]],
    status: [TASK_STATUSES[0], [Validators.required]],
  });

  constructor() {
    // Keep the form in sync with the selected task, and apply the
    // "not in the past" rule only when creating (the API allows editing a
    // task whose deadline has already passed).
    effect(() => {
      const task = this.task();
      const dueDate = this.form.controls.dueDate;

      if (task) {
        this.form.setValue({
          title: task.title,
          description: task.description ?? '',
          dueDate: task.dueDate.substring(0, 10),
          status: task.status,
        });
        dueDate.setValidators([Validators.required]);
      } else {
        this.form.reset({ title: '', description: '', dueDate: '', status: TASK_STATUSES[0] });
        dueDate.setValidators([Validators.required, notPastDate]);
      }
      dueDate.updateValueAndValidity({ emitEvent: false });
    });
  }

  get isEditing(): boolean {
    return this.task() !== null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.save.emit({
      title: value.title.trim(),
      description: value.description.trim() ? value.description.trim() : null,
      // Send end-of-day so a task due "today" is not treated as past.
      dueDate: new Date(`${value.dueDate}T23:59:59`).toISOString(),
      status: value.status,
    });
  }
}
