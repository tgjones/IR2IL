#include <stdio.h>

int main()
{
  const int len = 10000;
  int i;
  double a[len], b[len];

  for (i = 0; i < len; ++i) {
    a[i] = 0.0;
    b[i] = 0.0;
  }

  for (i = 0; i < len; ++i) {
    if (a[i] != b[i]) {
      printf("NG\n");
      return 0;
    }
  }

  printf("OK\n");

  return 0;
}
