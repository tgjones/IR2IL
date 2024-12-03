
#include <stdio.h>
#include <complex.h>

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

void init(float _Complex *a,float _Complex *b,float _Complex *c,float _Complex *res) {
  int i;

  for (i = 0; i < n; i++) {
    a[i] = i;
    b[i] = 2;
    c[i] = i;
    res[i] = i * 3;
  }
}

void test(float _Complex *a,float _Complex *b,float _Complex *c) {
  int i;
  PA_START
  for(i = 0;i < n;i++ ) {
    a[i] = -(a[i] * b[i] - c[i]);
  }
  PA_STOP
}

MAIN() {
  float _Complex a[n],b[n],c[n],res[n];
  int i;
  init (a,b,c,res);
  for (i = 0;i < 2;i++ ) {
    test (a,b,c);
  }
  for (i = 0;i < n;i++) {
    if (a[i] != res[i]) {
      goto NG;
    }
  }
  OK_PRINT;
  goto OK;
  NG: ERROR_PRINT ;
  OK:;
}
