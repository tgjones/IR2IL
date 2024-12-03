#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double



void dump(unsigned long long int arg, int len)
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

struct A { unsigned long long int a[3], b;};

unsigned long long int foo()
{
   struct A x[] = { [0].a = { 1 }, [0].b = 4 };
   return x[0].b;
}

int main()
{
   unsigned long long int x = foo();
   dump(x, sizeof(x));
   return 0;	
}
