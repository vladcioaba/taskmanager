export interface Task {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  createdAt: string;
  completedAt?: string;
  priority: Priority;
}

export interface TaskCreateDto {
  title: string;
  description?: string;
  priority: Priority;
}

export interface TaskUpdateDto {
  title?: string;
  description?: string;
  isCompleted?: boolean;
  priority?: Priority;
}

export enum Priority {
  Low = 1,
  Medium = 2,
  High = 3
}

export const PriorityLabels = {
  [Priority.Low]: 'Low',
  [Priority.Medium]: 'Medium',
  [Priority.High]: 'High'
};

export const PriorityColors = {
  [Priority.Low]: 'bg-blue-100 text-blue-800',
  [Priority.Medium]: 'bg-yellow-100 text-yellow-800',
  [Priority.High]: 'bg-red-100 text-red-800'
};