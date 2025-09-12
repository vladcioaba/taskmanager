import React, { useState } from 'react';
import { Check, Edit2, Trash2, Calendar, Clock } from 'lucide-react';
import type { Task } from '../types/Task';
import { Priority, PriorityLabels, PriorityColors } from '../types/Task';

interface TaskCardProps {
  task: Task;
  onToggleComplete: (id: number, completed: boolean) => void;
  onEdit: (task: Task) => void;
  onDelete: (id: number) => void;
}

const TaskCard: React.FC<TaskCardProps> = ({ task, onToggleComplete, onEdit, onDelete }) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleToggleComplete = async () => {
    setIsLoading(true);
    try {
      await onToggleComplete(task.id, !task.isCompleted);
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  return (
    <div className={`bg-white rounded-lg shadow-md p-4 border-l-4 ${
      task.isCompleted 
        ? 'border-green-500 bg-gray-50' 
        : task.priority === Priority.High 
          ? 'border-red-500'
          : task.priority === Priority.Medium
            ? 'border-yellow-500'
            : 'border-blue-500'
    }`}>
      <div className="flex items-start justify-between">
        <div className="flex items-start space-x-3 flex-1">
          <button
            onClick={handleToggleComplete}
            disabled={isLoading}
            className={`mt-1 w-5 h-5 rounded-full border-2 flex items-center justify-center transition-colors ${
              task.isCompleted
                ? 'bg-green-500 border-green-500 text-white'
                : 'border-gray-300 hover:border-green-500'
            } ${isLoading ? 'opacity-50' : ''}`}
          >
            {task.isCompleted && <Check size={12} />}
          </button>
          
          <div className="flex-1 min-w-0">
            <h3 className={`text-lg font-semibold ${
              task.isCompleted ? 'line-through text-gray-500' : 'text-gray-900'
            }`}>
              {task.title}
            </h3>
            
            {task.description && (
              <p className={`mt-1 text-sm ${
                task.isCompleted ? 'text-gray-400' : 'text-gray-600'
              }`}>
                {task.description}
              </p>
            )}
            
            <div className="mt-3 flex flex-wrap gap-2 text-xs">
              <span className={`px-2 py-1 rounded-full font-medium ${PriorityColors[task.priority]}`}>
                {PriorityLabels[task.priority]} Priority
              </span>
              
              <span className="flex items-center text-gray-500">
                <Calendar size={12} className="mr-1" />
                Created: {formatDate(task.createdAt)}
              </span>
              
              {task.isCompleted && task.completedAt && (
                <span className="flex items-center text-green-600">
                  <Clock size={12} className="mr-1" />
                  Completed: {formatDateTime(task.completedAt)}
                </span>
              )}
            </div>
          </div>
        </div>
        
        <div className="flex space-x-2 ml-4">
          <button
            onClick={() => onEdit(task)}
            className="p-2 text-gray-400 hover:text-blue-500 transition-colors"
            title="Edit task"
          >
            <Edit2 size={16} />
          </button>
          <button
            onClick={() => onDelete(task.id)}
            className="p-2 text-gray-400 hover:text-red-500 transition-colors"
            title="Delete task"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};

export default TaskCard;