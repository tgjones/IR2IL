#include<stdio.h>
#include<complex.h>

#define _Bool unsigned char
#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double



void dump(char arg, int len)
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

_Bool g(char *x) { return *x; }

char foo()
{
   char y,*x = &y; *x = 2;
   return g(x);
}

int main()
{
   char x = foo();
   dump(x, sizeof(x));
   return 0;
}
