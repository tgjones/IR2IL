#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double

void dump(CD arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}

union A { CD a, b, c;};

CD foo()
{
   union A y = { .a = 1 }; int z,*x = &z; *x = y.a;
   return y.a;
}

int main()
{
   CD x = foo();
   dump(x, sizeof(x));
   return 0;
}