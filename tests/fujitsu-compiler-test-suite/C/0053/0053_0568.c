#include<stdio.h>
static int a;
int f();
int main()
{
  static int a;
  a = 20;
  if( a==20 && f() )
   printf("\n");
}
int f()
{
  if( a==0 )
   return 1;
  else
   return 0;
}
