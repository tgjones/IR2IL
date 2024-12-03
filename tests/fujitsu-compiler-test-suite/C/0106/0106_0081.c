#include <stdio.h>

int main()
{
  const int len = 10000;
  int i;
  double a[len], b[len];

  for (i = 0; i < len; ++i) {
    a[i] = (double)(i + 1);
    b[i] = (double)i;
  }

  for (i = 0; i < len; ++i) {
    if (a[i] != (b[i] + 1.0)) {
      printf("NG\n");
      return 0;
    }
  }

  printf("OK\n");
  return 0;
}
