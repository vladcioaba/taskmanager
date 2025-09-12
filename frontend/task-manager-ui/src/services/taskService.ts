import axios from 'axios';
import type { Task, TaskCreateDto, TaskUpdateDto } from '../types/Task';

const API_BASE_URL = 'http://localhost:5215/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export const taskService = {
  // Get all tasks with optional filter
  async getTasks(isCompleted?: boolean): Promise<Task[]> {
    const params = isCompleted !== undefined ? { isCompleted } : {};
    const response = await apiClient.get<Task[]>('/tasks', { params });
    return response.data;
  },

  // Get a specific task by ID
  async getTask(id: number): Promise<Task> {
    const response = await apiClient.get<Task>(`/tasks/${id}`);
    return response.data;
  },

  // Create a new task
  async createTask(task: TaskCreateDto): Promise<Task> {
    const response = await apiClient.post<Task>('/tasks', task);
    return response.data;
  },

  // Update an existing task
  async updateTask(id: number, task: TaskUpdateDto): Promise<Task> {
    const response = await apiClient.put<Task>(`/tasks/${id}`, task);
    return response.data;
  },

  // Delete a task
  async deleteTask(id: number): Promise<void> {
    await apiClient.delete(`/tasks/${id}`);
  },

  // Toggle task completion
  async toggleTaskCompletion(id: number, isCompleted: boolean): Promise<Task> {
    const response = await apiClient.put<Task>(`/tasks/${id}`, { isCompleted });
    return response.data;
  },
};