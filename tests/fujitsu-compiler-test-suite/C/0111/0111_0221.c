#include <stdio.h>
void fatal_error (void);
int main()
{
  int i;
  int m2 = -2, m1 = -1, ze = 0, one = 1, five = 5, six = 6 ;
  int eig = 8, nine = 9, ten = 10, ele = 11;
  for (i = 0; i != 10; i += 2)
    {
      if (i != -1 ) printf("OK\n");
      if (i != 0 ) printf("OK\n");
      if (i != 1 ) printf("OK\n");
      if (i != 8 ) printf("OK\n");
      if (i != 9 ) printf("OK\n");
      if (i != 10 ) printf("OK\n");
      if (i != 11 ) printf("OK\n");
    }
  if (i != 10)
    fatal_error ();
}
void fatal_error ()
{
  printf("NG\n");
}