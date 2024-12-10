#include<stdio.h>
#include<complex.h>

#define _Bool unsigned char
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

_Bool g() { return (_Bool)-1; }

CD foo()
{
   _Bool z = -8,*y = &z; CD x = *y;
   return x;
}

int main()
{
   CD x = foo();
   dump(x, sizeof(x));
   return 0;
}