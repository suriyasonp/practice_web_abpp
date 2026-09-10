export interface Todo {
  id: number
  title: string
  isCompleted: boolean
}

export interface CreateTodoRequest {
  title: string
}

export interface UpdateTodoRequest {
  title: string
  isCompleted: boolean
}

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  accessToken: string
}