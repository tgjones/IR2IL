#include <stdio.h>
#include <stdarg.h>

#if defined(__i386) || defined(__x86_64__) || defined(__aarch64__)

typedef   unsigned int    test21_t;
typedef   unsigned int    test22_t;
typedef   unsigned int    test23_t;
typedef   unsigned int    test24_t;
#else
typedef   unsigned char   test21_t;
typedef   unsigned char   test22_t;
typedef   unsigned char   test23_t;
typedef   unsigned char   test24_t;
#endif

typedef   unsigned int   test11_t;
typedef   unsigned int   test12_t;
typedef   unsigned int   test13_t;
typedef   unsigned int   test14_t;

typedef   unsigned long long   test31_t;
typedef   unsigned long long   test32_t;
typedef   unsigned long long   test33_t;
typedef   unsigned long long   test34_t;

void foo1(int a1,int a2,int a3,int a4, ...)
{

   va_list ap;
   va_start(ap,a4);

   printf("%d,%d,%d,%d,",a1,a2,a3,a4);

   {
      test11_t a5=va_arg(ap,test11_t);
      test12_t a6=va_arg(ap,test12_t);
      test13_t a7=va_arg(ap,test13_t);
      test14_t a8=va_arg(ap,test14_t);
      printf("%d,%d,%d,%d\n",(int )a5,(int )a6,(int )a7,(int )a8);
   }
}

void foo2(int a1,int a2,int a3,int a4, ...)
{

   va_list ap;
   va_start(ap,a4);

   printf("%d,%d,%d,%d,",a1,a2,a3,a4);

   {
      test21_t a5=va_arg(ap,test21_t);
      test22_t a6=va_arg(ap,test22_t);
      test23_t a7=va_arg(ap,test23_t);
      test24_t a8=va_arg(ap,test24_t);
      printf("%d,%d,%d,%d\n",(int )a5,(int )a6,(int )a7,(int )a8);
   }
}

void foo3(int a1,int a2,int a3,int a4, ...)
{

   va_list ap;
   va_start(ap,a4);

   printf("%d,%d,%d,%d,",a1,a2,a3,a4);

   {
      test31_t a5=va_arg(ap,test31_t);
      test32_t a6=va_arg(ap,test32_t);
      test33_t a7=va_arg(ap,test33_t);
      test34_t a8=va_arg(ap,test34_t);
      printf("%d,%d,%d,%d\n",(int )a5,(int )a6,(int )a7,(int )a8);
   }
}
int main()
{
   foo1(1,2,3,4,(test11_t)5,(test12_t)6,(test13_t)7,(test14_t)8);
   foo2(1,2,3,4,(test21_t)5,(test22_t)6,(test23_t)7,(test24_t)8);
   foo3(1,2,3,4,(test31_t)5,(test32_t)6,(test33_t)7,(test34_t)8);
}
