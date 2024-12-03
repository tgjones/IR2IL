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

#if defined(SHIFT_SIZE)
#define SS SHIFT_SIZE 
#else
#define SS 3
#endif

void init(long long * dest, long long * src1, long long * src2, long long * res) {
  long long i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    dest[i] = 0LL;
    src1[i] = (long long)i;
    src2[i] = (long long)i;
    res[i] = (i << SS) + i;
  }
}

void test(long long * restrict dest, long long * restrict src1, long long * restrict src2) {
  int i;
  for(i = 0;i < N;i++ ) {
    dest[i] = (src1[i] << SS) + src2[i];
  }
}

int MAINF() {
  long long dest[N], src1[N], src2[N], res[N];
  int i;
  init (dest, src1, src2, res);
  for (i = 0;i < 2;i++ ) {
    test (dest, src1, src2);
  }
  for (i = 0;i < N;i++) {
    if (dest[i] != res[i]) {
      PRINT_NG;
    }
  }
  PRINT_OK;
  return 0;
}
