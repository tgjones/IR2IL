#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
  int (*fptr)();
  int   func();
  int a,b,c;
  fptr = func;
  printf("********** 22 TEST START **********\n");
 
  a=(*fptr)();
 
  if (a == 1)
      printf("***** 22 - 01 ***** O   K *****\n");
    else
      printf("***** 22 - 01 ***** N   G *****\n");
 
  printf("********** 22 TEST  END  **********\n");
exit (0);
}
 
int   func()                
{
  return 1;
}
