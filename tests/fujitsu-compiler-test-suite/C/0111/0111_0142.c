#include <stdio.h>
void fatal_error (void);
int main()
{
  int i;
  int m2 = -2, m1 = -1, ze = 0, one = 1, five = 5, six = 6 ;
  int eig = 8, nine = 9, ten = 10, ele = 11;
  for (i = 9; i <= 0; i-- )
    {
      if (i == -1 ) fatal_error ();
      if (i == 0 ) fatal_error ();
      if (i == 1 ) fatal_error ();
      if (i == 8 ) fatal_error ();
      if (i == 9 ) fatal_error ();
      if (i == 10 ) fatal_error ();
      if (i == 11 ) fatal_error ();
      if (m1 == i) fatal_error ();
      if (ze == i) fatal_error ();
      if (one == i) fatal_error ();
      if (eig == i) fatal_error ();
      if (nine == i) fatal_error ();
      if (ten == i) fatal_error ();
      if (ele == i) fatal_error ();
    }
  if (i != 9)
    fatal_error ();
  else
    printf("OK\n");
}
void fatal_error ()
{
  printf("NG\n");
}
