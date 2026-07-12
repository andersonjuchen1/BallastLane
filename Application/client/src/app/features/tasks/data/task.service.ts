import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateTaskRequest,
  TaskResponse,
  TaskStatus,
  UpdateTaskRequest,
} from './task.models';

/**
 * Owns the current user's task list as signals. `load` refreshes the list for
 * the active status filter; mutations return observables so the caller can
 * drive loading/error UI and then refresh.
 */
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly _tasks = signal<TaskResponse[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly tasks = this._tasks.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  load(status: TaskStatus | null = null): void {
    this._loading.set(true);
    this._error.set(null);

    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }

    this.http.get<TaskResponse[]>(`${this.baseUrl}/tasks`, { params }).subscribe({
      next: (tasks) => {
        this._tasks.set(tasks);
        this._loading.set(false);
      },
      error: () => {
        this._error.set('Could not load your tasks. Please try again.');
        this._loading.set(false);
      },
    });
  }

  create(request: CreateTaskRequest): Observable<TaskResponse> {
    return this.http.post<TaskResponse>(`${this.baseUrl}/tasks`, request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskResponse> {
    return this.http.put<TaskResponse>(`${this.baseUrl}/tasks/${id}`, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tasks/${id}`);
  }
}
