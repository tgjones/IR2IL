#include<stdio.h>
int main()
{
  static int a;
  struct {
           int a;
           int b;
         }st;
  st.a=20;
  a=10;
  if( a==10 && st.a==20 )
    printf(" TEST OK \n");
  else
    printf(" TEST NG = %d \n",a);
}
