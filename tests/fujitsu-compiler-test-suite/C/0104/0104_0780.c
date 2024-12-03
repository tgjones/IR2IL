#include <stdio.h>
#include <stdlib.h>
#include <math.h>

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

void init(long long * dest, long long * res, long long * src) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    dest[i] = 0;
    res[i] = i + i;
    src[i] = i;
  }
}

void test(long long * restrict dest, long long * restrict src, int idx) {
  int i;
  for(i = 0;i < N;i++ ) {
    dest[i] = src[i] + (i*idx);
  }
}

int MAINF() {
  long long dest[N], src[N], res[N];
  int i;
  init (dest, res, src);
  for (i = 0;i < 2;i++ ) {
    test (dest, src, i);
  }
  for (i = 0;i < N;i++) {
    if (dest[i] != res[i]) {
      PRINT_NG;
    }
  }
  PRINT_OK;
  return 0;
}
