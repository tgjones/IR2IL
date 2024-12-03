extern int printf(const char *, ...);

#if defined(__GNUC__) 

#ifndef TYPE
#define TYPE signed long int
#endif

void foo0( TYPE *x, TYPE *y) {
  __atomic_load(x,y,__ATOMIC_RELAXED);
}
void foo1( TYPE *x, TYPE *y) {
  __atomic_load(x,y,__ATOMIC_CONSUME);
}
void foo2( TYPE *x, TYPE *y) {
  __atomic_load(x,y,__ATOMIC_ACQUIRE);
}
void foo5( TYPE *x, TYPE *y) {
  __atomic_load(x,y,__ATOMIC_SEQ_CST);
}

#ifndef NO_EXEC
int main() {

  TYPE a0, a1;
  a0 = 0x7f;
  a1 = 0x7e;
  printf("0x%x\n", (int)a0);
  foo0(&a0, &a1);
  printf("0x%x\n", (int)a1);
  printf("0x%x\n", (int)a0);
  a0 = 0x7f;
  a1 = 0x7e;
  printf("0x%x\n", (int)a0);
  foo1(&a0, &a1);
  printf("0x%x\n", (int)a1);
  printf("0x%x\n", (int)a0);
  a0 = 0x7f;
  a1 = 0x7e;
  printf("0x%x\n", (int)a0);
  foo2(&a0, &a1);
  printf("0x%x\n", (int)a1);
  printf("0x%x\n", (int)a0);
  a0 = 0x7f;
  a1 = 0x7e;
  printf("0x%x\n", (int)a0);
  foo5(&a0, &a1);
  printf("0x%x\n", (int)a1);
  printf("0x%x\n", (int)a0);

  return 0;
}
#endif

#else
int main() {
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);
  printf("0x%x\n", 0x7f);

  return 0;
}
#endif
