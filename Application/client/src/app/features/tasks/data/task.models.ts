// Request/response contracts for the task endpoints (mirror the .NET DTOs).

export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export const TASK_STATUSES: readonly TaskStatus[] = ['Pending', 'InProgress', 'Completed'] as const;

/** Human-friendly labels for display. */
export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  Pending: 'Pending',
  InProgress: 'In progress',
  Completed: 'Completed',
};

export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDate: string;   // ISO-8601 UTC
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  dueDate: string;
  status?: TaskStatus;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  dueDate: string;
  status: TaskStatus;
}
