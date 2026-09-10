import axios from 'axios'
import type {
  CreateTodoRequest,
  LoginRequest,
  LoginResponse,
  Todo,
  UpdateTodoRequest,
} from '@/types/todo'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export async function login(request: LoginRequest): Promise<void> {
  const response = await api.post<LoginResponse>('/api/auth/login', request)
  localStorage.setItem('accessToken', response.data.accessToken)
}

export function logout(): void {
  localStorage.removeItem('accessToken')
}

export function hasToken(): boolean {
  return localStorage.getItem('accessToken') !== null
}

export async function getTodos(): Promise<Todo[]> {
  const response = await api.get<Todo[]>('/api/todos')
  return response.data
}

export async function createTodo(request: CreateTodoRequest): Promise<Todo> {
  const response = await api.post<Todo>('/api/todos', request)
  return response.data
}

export async function updateTodo(
  id: number,
  request: UpdateTodoRequest,
): Promise<Todo> {
  const response = await api.put<Todo>(`/api/todos/${id}`, request)
  return response.data
}

export async function deleteTodo(id: number): Promise<void> {
  await api.delete(`/api/todos/${id}`)
}