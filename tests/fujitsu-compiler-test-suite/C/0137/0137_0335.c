#include <stdlib.h>
#include <stdio.h>
#include <math.h>
int sub (int L);
int main (void)
{
  printf("***** unroll069 ***** unroll 1 never 0 not 0\n");
  sub (10);
  printf("***** unroll069 ***** end\n");
exit (0);
}
int sub (int L)
{
  int i;
  printf ("1 2 3 4 5 6 7 8 9\n");
  for (i = 1; i < L; i++)
    printf ("%d ", i);
  printf ("\n");
}