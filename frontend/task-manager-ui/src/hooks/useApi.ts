import { useState, useCallback } from 'react';
import axios from 'axios';

interface UseApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

interface UseApiActions<T> {
  execute: (...args: any[]) => Promise<T | undefined>;
  setData: (data: T | null) => void;
  setError: (error: string | null) => void;
  reset: () => void;
}

export function useApi<T>(
  apiFunction: (...args: any[]) => Promise<T>
): UseApiState<T> & UseApiActions<T> {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const execute = useCallback(
    async (...args: any[]): Promise<T | undefined> => {
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      try {
        const result = await apiFunction(...args);
        setState(prev => ({ ...prev, data: result, loading: false }));
        return result;
      } catch (error) {
        let errorMessage = 'An unexpected error occurred';
        
        if (axios.isAxiosError(error)) {
          if (error.response) {
            // Server responded with error status
            errorMessage = error.response.data?.message || 
                          error.response.data || 
                          `Server error: ${error.response.status}`;
          } else if (error.request) {
            // Network error
            errorMessage = 'Network error. Please check your connection and try again.';
          } else {
            errorMessage = error.message;
          }
        } else if (error instanceof Error) {
          errorMessage = error.message;
        }
        
        setState(prev => ({ ...prev, error: errorMessage, loading: false }));
        console.error('API Error:', error);
        return undefined;
      }
    },
    [apiFunction]
  );

  const setData = useCallback((data: T | null) => {
    setState(prev => ({ ...prev, data }));
  }, []);

  const setError = useCallback((error: string | null) => {
    setState(prev => ({ ...prev, error }));
  }, []);

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  return {
    ...state,
    execute,
    setData,
    setError,
    reset,
  };
}