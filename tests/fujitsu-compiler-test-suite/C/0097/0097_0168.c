#include<stdio.h>
static void sub(int a, int b, int *c) 
{
  int i=0;
  for (i=0; i < 10; ++i) {
    if (i > 5) {
      *c = *c + a * b;
    }
    if (i == 9) {
      goto out;
    }
  }
out:
  return;
}

int main()
{
  int a,b,c;
  a = 1;
  b = 2;
  c = 0;
  sub(a,b,&c);
  if (c == 8) {
    printf("OK\n");
    return 0;
  } else {
    printf("NG\n");
    return 1;
  }
}
