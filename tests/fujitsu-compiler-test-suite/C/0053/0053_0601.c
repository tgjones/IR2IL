#include<stdio.h>
int f()
{
  int a=10;
  if( a==10 )
   return 1;
  else
   return 0;
}

extern int a=20;
int main()
{
  if( a==20 && f() )
    printf("\n");
}
