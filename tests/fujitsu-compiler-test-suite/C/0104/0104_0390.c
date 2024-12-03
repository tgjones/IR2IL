#include <stdio.h>
#include <stdlib.h>

#define PRINT_NG puts("NG")
#define PRINT_OK puts("OK")
#define MAINF main

#if defined(ROLL_TIMES)
#define N ROLL_TIMES
#elif defined(MOD)
#define N 513
#else
#define N 512
#endif

#define CONST_DISTANCE 33

void init(double * a, double *b, double *res) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    a[i] = 0;
    b[i] = 0;
    res[i] = 0;
  }
#pragma clang loop vectorize(disable)
  for (i = 0; i < N-2; i+=CONST_DISTANCE) {
    b[i] = i;
    b[i+1] = i;
    b[i+2] = i;
    res[i] = i;
    res[i+1] = i;
    res[i+2] = i;
  }
}

void test(double * restrict a, double * restrict b) {
  int i;
  for(i = 0; i < N-2; i+=CONST_DISTANCE) {
    a[i] = b[i];
    a[i+1] = b[i+1];
    a[i+2] = b[i+2];
  }
}

int MAINF() {
  double res[N], a[N];
  double b[N];
  int i;

  init (a, b, res);
  for (i = 0; i < 2; i++) {
    test(a, b);
  }
  for (i = 0; i < N; i++) {
    if (a[i] != res[i]) {
      PRINT_NG;
    }
  }
  PRINT_OK;
  return 0;
}
