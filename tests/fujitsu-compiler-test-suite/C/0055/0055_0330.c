extern int printf(const char *, ...);

int main() {
  printf("  __atomic_thread_fence(0) \n");
  printf("  __atomic_thread_fence(1) \n");
  printf("  __atomic_thread_fence(2) \n");
  printf("  __atomic_thread_fence(3) \n");
  printf("  __atomic_thread_fence(4) \n");
  printf("  __atomic_thread_fence(5) \n");

  return 0;
}