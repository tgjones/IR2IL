#include <stdlib.h>
#include <stdio.h>

#define MAIN int main
#define OK_PRINT puts("OK")
#define ERROR_PRINT puts("NG")
#define PA_START
#define PA_STOP

#if defined(NOMOD)
#define n 64
#else
#define n 65
#endif

void init(float *a,unsigned short int *b,unsigned short int *res) {
  int i;

  for (i = 0; i < n; i++) {
    a[i] = i;
    b[i] = 0;
    res[i] = i;
  }
}

void test(float *a,unsigned short int *b) {
  int i;
  PA_START
  for(i = 0;i < n;i++ ) {
    b[i] = (unsigned short int)a[i];
  }
  PA_STOP
}

MAIN() {
  float a[n];
  unsigned short int b[n],res[n];
  int i;
  init (a,b,res);
  for (int i = 0;i < 2;i++ ) {
    test (a,b);
  }
  for (i = 0;i < n;i++) {
    if (b[i] != res[i]) {
      goto NG;
    }
  }
  OK_PRINT;
  goto OK;
  NG: ERROR_PRINT ;
  OK:;
}
