#include<stdio.h>
int f()
{
  static int a=10;
  if( a==10 )
   return 1;
  else
   return 0;
}

static int a;
int main()
{
  if( a==0 && f() )
    printf("\n");
}
