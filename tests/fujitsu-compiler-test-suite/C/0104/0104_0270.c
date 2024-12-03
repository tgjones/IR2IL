
#include <stdio.h>
#include <stdlib.h>
#include <complex.h>
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

void init(double *b, int *c) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    c[i] = i;
    b[i] = (double)i * 2.0 ;
  }
}

void check(double *a) {
  int i;

  for (i = 0;i < N;i++) {
    if (a[i] != (double)i * 2.0) {
      PRINT_NG;
    }
  }
}

double test(double * restrict b, int * restrict c) {
  int i;
  double a[N];

  init(b, c);
  for(i = 0;i < N;i++ ) {
    a[c[i]] = b[i];
  }

  check(a);

  return a[N-1];
}

int MAINF() {
  double a, b[N];
  int c[N];

  a = test (b,c);

  PRINT_OK;
  return 0;
}
