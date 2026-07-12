import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ConfirmDialog } from '../../../shared/ui/confirm-dialog/confirm-dialog';
import {
  TASK_STATUSES,
  TASK_STATUS_LABELS,
  TaskResponse,
  TaskStatus,
  UpdateTaskRequest,
} from '../../../shared/models/task.models';
import { TaskForm } from '../task-form/task-form';
import { TaskService } from '../task.service';

@Component({
  selector: 'app-task-list',
  imports: [DatePipe, TaskForm, ConfirmDialog],
  templateUrl: './task-list.html',
})
export class TaskList {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);

  readonly user = this.auth.currentUser;
  readonly tasks = this.taskService.tasks;
  readonly loading = this.taskService.loading;
  readonly error = this.taskService.error;

  readonly statuses = TASK_STATUSES;
  readonly statusLabels = TASK_STATUS_LABELS;
  readonly filter = signal<TaskStatus | null>(null);

  // Create/edit modal. `undefined` = closed, `null` = creating, task = editing.
  readonly editing = signal<TaskResponse | null | undefined>(undefined);
  readonly saving = signal(false);
  readonly formError = signal<string | null>(null);

  // Delete confirmation.
  readonly pendingDelete = signal<TaskResponse | null>(null);
  readonly deleteBusy = signal(false);

  constructor() {
    this.auth.loadCurrentUser().subscribe({ error: () => {} });
    this.taskService.load(null);
  }

  setFilter(status: TaskStatus | null): void {
    this.filter.set(status);
    this.taskService.load(status);
  }

  openCreate(): void {
    this.formError.set(null);
    this.editing.set(null);
  }

  openEdit(task: TaskResponse): void {
    this.formError.set(null);
    this.editing.set(task);
  }

  closeForm(): void {
    this.editing.set(undefined);
  }

  onSave(request: UpdateTaskRequest): void {
    this.saving.set(true);
    this.formError.set(null);

    const current = this.editing();
    const request$ = current
      ? this.taskService.update(current.id, request)
      : this.taskService.create(request);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeForm();
        this.taskService.load(this.filter());
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(this.messageFor(error));
      },
    });
  }

  askDelete(task: TaskResponse): void {
    this.pendingDelete.set(task);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const task = this.pendingDelete();
    if (!task) {
      return;
    }
    this.deleteBusy.set(true);
    this.taskService.remove(task.id).subscribe({
      next: () => {
        this.deleteBusy.set(false);
        this.pendingDelete.set(null);
        this.taskService.load(this.filter());
      },
      error: () => {
        this.deleteBusy.set(false);
        this.pendingDelete.set(null);
      },
    });
  }

  changeStatus(task: TaskResponse, status: TaskStatus): void {
    if (status === task.status) {
      return;
    }
    this.taskService
      .update(task.id, {
        title: task.title,
        description: task.description,
        dueDate: task.dueDate,
        status,
      })
      .subscribe({
        next: () => this.taskService.load(this.filter()),
        error: () => this.taskService.load(this.filter()),
      });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  private messageFor(error: HttpErrorResponse): string {
    if (error.status === 400 && error.error?.errors) {
      const errors = error.error.errors as Record<string, string[]>;
      const messages = Object.values(errors).flat();
      if (messages.length) {
        return messages.join(' ');
      }
    }
    return 'Could not save the task. Please try again.';
  }
}
