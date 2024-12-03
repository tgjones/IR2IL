#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double

void dump(CF arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}




CF foo()
{
   CF result, *P = &result;
   CF resultarray[10];
   ;
   result = ((7893457.1793 + 12345.3456i));
   *P = ((7893457.1793 + 12345.3456i));
   resultarray[9] = ((7893457.1793 + 12345.3456i));
   dump(result, sizeof(result));
   dump(*P, sizeof(result));
   dump(resultarray[9], sizeof(result));
   return result;
}

int main()
{
   volatile int X = foo();
   return 0;	
}
