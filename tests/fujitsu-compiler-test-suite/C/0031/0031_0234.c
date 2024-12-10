#include<stdio.h>
#include<complex.h>

#define _Bool unsigned char
#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double


void dump(CL arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}

_Bool g() { return (_Bool)-1; }

CL foo()
{
   _Bool y = 20; CL z,*x = &z; *x = y;
   return *x;
}

int main()
{
   CL x = foo();
   dump(x, sizeof(x));
   return 0;
}