#include <stdio.h>
#include <stdlib.h>

#define PRINT_NG puts("NG")
#define PRINT_OK puts("OK")
#define MAINF main

#if defined(ROLL_TIMES)
#define N ROLL_TIMES
#elif defined(MOD)
#define N 65
#else
#define N 64
#endif

void init(unsigned long long * dest) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    dest[i] = 0;
  }
}

void test41(unsigned long long * restrict dest, unsigned long long b) {
  int i;
  for(i = 0;i < N; i++ ) {
    dest[i] = b;
  }
}

int MAINF() {
  unsigned long long dest[N], b;
  int i;
  init(dest);
  b = 1;

  for (i = 0;i < 2;i++ ) {
    test41(dest, b);
  }
  for (i = 0;i < N;i++) {
    if (dest[i] != b) {
      PRINT_NG;
    }
  }

  PRINT_OK;
  return 0;
}