#include <stdio.h>


#pragma omp declare simd simdlen(8) notinbranch

long int foo1x(int x, int y)
{
  return x+y;
}

void init(int x[], int y[], int n)
{
  int i;
  for (i=0; i<n; i++) {
    x[i] = i;
    y[i] = n-i;
  }
}

int check(long int x[], long int y[], int n)
{
  int i;
  for (i=0; i<n; i++) {
    if (x[i] != y[i]) break; 
  }
  if (i != n) {
    printf("ng\n");
    return 1;
  } else {
    printf("ok\n");
  }
  return 0;
}

int main(void)
{
  const int N=100;
  int a[100], b[100];
  long int r1[100], r2[100];
  int i;

  init(a, b, N);

  for (i=0; i<N; i++) {
    r1[i] = foo1x(a[i], b[i]);
  }

#pragma omp simd simdlen(8)
  for (i=0; i<N; i++) {
    r2[i] = foo1x(a[i], b[i]);
  }

  check(r1, r2, N);
}
