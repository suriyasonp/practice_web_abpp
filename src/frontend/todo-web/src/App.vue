<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  createTodo,
  deleteTodo,
  getTodos,
  hasToken,
  login,
  logout,
  updateTodo,
} from "@/services/api";
import type { Todo } from "@/types/todo";

const todos = ref<Todo[]>([]);
const username = ref("student");
const password = ref("password");
const newTitle = ref("");
const isAuthenticated = ref(hasToken());
const isLoading = ref(false);
const errorMessage = ref("");

const remainingCount = computed(
  () => todos.value.filter((todo) => !todo.isCompleted).length,
);

function showError(error: unknown): void {
  console.error(error);
  errorMessage.value =
    "Request failed. Check that the API is running and try again.";
}

async function loadTodos(): Promise<void> {
  isLoading.value = true;
  errorMessage.value = "";
  try {
    todos.value = await getTodos();
  } catch (error) {
    showError(error);
  } finally {
    isLoading.value = false;
  }
}

async function handleLogin(): Promise<void> {
  isLoading.value = true;
  errorMessage.value = "";
  try {
    await login({ username: username.value, password: password.value });
    isAuthenticated.value = true;
    await loadTodos();
  } catch (error) {
    showError(error);
  } finally {
    isLoading.value = false;
  }
}

async function handleCreate(): Promise<void> {
  const title = newTitle.value.trim();
  if (!title) return;
  try {
    todos.value.push(await createTodo({ title }));
    newTitle.value = "";
  } catch (error) {
    showError(error);
  }
}

async function handleToggle(todo: Todo): Promise<void> {
  try {
    const updated = await updateTodo(todo.id, {
      title: todo.title,
      isCompleted: !todo.isCompleted,
    });
    Object.assign(todo, updated);
  } catch (error) {
    showError(error);
  }
}

async function handleRename(todo: Todo): Promise<void> {
  const title = window.prompt("Rename Todo", todo.title)?.trim();
  if (!title || title === todo.title) return;
  try {
    const updated = await updateTodo(todo.id, {
      title,
      isCompleted: todo.isCompleted,
    });
    Object.assign(todo, updated);
  } catch (error) {
    showError(error);
  }
}

async function handleDelete(todo: Todo): Promise<void> {
  if (!window.confirm(`Delete "${todo.title}"?`)) return;
  try {
    await deleteTodo(todo.id);
    todos.value = todos.value.filter((item) => item.id !== todo.id);
  } catch (error) {
    showError(error);
  }
}

function handleLogout(): void {
  logout();
  todos.value = [];
  isAuthenticated.value = false;
}

onMounted(() => {
  if (isAuthenticated.value) void loadTodos();
});
</script>

<template>
  <main class="min-h-screen px-4 py-10 text-zinc-950">
    <section class="mx-auto max-w-2xl">
      <header class="mb-8 border-b-4 border-amber-400 pb-5">
        <p class="mb-2 text-sm font-bold uppercase tracking-widest">
          KU Web Technology Workshop
        </p>
        <div class="flex items-end justify-between gap-4">
          <div>
            <h1 class="text-4xl font-black tracking-tight sm:text-5xl">
              Todo App
            </h1>
            <p class="mt-2 text-zinc-600">
              Vue 3 · TypeScript · Tailwind CSS · .NET 10
            </p>
          </div>
          <button
            v-if="isAuthenticated"
            class="border border-zinc-300 bg-white px-4 py-2 font-semibold hover:bg-zinc-100"
            type="button"
            @click="handleLogout"
          >
            Log out
          </button>
        </div>
      </header>

      <p
        v-if="errorMessage"
        class="mb-5 border-l-4 border-red-600 bg-red-50 p-4 text-red-800"
        role="alert"
      >
        {{ errorMessage }}
      </p>

      <form
        v-if="!isAuthenticated"
        class="border border-zinc-200 bg-white p-6 shadow-sm"
        @submit.prevent="handleLogin"
      >
        <h2 class="mb-5 text-2xl font-bold">Sign in</h2>
        <label class="mb-4 block">
          <span class="mb-1 block font-semibold">Username</span>
          <input
            v-model="username"
            class="w-full border border-zinc-300 px-3 py-2 outline-none focus:border-zinc-950"
            autocomplete="username"
            required
          />
        </label>
        <label class="mb-5 block">
          <span class="mb-1 block font-semibold">Password</span>
          <input
            v-model="password"
            class="w-full border border-zinc-300 px-3 py-2 outline-none focus:border-zinc-950"
            type="password"
            autocomplete="current-password"
            required
          />
        </label>
        <button
          class="w-full bg-zinc-950 px-4 py-3 font-bold text-white hover:bg-zinc-800 disabled:opacity-50"
          type="submit"
          :disabled="isLoading"
        >
          {{ isLoading ? "Signing in…" : "Sign in" }}
        </button>
        <p class="mt-4 text-sm text-zinc-500">
          Workshop account: student / password
        </p>
      </form>

      <section v-else>
        <form class="mb-6 flex gap-2" @submit.prevent="handleCreate">
          <input
            v-model="newTitle"
            class="min-w-0 flex-1 border border-zinc-300 bg-white px-4 py-3 outline-none focus:border-zinc-950"
            placeholder="What needs to be done?"
            aria-label="New Todo title"
          />
          <button
            class="bg-amber-400 px-5 py-3 font-bold hover:bg-amber-300"
            type="submit"
          >
            Add
          </button>
        </form>

        <div
          class="mb-3 flex items-center justify-between text-sm text-zinc-600"
        >
          <span>{{ remainingCount }} remaining</span>
          <button
            type="button"
            class="font-semibold underline"
            @click="loadTodos"
          >
            Refresh
          </button>
        </div>

        <p v-if="isLoading" class="border border-zinc-200 bg-white p-5">
          Loading…
        </p>
        <p
          v-else-if="todos.length === 0"
          class="border border-dashed border-zinc-300 p-8 text-center text-zinc-500"
        >
          No Todos yet. Add your first one above.
        </p>
        <ul v-else class="space-y-3">
          <li
            v-for="todo in todos"
            :key="todo.id"
            class="flex items-center gap-3 border border-zinc-200 bg-white p-4 shadow-sm"
          >
            <input
              class="size-5 accent-amber-400"
              type="checkbox"
              :checked="todo.isCompleted"
              :aria-label="`Mark ${todo.title} completed`"
              @change="handleToggle(todo)"
            />
            <span
              class="min-w-0 flex-1 break-words"
              :class="todo.isCompleted ? 'text-zinc-400 line-through' : ''"
              >{{ todo.title }}</span
            >
            <button
              class="font-semibold text-zinc-600 hover:text-zinc-950"
              type="button"
              @click="handleRename(todo)"
            >
              Edit
            </button>
            <button
              class="font-semibold text-red-700 hover:text-red-900"
              type="button"
              @click="handleDelete(todo)"
            >
              Delete
            </button>
          </li>
        </ul>
      </section>
    </section>
  </main>
</template>
