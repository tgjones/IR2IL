#include <stdio.h>
#include <stdlib.h>
#include <limits.h>

#define PRINT_NG puts("NG")
#define PRINT_OK puts("OK")

#if defined(ROLL_TIMES)
#define N ROLL_TIMES
#elif defined(MOD)
#define N 65
#else
#define N 64
#endif

void init(int *a, int *res, int *dest) {
  int i;
  if ((long long int)INT_MAX < (long long int)N-1) {
    *res = (signed int)INT_MAX;
  } else {
    *res = (signed int)((long long int)N-1);
  }
  *dest = 0;

#pragma clang loop vectorize(disable)
  for (i = 0; i < N; i++) {
    a[i] = i;
  }
}

void test(int * restrict a, int * restrict dest) {
  int i;
  int tmp = 0;
  for(i = 0;i < N; i++) {
    if (tmp < a[i]) {
      tmp = a[i];
    }
  }
  *dest = tmp;
}

int main() {
  int a[N];
  int dest, res;
  int i;
  for (i = 0; i < 2; i++) {
    init (a,&res, &dest);
    test (a,&dest);
  }
  if (dest != res) {
    PRINT_NG;
  }

  PRINT_OK;
  return 0;
}
