#include<stdio.h>
#define ml(x) ( ( ( ( ( ( ( ( ( ( ( ( ( ( ( ( ( ( x + 
#define m1(x) ( ( ( ( ( ( ( ( ( x ) ) ) ) ) ) ) ) )   
#define mr(x) + x ) ) ) ) ) ) ) ) ) ) ) ) ) ) ) ) ) ) 
#define m2(x) ml(x) ml(x) m1(x) mr(x) mr(x)           
#define m3(x) m2(x) + m2(x) + m2(x) + m2(x) + m2(x)   
#define m4(x) m3(x) + m3(x)                           
int main()
{
  printf("********** \n");
  {
    unsigned int a;

    a = 0u;
    if (a == 0u)
        printf("***** \n");
    a=0u;
    if (a == 0u)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +0+m4(0); a = 0U;
    if (a == 0u)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + 0 + m4(0);a=0U;
    if (a == 0u)
        printf("***** \n");
  }
  {
    unsigned int a;

    a = 037777777776u;
    if (a == 4294967294u)
        printf("***** \n");
    a=037777777776U;
    if (a == 4294967294u)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +m4(0); a = 037777777776U;
    if (a == 4294967294u)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + m4(0); a=037777777776u;
    if (a == 4294967294u)
        printf("***** \n");
  }
  {
    unsigned int a;

    a = 037777777777U;
    if (a == 4294967295u)
        printf("***** \n");
    a=037777777777u;
    if (a == 4294967295u)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 037777777777u;
    if (a == 4294967295u)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=037777777777u;
    if (a == 4294967295u)
        printf("***** \n");
  }
  {
    long int a;

    a = 0l;
    if (a == 0l)
        printf("***** \n");
    a=0l;
    if (a == 0l)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +0+m4(0); a = 0L;
    if (a == 0l)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + 0 + m4(0);a=0L;
    if (a == 0l)
        printf("***** \n");
  }
  {
    long int a;

    a = 017777777776l;
    if (a == 2147483646l)
        printf("***** \n");
    a=017777777776L;
    if (a == 2147483646l)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +m4(0); a = 017777777776L;
    if (a == 2147483646l)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + m4(0); a=017777777776l;
    if (a == 2147483646l)
        printf("***** \n");
  }
  {
    long int a;

    a = 017777777777L;
    if (a == 2147483647l)
        printf("***** \n");
    a=017777777777l;
    if (a == 2147483647l)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 017777777777l;
    if (a == 2147483647l)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=017777777777l;
    if (a == 2147483647l)
        printf("***** \n");
  }
  {
    unsigned long int a;

    a = 020000000000L;
    if (a == 2147483648ul)
        printf("***** \n");
    a=020000000000l;
    if (a == 2147483648ul)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 020000000000l;
    if (a == 2147483648ul)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=020000000000l;
    if (a == 2147483648ul)
        printf("***** \n");
  }
  {
    unsigned long int a;

    a = 0lu;
    if (a == 0ul)
        printf("***** \n");
    a=0lu;
    if (a == 0ul)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +0+m4(0); a = 0LU;
    if (a == 0ul)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + 0 + m4(0);a=0LU;
    if (a == 0ul)
        printf("***** \n");
  }
  {
    unsigned long int a;

    a = 037777777776ul;
    if (a == 4294967294lu)
        printf("***** \n");
    a=037777777776Ul;
    if (a == 4294967294lu)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +m4(0); a = 037777777776LU;
    if (a == 4294967294lu)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + m4(0); a=037777777776ul;
    if (a == 4294967294lu)
        printf("***** \n");
  }
  {
    unsigned long int a;

    a = 037777777777lU;
    if (a == 4294967295ul)
        printf("***** \n");
    a=037777777777lu;
    if (a == 4294967295ul)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 037777777777ul;
    if (a == 4294967295lu)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=037777777777lu;
    if (a == 4294967295ul)
        printf("***** \n");
  }
  {
    int a;

    a = 0;
    if (a == 0)
        printf("***** \n");
    a=0;
    if (a == 0)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +0+m4(0); a = 0;
    if (a == 0)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + 0 + m4(0);a=0;
    if (a == 0)
        printf("***** \n");
  }
  {
    int a;

    a = 017777777776;
    if (a == 2147483646)
        printf("***** \n");
    a=017777777776;
    if (a == 2147483646)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 +m4(0); a = 017777777776;
    if (a == 2147483646)
        printf("***** \n");
    a = 0 + 0 + 0 + 0 + m4(0); a=017777777776;
    if (a == 2147483646)
        printf("***** \n");
  }
  {
    int a;

    a = 017777777777;
    if (a == 2147483647)
        printf("***** \n");
    a=017777777777;
    if (a == 2147483647)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 017777777777;
    if (a == 2147483647)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=017777777777;
    if (a == 2147483647)
        printf("***** \n");
  }
  {
    unsigned int a;

    a = 020000000000;
    if (a == 2147483648u)
        printf("***** \n");
    a=020000000000;
    if (a == 2147483648u)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a = 020000000000;
    if (a == 2147483648u)
        printf("***** \n");
    a = 0 + 0 + 0 + m4(0); a=020000000000;
    if (a == 2147483648u)
        printf("***** \n");
  }
  printf("********** \n");
}
