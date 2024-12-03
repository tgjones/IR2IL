#include <stdio.h>
#include <stdlib.h>
#include <limits.h>

#define MIN1(a,b) (a < b) ? a: b
#define MIN2(a,b) (a <= b) ? a: b

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

void init(unsigned short * dest, unsigned short * src1, unsigned short * src2) {
  int i;
#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    dest[i] = 0U;
    src1[i] = (unsigned short)i+10U;
    src2[i] = (unsigned short)i+8U;
  }
}

void test1(unsigned short * restrict dest, unsigned short * restrict src1, unsigned short * restrict src2) {
  int i;
  for(i = 0;i < N;i++ ) {
    dest[i] = MIN1(src1[i], src2[i]);
  }
}

void test2(unsigned short * restrict dest, unsigned short * restrict src1, unsigned short * restrict src2) {
  int i;
  for(i = 0;i < N;i++ ) {
    dest[i] = MIN2(src1[i], src2[i]);
  }
}

int MAINF() {
  unsigned short dest[N], src1[N], src2[N];
  int i;
  init (dest, src1, src2);
  for (i = 0;i < 2;i++ ) {
    test1 (dest, src1, src2);
    test2 (dest, src1, src2);
  }
  if (N > USHRT_MAX-10) {
    for (i = 0;i <= USHRT_MAX-10;i++) {
      if (dest[i] != src2[i]) {
	PRINT_NG;
      }
    }
  } else {
    for (i = 0;i < N;i++) {
      if (dest[i] != src2[i]) {
	PRINT_NG;
      }
    }
  }
  PRINT_OK;
  return 0;
}
