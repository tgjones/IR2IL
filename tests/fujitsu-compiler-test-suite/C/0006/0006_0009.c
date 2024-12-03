#include <stdio.h>
#include <complex.h>
#include <math.h>

#if !defined(ROLL_TIMES)
#define ROLL_TIMES 16
#endif

#define N (ROLL_TIMES*2)
#define TYPE double _Complex
#define INIT_VALUE -0.33-0.99*I

#define IS_EQUAL(a, b) ((a==0.0 && cabs(b)<=1E-14) || (cabs(a-b)/cabs(a))<=1E-14)

TYPE a[N],b[N],c[N],d[N];

void init(void) {
  int i;  
  for(i=0; i<ROLL_TIMES;++i) {
    a[i] = cos(i*1.1) * I + 2.1 * i;
  }
}

TYPE sum_ans(void) {
  int i;
  TYPE sum=INIT_VALUE;  
  for(i=0; i<ROLL_TIMES;++i) {
    sum += a[i];
  }
  return sum;
}

TYPE sum(void) {
  int i;
  TYPE sum=INIT_VALUE;
  for(i=0; i<ROLL_TIMES;++i) {
    sum += a[i];
  }
  return sum;
}

int main()
{
  TYPE ans, res; 
  
  init();
  ans = sum_ans();
  res = sum();
  if(IS_EQUAL(ans, res)) {
    puts("OK");
  } else {
    puts("NG");
  }
  return 0;
}
