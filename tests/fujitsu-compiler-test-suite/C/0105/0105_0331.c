#include <stdio.h>
#include <math.h>

#define LENGTH 	10000

double reduction_add(double a[LENGTH]);

int main()
{
  int i;
  double a[LENGTH], r;
  const double k = 0.00012;
  const double base = 1.26000;
  const double margin = 1.0e-5;

  for (i = 0; i < LENGTH; ++i) {
    if ((i % 2) == 0) {
      a[i] = 1.1 * k;
    }
    else {
      a[i] = 1.0 * k;
    }
  }

  r = reduction_add(a);

  if (fabs(r - base) >= margin) {
    printf("NG\n");
  }
  else {
    printf("OK\n");
  }

  return 0;
}

double reduction_add(double a[LENGTH])
{
  double r = 0.0;
  int i;

  for (i = 0; i < LENGTH; ++i) {
    r += a[i];
  }

  return r;
}
