#include<stdio.h>
int a=20;
int f();
int main()
{
  static int a;
  a=10;
  {
    extern int a;

    if( a==20 && f() )
      printf(" TEST OK \n");
    else
      printf(" TEST NG = %d \n",a);
  }
}
int f()
{
  if( a==20 )
    return 1;
  else
    return 0;
}