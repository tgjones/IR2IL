#include<stdio.h>
int main(void)
{
  signed short i;
  int cnt;
  signed char incr;
  incr = 1;

  
  cnt = 0;
#pragma omp parallel for schedule(static,1)
  for (i = 10; i > 0; i-=incr) {
#pragma omp atomic
    cnt++;
  }
  if (cnt != 10) {
    printf("test1-1 ng: i-=incr: cnt=%d\n", cnt);
  }

  cnt = 0;
#pragma omp parallel for schedule(static,1)
  for (i = 10; i > 0; i=i-incr) {
#pragma omp atomic
    cnt++;
  }
  if (cnt != 10) {
    printf("test1-2 ng: i=i-incr: cnt=%d\n", cnt);
  }

  
  cnt = 0;
#pragma omp parallel
  {
#pragma omp for schedule(static,1)
    for (i = 10; i > 0; i-=incr) {
#pragma omp atomic
      cnt++;
    }
  }
  if (cnt != 10) {
    printf("test2-1 ng: i-=incr: cnt=%d\n", cnt);
  }

  cnt = 0;
#pragma omp parallel
  {
#pragma omp for schedule(static,1)
    for (i = 10; i > 0; i=i-incr) {
#pragma omp atomic
      cnt++;
    }
  }
  if (cnt != 10) {
    printf("test2-2 ng: i=i-incr: cnt=%d\n", cnt);
  }

  printf("pass\n");
  return 0;
}
