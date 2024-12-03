#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <complex.h>

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

#define equal_check(val1, val2) (fabsf(crealf(val1) - crealf(val2))/fabsf(crealf(val1)) >= 1.0e-6F || fabsf(cimagf(val1) - cimagf(val2))/fabsf(cimagf(val1)) >= 1.0e-6F)

void init(double *a, double *b, int *mask) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    a[i] = (i+1) * 2.0;
    b[i] = (i+1) * 3.0;
    mask[i] = 1;
  }
}

void test(float _Complex * restrict dest, double * restrict a, double * restrict b, int * restrict mask) {
  int i;
#ifdef HAS_MASK
  for(i = 0;i < N;i++ ) {
    if (mask[i] == 1) {
      dest[i] = !(a[i] > b[i]) ? (1.0F+1.0iF) : (0.0F+0.0iF);
    }
  }
#else
  for(i = 0;i < N;i++ ) {
    dest[i] = !(a[i] > b[i]) ? (1.0F+1.0iF) : (0.0F+0.0iF);
  }
#endif
}

int MAINF() {
  double a[N],b[N];
  float _Complex dest[N];
  int mask[N];
  int i;
  init (a,b,mask);
  for (i = 0;i < 2;i++ ) {
    test (dest,a,b,mask);
  }
  for (i = 0;i < N;i++) {
    if (equal_check(dest[i],(1.0F+1.0iF))) {
      PRINT_NG;
    }
  }
  PRINT_OK;
  return 0;
}
