#include <stdio.h>
#include <math.h>

#define N 100
void sub(double a[][N], const double b[][N], const double c[][N], const double d[][N],
	 const int m1[][N], const int m2[][N],
	 const int m3[][N], const int m4[][N])
{
  int i,j;
  
  for (j=0; j<N; ++j) {
    for (i=0; i<N; ++i) {
      if (m1[j][i] % m2[j][i] != 0) {
	if (m3[j][i] % m4[j][i] != 0) {
	  a[j][i] = b[j][i] + c[j][i]*d[j][i];
	} else {
	  a[j][i] = b[j][i] - c[j][i]*d[j][i];
	}
      }
    }
  }
}

#define ANS  416892.551117
#define ABSOLUTE_ERROR       0.00001
#define MAX_MAC(a,b) (((a) > (b)) ? (a) : (b))
#define MIN_MAC(a,b) (((a) < (b)) ? (a) : (b))
int main(void) 
{
  double a[N][N],b[N][N],c[N][N],d[N][N];
  int m1[N][N],m2[N][N],m3[N][N],m4[N][N];
  double res;
  int i, j, num;

  for (j=0; j<N; ++j) {
    for (i=0; i<N; ++i) {
      num = (i+1)+j*N;
      a[j][i] = (double)num/1.0/N;
      b[j][i] = (double)num/2.0/N;
      c[j][i] = (double)num/3.0/N;
      d[j][i] = (double)num/4.0/N;
      m1[j][i] = (num%4 == 0) ? MAX_MAC(num%2,1) : MIN_MAC(-num%2,-1);
      m2[j][i] = (num%8 == 0) ? MIN_MAC(-num%3,-1) : MAX_MAC(num%3,1);
      m3[j][i] = (num%12 == 0) ? MIN_MAC(-num%4,-1) : MAX_MAC(num%4,1);
      m4[j][i] = (num%16 == 0) ? MAX_MAC(num%5,1) : MIN_MAC(-num%5,-1);
    }
  }

  sub((double (*)[N])a,(const double (*)[N])b,(const double (*)[N])c,(const double (*)[N])d,
      (const int (*)[N])m1,(const int (*)[N])m2,
      (const int (*)[N])m3,(const int (*)[N])m4);

  res = 0;
  for (j=0; j<N; ++j) {
    for (i=0; i<N; ++i) {
      res += a[j][i];
    }
  }
  
  if (fabs(res - ANS) <= ABSOLUTE_ERROR) {
    fprintf(stdout, "ok\n");
  } else {
    fprintf(stdout, "ng: %lf != %lf\n", res, ANS);
  }
}
