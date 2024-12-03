// The pattern of (complex[] + complex[] = complex[]) .

#include <stdio.h>
#include <complex.h>

#define elm_num 5

void sub(double _Complex a[], double _Complex b[])
{
  double _Complex e[elm_num];
  int j;

  for (j = 0; j < elm_num; j++) {
    e[j] = a[j] + b[j];
  }

  if ((creal(e[0]) == 70.0 && cimag(e[0]) == 12.0) &&
      (creal(e[1]) == 76.0 && cimag(e[1]) == 12.0) &&
      (creal(e[2]) == 82.0 && cimag(e[2]) == 12.0) &&
      (creal(e[3]) == 88.0 && cimag(e[3]) == 12.0) &&
      (creal(e[4]) == 94.0 && cimag(e[4]) == 12.0)) {
    printf("OK\n");
  }else{
    printf("NG (%f,%f),(%f,%f),(%f,%f),(%f,%f),(%f,%f)\n",
	   creal(e[0]),cimag(e[0]),creal(e[1]),cimag(e[1]),creal(e[2]),cimag(e[2]),
	   creal(e[3]),cimag(e[3]),creal(e[4]),cimag(e[4]));
  }
  return;
}
int main()
{
  double _Complex a[elm_num], b[elm_num];
  double c,d;
  int i;

  c = 50.000;
  d = 20.000;
  for (i = 0; i < elm_num; i++){
    a[i] = c + 7.000i;
    b[i] = d + 5.000i;
    c += 4.000;
    d += 2.000;
  }
  
  sub (a,b);
}
