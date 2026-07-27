import type { AuthResponse } from '../auth/types';
import type { ProjectBoard, WorkItem, WorkStatus } from '../projects/types';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api';
const STORAGE_KEY = 'devopshub.session';

export function getSession(): AuthResponse | null {
  const value = localStorage.getItem(STORAGE_KEY);
  return value ? JSON.parse(value) as AuthResponse : null;
}

export function setSession(value: AuthResponse | null) {
  if (value) localStorage.setItem(STORAGE_KEY, JSON.stringify(value));
  else localStorage.removeItem(STORAGE_KEY);
}

export async function request<T>(path: string, options: RequestInit = {}, retry = true): Promise<T> {
  const session = getSession();
  const normalizedPath = path.startsWith('/api/') ? path.slice(4) : path;
  const response = await fetch(`${API_URL}${normalizedPath}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(session?.accessToken ? { Authorization: `Bearer ${session.accessToken}` } : {}),
      ...options.headers,
    },
  });

  if (response.status === 401 && retry && session?.refreshToken) {
    const refreshResponse = await fetch(`${API_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: session.refreshToken }),
    });
    if (refreshResponse.ok) {
      setSession(await refreshResponse.json() as AuthResponse);
      return request<T>(normalizedPath, options, false);
    }
    setSession(null);
  }

  if (!response.ok) {
    const body = await response.json().catch(() => ({ message: 'Request failed.' }));
    throw new Error(body.message ?? body.title ?? `Request failed: ${response.status}`);
  }
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export const api = {
  login: (email: string, password: string) => request<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  register: (email: string, displayName: string, password: string) => request<AuthResponse>('/auth/register', { method: 'POST', body: JSON.stringify({ email, displayName, password }) }),
  logout: (refreshToken: string) => request<void>('/auth/logout', { method: 'POST', body: JSON.stringify({ refreshToken }) }),
  board: (projectId: string) => request<ProjectBoard>(`/projects/${projectId}/board`),
  moveItem: (id: string, status: WorkStatus, position: number, sprintId?: string) => request<void>(`/items/${id}/move`, {
    method: 'PATCH',
    body: JSON.stringify({ status, position, sprintId: sprintId ?? null }),
  }),
  createItem: (projectId: string, input: Partial<WorkItem> & { title: string }) => request<WorkItem>(`/projects/${projectId}/items`, {
    method: 'POST',
    body: JSON.stringify({
      title: input.title,
      description: input.description ?? null,
      type: input.type ?? 'Task',
      priority: input.priority ?? 'Medium',
      storyPoints: input.storyPoints ?? 0,
      assigneeId: null,
      sprintId: input.sprintId ?? null,
      epicId: input.epicId ?? null,
      parentId: null,
      dueDate: input.dueDate ?? null,
    }),
  }),
};
