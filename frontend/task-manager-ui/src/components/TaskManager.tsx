import React, { useState, useEffect } from 'react';
import { Plus, Filter, RefreshCw } from 'lucide-react';
import TaskCard from './TaskCard';
import TaskForm from './TaskForm';
import type { Task, TaskCreateDto, TaskUpdateDto } from '../types/Task';
import { taskService } from '../services/taskService';

type FilterType = 'all' | 'active' | 'completed';

const TaskManager: React.FC = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [filteredTasks, setFilteredTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<FilterType>('all');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  // Load tasks on component mount
  useEffect(() => {
    loadTasks();
  }, []);

  // Apply filter when tasks or filter changes
  useEffect(() => {
    applyFilter();
  }, [tasks, filter]);

  const loadTasks = async () => {
    try {
      setLoading(true);
      setError(null);
      const tasksData = await taskService.getTasks();
      setTasks(tasksData);
    } catch (err) {
      setError('Failed to load tasks. Please try again.');
      console.error('Error loading tasks:', err);
    } finally {
      setLoading(false);
    }
  };

  const applyFilter = () => {
    let filtered: Task[];
    switch (filter) {
      case 'active':
        filtered = tasks.filter(task => !task.isCompleted);
        break;
      case 'completed':
        filtered = tasks.filter(task => task.isCompleted);
        break;
      default:
        filtered = tasks;
    }
    setFilteredTasks(filtered);
  };

  const handleCreateTask = async (taskData: TaskCreateDto | TaskUpdateDto) => {
    try {
      setFormLoading(true);
      const newTask = await taskService.createTask(taskData as TaskCreateDto);
      setTasks(prev => [newTask, ...prev]);
      setIsFormOpen(false);
    } catch (err) {
      setError('Failed to create task. Please try again.');
      console.error('Error creating task:', err);
      throw err;
    } finally {
      setFormLoading(false);
    }
  };

  const handleUpdateTask = async (taskData: TaskCreateDto | TaskUpdateDto) => {
    if (!editingTask) return;

    try {
      setFormLoading(true);
      const updatedTask = await taskService.updateTask(editingTask.id, taskData as TaskUpdateDto);
      setTasks(prev => prev.map(task => 
        task.id === editingTask.id ? updatedTask : task
      ));
      setIsFormOpen(false);
      setEditingTask(null);
    } catch (err) {
      setError('Failed to update task. Please try again.');
      console.error('Error updating task:', err);
      throw err;
    } finally {
      setFormLoading(false);
    }
  };

  const handleToggleComplete = async (id: number, isCompleted: boolean) => {
    try {
      const updatedTask = await taskService.toggleTaskCompletion(id, isCompleted);
      setTasks(prev => prev.map(task => 
        task.id === id ? updatedTask : task
      ));
    } catch (err) {
      setError('Failed to update task status. Please try again.');
      console.error('Error toggling task completion:', err);
    }
  };

  const handleDeleteTask = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      await taskService.deleteTask(id);
      setTasks(prev => prev.filter(task => task.id !== id));
    } catch (err) {
      setError('Failed to delete task. Please try again.');
      console.error('Error deleting task:', err);
    }
  };

  const handleEditTask = (task: Task) => {
    setEditingTask(task);
    setIsFormOpen(true);
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setEditingTask(null);
  };

  const getTaskCounts = () => {
    const total = tasks.length;
    const completed = tasks.filter(task => task.isCompleted).length;
    const active = total - completed;
    return { total, active, completed };
  };

  const counts = getTaskCounts();

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="animate-spin h-8 w-8 text-blue-500 mx-auto mb-2" />
          <p className="text-gray-600">Loading tasks...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto py-8 px-4">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Task Manager</h1>
          <p className="text-gray-600">
            Organize your tasks and boost your productivity
          </p>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="bg-white rounded-lg shadow p-4">
            <div className="text-2xl font-bold text-blue-600">{counts.total}</div>
            <div className="text-sm text-gray-600">Total Tasks</div>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <div className="text-2xl font-bold text-yellow-600">{counts.active}</div>
            <div className="text-sm text-gray-600">Active Tasks</div>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <div className="text-2xl font-bold text-green-600">{counts.completed}</div>
            <div className="text-sm text-gray-600">Completed</div>
          </div>
        </div>

        {/* Controls */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div className="flex items-center space-x-2">
            <Filter size={20} className="text-gray-400" />
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value as FilterType)}
              className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="all">All Tasks ({counts.total})</option>
              <option value="active">Active ({counts.active})</option>
              <option value="completed">Completed ({counts.completed})</option>
            </select>
          </div>

          <div className="flex items-center space-x-2">
            <button
              onClick={loadTasks}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center space-x-2"
            >
              <RefreshCw size={16} />
              <span>Refresh</span>
            </button>
            <button
              onClick={() => setIsFormOpen(true)}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center space-x-2"
            >
              <Plus size={16} />
              <span>Add Task</span>
            </button>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
            <p className="text-red-800">{error}</p>
            <button
              onClick={() => setError(null)}
              className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
            >
              Dismiss
            </button>
          </div>
        )}

        {/* Tasks List */}
        <div className="space-y-4">
          {filteredTasks.length === 0 ? (
            <div className="text-center py-12">
              <div className="text-gray-400 mb-2">
                <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
              </div>
              <p className="text-gray-500">
                {filter === 'all' 
                  ? 'No tasks yet. Create your first task!'
                  : filter === 'active'
                    ? 'No active tasks. Great job!'
                    : 'No completed tasks yet.'
                }
              </p>
              {filter === 'all' && (
                <button
                  onClick={() => setIsFormOpen(true)}
                  className="mt-4 inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                >
                  <Plus size={16} className="mr-2" />
                  Create Task
                </button>
              )}
            </div>
          ) : (
            filteredTasks.map((task) => (
              <TaskCard
                key={task.id}
                task={task}
                onToggleComplete={handleToggleComplete}
                onEdit={handleEditTask}
                onDelete={handleDeleteTask}
              />
            ))
          )}
        </div>

        {/* Task Form Modal */}
        <TaskForm
          isOpen={isFormOpen}
          onClose={handleCloseForm}
          onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
          editingTask={editingTask}
          isLoading={formLoading}
        />
      </div>
    </div>
  );
};

export default TaskManager;