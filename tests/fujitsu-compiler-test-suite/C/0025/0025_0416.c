#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double



void dump(int arg, int len)
{
  if(len == 1)
    printf("%d\n",arg);
  else if(len == 2)
    printf("%hd\n",arg);
  else if(len == 4)
#ifdef 	FLOAT_FLAG
    printf("%f\n",arg);
#else
    printf("%d\n",arg);
#endif
  else if(len == 8)
#ifdef 	DOUBLE_FLAG
    printf("%lf\n",arg);
#else
    printf("%lld\n",arg);
#endif
  else if(len == 16 || len == 12)
    printf("%Lf\n",arg);
}

int bar(int arg)
{
  dump(arg, sizeof(arg));
  return arg;
}

int foo()
{
   int x = 20; CD y = 10.1234 + 12345.3456i; x -= y;
   return bar(x);
}

int main()
{
   volatile int X = foo();
   return 0;	
}