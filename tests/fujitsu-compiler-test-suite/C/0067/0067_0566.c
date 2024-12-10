#include<stdio.h>
int main(void)
{
  unsigned long long i;
  int cnt;
  int n = 1;

  
  cnt = 0;
#pragma omp parallel for schedule(static,n)
  for (i = 10; i > 0; i-=1LL) {
#pragma omp atomic
    cnt++;
  }
  if (cnt != 10) {
    printf("test1-1 ng: i-=1LL: cnt=%d\n", cnt);
  }

  cnt = 0;
#pragma omp parallel for schedule(static,n)
  for (i = 10; i > 0; i=i-1LL) {
#pragma omp atomic
    cnt++;
  }
  if (cnt != 10) {
    printf("test1-2 ng: i=i-1LL: cnt=%d\n", cnt);
  }

  
  cnt = 0;
#pragma omp parallel
  {
#pragma omp for schedule(static,n)
    for (i = 10; i > 0; i-=1LL) {
#pragma omp atomic
      cnt++;
    }
  }
  if (cnt != 10) {
    printf("test2-1 ng: i-=1LL: cnt=%d\n", cnt);
  }

  cnt = 0;
#pragma omp parallel
  {
#pragma omp for schedule(static,n)
    for (i = 10; i > 0; i=i-1LL) {
#pragma omp atomic
      cnt++;
    }
  }
  if (cnt != 10) {
    printf("test2-2 ng: i=i-1LL: cnt=%d\n", cnt);
  }

  printf("pass\n");
  return 0;
}